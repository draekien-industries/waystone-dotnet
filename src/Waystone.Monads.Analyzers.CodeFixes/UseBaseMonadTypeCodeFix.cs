namespace Waystone.Monads.Analyzers;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using System.Collections.Immutable;
using System.Composition;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public sealed class UseBaseMonadTypeCodeFix : MonadCodeFix
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create("WM2011");

    protected override void Register(
        CodeFixContext context,
        Diagnostic diagnostic,
        SyntaxNode node,
        SemanticModel model,
        MonadSymbols symbols)
    {
        if (model.GetSymbolInfo(node, context.CancellationToken).Symbol is not
                INamedTypeSymbol type
         || symbols.BaseCaseOf(type) is not { } declared)
        {
            return;
        }

        var replacement = TypeNameOf(declared, model, node.SpanStart);

        context.RegisterCodeFix(
            CodeAction.Create(
                "Use " + Semantics.Display(declared),
                token => ReplaceAsync(
                    context.Document,
                    node,
                    replacement,
                    token),
                nameof(UseBaseMonadTypeCodeFix)),
            diagnostic);
    }
}
