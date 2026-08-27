namespace Waystone.Monads.Analyzers;

using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public sealed class UseAndThenCodeFix : MonadCodeFix
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create("WM2005");

    protected override void Register(
        CodeFixContext context,
        Diagnostic diagnostic,
        SyntaxNode node,
        SemanticModel model,
        MonadSymbols symbols)
    {
        if (node.FirstAncestorOrSelf<InvocationExpressionSyntax>() is not
            {
                Expression: MemberAccessExpressionSyntax
                {
                    Expression: InvocationExpressionSyntax
                    {
                        Expression: MemberAccessExpressionSyntax map,
                    } mapped,
                } flatten,
            })
        {
            return;
        }

        if (flatten.Name.Identifier.ValueText != "Flatten"
         || map.Name.Identifier.ValueText != "Map")
        {
            return;
        }

        var replacement = mapped.WithExpression(
            map.WithName(SyntaxFactory.IdentifierName("AndThen")));

        context.RegisterCodeFix(
            CodeAction.Create(
                "Use AndThen",
                token => ReplaceAsync(
                    context.Document,
                    node.FirstAncestorOrSelf<InvocationExpressionSyntax>()!,
                    replacement,
                    token),
                nameof(UseAndThenCodeFix)),
            diagnostic);
    }
}
