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
    /// <see langword="null" /> when there is none — either because
    /// <paramref name="type" /> is not awaitable, or because it is a non-generic
    /// <c>Task</c> or <c>ValueTask</c>, whose await yields no value. Call
    /// <see cref="IsAwaitable" /> to tell those two apart.
    /// </summary>
    public static ITypeSymbol? Unwrap(ITypeSymbol type)
    {
        if (!IsAwaitable(type)) return null;

        var named = (INamedTypeSymbol)type;

        return named.TypeArguments.Length == 1 ? named.TypeArguments[0] : null;
    }
}
