namespace Waystone.SourceGenerators.AsyncSurface;

using Microsoft.CodeAnalysis;

/// <summary>
/// The three constructed generics the async-surface rules compare against, looked
/// up once per compilation rather than once per symbol.
/// </summary>
internal sealed class MonadTypes
{
    private MonadTypes(
        INamedTypeSymbol task,
        INamedTypeSymbol option,
        INamedTypeSymbol result)
    {
        Task = task;
        Option = option;
        Result = result;
    }

    private INamedTypeSymbol Task { get; }

    private INamedTypeSymbol Option { get; }

    private INamedTypeSymbol Result { get; }

    /// <summary>
    /// Loads the types from a compilation, or returns null when any of them is
    /// absent — which is every compilation that does not reference this library,
    /// and is why the rules cost nothing there.
    /// </summary>
    /// <param name="compilation">The compilation being analysed.</param>
    /// <returns>The loaded types, or null when the compilation has no monads.</returns>
    public static MonadTypes? Load(Compilation compilation)
    {
        INamedTypeSymbol? task =
            compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1");
        INamedTypeSymbol? option =
            compilation.GetTypeByMetadataName("Waystone.Monads.Options.Option`1");
        INamedTypeSymbol? result =
            compilation.GetTypeByMetadataName("Waystone.Monads.Results.Result`2");

        return task is null || option is null || result is null
            ? null
            : new MonadTypes(task, option, result);
    }

    /// <summary>
    /// Checks whether a type is a <c>Task</c> whose type argument is one of this
    /// library's monads.
    /// </summary>
    /// <param name="type">The type to test, usually a member's return type.</param>
    /// <param name="monad">
    /// The monad the task wraps when this returns true, so a caller can name it in
    /// a message without unwrapping the task a second time.
    /// </param>
    /// <returns>True if the type is a task over a monad; false otherwise.</returns>
    public bool IsTaskOfMonad(INamedTypeSymbol type, out ITypeSymbol? monad)
    {
        monad = null;

        if (!SymbolEqualityComparer.Default.Equals(
                type.OriginalDefinition,
                Task))
        {
            return false;
        }

        ITypeSymbol argument = type.TypeArguments[0];

        if (!IsMonad(argument)) return false;

        monad = argument;

        return true;
    }

    /// <summary>
    /// Checks whether a type is an <c>Option</c> or a <c>Result</c>, ignoring the
    /// type arguments it was constructed with.
    /// </summary>
    /// <param name="type">The type to test.</param>
    /// <returns>True if the type is one of this library's monads; false otherwise.</returns>
    public bool IsMonad(ITypeSymbol type) =>
        type is INamedTypeSymbol named
     && (SymbolEqualityComparer.Default.Equals(named.OriginalDefinition, Option)
      || SymbolEqualityComparer.Default.Equals(named.OriginalDefinition, Result));
}
