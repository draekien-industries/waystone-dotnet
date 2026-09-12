namespace Waystone.Monads.Analyzers;

using System;
using System.Linq;
using System.Reflection;
using Options;
using Shouldly;
using Xunit;

/// <remarks>
/// Pins <see cref="MonadShape.ExpectedAsyncShapes" />, which is the only member of
/// <see cref="MonadShape" /> that states what <em>should</em> exist rather than
/// rendering what does.
/// <para>
/// Every grid test works by subtracting the surface that exists from the surface
/// the rule demands, so an error in the derivation shrinks the demand and the
/// subtraction comes out empty. <see cref="CoreShapeGridTests" /> and
/// <see cref="AwaitedShapeGridTests" /> would both pass, and would go on passing
/// through exactly the kind of gap they were written to catch. Nothing they assert
/// can detect it, because they consume the derivation rather than checking it.
/// </para>
/// <para>
/// So the expected shapes are spelled out in full here rather than computed. A
/// derivation asserted against a second derivation proves only that the two agree.
/// </para>
/// </remarks>
public sealed class MonadShapeTests
{
    private const string OnSomeAsync = "Func<T, Awaitable<TOut>>";
    private const string OnSomeSync = "Func<T, TOut>";
    private const string OnNoneAsync = "Func<Awaitable<TOut>>";
    private const string OnNoneSync = "Func<TOut>";

    /// <remarks>
    /// <c>Option&lt;T&gt;.Match&lt;TOut&gt;</c> is the worst case in the library at
    /// two delegates, so it is the case where the subset enumeration can be wrong
    /// in a way a one-delegate member would hide — a bound that stopped one early
    /// still produces the single shape a <c>Map</c> needs.
    /// </remarks>
    [Fact]
    public void TwoDelegatesDeriveTheThreeAsyncSubsets() =>
        Derive(includeAllSync: false)
           .ShouldBe(
                new[]
                {
                    Expected(OnSomeAsync, OnNoneAsync),
                    Expected(OnSomeAsync, OnNoneSync),
                    Expected(OnSomeSync, OnNoneAsync),
                });

    /// <remarks>
    /// The all-synchronous shape is the whole of the difference between rule 1 and
    /// rule 4, so the flag is pinned by the shape it adds rather than by a count.
    /// </remarks>
    [Fact]
    public void IncludingTheAllSyncShapeAddsItAndNothingElse() =>
        Derive(includeAllSync: true)
           .ShouldBe(
                new[]
                {
                    Expected(OnSomeAsync, OnNoneAsync),
                    Expected(OnSomeAsync, OnNoneSync),
                    Expected(OnSomeSync, OnNoneAsync),
                    Expected(OnSomeSync, OnNoneSync),
                });

    /// <remarks>
    /// A one-delegate member derives exactly one shape, which is the <c>2^N - 1</c>
    /// bound at <c>N = 1</c>. A bound written <c>2^N</c> by mistake passes
    /// <see cref="TwoDelegatesDeriveTheThreeAsyncSubsets" /> if the extra shape
    /// happens to exist on the monad, and fails here.
    /// </remarks>
    [Fact]
    public void OneDelegateDerivesOneShape() =>
        MonadShape
           .ExpectedAsyncShapes(
                MonadShape
                   .Declared(typeof(Option<>))
                   .Single(
                        method => method.Name == nameof(Option<int>.Map)
                               && !MonadShape.DeclaresState(method)))
           .ShouldHaveSingleItem()
           .ShouldBe("MapAsync(Func<T, Awaitable<TOut>>) -> ValueTask<Option<TOut>>");

    private static string[] Derive(bool includeAllSync) =>
        MonadShape.ExpectedAsyncShapes(ValueReturningMatch(), includeAllSync)
                  .OrderBy(shape => shape, StringComparer.Ordinal)
                  .ToArray();

    private static string Expected(string onSome, string onNone) =>
        $"MatchAsync({onSome}, {onNone}) -> ValueTask<TOut>";

    /// <remarks>
    /// <c>Match</c> is overloaded on what it returns, and the void-returning form
    /// takes <c>Action</c>s. Selecting on the return type being a type parameter is
    /// what separates them without naming delegate types the test is meant to be
    /// asserting about.
    /// </remarks>
    private static MethodInfo ValueReturningMatch() =>
        MonadShape
           .Declared(typeof(Option<>))
           .Single(
                method => method.Name == nameof(Option<int>.Match)
                       && !MonadShape.DeclaresState(method)
                       && method.ReturnType.IsGenericParameter);
}
