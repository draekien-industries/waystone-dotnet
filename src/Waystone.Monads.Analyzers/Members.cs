namespace Waystone.Monads.Analyzers;

using Microsoft.CodeAnalysis;

internal static class Members
{
    public static bool IsOrdinary(ISymbol symbol) =>
        !symbol.IsImplicitlyDeclared
     && symbol switch
        {
            IMethodSymbol method => method.MethodKind == MethodKind.Ordinary,
            IPropertySymbol property => !property.IsIndexer,
            _ => false,
        };

    public static ITypeSymbol? ReturnTypeOf(ISymbol symbol) =>
        symbol switch
        {
            IMethodSymbol method => method.ReturnType,
            IPropertySymbol property => property.Type,
            _ => null,
        };
}
