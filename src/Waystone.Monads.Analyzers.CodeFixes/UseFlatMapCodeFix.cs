namespace Waystone.Monads.Analyzers;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Composition;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public sealed class UseFlatMapCodeFix : MonadCodeFix
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
            map.WithName(SyntaxFactory.IdentifierName("FlatMap")));

        context.RegisterCodeFix(
            CodeAction.Create(
                "Use FlatMap",
                token => ReplaceAsync(
                    context.Document,
                    node.FirstAncestorOrSelf<InvocationExpressionSyntax>()!,
                    replacement,
                    token),
                nameof(UseFlatMapCodeFix)),
            diagnostic);
    }
}
