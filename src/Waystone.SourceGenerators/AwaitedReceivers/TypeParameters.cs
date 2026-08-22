namespace Waystone.SourceGenerators.AwaitedReceivers;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

internal static class TypeParameters
{
    /// <summary>
    /// The subset of <paramref name="candidates" /> that <paramref name="type" />
    /// mentions, in declaration order.
    /// </summary>
    public static ImmutableArray<ITypeParameterSymbol> ReferencedBy(
        ITypeSymbol type,
        ImmutableArray<ITypeParameterSymbol> candidates)
    {
        var referenced = new HashSet<ITypeParameterSymbol>(SymbolEqualityComparer.Default);

        Collect(type, referenced);

        return candidates.Where(referenced.Contains).ToImmutableArray();
    }

    public static string Render(ImmutableArray<ITypeParameterSymbol> parameters) =>
        parameters.IsDefaultOrEmpty
            ? string.Empty
            : $"<{string.Join(", ", parameters.Select(static p => p.Name))}>";

    public static IEnumerable<string> Constraints(
        ImmutableArray<ITypeParameterSymbol> parameters)
    {
        if (parameters.IsDefaultOrEmpty) yield break;

        foreach (ITypeParameterSymbol parameter in parameters)
        {
            string? clause = Constraint(parameter);

            if (clause is not null) yield return clause;
        }
    }

    private static string? Constraint(ITypeParameterSymbol parameter)
    {
        var parts = new List<string>();

        if (parameter.HasNotNullConstraint) parts.Add("notnull");

        if (parameter.HasUnmanagedTypeConstraint) parts.Add("unmanaged");
        else if (parameter.HasValueTypeConstraint) parts.Add("struct");

        if (parameter.HasReferenceTypeConstraint)
        {
            parts.Add(
                parameter.ReferenceTypeConstraintNullableAnnotation
             == NullableAnnotation.Annotated
                    ? "class?"
                    : "class");
        }

        foreach (ITypeSymbol constraint in parameter.ConstraintTypes)
        {
            parts.Add(constraint.ToDisplayString(Display.Format));
        }

        if (parameter.HasConstructorConstraint) parts.Add("new()");

        return parts.Count == 0
            ? null
            : $"where {parameter.Name} : {string.Join(", ", parts)}";
    }

    private static void Collect(ITypeSymbol type, HashSet<ITypeParameterSymbol> into)
    {
        switch (type)
        {
            case ITypeParameterSymbol parameter:
                into.Add(parameter);

                break;
            case IArrayTypeSymbol array:
                Collect(array.ElementType, into);

                break;
            case INamedTypeSymbol named:
                foreach (ITypeSymbol argument in named.TypeArguments)
                {
                    Collect(argument, into);
                }

                break;
        }
    }
}

internal static class Display
{
    public static readonly SymbolDisplayFormat Format =
        SymbolDisplayFormat.FullyQualifiedFormat.AddMiscellaneousOptions(
            SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);
}

internal static class Identifiers
{
    public static string CamelCase(string name) =>
        name.Length == 0
            ? name
            : char.ToLowerInvariant(name[0]) + name.Substring(1);

    public static string Escape(string name) =>
        SyntaxFacts.GetKeywordKind(name) == SyntaxKind.None ? name : "@" + name;

    public static string Default(IParameterSymbol parameter)
    {
        object? value = parameter.ExplicitDefaultValue;

        if (value is null)
        {
            return parameter.Type.IsValueType ? "default" : "null";
        }

        return SymbolDisplay.FormatPrimitive(value, true, false);
    }
}
