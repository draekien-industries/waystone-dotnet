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
public sealed class UseEagerVariantCodeFix : MonadCodeFix
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create("WM2024");

    private protected override void Register(
        CodeFixContext context,
        Diagnostic diagnostic,
        SyntaxNode node,
        SemanticModel model,
        MonadSymbols symbols)
    {
        if (MemberInvocationAt(node) is not var (invocation, access)
         || !FreeDelegateAnalyzer.EagerSiblings.TryGetValue(
                access.Name.Identifier.ValueText,
                out string? eager)
         || Unwrappable(invocation) is not { } body)
        {
            return;
        }

        var arguments = invocation.ArgumentList.Arguments;

        var replacement = invocation
           .WithExpression(
                access.WithName(SyntaxFactory.IdentifierName(eager)))
           .WithArgumentList(
                invocation.ArgumentList.WithArguments(
                    SyntaxFactory.SeparatedList(
                        new[] { arguments[0].WithExpression(body) }
                           .Concat(arguments.Skip(1)))));

        context.RegisterCodeFix(
            CodeAction.Create(
                "Use " + eager + "()",
                token => ReplaceAsync(
                    context.Document,
                    invocation,
                    replacement,
                    token),
                nameof(UseEagerVariantCodeFix)),
            diagnostic);
    }

    /// <remarks>
    /// Declines two shapes the analyzer still reports, rather than emitting source
    /// that does not compile. A lambda anywhere but first means a state overload,
    /// whose leading argument the eager sibling has nowhere to put. A block body
    /// means the expression has to be lifted out of a <c>return</c>, and any
    /// statement beside it would be dropped on the way.
    /// </remarks>
    private static ExpressionSyntax? Unwrappable(
        InvocationExpressionSyntax invocation)
    {
        var arguments = invocation.ArgumentList.Arguments;

        return arguments.Count > 0
            && arguments[0].Expression is LambdaExpressionSyntax lambda
                ? lambda.ExpressionBody
                : null;
    }
}
