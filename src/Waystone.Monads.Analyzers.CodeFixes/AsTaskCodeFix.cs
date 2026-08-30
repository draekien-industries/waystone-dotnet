namespace Waystone.Monads.Analyzers;

using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public sealed class AsTaskCodeFix : CodeFixProvider
{
    private const string TaskNamespace = "System.Threading.Tasks";

    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create("CS0029", "CS1503");

    public override FixAllProvider GetFixAllProvider() =>
        WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document
           .GetSyntaxRootAsync(context.CancellationToken)
           .ConfigureAwait(false);

        var model = await context.Document
           .GetSemanticModelAsync(context.CancellationToken)
           .ConfigureAwait(false);

        if (root is null || model is null)
        {
            return;
        }

        var symbols = MonadSymbols.TryCreate(model.Compilation);

        foreach (var diagnostic in context.Diagnostics)
        {
            var node = root.FindNode(
                diagnostic.Location.SourceSpan,
                getInnermostNodeForTie: true);

            Register(context, diagnostic, node, model, symbols);
        }
    }

    private static void Register(
        CodeFixContext context,
        Diagnostic diagnostic,
        SyntaxNode node,
        SemanticModel model,
        MonadSymbols? symbols)
    {
        if (node is not ExpressionSyntax expression)
        {
            return;
        }

        if (model.GetTypeInfo(expression).Type is not INamedTypeSymbol source
         || !IsValueTask(source))
        {
            return;
        }

        if (DeclaresAChain(expression, source, model, symbols))
        {
            return;
        }

        if (!TargetsTask(expression, source, model))
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                "Convert to Task with AsTask()",
                token => ReplaceAsync(context.Document, expression, token),
                nameof(AsTaskCodeFix)),
            diagnostic);
    }

    /// <remarks>
    /// A member declared to return a <c>Task</c> of one of this library's monads,
    /// whose whole body is the chain producing it, is the pre-7.0.0 workaround for
    /// reusing an async chain as a step — back when the chaining steps took a
    /// <c>Task</c>-returning delegate and a chain returned a <c>ValueTask</c>. They
    /// now take a <c>ValueTask</c>-returning one, so <c>.AsTask()</c> is never needed
    /// to chain and offering it here is advice that contradicts <c>WM2022</c>, which
    /// tells the consumer to declare that same member <c>ValueTask</c>.
    /// <para>
    /// All three clauses are load-bearing, because the carried type alone separates
    /// nothing. A monad reaches a <c>Task</c>-typed local, field, argument or
    /// <c>Task.WhenAll</c> element for reasons that have nothing to do with chaining,
    /// so the position is checked; and <c>Result.TryAsync</c> in a declared body is a
    /// factory rather than a chain — the break this fix was written for, at
    /// <c>AsyncFactories.cs</c> in the previous-major sample — so the call is checked
    /// too.
    /// </para>
    /// <para>
    /// What it deliberately does not check is whether the member is passed to a
    /// chaining step anywhere, which is the only thing that proves <c>WM2022</c>
    /// would disagree. That answer is not in this document, and a search that stops
    /// at the file would decline or not depending on where the caller happens to
    /// live. So a chain declared <c>Task</c> for genuine interop loses the fix, and
    /// typing <c>.AsTask()</c> by hand is the cost.
    /// </para>
    /// </remarks>
    private static bool DeclaresAChain(
        ExpressionSyntax expression,
        INamedTypeSymbol source,
        SemanticModel model,
        MonadSymbols? symbols) =>
        symbols is not null
     && source.TypeArguments.Length == 1
     && symbols.IsMonad(source.TypeArguments[0])
     && IsDeclaredBody(expression)
     && IsChainingCall(expression, model, symbols);

    private static bool IsChainingCall(
        ExpressionSyntax expression,
        SemanticModel model,
        MonadSymbols symbols) =>
        expression is InvocationExpressionSyntax
        {
            Expression: MemberAccessExpressionSyntax access,
        } invocation
     && model.GetSymbolInfo(invocation).Symbol is IMethodSymbol step
     && symbols.IsMonadCandidate(
            step,
            model.GetTypeInfo(access.Expression).Type);

    private static bool IsDeclaredBody(ExpressionSyntax expression) =>
        expression.Parent switch
        {
            ArrowExpressionClauseSyntax
            {
                Parent: MethodDeclarationSyntax or LocalFunctionStatementSyntax,
            } => true,
            ReturnStatementSyntax statement => statement.Ancestors()
                   .FirstOrDefault(
                        ancestor => ancestor is MethodDeclarationSyntax
                            or LocalFunctionStatementSyntax
                            or AnonymousFunctionExpressionSyntax)
                is MethodDeclarationSyntax or LocalFunctionStatementSyntax,
            _ => false,
        };

    private static bool TargetsTask(
        ExpressionSyntax expression,
        INamedTypeSymbol source,
        SemanticModel model) =>
        ConversionTargets.Of(expression, model)
           .Any(target => AsTaskBridges(source, target));

    private static bool AsTaskBridges(
        INamedTypeSymbol source,
        ITypeSymbol target) =>
        target is INamedTypeSymbol named
     && IsTask(named)
     && named.Arity == source.Arity
     && (named.Arity == 0
      || named.TypeArguments[0] is ITypeParameterSymbol
      || SymbolEqualityComparer.Default.Equals(
             named.TypeArguments[0],
             source.TypeArguments[0]));

    private static bool IsValueTask(INamedTypeSymbol type) =>
        type is { Name: "ValueTask", Arity: 0 or 1 }
     && type.ContainingNamespace?.ToDisplayString() == TaskNamespace;

    private static bool IsTask(INamedTypeSymbol type) =>
        type is { Name: "Task", Arity: 0 or 1 }
     && type.ContainingNamespace?.ToDisplayString() == TaskNamespace;

    private static async Task<Document> ReplaceAsync(
        Document document,
        ExpressionSyntax expression,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken)
           .ConfigureAwait(false);

        var replacement = SyntaxFactory.InvocationExpression(
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    Receiver(expression),
                    SyntaxFactory.IdentifierName("AsTask")))
           .WithArgumentList(SyntaxFactory.ArgumentList());

        var updated = root!.ReplaceNode(
            expression,
            replacement.WithTriviaFrom(expression)
               .WithAdditionalAnnotations(
                    Simplifier.Annotation,
                    Formatter.Annotation));

        return document.WithSyntaxRoot(updated);
    }

    private static ExpressionSyntax Receiver(ExpressionSyntax expression) =>
        expression is InvocationExpressionSyntax
            or MemberAccessExpressionSyntax
            or IdentifierNameSyntax
            or ElementAccessExpressionSyntax
            or ParenthesizedExpressionSyntax
            ? expression.WithoutTrivia()
            : SyntaxFactory.ParenthesizedExpression(
                expression.WithoutTrivia());
}
