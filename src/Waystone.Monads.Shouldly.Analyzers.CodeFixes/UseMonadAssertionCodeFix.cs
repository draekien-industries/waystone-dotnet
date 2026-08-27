namespace Waystone.Monads.Shouldly.Analyzers;

using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public sealed class UseMonadAssertionCodeFix : AssertionCodeFix
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(Rules.RawAssertion.Id);

    protected override void Register(
        CodeFixContext context,
        Diagnostic diagnostic,
        InvocationExpressionSyntax assertion,
        MemberAccessExpressionSyntax access,
        string replacement)
    {
        var monad = MonadOf(access);

        if (monad is null)
        {
            return;
        }

        var rewritten = SyntaxFactory
           .InvocationExpression(
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    monad.WithoutTrivia(),
                    SyntaxFactory.IdentifierName(replacement)))
           .WithArgumentList(assertion.ArgumentList);

        context.RegisterCodeFix(
            CodeAction.Create(
                "Use " + replacement,
                token => ReplaceAsync(
                    context.Document,
                    assertion,
                    rewritten,
                    token),
                nameof(UseMonadAssertionCodeFix)),
            diagnostic);
    }

    /// <summary>
    /// Gets the monad the raw assertion read from, which is the receiver the
    /// replacement is called on.
    /// </summary>
    /// <remarks>
    /// The two reported shapes differ by one level: a state read puts the monad behind
    /// a member access and an unwrap puts it behind an invocation. Both hand back the
    /// same receiver, which is why one fix covers both and why the argument list
    /// transfers unchanged — <c>ShouldBe(expected)</c> and
    /// <c>ShouldBeSomeValue(expected)</c> take the expected value first and a custom
    /// message second, and the analyzer has already refused any other argument shape.
    /// </remarks>
    private static ExpressionSyntax? MonadOf(
        MemberAccessExpressionSyntax access) =>
        access.Expression switch
        {
            MemberAccessExpressionSyntax state => state.Expression,
            InvocationExpressionSyntax
            {
                Expression: MemberAccessExpressionSyntax unwrap,
            } => unwrap.Expression,
            _ => null,
        };
}
