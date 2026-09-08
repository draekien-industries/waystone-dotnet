namespace Waystone.Monads.Results;

using Extensions;
using Shouldly;
using System;
using System.Collections.Generic;
using Xunit;

/// <remarks>
/// The <see cref="Options.Option{T}.Bound{TState}" /> tests explain the shape; this is
/// the same guard on the result side, where every delegate receives a value as
/// well as the state and the two branches hand over different ones.
/// <para>
/// That difference is what these pin hardest. A member forwarding to its
/// sibling — <c>Inspect</c> for <c>InspectErr</c>, <c>AndThen</c> for
/// <c>OrElse</c> — compiles, runs, and is only visible when the case that
/// should have been skipped is the one exercised.
/// </para>
/// </remarks>
public sealed class ResultBoundTests
{
    private static readonly Result<int, string> OkTwo =
        Result.Ok<int, string>(2);

    private static readonly Result<int, string> ErrBad =
        Result.Err<int, string>("bad");

    [Fact]
    public void IsOkAndReadsTheBoundStateAndIsFalseForAnError()
    {
        OkTwo.With(2).IsOkAnd(static (v, s) => v == s).ShouldBeTrue();
        OkTwo.With(3).IsOkAnd(static (v, s) => v == s).ShouldBeFalse();
        ErrBad.With(2).IsOkAnd(static (v, s) => v == s).ShouldBeFalse();
    }

    [Fact]
    public void IsErrAndReadsTheErrorAndIsFalseForASuccess()
    {
        ErrBad.With("bad").IsErrAnd(static (e, s) => e == s).ShouldBeTrue();
        ErrBad.With("other").IsErrAnd(static (e, s) => e == s).ShouldBeFalse();
        OkTwo.With("bad").IsErrAnd(static (e, s) => e == s).ShouldBeFalse();
    }

    [Fact]
    public void MatchHandsTheStateAndTheBranchValueToWhicheverBranchRuns()
    {
        OkTwo.With(10)
             .Match(static (v, s) => v + s, static (e, s) => e.Length + s)
             .ShouldBe(12);

        ErrBad.With(10)
              .Match(static (v, s) => v + s, static (e, s) => e.Length + s)
              .ShouldBe(13);
    }

    [Fact]
    public void TheActionMatchHandsTheStateToWhicheverBranchRuns()
    {
        var fromOk = new List<string>();
        OkTwo.With(fromOk)
             .Match(static (v, s) => s.Add($"ok:{v}"), static (e, s) => s.Add($"err:{e}"));
        fromOk.ShouldBe(["ok:2"]);

        var fromErr = new List<string>();
        ErrBad.With(fromErr)
              .Match(static (v, s) => s.Add($"ok:{v}"), static (e, s) => s.Add($"err:{e}"));
        fromErr.ShouldBe(["err:bad"]);
    }

    [Fact]
    public void AndThenAppliesTheStateAndLeavesAnErrorAlone()
    {
        OkTwo.With(10)
             .AndThen(static (v, s) => Result.Ok<int, string>(v + s))
             .ShouldBe(Result.Ok<int, string>(12));

        OkTwo.With(10)
             .AndThen(static (_, _) => Result.Err<int, string>("stopped"))
             .ShouldBe(Result.Err<int, string>("stopped"));

        ErrBad.With(10)
              .AndThen(static (v, s) => Result.Ok<int, string>(v + s))
              .ShouldBe(Result.Err<int, string>("bad"));
    }

    [Fact]
    public void OrElseRecoversFromTheErrorAndLeavesASuccessAlone()
    {
        ErrBad.With(10)
              .OrElse(static (e, s) => Result.Ok<int, int>(e.Length + s))
              .ShouldBe(Result.Ok<int, int>(13));

        ErrBad.With(10)
              .OrElse(static (_, s) => Result.Err<int, int>(s))
              .ShouldBe(Result.Err<int, int>(10));

        OkTwo.With(10)
             .OrElse(static (e, s) => Result.Ok<int, int>(e.Length + s))
             .ShouldBe(Result.Ok<int, int>(2));
    }

    [Fact]
    public void UnwrapOrElseBuildsTheFallbackFromTheErrorAndTheState()
    {
        OkTwo.With(10).UnwrapOrElse(static (e, s) => e.Length + s).ShouldBe(2);
        ErrBad.With(10).UnwrapOrElse(static (e, s) => e.Length + s).ShouldBe(13);
    }

    [Fact]
    public void InspectRunsOnlyForOkAndReturnsTheResultUnchanged()
    {
        var fromOk = new List<int>();
        OkTwo.With(fromOk).Inspect(static (v, s) => s.Add(v)).ShouldBe(OkTwo);
        fromOk.ShouldBe([2]);

        var fromErr = new List<int>();
        ErrBad.With(fromErr).Inspect(static (v, s) => s.Add(v)).ShouldBe(ErrBad);
        fromErr.ShouldBeEmpty();
    }

    [Fact]
    public void InspectErrRunsOnlyForErrAndReturnsTheResultUnchanged()
    {
        var fromErr = new List<string>();
        ErrBad.With(fromErr)
              .InspectErr(static (e, s) => s.Add(e))
              .ShouldBe(ErrBad);
        fromErr.ShouldBe(["bad"]);

        var fromOk = new List<string>();
        OkTwo.With(fromOk).InspectErr(static (e, s) => s.Add(e)).ShouldBe(OkTwo);
        fromOk.ShouldBeEmpty();
    }

    [Fact]
    public void MapAppliesTheStateToTheOkValueAndLeavesAnErrorAlone()
    {
        OkTwo.With(10)
             .Map(static (v, s) => v + s)
             .ShouldBe(Result.Ok<int, string>(12));

        ErrBad.With(10)
              .Map(static (v, s) => v + s)
              .ShouldBe(Result.Err<int, string>("bad"));
    }

    [Fact]
    public void MapOrFallsBackToTheDefaultValueForAnError()
    {
        OkTwo.With(10).MapOr(-1, static (v, s) => v + s).ShouldBe(12);
        ErrBad.With(10).MapOr(-1, static (v, s) => v + s).ShouldBe(-1);
    }

    [Fact]
    public void MapOrDefaultFallsBackToTheDefaultOfTheOutputType()
    {
        OkTwo.With("!").MapOrDefault(static (v, s) => v + s).ShouldBe("2!");
        ErrBad.With("!").MapOrDefault(static (v, s) => v + s).ShouldBeNull();
    }

    [Fact]
    public void MapOrElseHandsTheStateToBothDelegates()
    {
        OkTwo.With(10)
             .MapOrElse(static (e, s) => e.Length + s, static (v, s) => v + s)
             .ShouldBe(12);

        ErrBad.With(10)
              .MapOrElse(static (e, s) => e.Length + s, static (v, s) => v + s)
              .ShouldBe(13);
    }

    [Fact]
    public void MapErrRestatesTheErrorAndLeavesASuccessAlone()
    {
        ErrBad.With("!")
              .MapErr(static (e, s) => e + s)
              .ShouldBe(Result.Err<int, string>("bad!"));

        OkTwo.With("!")
             .MapErr(static (e, s) => e + s)
             .ShouldBe(Result.Ok<int, string>(2));
    }

    /// <remarks>
    /// As on the option side: a struct cannot stop a consumer writing
    /// <c>default</c>, so what matters is that the failure names its cause
    /// rather than surfacing as a <see cref="NullReferenceException" /> from
    /// whichever member was reached.
    /// </remarks>
    [Fact]
    public void ADefaultBoundThrowsRatherThanDereferencingNothing()
    {
        Result<int, string>.Bound<int> bound = default;

        Should.Throw<InvalidOperationException>(
                  () => bound.Map(static (v, s) => v + s))
              .Message.ShouldContain("Build one by calling With");
    }

    /// <remarks>
    /// The state being spent rather than carried, pinned on the result side too.
    /// A member that started returning the wrapper would let this chain compile
    /// with the second <c>With</c> removed.
    /// </remarks>
    [Fact]
    public void StateIsSpentByTheCallThatUsesItSoAChainRebinds()
    {
        Result<string, string> result = Result.Ok<int, string>(2)
                                              .With(10)
                                              .Map(static (v, s) => v + s)
                                              .Map(static v => v * 2)
                                              .With("!")
                                              .Map(static (v, s) => v + s);

        result.ShouldBe(Result.Ok<string, string>("24!"));
    }
}
