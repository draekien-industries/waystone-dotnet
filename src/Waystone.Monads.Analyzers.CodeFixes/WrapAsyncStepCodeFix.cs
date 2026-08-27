namespace Waystone.Monads.Analyzers;

using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public sealed class WrapAsyncStepCodeFix : MonadCodeFix
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(Rules.TaskReturningAsyncStep.Id);

    /// <remarks>
    /// Only the wrap is offered, though WM2022's message names two corrections. The
    /// other one retypes the step's own declaration, which is safe only where it is
    /// already <c>async</c> — a <c>Task.FromResult</c> body does not convert — and
    /// changes a signature every other caller of that member sees. Neither is a
    /// judgement a fix reading one call site can make, so the message states it and
    /// leaves it to the reader.
    /// </remarks>
    protected override void Register(
        CodeFixContext context,
        Diagnostic diagnostic,
        SyntaxNode node,
        SemanticModel model,
        MonadSymbols symbols)
    {
        if (node.FirstAncestorOrSelf<ArgumentSyntax>() is not
            {
                Expression: { } group,
            }
         || GroupAt(group, model) is not { } method)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                $"Wrap '{method.Name}' in an async lambda",
                token => ReplaceAsync(
                    context.Document,
                    group,
                    Lambda(group, method),
                    token),
                nameof(WrapAsyncStepCodeFix)),
            diagnostic);
    }

    /// <summary>
    /// Gets the single method a group names, or null where it names several or none.
    /// </summary>
    /// <remarks>
    /// The overloaded case declines rather than picking one. The lambda's parameter
    /// name comes from the method's own, and there is no reason to prefer one
    /// overload's spelling of it over another's.
    /// <para>
    /// Only the candidates are read. WM2022 fires on a call that failed overload
    /// resolution, so the group inside it is always reported as a member group with
    /// no bound symbol.
    /// </para>
    /// </remarks>
    private static IMethodSymbol? GroupAt(
        ExpressionSyntax group,
        SemanticModel model)
    {
        var candidates = model.GetSymbolInfo(group)
           .CandidateSymbols
           .OfType<IMethodSymbol>()
           .Take(2)
           .ToImmutableArray();

        return candidates.Length == 1 ? candidates[0] : null;
    }

    private static ExpressionSyntax Lambda(
        ExpressionSyntax group,
        IMethodSymbol method)
    {
        string? parameter = method.Parameters.Length == 1
            ? method.Parameters[0].Name
            : null;

        var body = SyntaxFactory.AwaitExpression(
            SyntaxFactory.InvocationExpression(group.WithoutTrivia())
               .WithArgumentList(
                    SyntaxFactory.ArgumentList(
                        parameter is null
                            ? default
                            : SyntaxFactory.SingletonSeparatedList(
                                SyntaxFactory.Argument(
                                    SyntaxFactory.IdentifierName(parameter))))));

        var async = SyntaxFactory.TokenList(
            SyntaxFactory.Token(SyntaxKind.AsyncKeyword));

        return parameter is null
            ? SyntaxFactory.ParenthesizedLambdaExpression()
               .WithModifiers(async)
               .WithExpressionBody(body)
            : SyntaxFactory.SimpleLambdaExpression(
                    SyntaxFactory.Parameter(
                        SyntaxFactory.Identifier(parameter)))
               .WithModifiers(async)
               .WithExpressionBody(body);
    }
}
