namespace Waystone.Monads.Schemas.SourceGenerators;

using System.Collections.Generic;
using Microsoft.CodeAnalysis;

internal static class Symbols
{
    /// <summary>
    /// The types a symbol is nested inside, outermost first, so a caller reopens them
    /// in the order the compiler needs them declared.
    /// </summary>
    /// <remarks>
    /// Lives here rather than on a writer because both halves of the generator need
    /// it and neither owns it: the hint name is built from the same chain the emitted
    /// file nests through.
    /// </remarks>
    public static IReadOnlyList<INamedTypeSymbol> Containers(
        INamedTypeSymbol type)
    {
        var containers = new List<INamedTypeSymbol>();

        for (INamedTypeSymbol? container = type.ContainingType;
             container is not null;
             container = container.ContainingType)
        {
            containers.Insert(0, container);
        }

        return containers;
    }
}
