namespace Waystone.Monads.Analyzers;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public abstract class MonadCodeFix : CodeFixProvider
{
    public sealed override FixAllProvider GetFixAllProvider() =>
        WellKnownFixAllProviders.BatchFixer;

    public sealed override async Task RegisterCodeFixesAsync(
        CodeFixContext context)
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

        if (symbols is null)
        {
            return;
        }

        foreach (var diagnostic in context.Diagnostics)
        {
            var node = root.FindNode(
                diagnostic.Location.SourceSpan,
                getInnermostNodeForTie: true);

            Register(context, diagnostic, node, model, symbols);
        }
    }

    protected abstract void Register(
        CodeFixContext context,
        Diagnostic diagnostic,
        SyntaxNode node,
        SemanticModel model,
        MonadSymbols symbols);

    protected static async Task<Document> ReplaceAsync(
        Document document,
        SyntaxNode target,
        SyntaxNode replacement,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken)
           .ConfigureAwait(false);

        var updated = root!.ReplaceNode(
            target,
            replacement.WithTriviaFrom(target)
               .WithAdditionalAnnotations(
                    Simplifier.Annotation,
                    Formatter.Annotation));

        return document.WithSyntaxRoot(updated);
    }

    protected static ExpressionSyntax OptionFactoryCall(
        string name,
        ITypeSymbol? typeArgument,
        SemanticModel model,
        int position,
        MonadSymbols symbols,
        params ExpressionSyntax[] arguments)
    {
        SimpleNameSyntax member = typeArgument is null
            ? SyntaxFactory.IdentifierName(name)
            : SyntaxFactory.GenericName(SyntaxFactory.Identifier(name))
               .WithTypeArgumentList(
                    SyntaxFactory.TypeArgumentList(
                        SyntaxFactory.SingletonSeparatedList(
                            TypeNameOf(typeArgument, model, position))));

        return SyntaxFactory.InvocationExpression(
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    OptionFactoryName(model, position, symbols),
                    member))
           .WithArgumentList(
                SyntaxFactory.ArgumentList(
                    SyntaxFactory.SeparatedList(
                        Enumerable.Select(
                            arguments,
                            SyntaxFactory.Argument))));
    }

    private static ExpressionSyntax OptionFactoryName(
        SemanticModel model,
        int position,
        MonadSymbols symbols) =>
        model.LookupNamespacesAndTypes(position, name: "Option")
           .Any(
                symbol => SymbolEqualityComparer.Default.Equals(
                    symbol,
                    symbols.OptionFactory))
            ? SyntaxFactory.IdentifierName("Option")
            : SyntaxFactory.ParseExpression(
                "Waystone.Monads.Options.Option");

    protected static string Display(ITypeSymbol type) =>
        type.WithNullableAnnotation(NullableAnnotation.None)
           .ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

    protected static TypeSyntax TypeNameOf(
        ITypeSymbol type,
        SemanticModel model,
        int position) =>
        SyntaxFactory.ParseTypeName(
            type.WithNullableAnnotation(NullableAnnotation.None)
               .ToMinimalDisplayString(model, position));
}
