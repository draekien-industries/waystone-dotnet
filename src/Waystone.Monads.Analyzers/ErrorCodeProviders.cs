namespace Waystone.Monads.Analyzers;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Waystone.Monads.SourceGenerators.ErrorCodes;

/// <summary>
/// The error codes an <c>[ErrorCodeProvider]</c> enum generates, resolved the way the
/// generator resolves them.
/// </summary>
/// <remarks>
/// Both <c>WM2018</c> and <c>WM2019</c> key on the generated code rather than on the
/// enum's name, so both have to apply the declared format and both have to apply the
/// same one. <c>ErrorCodeFormat</c> is the generator's own parser, compiled into this
/// assembly as a linked source file, so the three cannot disagree about what an enum
/// produces.
/// </remarks>
public static class ErrorCodeProviders
{
    /// <summary>
    /// One entry per member of <paramref name="type" /> when it is an attributed enum,
    /// and nothing at all when it is not.
    /// </summary>
    public static ImmutableArray<Provided> Collect(
        INamedTypeSymbol type,
        MonadSymbols symbols,
        string? assemblyFormat)
    {
        if (type.TypeKind != TypeKind.Enum) return ImmutableArray<Provided>.Empty;

        AttributeData? provider = type.GetAttributes()
           .FirstOrDefault(
                attribute => SymbolEqualityComparer.Default.Equals(
                    attribute.AttributeClass,
                    symbols.ErrorCodeProviderAttribute));

        if (provider is null) return ImmutableArray<Provided>.Empty;

        if (!ErrorCodeFormat.TryParse(
                DeclaredFormat(provider) ?? assemblyFormat ?? ErrorCodeFormat.Default,
                out ErrorCodeFormat? format,
                out _))
        {
            return ImmutableArray<Provided>.Empty;
        }

        ImmutableArray<Provided>.Builder provided =
            ImmutableArray.CreateBuilder<Provided>();

        foreach (IFieldSymbol member in type.GetMembers().OfType<IFieldSymbol>())
        {
            if (!member.IsConst) continue;

            provided.Add(new Provided(member, format!.Apply(type.Name, member.Name)));
        }

        return provided.ToImmutable();
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
            foreach (Provided provided in Collect(type, symbols, assemblyFormat))
            {
                codes.Add(provided.Code);
            }
        }

        return codes.ToImmutable();
    }

    /// <summary>
    /// The format the assembly declares through <c>[ErrorCodeFormat]</c>, or
    /// <c>null</c> when it declares none.
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

    private static string? DeclaredFormat(AttributeData provider)
    {
        foreach (KeyValuePair<string, TypedConstant> argument in
                 provider.NamedArguments)
        {
            if (argument.Key == "Format" && argument.Value.Value is string declared)
            {
                return declared;
            }
        }

        return null;
    }

    /// <summary>One enum member and the code it generates.</summary>
    public readonly struct Provided
    {
        public Provided(IFieldSymbol member, string code)
        {
            Member = member;
            Code = code;
        }

        public IFieldSymbol Member { get; }

        public string Code { get; }
    }
}
