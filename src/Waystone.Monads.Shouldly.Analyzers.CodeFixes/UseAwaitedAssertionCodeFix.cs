namespace Waystone.Monads.Shouldly.Analyzers;

using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public sealed class UseAwaitedAssertionCodeFix : AssertionCodeFix
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(Rules.ParenthesisedAwaitAssertion.Id);

    private protected override void Register(
        CodeFixContext context,
        Diagnostic diagnostic,
        InvocationExpressionSyntax assertion,
        MemberAccessExpressionSyntax access,
        string replacement)
    {
        if (access.Expression is not ParenthesizedExpressionSyntax
            {
                Expression: AwaitExpressionSyntax awaited,
            })
        {
            return;
        }

        var rewritten = SyntaxFactory.AwaitExpression(
            SyntaxFactory.Token(
                SyntaxFactory.TriviaList(),
                SyntaxKind.AwaitKeyword,
                SyntaxFactory.TriviaList(SyntaxFactory.Space)),
            SyntaxFactory
               .InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        awaited.Expression.WithoutTrivia(),
                        SyntaxFactory.IdentifierName(replacement)))
               .WithArgumentList(assertion.ArgumentList));

        context.RegisterCodeFix(
            CodeAction.Create(
                "Use " + replacement,
                token => ReplaceAsync(
                    context.Document,
                    assertion,
                    rewritten,
                    token),
                nameof(UseAwaitedAssertionCodeFix)),
            diagnostic);
    }
}
