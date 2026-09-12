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

    /// <remarks>
    /// The mixed overloads exist because WM2017 rewrites a capturing call to the
    /// binder without changing the member name, so every shape
    /// <see cref="Result{TOk,TErr}" /> declares needs one here or the fix it
    /// offers does not compile. Before DRA-211 only the both-asynchronous forms
    /// existed.
    /// <para>
    /// The branch that does not await is asserted through
    /// <see cref="ValueTask.IsCompleted" /> before the call is awaited, which
    /// pins what the summary promises — that case completes synchronously.
    /// </para>
    /// </remarks>
    [Fact]
    public async Task TheSideEffectMatchAsyncAwaitsOkAndCompletesErrSynchronously()
    {
        var fromOk = new List<string>();

        ValueTask ok = OkTwo.With(fromOk)
                            .MatchAsync(
                                 static (v, s) =>
                                 {
                                     s.Add($"ok:{v}");

                                     return Task.CompletedTask;
                                 },
                                 static (e, s) => s.Add($"err:{e}"));

        await ok;
        fromOk.ShouldBe(["ok:2"]);

        var fromErr = new List<string>();

        ValueTask err = ErrBad.With(fromErr)
                              .MatchAsync(
                                   static (v, s) =>
                                   {
                                       s.Add($"ok:{v}");

                                       return Task.CompletedTask;
                                   },
                                   static (e, s) => s.Add($"err:{e}"));

        err.IsCompleted.ShouldBeTrue();
        await err;
        fromErr.ShouldBe(["err:bad"]);
    }

    [Fact]
    public async Task TheSideEffectMatchAsyncAwaitsErrAndCompletesOkSynchronously()
    {
        var fromOk = new List<string>();

        ValueTask ok = OkTwo.With(fromOk)
                            .MatchAsync(
                                 static (v, s) => s.Add($"ok:{v}"),
                                 static (e, s) =>
                                 {
                                     s.Add($"err:{e}");

                                     return Task.CompletedTask;
                                 });

        ok.IsCompleted.ShouldBeTrue();
        await ok;
        fromOk.ShouldBe(["ok:2"]);

        var fromErr = new List<string>();

        ValueTask err = ErrBad.With(fromErr)
                              .MatchAsync(
                                   static (v, s) => s.Add($"ok:{v}"),
                                   static (e, s) =>
                                   {
                                       s.Add($"err:{e}");

                                       return Task.CompletedTask;
                                   });

        await err;
        fromErr.ShouldBe(["err:bad"]);
    }

    /// <remarks>
    /// The mixed value-returning binder shapes arrived with the core ones in
    /// DRA-211, in the same layer rather than after it —
    /// <c>BinderShapeCompletenessTests</c> fails on a core shape whose twin is
    /// missing, so the two cannot be split across changes.
    /// </remarks>
    /// <remarks>
    /// The bound state is a <see cref="TaskCompletionSource{TResult}" /> rather
    /// than a number, because <c>Task.FromResult</c> would defeat the assertion
    /// that matters here: it completes synchronously, so
    /// <see cref="ValueTask{TResult}.IsCompleted" /> reads true whichever branch
    /// ran. A source left uncompleted makes the two distinguishable — the
    /// awaiting branch reports false, and the branch that does not await reports
    /// true while a task that will never complete sits unawaited beside it.
    /// <para>
    /// That matters more here than anywhere else in this file: these two
    /// overloads have character-for-character identical bodies, and only the
    /// delegate types tell the two <c>ValueTask&lt;TOut&gt;</c> constructors
    /// apart. Nothing in the source of either one would look wrong if it were.
    /// </para>
    /// </remarks>
    [Fact]
    public async Task MatchAsyncAwaitsOkAndCompletesErrSynchronously()
    {
        var pending = new TaskCompletionSource<int>();

        ValueTask<int> ok = OkTwo.With(pending)
                                 .MatchAsync(
                                      static (v, s) => s.Task,
                                      static (e, s) => -1);

        ok.IsCompleted.ShouldBeFalse();
        pending.SetResult(12);
        (await ok).ShouldBe(12);

        ValueTask<int> err = ErrBad.With(new TaskCompletionSource<int>())
                                   .MatchAsync(
                                        static (v, s) => s.Task,
                                        static (e, s) => e.Length);

        err.IsCompleted.ShouldBeTrue();
        (await err).ShouldBe(3);
    }

    [Fact]
    public async Task MatchAsyncAwaitsErrAndCompletesOkSynchronously()
    {
        ValueTask<int> ok = OkTwo.With(new TaskCompletionSource<int>())
                                 .MatchAsync(
                                      static (v, s) => v,
                                      static (e, s) => s.Task);

        ok.IsCompleted.ShouldBeTrue();
        (await ok).ShouldBe(2);

        var pending = new TaskCompletionSource<int>();

        ValueTask<int> err = ErrBad.With(pending)
                                   .MatchAsync(
                                        static (v, s) => v,
                                        static (e, s) => s.Task);

        err.IsCompleted.ShouldBeFalse();
        pending.SetResult(4);
        (await err).ShouldBe(4);
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

    /// <remarks>
    /// Where the guard throws is decided by the step, not by the member: it
    /// reads <c>IsCompletedSuccessfully</c> and checks a finished task before
    /// the call returns. So this pair is not one test written twice — a guard
    /// reached on only one of the two paths is, to a caller who meets the
    /// other, the same as no guard at all.
    /// </remarks>
    [Fact]
    public void AndThenAsyncThrowsFromTheCallWhenACompletedStepIsNull()
    {
        ArgumentNullException thrown =
            Should.Throw<ArgumentNullException>(
                () => OkTwo.With(10)
                           .AndThenAsync(
                                static (_, _) =>
                                    new ValueTask<Result<int, string>>(
                                        default(Result<int, string>)!)));

        thrown.ParamName.ShouldBe("resultFactory");
    }

    /// <remarks>
    /// The bound state is the gate, which keeps the delegate
    /// <see langword="static" /> and holds its task incomplete until after the
    /// call has returned. <c>await Task.Yield()</c> does not: on an idle thread
    /// pool it can resume before the guard reads
    /// <c>IsCompletedSuccessfully</c>, and the throw then lands at the call,
    /// quietly turning this into a second copy of the test above.
    /// </remarks>
    [Fact]
    public async Task AndThenAsyncFaultsTheReturnedTaskWhenAPendingStepIsNull()
    {
        var gate = new TaskCompletionSource<bool>();

        ValueTask<Result<int, string>> pending =
            OkTwo.With(gate)
                 .AndThenAsync(
                      static async ValueTask<Result<int, string>> (_, s) =>
                      {
                          await s.Task;

                          return default(Result<int, string>)!;
                      });

        pending.IsCompleted.ShouldBeFalse();
        gate.SetResult(true);

        ArgumentNullException thrown =
            await Should.ThrowAsync<ArgumentNullException>(
                async () => await pending);

        thrown.ParamName.ShouldBe("resultFactory");
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
    public async Task MapOrNullAsyncAwaitsTheMapOnlyForTheOkValue()
    {
        var okSaw = new List<int>();

        int? fromOk =
            await OkTwo.With(okSaw)
                       .MapOrNullAsync(
                            static (v, s) =>
                            {
                                s.Add(v);

                                return Task.FromResult(v - 2);
                            });

        fromOk.ShouldBe(0);
        okSaw.ShouldBe(new[] { 2 });

        var errSaw = new List<int>();

        int? fromErr =
            await ErrBad.With(errSaw)
                        .MapOrNullAsync(
                             static (v, s) =>
                             {
                                 s.Add(v);

                                 return Task.FromResult(v - 2);
                             });

        fromErr.ShouldBeNull();
        errSaw.ShouldBeEmpty();
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
    public async Task MapOrElseAsyncAwaitsTheMapAndCompletesTheFallbackSynchronously()
    {
        var fromOk = new List<string>();

        ValueTask<int> ok = OkTwo.With(fromOk)
                                 .MapOrElseAsync(
                                      static (e, s) =>
                                      {
                                          s.Add($"default:{e}");

                                          return -1;
                                      },
                                      static (v, s) =>
                                      {
                                          s.Add($"map:{v}");

                                          return Task.FromResult(v);
                                      });

        (await ok).ShouldBe(2);
        fromOk.ShouldBe(["map:2"]);

        var fromErr = new List<string>();

        ValueTask<int> err = ErrBad.With(fromErr)
                                   .MapOrElseAsync(
                                        static (e, s) =>
                                        {
                                            s.Add($"default:{e}");

                                            return e.Length;
                                        },
                                        static (v, s) =>
                                        {
                                            s.Add($"map:{v}");

                                            return Task.FromResult(v);
                                        });

        err.IsCompleted.ShouldBeTrue();
        (await err).ShouldBe(3);
        fromErr.ShouldBe(["default:bad"]);
    }

    [Fact]
    public async Task MapOrElseAsyncAwaitsTheFallbackAndCompletesTheMapSynchronously()
    {
        var fromOk = new List<string>();

        ValueTask<int> ok = OkTwo.With(fromOk)
                                 .MapOrElseAsync(
                                      static (e, s) =>
                                      {
                                          s.Add($"default:{e}");

                                          return Task.FromResult(-1);
                                      },
                                      static (v, s) =>
                                      {
                                          s.Add($"map:{v}");

                                          return v;
                                      });

        ok.IsCompleted.ShouldBeTrue();
        (await ok).ShouldBe(2);
        fromOk.ShouldBe(["map:2"]);

        var fromErr = new List<string>();

        ValueTask<int> err = ErrBad.With(fromErr)
                                   .MapOrElseAsync(
                                        static (e, s) =>
                                        {
                                            s.Add($"default:{e}");

                                            return Task.FromResult(e.Length);
                                        },
                                        static (v, s) =>
                                        {
                                            s.Add($"map:{v}");

                                            return v;
                                        });

        (await err).ShouldBe(3);
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
    /// As on the option side: the outer method carries no
    /// <see langword="async" />, so the misuse arrives at the call. Asserted
    /// synchronously for that reason — a faulted task would throw nothing
    /// here.
    /// </remarks>
    [Fact]
    public void ADefaultBoundThrowsFromTheCallRatherThanFromTheAwait()
    {
        Result<int, string>.Bound<int> bound = default;

        InvalidOperationException thrown =
            Should.Throw<InvalidOperationException>(
                () => bound.MapAsync(
                    static (v, s) => Task.FromResult(v + s)));

        thrown.Message.ShouldContain("Build one by calling With");
    }

    /// <remarks>
    /// The result side reads its error through <c>UnwrapErr</c> after the ok
    /// check, so this pins the eager path on the branch that does that rather
    /// than only on the ok branch the option side covers.
    /// </remarks>
    [Fact]
    public void ADelegateThrowingBeforeItsTaskThrowsFromTheCall()
    {
        InvalidOperationException thrown =
            Should.Throw<InvalidOperationException>(
                () => ErrBad.With(2)
                            .MapErrAsync<string>(
                                 static (_, _) =>
                                     throw new InvalidOperationException(
                                         "thrown before the task")));

        thrown.Message.ShouldBe("thrown before the task");
    }
}
