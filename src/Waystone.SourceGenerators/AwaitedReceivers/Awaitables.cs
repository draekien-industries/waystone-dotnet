namespace Waystone.SourceGenerators.AwaitedReceivers;

using Microsoft.CodeAnalysis;

internal static class Awaitables
{
    private const string Task = "System.Threading.Tasks.Task";
    private const string ValueTask = "System.Threading.Tasks.ValueTask";

    public static bool IsAwaitable(ITypeSymbol type)
    {
        if (type is not INamedTypeSymbol named) return false;

        string name = named.OriginalDefinition.ToDisplayString(
            SymbolDisplayFormat.FullyQualifiedFormat
                               .WithGlobalNamespaceStyle(
                                    SymbolDisplayGlobalNamespaceStyle.Omitted)
                               .WithGenericsOptions(SymbolDisplayGenericsOptions.None));

        return name is Task or ValueTask;
    }

    /// <summary>
    /// The type an <c>await</c> of <paramref name="type" /> produces, or
    /// <see langword="null" /> when <paramref name="type" /> is not awaitable.
    /// </summary>
    public static ITypeSymbol? Unwrap(ITypeSymbol type)
    {
        if (!IsAwaitable(type)) return null;

        var named = (INamedTypeSymbol)type;

        return named.TypeArguments.Length == 1 ? named.TypeArguments[0] : null;
    }
}
