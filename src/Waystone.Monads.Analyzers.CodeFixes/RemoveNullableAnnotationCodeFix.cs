namespace Waystone.Monads.Analyzers;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Composition;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public sealed class RemoveNullableAnnotationCodeFix : MonadCodeFix
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create("WM1008");

    protected override void Register(
        CodeFixContext context,
        Diagnostic diagnostic,
        SyntaxNode node,
        SemanticModel model,
        MonadSymbols symbols)
    {
        if (node.FirstAncestorOrSelf<NullableTypeSyntax>() is not
                { } annotated)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                "Remove the nullable annotation",
                token => ReplaceAsync(
                    context.Document,
                    annotated,
                    annotated.ElementType,
                    token),
                nameof(RemoveNullableAnnotationCodeFix)),
            diagnostic);
    }
}
