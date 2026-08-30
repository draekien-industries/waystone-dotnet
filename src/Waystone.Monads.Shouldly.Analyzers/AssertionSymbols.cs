namespace Waystone.Monads.Shouldly.Analyzers;

using Microsoft.CodeAnalysis;

/// <summary>
/// The types both rules resolve by metadata name, and the gate that keeps them quiet
/// in a project without the assertions.
/// </summary>
/// <remarks>
/// Neither analyzer may reference Waystone.Monads or Waystone.Monads.Shouldly: the
/// assertions package loads these assemblies as analyzers, so a project reference
/// would be a build cycle. Resolving by name also gives the gate for free — a
/// consumer of the core library alone has no OptionAssertions, so
/// <see cref="TryCreate" /> returns null and every rule here goes silent rather than
/// offering a fix that produces source they cannot compile.
/// </remarks>
internal sealed class AssertionSymbols
{
    public const string OptionMetadataName = "Waystone.Monads.Options.Option`1";
    public const string SomeMetadataName = "Waystone.Monads.Options.Some`1";
    public const string NoneMetadataName = "Waystone.Monads.Options.None`1";
    public const string ResultMetadataName =
        "Waystone.Monads.Results.Result`2";
    public const string OkMetadataName = "Waystone.Monads.Results.Ok`2";
    public const string ErrMetadataName = "Waystone.Monads.Results.Err`2";
    public const string OptionAssertionsMetadataName =
        "Shouldly.OptionAssertions";
    public const string ResultAssertionsMetadataName =
        "Shouldly.ResultAssertions";

    private readonly INamedTypeSymbol _err;
    private readonly INamedTypeSymbol _none;
    private readonly INamedTypeSymbol _ok;
    private readonly INamedTypeSymbol _option;
    private readonly INamedTypeSymbol _result;
    private readonly INamedTypeSymbol _some;
    private readonly INamedTypeSymbol? _task;
    private readonly INamedTypeSymbol? _valueTask;

    private AssertionSymbols(
        INamedTypeSymbol option,
        INamedTypeSymbol some,
        INamedTypeSymbol none,
        INamedTypeSymbol result,
        INamedTypeSymbol ok,
        INamedTypeSymbol err,
        INamedTypeSymbol? task,
        INamedTypeSymbol? valueTask)
    {
        _option = option;
        _some = some;
        _none = none;
        _result = result;
        _ok = ok;
        _err = err;
        _task = task;
        _valueTask = valueTask;
    }

    public static AssertionSymbols? TryCreate(Compilation compilation)
    {
        if (compilation.GetTypeByMetadataName(OptionAssertionsMetadataName)
                is null
         || compilation.GetTypeByMetadataName(ResultAssertionsMetadataName)
                is null)
        {
            return null;
        }

        var option = compilation.GetTypeByMetadataName(OptionMetadataName);
        var some = compilation.GetTypeByMetadataName(SomeMetadataName);
        var none = compilation.GetTypeByMetadataName(NoneMetadataName);
        var result = compilation.GetTypeByMetadataName(ResultMetadataName);
        var ok = compilation.GetTypeByMetadataName(OkMetadataName);
        var err = compilation.GetTypeByMetadataName(ErrMetadataName);

        if (option is null
         || some is null
         || none is null
         || result is null
         || ok is null
         || err is null)
        {
            return null;
        }

        return new AssertionSymbols(
            option,
            some,
            none,
            result,
            ok,
            err,
            compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1"),
            compilation.GetTypeByMetadataName(
                "System.Threading.Tasks.ValueTask`1"));
    }

    public bool IsOption(ITypeSymbol? type) =>
        IsConstructedFrom(type, _option)
     || IsConstructedFrom(type, _some)
     || IsConstructedFrom(type, _none);

    public bool IsResult(ITypeSymbol? type) =>
        IsConstructedFrom(type, _result)
     || IsConstructedFrom(type, _ok)
     || IsConstructedFrom(type, _err);

    public bool IsMonad(ITypeSymbol? type) => IsOption(type) || IsResult(type);

    /// <summary>
    /// Gets the monad inside a <c>Task&lt;T&gt;</c> or <c>ValueTask&lt;T&gt;</c>, or
    /// null when <paramref name="type" /> is neither or wraps something else.
    /// </summary>
    /// <remarks>
    /// Returns null rather than the argument for a non-task, so a caller cannot
    /// mistake a bare monad for an awaited one. WMS2002 relies on that: the two
    /// receiver shapes take different rewrites.
    /// </remarks>
    public ITypeSymbol? AwaitedMonad(ITypeSymbol? type)
    {
        if (type is not INamedTypeSymbol { IsGenericType: true } named)
        {
            return null;
        }

        var definition = named.OriginalDefinition;

        if (!SymbolEqualityComparer.Default.Equals(definition, _task)
         && !SymbolEqualityComparer.Default.Equals(definition, _valueTask))
        {
            return null;
        }

        return IsMonad(named.TypeArguments[0])
            ? named.TypeArguments[0]
            : null;
    }

    private static bool IsConstructedFrom(
        ITypeSymbol? type,
        INamedTypeSymbol definition) =>
        type is INamedTypeSymbol named
     && SymbolEqualityComparer.Default.Equals(
            named.OriginalDefinition,
            definition);
}
