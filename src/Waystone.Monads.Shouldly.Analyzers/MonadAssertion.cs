namespace Waystone.Monads.Shouldly.Analyzers;

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

/// <summary>
/// Maps a raw assertion onto the assertion in Waystone.Monads.Shouldly that replaces
/// it, and names the awaited form of each.
/// </summary>
/// <remarks>
/// Shared by the analyzers and their code fixes, which is the point: a rule whose
/// message names one replacement while its fix writes another is worse than no rule,
/// and nothing in the build would catch the drift if each side held its own table.
/// </remarks>
internal static class MonadAssertion
{
    public const string ShouldlyNamespace = "Shouldly";

    /// <summary>
    /// The diagnostic property under which an analyzer records the assertion its fix
    /// should write.
    /// </summary>
    /// <remarks>
    /// The fixes read the replacement from here rather than deriving it a second time.
    /// A fix that re-ran the mapping could drift from the message the consumer was
    /// shown, and the drift would compile.
    /// </remarks>
    public const string ReplacementKey = "Replacement";

    /// <summary>The assertions that have an awaited form, which is every one of them.</summary>
    public static readonly ImmutableHashSet<string> Names =
        ImmutableHashSet.Create(
            "ShouldBeSome",
            "ShouldBeNone",
            "ShouldBeSomeValue",
            "ShouldBeOk",
            "ShouldBeErr",
            "ShouldBeOkValue",
            "ShouldBeErrValue");

    /// <summary>
    /// Gets the assertion that replaces a state property read against an asserted
    /// truth value, or null when the pair names no state.
    /// </summary>
    /// <param name="property">The property read off the monad, for example "IsSome".</param>
    /// <param name="assertedTrue">
    /// If true, the read was asserted with <c>ShouldBeTrue</c>. If false, with
    /// <c>ShouldBeFalse</c>, which selects the opposite assertion.
    /// </param>
    public static string? StateAssertion(string property, bool assertedTrue) =>
        (property, assertedTrue) switch
        {
            ("IsSome", true) or ("IsNone", false) => "ShouldBeSome",
            ("IsNone", true) or ("IsSome", false) => "ShouldBeNone",
            ("IsOk", true) or ("IsErr", false) => "ShouldBeOk",
            ("IsErr", true) or ("IsOk", false) => "ShouldBeErr",
            _ => null,
        };

    /// <summary>
    /// Gets the assertion that replaces an unwrap followed by a comparison, or null
    /// when the unwrap does not belong to the receiver's monad.
    /// </summary>
    /// <remarks>
    /// <c>UnwrapErr</c> on an option returns null rather than an error assertion.
    /// An option has no error half, so the call does not compile there and a rule that
    /// answered anyway would offer a fix for source that never existed.
    /// </remarks>
    /// <param name="unwrap">The unwrap called on the monad, for example "UnwrapErr".</param>
    /// <param name="isOption">
    /// If true, the receiver is an <c>Option</c>. If false, a <c>Result</c>, whose
    /// <c>Unwrap</c> names the Ok half rather than the Some half.
    /// </param>
    public static string? ValueAssertion(string unwrap, bool isOption) =>
        (unwrap, isOption) switch
        {
            ("Unwrap", true) => "ShouldBeSomeValue",
            ("Unwrap", false) => "ShouldBeOkValue",
            ("UnwrapErr", false) => "ShouldBeErrValue",
            _ => null,
        };

    /// <summary>
    /// Gets the awaited form of a synchronous assertion, or null when
    /// <paramref name="name" /> is not one of this package's assertions.
    /// </summary>
    public static string? Awaited(string name) =>
        Names.Contains(name) ? name + "Async" : null;

    /// <summary>
    /// Checks whether a method was declared in the <c>Shouldly</c> namespace.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Keyed on the namespace rather than on the declaring type, because Shouldly
    /// moves assertions between its extension classes across releases while the
    /// namespace has been stable. A consumer's own <c>ShouldBe</c> is what this
    /// excludes.
    /// </para>
    /// <para>
    /// Matches the name against the global namespace rather than calling
    /// <c>ToDisplayString</c>, which would build a string for every method the callers
    /// look at. They run on every invocation in a compilation, on every keystroke in an
    /// IDE, so this is the one comparison here worth writing the long way.
    /// </para>
    /// </remarks>
    public static bool IsShouldlyMethod(IMethodSymbol method) =>
        method.ContainingType?.ContainingNamespace is
        {
            Name: ShouldlyNamespace,
            ContainingNamespace.IsGlobalNamespace: true,
        };
}
