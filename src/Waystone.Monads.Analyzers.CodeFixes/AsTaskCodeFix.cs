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

        foreach (var diagnostic in context.Diagnostics)
        {
            var node = root.FindNode(
                diagnostic.Location.SourceSpan,
                getInnermostNodeForTie: true);

            Register(context, diagnostic, node, model);
        }
    }

    private static void Register(
        CodeFixContext context,
        Diagnostic diagnostic,
        SyntaxNode node,
        SemanticModel model)
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
