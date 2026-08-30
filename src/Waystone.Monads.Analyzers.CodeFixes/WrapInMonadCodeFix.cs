namespace Waystone.Monads.Analyzers;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public sealed class WrapInMonadCodeFix : MonadCodeFix
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create("CS0029", "CS1503");

    private protected override void Register(
        CodeFixContext context,
        Diagnostic diagnostic,
        SyntaxNode node,
        SemanticModel model,
        MonadSymbols symbols)
    {
        if (node is not ExpressionSyntax expression
         || model.GetTypeInfo(expression).Type is not { } source)
        {
            return;
        }

        var offered = new HashSet<string>();

        foreach (var target in ConversionTargets.Of(expression, model))
        {
            if (FactoryFor(target, source, symbols) is not var (factory,
                    member,
                    typeArguments)
             || !offered.Add(member))
            {
                continue;
            }

            context.RegisterCodeFix(
                CodeAction.Create(
                    $"Wrap in {factory.Name}.{member}()",
                    token => ReplaceAsync(
                        context.Document,
                        expression,
                        FactoryCall(
                            factory,
                            member,
                            typeArguments,
                            model,
                            expression.SpanStart,
                            expression),
                        token),
                    $"{nameof(WrapInMonadCodeFix)}.{member}"),
                diagnostic);
        }
    }

    /// <summary>
    /// Gets the factory member that lifts <paramref name="source" /> into
    /// <paramref name="target" />, or null when nothing does.
    /// </summary>
    /// <remarks>
    /// The removed conversion left no symbol to key a rewrite on, so the target's
    /// own type arguments are what identify the member: a source matching the ok
    /// type takes <c>Ok</c> and one matching the error type takes <c>Err</c>. Both
    /// match when a <c>Result</c> carries the same type twice, and the caller
    /// offers each in turn rather than choosing.
    ///
    /// The derived cases are excluded because their factories return the base type,
    /// so a rewrite naming one would not compile.
    /// </remarks>
    private static (INamedTypeSymbol Factory, string Member,
        ImmutableArray<ITypeSymbol> TypeArguments)? FactoryFor(
            ITypeSymbol target,
            ITypeSymbol source,
            MonadSymbols symbols)
    {
        if (target is not INamedTypeSymbol named
         || symbols.IsDerivedCase(target))
        {
            return null;
        }

        if (symbols.IsOption(target)
         && Same(named.TypeArguments[0], source))
        {
            return (symbols.OptionFactory, "Some", named.TypeArguments);
        }

        if (!symbols.IsResult(target))
        {
            return null;
        }

        if (Same(named.TypeArguments[0], source))
        {
            return (symbols.ResultFactory, "Ok", named.TypeArguments);
        }

        return Same(named.TypeArguments[1], source)
            ? (symbols.ResultFactory, "Err", named.TypeArguments)
            : null;
    }

    private static bool Same(ITypeSymbol left, ITypeSymbol right) =>
        SymbolEqualityComparer.Default.Equals(
            left.WithNullableAnnotation(NullableAnnotation.None),
            right.WithNullableAnnotation(NullableAnnotation.None));
}
