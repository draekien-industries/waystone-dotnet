namespace Waystone.Monads.Analyzers;

using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;

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

    protected static (InvocationExpressionSyntax Invocation,
        MemberAccessExpressionSyntax Access)? MemberInvocationAt(
            SyntaxNode node) =>
        node.FirstAncestorOrSelf<InvocationExpressionSyntax>() is
            {
                Expression: MemberAccessExpressionSyntax access,
            } invocation
            ? (invocation, access)
            : null;

    protected static ExpressionSyntax FactoryCall(
        INamedTypeSymbol factory,
        string name,
        ImmutableArray<ITypeSymbol> typeArguments,
        SemanticModel model,
        int position,
        params ExpressionSyntax[] arguments)
    {
        SimpleNameSyntax member = typeArguments.IsEmpty
            ? SyntaxFactory.IdentifierName(name)
            : SyntaxFactory.GenericName(SyntaxFactory.Identifier(name))
               .WithTypeArgumentList(
                    SyntaxFactory.TypeArgumentList(
                        SyntaxFactory.SeparatedList(
                            typeArguments.Select(
                                typeArgument => TypeNameOf(
                                    typeArgument,
                                    model,
                                    position)))));

        return SyntaxFactory.InvocationExpression(
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    FactoryName(factory, model, position),
                    member))
           .WithArgumentList(
                SyntaxFactory.ArgumentList(
                    SyntaxFactory.SeparatedList(
                        arguments.Select(
                            argument => SyntaxFactory.Argument(
                                argument.WithoutTrivia())))));
    }

    private static ExpressionSyntax FactoryName(
        INamedTypeSymbol factory,
        SemanticModel model,
        int position) =>
        model.LookupNamespacesAndTypes(position, name: factory.Name)
           .Any(
                symbol => SymbolEqualityComparer.Default.Equals(
                    symbol,
                    factory))
            ? SyntaxFactory.IdentifierName(factory.Name)
            : SyntaxFactory.ParseExpression(factory.ToDisplayString());

    protected static TypeSyntax TypeNameOf(
        ITypeSymbol type,
        SemanticModel model,
        int position) =>
        SyntaxFactory.ParseTypeName(
            type.WithNullableAnnotation(NullableAnnotation.None)
               .ToMinimalDisplayString(model, position));
}
