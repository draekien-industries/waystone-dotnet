namespace Waystone.Monads.Analyzers;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Waystone.Monads.SourceGenerators.ErrorCodes;

/// <summary>
/// The error codes an <c>[ErrorCodeCatalog]</c> enum generates, resolved the way the
/// generator resolves them.
/// </summary>
/// <remarks>
/// Both <c>WM2018</c> and <c>WM2019</c> key on the generated code rather than on the
/// enum's name, so both have to apply the declared format and both have to apply the
/// same one. <c>ErrorCodeFormat</c> is the generator's own parser, compiled into this
/// assembly as a linked source file, so the three cannot disagree about what an enum
/// produces.
/// </remarks>
public static class ErrorCodeCatalogs
{
    /// <summary>
    /// One entry per member of <paramref name="type" /> when it is an attributed enum,
    /// and nothing at all when it is not.
    /// </summary>
    /// <remarks>
    /// Also nothing when the format the enum or the assembly declares does not parse:
    /// the generator reports <c>WMG0005</c> for that and emits no codes, so there is
    /// nothing for the registry rules to compare against either.
    /// </remarks>
    public static ImmutableArray<Declared> Collect(
        INamedTypeSymbol type,
        MonadSymbols symbols,
        string? assemblyFormat)
    {
        if (type.TypeKind != TypeKind.Enum) return ImmutableArray<Declared>.Empty;

        AttributeData? catalog = type.GetAttributes()
           .FirstOrDefault(
                attribute => SymbolEqualityComparer.Default.Equals(
                    attribute.AttributeClass,
                    symbols.ErrorCodeCatalogAttribute));

        if (catalog is null) return ImmutableArray<Declared>.Empty;

        if (!ErrorCodeFormat.TryParse(
                DeclaredFormat(catalog) ?? assemblyFormat ?? ErrorCodeFormat.Default,
                out ErrorCodeFormat? format,
                out _))
        {
            return ImmutableArray<Declared>.Empty;
        }

        ImmutableArray<Declared>.Builder declared =
            ImmutableArray.CreateBuilder<Declared>();

        foreach (IFieldSymbol member in type.GetMembers().OfType<IFieldSymbol>())
        {
            if (!member.IsConst) continue;

            declared.Add(new Declared(member, format!.Apply(type.Name, member.Name)));
        }

        return declared.ToImmutable();
    }

    /// <summary>
    /// Every code the compilation's attributed enums generate, wherever they are
    /// declared.
    /// </summary>
    /// <remarks>
    /// This walks the whole compilation, which an analyzer must not do — it is here
    /// for the code fix, which has no symbol callbacks to hang off and one edit to
    /// make.
    /// </remarks>
    public static ImmutableArray<string> CodesIn(
        Compilation compilation,
        MonadSymbols symbols)
    {
        string? assemblyFormat = AssemblyFormat(compilation);

        ImmutableArray<string>.Builder codes = ImmutableArray.CreateBuilder<string>();

        foreach (INamedTypeSymbol type in Types(compilation.GlobalNamespace))
        {
            foreach (Declared declared in Collect(type, symbols, assemblyFormat))
            {
                codes.Add(declared.Code);
            }
        }

        return codes.ToImmutable();
    }

    /// <summary>
    /// The format the assembly declares through <c>[ErrorCodeFormat]</c>, or
    /// <see langword="null" /> when it declares none.
    /// </summary>
    public static string? AssemblyFormat(Compilation compilation)
    {
        foreach (AttributeData attribute in compilation.Assembly.GetAttributes())
        {
            if (attribute.AttributeClass?.ToDisplayString()
             != MonadSymbols.ErrorCodeFormatAttributeMetadataName)
            {
                continue;
            }

            if (attribute.ConstructorArguments.Length == 1
             && attribute.ConstructorArguments[0].Value is string format)
            {
                return format;
            }
        }

        return null;
    }

    private static IEnumerable<INamedTypeSymbol> Types(INamespaceSymbol space)
    {
        foreach (INamespaceOrTypeSymbol member in space.GetMembers())
        {
            if (member is INamespaceSymbol nested)
            {
                foreach (INamedTypeSymbol type in Types(nested)) yield return type;
            }
            else if (member is INamedTypeSymbol type)
            {
                yield return type;

                foreach (INamedTypeSymbol inner in Nested(type)) yield return inner;
            }
        }
    }

    private static IEnumerable<INamedTypeSymbol> Nested(INamedTypeSymbol type)
    {
        foreach (INamedTypeSymbol member in type.GetTypeMembers())
        {
            yield return member;

            foreach (INamedTypeSymbol inner in Nested(member)) yield return inner;
        }
    }

    private static string? DeclaredFormat(AttributeData catalog)
    {
        foreach (KeyValuePair<string, TypedConstant> argument in
                 catalog.NamedArguments)
        {
            if (argument.Key == "Format" && argument.Value.Value is string declared)
            {
                return declared;
            }
        }

        return null;
    }

    /// <summary>One enum member and the code it generates.</summary>
    public readonly struct Declared
    {
        public Declared(IFieldSymbol member, string code)
        {
            Member = member;
            Code = code;
        }

        public IFieldSymbol Member { get; }

        public string Code { get; }
    }
}
