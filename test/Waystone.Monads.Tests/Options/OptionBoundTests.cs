namespace Waystone.Monads.Options;

using Extensions;
using Results;
using Shouldly;
using System;
using System.Collections.Generic;
using Xunit;

/// <remarks>
/// Every member of <see cref="Option{T}.Bound{TState}" /> forwards to the state
/// overload of the same name, so what needs pinning is that it forwards to the
/// *right* one and hands over the *bound* state — a forward to a sibling, or one
/// that drops the state, still compiles.
/// <para>
/// Both cases are exercised for every member, because a forward that ignores the
/// receiver's case is invisible on whichever branch happens to be tested.
/// </para>
/// <para>
/// Where a member needs a side effect observed, the state itself is the
/// recording device. That keeps every delegate here <c>static</c>, so the tests
/// demonstrate the pattern they exist to defend rather than quietly capturing.
/// </para>
/// </remarks>
public sealed class OptionBoundTests
{
    private static readonly Option<int> SomeTwo = Option.Some(2);

    private static readonly Option<int> NoneInt = Option.None<int>();

    [Fact]
    public void IsSomeAndReadsTheBoundState()
    {
        SomeTwo.With(2).IsSomeAnd(static (v, s) => v == s).ShouldBeTrue();
        SomeTwo.With(3).IsSomeAnd(static (v, s) => v == s).ShouldBeFalse();
    }

    [Fact]
    public void IsSomeAndIsFalseForNoneWithoutInvokingThePredicate()
    {
        var invoked = new List<int>();

        NoneInt.With(invoked)
            .IsSomeAnd(
                static (v, s) =>
                {
                    s.Add(v);

                    return true;
                })
            .ShouldBeFalse();

        invoked.ShouldBeEmpty();
    }

    [Fact]
    public void IsNoneOrReadsTheBoundState()
    {
        SomeTwo.With(2).IsNoneOr(static (v, s) => v == s).ShouldBeTrue();
        SomeTwo.With(3).IsNoneOr(static (v, s) => v == s).ShouldBeFalse();
    }

    [Fact]
    public void IsNoneOrIsTrueForNoneWithoutInvokingThePredicate()
    {
        var invoked = new List<int>();

        NoneInt.With(invoked)
            .IsNoneOr(
                static (v, s) =>
                {
                    s.Add(v);

                    return false;
                })
            .ShouldBeTrue();

        invoked.ShouldBeEmpty();
    }

    [Fact]
    public void MatchHandsTheStateToWhicheverBranchRuns()
    {
        SomeTwo.With(10).Match(static (v, s) => v + s, static s => s).ShouldBe(12);
        NoneInt.With(10).Match(static (v, s) => v + s, static s => s).ShouldBe(10);
    }

    [Fact]
    public void TheActionMatchHandsTheStateToWhicheverBranchRuns()
    {
        var fromSome = new List<int>();
        SomeTwo.With(fromSome).Match(static (v, s) => s.Add(v), static s => s.Add(-1));
        fromSome.ShouldBe([2]);

        var fromNone = new List<int>();
        NoneInt.With(fromNone).Match(static (v, s) => s.Add(v), static s => s.Add(-1));
        fromNone.ShouldBe([-1]);
    }

    [Fact]
    public void UnwrapOrElseFallsBackToTheStateOnlyForNone()
    {
        SomeTwo.With(10).UnwrapOrElse(static s => s).ShouldBe(2);
        NoneInt.With(10).UnwrapOrElse(static s => s).ShouldBe(10);
    }

    [Fact]
    public void MapAppliesTheStateToTheContainedValue()
    {
        SomeTwo.With(10).Map(static (v, s) => v + s).ShouldBe(Option.Some(12));
        NoneInt.With(10).Map(static (v, s) => v + s).ShouldBe(Option.None<int>());
    }

    [Fact]
    public void AndThenAppliesTheStateAndKeepsTheProducedCase()
    {
        SomeTwo.With(10)
            .AndThen(static (v, s) => Option.Some(v + s))
            .ShouldBe(Option.Some(12));

        SomeTwo.With(10)
            .AndThen(static (_, _) => Option.None<int>())
            .ShouldBe(Option.None<int>());

        NoneInt.With(10)
            .AndThen(static (v, s) => Option.Some(v + s))
            .ShouldBe(Option.None<int>());
    }

    [Fact]
    public void MapOrFallsBackToTheDefaultValueForNone()
    {
        SomeTwo.With(10).MapOr(-1, static (v, s) => v + s).ShouldBe(12);
        NoneInt.With(10).MapOr(-1, static (v, s) => v + s).ShouldBe(-1);
    }

    [Fact]
    public void MapOrDefaultFallsBackToTheDefaultOfTheOutputType()
    {
        SomeTwo.With("!").MapOrDefault(static (v, s) => v + s).ShouldBe("2!");
        NoneInt.With("!").MapOrDefault(static (v, s) => v + s).ShouldBeNull();
    }

    [Fact]
    public void ZipWithCombinesBothValuesAndTheBoundState()
    {
        SomeTwo.With(10)
               .ZipWith(
                    Option.Some(3),
                    static (v, o, s) => v + o + s)
               .ShouldBeSomeValue(15);

        SomeTwo.With(10)
               .ZipWith(
                    Option.None<int>(),
                    static (v, o, s) => v + o + s)
               .ShouldBeNone();

        NoneInt.With(10)
               .ZipWith(
                    Option.Some(3),
                    static (v, o, s) => v + o + s)
               .ShouldBeNone();
    }

    [Fact]
    public void ReduceKeepsALoneValueRatherThanDiscardingIt()
    {
        SomeTwo.With(10)
               .Reduce(Option.Some(3), static (a, b, s) => a + b + s)
               .ShouldBeSomeValue(15);

        SomeTwo.With(10)
               .Reduce(Option.None<int>(), static (a, b, s) => a + b + s)
               .ShouldBeSomeValue(2);

        NoneInt.With(10)
               .Reduce(Option.Some(3), static (a, b, s) => a + b + s)
               .ShouldBeSomeValue(3);

        NoneInt.With(10)
               .Reduce(Option.None<int>(), static (a, b, s) => a + b + s)
               .ShouldBeNone();
    }

    [Fact]
    public void MapOrNullUsesNullForTheAbsentCaseRatherThanTheDefault()
    {
        SomeTwo.With(10).MapOrNull(static (v, s) => v + s).ShouldBe(12);

        SomeTwo.With(-2).MapOrNull(static (v, s) => v + s).ShouldBe(0);

        NoneInt.With(10).MapOrNull(static (v, s) => v + s).ShouldBeNull();
    }

    [Fact]
    public void MapOrElseHandsTheStateToBothDelegates()
    {
        SomeTwo.With(10)
            .MapOrElse(static s => s * 2, static (v, s) => v + s)
            .ShouldBe(12);

        NoneInt.With(10)
            .MapOrElse(static s => s * 2, static (v, s) => v + s)
            .ShouldBe(20);
    }

    [Fact]
    public void InspectRunsOnlyForSomeAndReturnsTheOptionUnchanged()
    {
        var fromSome = new List<int>();
        SomeTwo.With(fromSome).Inspect(static (v, s) => s.Add(v)).ShouldBe(SomeTwo);
        fromSome.ShouldBe([2]);

        var fromNone = new List<int>();
        NoneInt.With(fromNone).Inspect(static (v, s) => s.Add(v)).ShouldBe(NoneInt);
        fromNone.ShouldBeEmpty();
    }

    [Fact]
    public void FilterKeepsOnlyAValueThePredicateAccepts()
    {
        SomeTwo.With(2).Filter(static (v, s) => v == s).ShouldBe(Option.Some(2));
        SomeTwo.With(3).Filter(static (v, s) => v == s).ShouldBe(Option.None<int>());
        NoneInt.With(2).Filter(static (v, s) => v == s).ShouldBe(Option.None<int>());
    }

    [Fact]
    public void OrElseSubstitutesFromTheStateOnlyForNone()
    {
        SomeTwo.With(10)
            .OrElse(static s => Option.Some(s))
            .ShouldBe(Option.Some(2));

        NoneInt.With(10)
            .OrElse(static s => Option.Some(s))
            .ShouldBe(Option.Some(10));

        NoneInt.With(10)
            .OrElse(static _ => Option.None<int>())
            .ShouldBe(Option.None<int>());
    }

    [Fact]
    public void OkOrElseBuildsTheErrorFromTheStateOnlyForNone()
    {
        SomeTwo.With("missing")
            .OkOrElse(static s => s)
            .ShouldBe(Result.Ok<int, string>(2));

        NoneInt.With("missing")
            .OkOrElse(static s => s)
            .ShouldBe(Result.Err<int, string>("missing"));
    }

    /// <remarks>
    /// A struct cannot stop a consumer writing <c>default</c>, so the only
    /// question is what happens next. It reports rather than throwing a
    /// <see cref="NullReferenceException" /> from whichever member was called,
    /// which would name neither the cause nor the fix.
    /// </remarks>
    [Fact]
    public void ADefaultBoundThrowsRatherThanDereferencingNothing()
    {
        Option<int>.Bound<int> bound = default;

        Should.Throw<InvalidOperationException>(
                  () => bound.Map(static (v, s) => v + s))
              .Message.ShouldContain("Build one by calling With");
    }

    /// <remarks>
    /// The state being spent rather than carried is the whole design, so it is
    /// pinned rather than left as a property of the return types. A member that
    /// started returning the wrapper would let this chain compile with the
    /// second <c>With</c> removed.
    /// </remarks>
    [Fact]
    public void StateIsSpentByTheCallThatUsesItSoAChainRebinds()
    {
        Option<string> result = Option.Some(2)
                                      .With(10)
                                      .Map(static (v, s) => v + s)
                                      .Map(static v => v * 2)
                                      .With("!")
                                      .Map(static (v, s) => v + s);

        result.ShouldBe(Option.Some("24!"));
    }
}
