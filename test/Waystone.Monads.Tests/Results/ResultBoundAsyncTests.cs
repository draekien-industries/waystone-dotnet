namespace Waystone.Monads.Results;

using Extensions;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

/// <remarks>
/// The result side of
/// <see cref="Options.Option{T}.Bound{TState}" />'s async tests. Same
/// obligation — the delegate must be awaited on exactly one branch — with one
/// difference that changes what the tests have to say: both cases carry a value,
/// so a member can reach the right branch and still hand it the wrong thing.
/// <para>
/// So these assert on the value the delegate received, not only on the value the
/// member returned. A member that read the ok value where it should have read the
/// error compiles, and several of these would pass without that check.
/// </para>
/// </remarks>
public sealed class ResultBoundAsyncTests
{
    private static readonly Result<int, string> OkTwo =
        Result.Ok<int, string>(2);

    private static readonly Result<int, string> ErrBad =
        Result.Err<int, string>("bad");

    [Fact]
    public async Task IsOkAndAsyncAwaitsThePredicateOnlyForOk()
    {
        (await OkTwo.With(2)
                    .IsOkAndAsync(static (v, s) => Task.FromResult(v == s)))
           .ShouldBeTrue();

        (await OkTwo.With(3)
                    .IsOkAndAsync(static (v, s) => Task.FromResult(v == s)))
           .ShouldBeFalse();

        var invoked = new List<int>();

        bool answer = await ErrBad.With(invoked)
                                  .IsOkAndAsync(
                                       static (v, s) =>
                                       {
                                           s.Add(v);

                                           return Task.FromResult(true);
                                       });

        answer.ShouldBeFalse();
        invoked.ShouldBeEmpty();
    }

    [Fact]
    public async Task IsErrAndAsyncAwaitsThePredicateOnlyForErr()
    {
        (await ErrBad.With("bad")
                     .IsErrAndAsync(static (e, s) => Task.FromResult(e == s)))
           .ShouldBeTrue();

        (await ErrBad.With("other")
                     .IsErrAndAsync(static (e, s) => Task.FromResult(e == s)))
           .ShouldBeFalse();

        var invoked = new List<string>();

        bool answer = await OkTwo.With(invoked)
                                 .IsErrAndAsync(
                                      static (e, s) =>
                                      {
                                          s.Add(e);

                                          return Task.FromResult(true);
                                      });

        answer.ShouldBeFalse();
        invoked.ShouldBeEmpty();
    }

    [Fact]
    public async Task MatchAsyncHandsEachBranchItsOwnValue()
    {
        var fromOk = new List<string>();

        int ok = await OkTwo.With(fromOk)
                            .MatchAsync(
                                 static (v, s) =>
                                 {
                                     s.Add($"ok:{v}");

                                     return Task.FromResult(v);
                                 },
                                 static (e, s) =>
                                 {
                                     s.Add($"err:{e}");

                                     return Task.FromResult(e.Length);
                                 });

        ok.ShouldBe(2);
        fromOk.ShouldBe(["ok:2"]);

        var fromErr = new List<string>();

        int err = await ErrBad.With(fromErr)
                              .MatchAsync(
                                   static (v, s) =>
                                   {
                                       s.Add($"ok:{v}");

                                       return Task.FromResult(v);
                                   },
                                   static (e, s) =>
                                   {
                                       s.Add($"err:{e}");

                                       return Task.FromResult(e.Length);
                                   });

        err.ShouldBe(3);
        fromErr.ShouldBe(["err:bad"]);
    }

    [Fact]
    public async Task TheSideEffectMatchAsyncRunsOnlyTheBranchSelected()
    {
        var fromOk = new List<string>();

        await OkTwo.With(fromOk)
                   .MatchAsync(
                        static (v, s) =>
                        {
                            s.Add($"ok:{v}");

                            return Task.CompletedTask;
                        },
                        static (e, s) =>
                        {
                            s.Add($"err:{e}");

                            return Task.CompletedTask;
                        });

        fromOk.ShouldBe(["ok:2"]);

        var fromErr = new List<string>();

        await ErrBad.With(fromErr)
                    .MatchAsync(
                         static (v, s) =>
                         {
                             s.Add($"ok:{v}");

                             return Task.CompletedTask;
                         },
                         static (e, s) =>
                         {
                             s.Add($"err:{e}");

                             return Task.CompletedTask;
                         });

        fromErr.ShouldBe(["err:bad"]);
    }

    [Fact]
    public async Task AndThenAsyncAppliesTheStateAndLeavesAnErrorAlone()
    {
        Result<int, string> produced =
            await OkTwo.With(10)
                       .AndThenAsync(
                            static (v, s) =>
                                new ValueTask<Result<int, string>>(
                                    Result.Ok<int, string>(v + s)));

        produced.ShouldBe(Result.Ok<int, string>(12));

        Result<int, string> stopped =
            await OkTwo.With(10)
                       .AndThenAsync(
                            static (_, _) =>
                                new ValueTask<Result<int, string>>(
                                    Result.Err<int, string>("stopped")));

        stopped.ShouldBe(Result.Err<int, string>("stopped"));

        var invoked = new List<int>();

        Result<int, string> skipped =
            await ErrBad.With(invoked)
                        .AndThenAsync(
                             static (v, s) =>
                             {
                                 s.Add(v);

                                 return new ValueTask<Result<int, string>>(
                                     Result.Ok<int, string>(v));
                             });

        skipped.ShouldBe(Result.Err<int, string>("bad"));
        invoked.ShouldBeEmpty();
    }

    [Fact]
    public async Task OrElseAsyncRecoversFromTheErrorAndLeavesASuccessAlone()
    {
        Result<int, int> recovered =
            await ErrBad.With(10)
                        .OrElseAsync(
                             static (e, s) =>
                                 new ValueTask<Result<int, int>>(
                                     Result.Ok<int, int>(e.Length + s)));

        recovered.ShouldBe(Result.Ok<int, int>(13));

        Result<int, int> failedAgain =
            await ErrBad.With(10)
                        .OrElseAsync(
                             static (_, s) =>
                                 new ValueTask<Result<int, int>>(
                                     Result.Err<int, int>(s)));

        failedAgain.ShouldBe(Result.Err<int, int>(10));

        var invoked = new List<string>();

        Result<int, int> skipped =
            await OkTwo.With(invoked)
                       .OrElseAsync(
                            static (e, s) =>
                            {
                                s.Add(e);

                                return new ValueTask<Result<int, int>>(
                                    Result.Err<int, int>(-1));
                            });

        skipped.ShouldBe(Result.Ok<int, int>(2));
        invoked.ShouldBeEmpty();
    }

    [Fact]
    public async Task UnwrapOrElseAsyncBuildsTheFallbackFromTheError()
    {
        var invoked = new List<string>();

        int fromOk = await OkTwo.With(invoked)
                                .UnwrapOrElseAsync(
                                     static (e, s) =>
                                     {
                                         s.Add(e);

                                         return Task.FromResult(-1);
                                     });

        fromOk.ShouldBe(2);
        invoked.ShouldBeEmpty();

        int fromErr = await ErrBad.With(10)
                                  .UnwrapOrElseAsync(
                                       static (e, s) =>
                                           Task.FromResult(e.Length + s));

        fromErr.ShouldBe(13);
    }

    [Fact]
    public async Task InspectAsyncAwaitsTheActionOnlyForOk()
    {
        var fromOk = new List<int>();

        Result<int, string> ok = await OkTwo.With(fromOk)
                                            .InspectAsync(
                                                 static (v, s) =>
                                                 {
                                                     s.Add(v);

                                                     return Task.CompletedTask;
                                                 });

        ok.ShouldBe(OkTwo);
        fromOk.ShouldBe([2]);

        var fromErr = new List<int>();

        Result<int, string> err = await ErrBad.With(fromErr)
                                              .InspectAsync(
                                                   static (v, s) =>
                                                   {
                                                       s.Add(v);

                                                       return Task
                                                          .CompletedTask;
                                                   });

        err.ShouldBe(ErrBad);
        fromErr.ShouldBeEmpty();
    }

    [Fact]
    public async Task InspectErrAsyncAwaitsTheActionOnlyForErr()
    {
        var fromErr = new List<string>();

        Result<int, string> err = await ErrBad.With(fromErr)
                                              .InspectErrAsync(
                                                   static (e, s) =>
                                                   {
                                                       s.Add(e);

                                                       return Task
                                                          .CompletedTask;
                                                   });

        err.ShouldBe(ErrBad);
        fromErr.ShouldBe(["bad"]);

        var fromOk = new List<string>();

        Result<int, string> ok = await OkTwo.With(fromOk)
                                            .InspectErrAsync(
                                                 static (e, s) =>
                                                 {
                                                     s.Add(e);

                                                     return Task.CompletedTask;
                                                 });

        ok.ShouldBe(OkTwo);
        fromOk.ShouldBeEmpty();
    }

    [Fact]
    public async Task MapAsyncTransformsTheOkValueAndLeavesAnErrorAlone()
    {
        Result<int, string> fromOk =
            await OkTwo.With(10)
                       .MapAsync(static (v, s) => Task.FromResult(v + s));

        fromOk.ShouldBe(Result.Ok<int, string>(12));

        var invoked = new List<int>();

        Result<int, string> fromErr =
            await ErrBad.With(invoked)
                        .MapAsync(
                             static (v, s) =>
                             {
                                 s.Add(v);

                                 return Task.FromResult(v);
                             });

        fromErr.ShouldBe(Result.Err<int, string>("bad"));
        invoked.ShouldBeEmpty();
    }

    [Fact]
    public async Task MapOrAsyncFallsBackToTheDefaultValueForAnError()
    {
        int fromOk =
            await OkTwo.With(10)
                       .MapOrAsync(-1, static (v, s) => Task.FromResult(v + s));

        fromOk.ShouldBe(12);

        int fromErr =
            await ErrBad.With(10)
                        .MapOrAsync(-1, static (v, s) => Task.FromResult(v + s));

        fromErr.ShouldBe(-1);
    }

    [Fact]
    public async Task MapOrDefaultAsyncFallsBackToTheDefaultOfTheOutputType()
    {
        string? fromOk =
            await OkTwo.With("!")
                       .MapOrDefaultAsync(
                            static (v, s) => Task.FromResult(v + s));

        fromOk.ShouldBe("2!");

        string? fromErr =
            await ErrBad.With("!")
                        .MapOrDefaultAsync(
                             static (v, s) => Task.FromResult(v + s));

        fromErr.ShouldBeNull();
    }

    [Fact]
    public async Task MapOrElseAsyncAwaitsExactlyOneOfItsTwoDelegates()
    {
        var fromOk = new List<string>();

        int ok = await OkTwo.With(fromOk)
                            .MapOrElseAsync(
                                 static (e, s) =>
                                 {
                                     s.Add($"default:{e}");

                                     return Task.FromResult(-1);
                                 },
                                 static (v, s) =>
                                 {
                                     s.Add($"map:{v}");

                                     return Task.FromResult(v);
                                 });

        ok.ShouldBe(2);
        fromOk.ShouldBe(["map:2"]);

        var fromErr = new List<string>();

        int err = await ErrBad.With(fromErr)
                              .MapOrElseAsync(
                                   static (e, s) =>
                                   {
                                       s.Add($"default:{e}");

                                       return Task.FromResult(e.Length);
                                   },
                                   static (v, s) =>
                                   {
                                       s.Add($"map:{v}");

                                       return Task.FromResult(v);
                                   });

        err.ShouldBe(3);
        fromErr.ShouldBe(["default:bad"]);
    }

    [Fact]
    public async Task MapErrAsyncRestatesTheErrorAndLeavesASuccessAlone()
    {
        Result<int, string> fromErr =
            await ErrBad.With("!")
                        .MapErrAsync(static (e, s) => Task.FromResult(e + s));

        fromErr.ShouldBe(Result.Err<int, string>("bad!"));

        var invoked = new List<string>();

        Result<int, string> fromOk =
            await OkTwo.With(invoked)
                       .MapErrAsync(
                            static (e, s) =>
                            {
                                s.Add(e);

                                return Task.FromResult(e);
                            });

        fromOk.ShouldBe(Result.Ok<int, string>(2));
        invoked.ShouldBeEmpty();
    }

    /// <remarks>
    /// As on the option side: an async member returns a faulted task rather than
    /// throwing from the call, so the misuse surfaces at the await instead.
    /// </remarks>
    [Fact]
    public async Task ADefaultBoundFaultsRatherThanDereferencingNothing()
    {
        Result<int, string>.Bound<int> bound = default;

        InvalidOperationException thrown =
            await Should.ThrowAsync<InvalidOperationException>(
                async () => await bound.MapAsync(
                    static (v, s) => Task.FromResult(v + s)));

        thrown.Message.ShouldContain("Build one by calling With");
    }
}
