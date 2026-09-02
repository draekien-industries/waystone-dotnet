namespace Waystone.Monads.Schemas.SourceGenerators;

using System.Collections.Generic;
using System.Text;
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

    /// <summary>
    /// The reopening declaration of a type, with no accessibility and no
    /// constraints. A partial declaration may omit both, and repeating either only
    /// creates a second place they can disagree. The type parameters are repeated,
    /// because they have to be.
    /// </summary>
    public static string Declaration(INamedTypeSymbol type)
    {
        var declaration = new StringBuilder("partial ");

        declaration.Append(KeywordOf(type));
        declaration.Append(' ');
        declaration.Append(type.Name);

        if (type.TypeParameters.Length == 0) return declaration.ToString();

        declaration.Append('<');

        for (var index = 0; index < type.TypeParameters.Length; index++)
        {
            if (index > 0) declaration.Append(", ");

            declaration.Append(type.TypeParameters[index].Name);
        }

        declaration.Append('>');

        return declaration.ToString();
    }

    private static string KeywordOf(INamedTypeSymbol type)
    {
        if (type.TypeKind == TypeKind.Interface) return "interface";

        if (type.TypeKind == TypeKind.Struct)
        {
            return type.IsRecord ? "record struct" : "struct";
        }

        return type.IsRecord ? "record" : "class";
    }
}
