namespace Waystone.Monads.Options;

using Extensions;
using Results;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

/// <remarks>
/// The async members on <see cref="Option{T}.Bound{TState}" /> do not forward to
/// a state overload, because none exists — they match on the case themselves. So
/// unlike the sync tests, what needs pinning is not which overload was reached
/// but that the delegate is awaited on exactly one branch.
/// <para>
/// Every test therefore exercises both cases, and the skipped branch is asserted
/// by an empty recorder rather than only by the return value. A member that
/// awaited its delegate on the empty branch would still return the right answer
/// for most of these; the recorder is what catches it.
/// </para>
/// <para>
/// The recorder is the bound state, which keeps every delegate
/// <see langword="static" />.
/// </para>
/// <para>
/// Every member with a branch that does not await is written without
/// <see langword="async" />, so the state machine is built only on the branch
/// that awaits. That is what the last two tests here pin: the outer method
/// runs eagerly, so a failure reaching it arrives at the call rather than at
/// the await.
/// </para>
/// </remarks>
public sealed class OptionBoundAsyncTests
{
    private static readonly Option<int> SomeTwo = Option.Some(2);

    private static readonly Option<int> NoneInt = Option.None<int>();

    [Fact]
    public async Task IsSomeAndAsyncAwaitsThePredicateOnlyForSome()
    {
        (await SomeTwo.With(2)
                      .IsSomeAndAsync(
                           static (v, s) => Task.FromResult(v == s)))
           .ShouldBeTrue();

        (await SomeTwo.With(3)
                      .IsSomeAndAsync(
                           static (v, s) => Task.FromResult(v == s)))
           .ShouldBeFalse();

        var invoked = new List<int>();

        bool answer = await NoneInt.With(invoked)
                                   .IsSomeAndAsync(
                                        static (v, s) =>
                                        {
                                            s.Add(v);

                                            return Task.FromResult(true);
                                        });

        answer.ShouldBeFalse();
        invoked.ShouldBeEmpty();
    }

    [Fact]
    public async Task IsNoneOrAsyncAwaitsThePredicateOnlyForSome()
    {
        (await SomeTwo.With(2)
                      .IsNoneOrAsync(
                           static (v, s) => Task.FromResult(v == s)))
           .ShouldBeTrue();

        (await SomeTwo.With(3)
                      .IsNoneOrAsync(
                           static (v, s) => Task.FromResult(v == s)))
           .ShouldBeFalse();

        var invoked = new List<int>();

        bool answer = await NoneInt.With(invoked)
                                   .IsNoneOrAsync(
                                        static (v, s) =>
                                        {
                                            s.Add(v);

                                            return Task.FromResult(false);
                                        });

        answer.ShouldBeTrue();
        invoked.ShouldBeEmpty();
    }

    [Fact]
    public async Task MatchAsyncAwaitsTheBranchForTheOptionsCase()
    {
        var fromSome = new List<string>();

        int some = await SomeTwo.With(fromSome)
                                .MatchAsync(
                                     static (v, s) =>
                                     {
                                         s.Add($"some:{v}");

                                         return Task.FromResult(v);
                                     },
                                     static s =>
                                     {
                                         s.Add("none");

                                         return Task.FromResult(-1);
                                     });

        some.ShouldBe(2);
        fromSome.ShouldBe(["some:2"]);

        var fromNone = new List<string>();

        int none = await NoneInt.With(fromNone)
                                .MatchAsync(
                                     static (v, s) =>
                                     {
                                         s.Add($"some:{v}");

                                         return Task.FromResult(v);
                                     },
                                     static s =>
                                     {
                                         s.Add("none");

                                         return Task.FromResult(-1);
                                     });

        none.ShouldBe(-1);
        fromNone.ShouldBe(["none"]);
    }

    /// <remarks>
    /// The mixed overloads exist because WM2017 rewrites a capturing call to the
    /// binder without changing the member name, so every shape
    /// <see cref="Option{T}" /> declares needs one here or the fix it offers does
    /// not compile. Before DRA-211 only the both-asynchronous form existed.
    /// <para>
    /// The branch that does not await is asserted through
    /// <see cref="ValueTask{TResult}.IsCompleted" /> before the result is read,
    /// which pins what the summary promises — that case completes synchronously.
    /// Reading the value first would await the completion into existence and
    /// assert nothing.
    /// </para>
    /// </remarks>
    [Fact]
    public async Task MatchAsyncAwaitsTheSomeBranchAndCompletesNoneSynchronously()
    {
        var fromSome = new List<string>();

        ValueTask<int> some = SomeTwo.With(fromSome)
                                     .MatchAsync(
                                          static (v, s) =>
                                          {
                                              s.Add($"some:{v}");

                                              return Task.FromResult(v);
                                          },
                                          static s =>
                                          {
                                              s.Add("none");

                                              return -1;
                                          });

        (await some).ShouldBe(2);
        fromSome.ShouldBe(["some:2"]);

        var fromNone = new List<string>();

        ValueTask<int> none = NoneInt.With(fromNone)
                                     .MatchAsync(
                                          static (v, s) =>
                                          {
                                              s.Add($"some:{v}");

                                              return Task.FromResult(v);
                                          },
                                          static s =>
                                          {
                                              s.Add("none");

                                              return -1;
                                          });

        none.IsCompleted.ShouldBeTrue();
        (await none).ShouldBe(-1);
        fromNone.ShouldBe(["none"]);
    }

    [Fact]
    public async Task MatchAsyncAwaitsTheNoneBranchAndCompletesSomeSynchronously()
    {
        var fromSome = new List<string>();

        ValueTask<int> some = SomeTwo.With(fromSome)
                                     .MatchAsync(
                                          static (v, s) =>
                                          {
                                              s.Add($"some:{v}");

                                              return v;
                                          },
                                          static s =>
                                          {
                                              s.Add("none");

                                              return Task.FromResult(-1);
                                          });

        some.IsCompleted.ShouldBeTrue();
        (await some).ShouldBe(2);
        fromSome.ShouldBe(["some:2"]);

        var fromNone = new List<string>();

        ValueTask<int> none = NoneInt.With(fromNone)
                                     .MatchAsync(
                                          static (v, s) =>
                                          {
                                              s.Add($"some:{v}");

                                              return v;
                                          },
                                          static s =>
                                          {
                                              s.Add("none");

                                              return Task.FromResult(-1);
                                          });

        (await none).ShouldBe(-1);
        fromNone.ShouldBe(["none"]);
    }

    /// <remarks>
    /// The void-returning binder shapes arrived with the core ones in DRA-211,
    /// in the same layer rather than after it — <c>BinderShapeCompletenessTests</c>
    /// fails on a core shape whose twin is missing, so the two cannot be split
    /// across changes.
    /// </remarks>
    [Fact]
    public async Task TheSideEffectMatchAsyncRunsOnlyTheBranchSelected()
    {
        var fromSome = new List<string>();

        await SomeTwo.With(fromSome)
                     .MatchAsync(
                          static (v, s) =>
                          {
                              s.Add($"some:{v}");

                              return Task.CompletedTask;
                          },
                          static s =>
                          {
                              s.Add("none");

                              return Task.CompletedTask;
                          });

        fromSome.ShouldBe(["some:2"]);

        var fromNone = new List<string>();

        await NoneInt.With(fromNone)
                     .MatchAsync(
                          static (v, s) =>
                          {
                              s.Add($"some:{v}");

                              return Task.CompletedTask;
                          },
                          static s =>
                          {
                              s.Add("none");

                              return Task.CompletedTask;
                          });

        fromNone.ShouldBe(["none"]);
    }

    [Fact]
    public async Task TheSideEffectMatchAsyncAwaitsSomeAndCompletesNoneSynchronously()
    {
        var fromSome = new List<string>();

        await SomeTwo.With(fromSome)
                     .MatchAsync(
                          static (v, s) =>
                          {
                              s.Add($"some:{v}");

                              return Task.CompletedTask;
                          },
                          static s => s.Add("none"));

        fromSome.ShouldBe(["some:2"]);

        var fromNone = new List<string>();

        ValueTask none = NoneInt.With(fromNone)
                                .MatchAsync(
                                     static (v, s) =>
                                     {
                                         s.Add($"some:{v}");

                                         return Task.CompletedTask;
                                     },
                                     static s => s.Add("none"));

        none.IsCompleted.ShouldBeTrue();
        await none;
        fromNone.ShouldBe(["none"]);
    }

    [Fact]
    public async Task TheSideEffectMatchAsyncAwaitsNoneAndCompletesSomeSynchronously()
    {
        var fromSome = new List<string>();

        ValueTask some = SomeTwo.With(fromSome)
                                .MatchAsync(
                                     static (v, s) => s.Add($"some:{v}"),
                                     static s =>
                                     {
                                         s.Add("none");

                                         return Task.CompletedTask;
                                     });

        some.IsCompleted.ShouldBeTrue();
        await some;
        fromSome.ShouldBe(["some:2"]);

        var fromNone = new List<string>();

        await NoneInt.With(fromNone)
                     .MatchAsync(
                          static (v, s) => s.Add($"some:{v}"),
                          static s =>
                          {
                              s.Add("none");

                              return Task.CompletedTask;
                          });

        fromNone.ShouldBe(["none"]);
    }

    [Fact]
    public async Task UnwrapOrElseAsyncAwaitsTheFactoryOnlyForNone()
    {
        var invoked = new List<int>();

        int fromSome = await SomeTwo.With(invoked)
                                    .UnwrapOrElseAsync(
                                         static s =>
                                         {
                                             s.Add(-1);

                                             return Task.FromResult(99);
                                         });

        fromSome.ShouldBe(2);
        invoked.ShouldBeEmpty();

        int fromNone = await NoneInt.With(10)
                                    .UnwrapOrElseAsync(
                                         static s => Task.FromResult(s));

        fromNone.ShouldBe(10);
    }

    [Fact]
    public async Task MapAsyncAwaitsTheTransformOnlyForSome()
    {
        Option<int> fromSome =
            await SomeTwo.With(10)
                         .MapAsync(static (v, s) => Task.FromResult(v + s));

        fromSome.ShouldBe(Option.Some(12));

        var invoked = new List<int>();

        Option<int> fromNone = await NoneInt.With(invoked)
                                            .MapAsync(
                                                 static (v, s) =>
                                                 {
                                                     s.Add(v);

                                                     return Task.FromResult(v);
                                                 });

        fromNone.ShouldBe(Option.None<int>());
        invoked.ShouldBeEmpty();
    }

    [Fact]
    public async Task AndThenAsyncKeepsWhicheverCaseTheStepReturns()
    {
        Option<int> produced =
            await SomeTwo.With(10)
                         .AndThenAsync(
                              static (v, s) =>
                                  new ValueTask<Option<int>>(
                                      Option.Some(v + s)));

        produced.ShouldBe(Option.Some(12));

        Option<int> stopped =
            await SomeTwo.With(10)
                         .AndThenAsync(
                              static (_, _) =>
                                  new ValueTask<Option<int>>(
                                      Option.None<int>()));

        stopped.ShouldBe(Option.None<int>());

        var invoked = new List<int>();

        Option<int> skipped =
            await NoneInt.With(invoked)
                         .AndThenAsync(
                              static (v, s) =>
                              {
                                  s.Add(v);

                                  return new ValueTask<Option<int>>(
                                      Option.Some(v));
                              });

        skipped.ShouldBe(Option.None<int>());
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
                () => SomeTwo.With(10)
                             .AndThenAsync(
                                  static (_, _) =>
                                      new ValueTask<Option<int>>(
                                          default(Option<int>)!)));

        thrown.ParamName.ShouldBe("optionFactory");
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

        ValueTask<Option<int>> pending =
            SomeTwo.With(gate)
                   .AndThenAsync(
                        static async ValueTask<Option<int>> (_, s) =>
                        {
                            await s.Task;

                            return default(Option<int>)!;
                        });

        pending.IsCompleted.ShouldBeFalse();
        gate.SetResult(true);

        ArgumentNullException thrown =
            await Should.ThrowAsync<ArgumentNullException>(
                async () => await pending);

        thrown.ParamName.ShouldBe("optionFactory");
    }

    [Fact]
    public async Task MapOrAsyncFallsBackToTheDefaultValueForNone()
    {
        int fromSome =
            await SomeTwo.With(10)
                         .MapOrAsync(-1, static (v, s) => Task.FromResult(v + s));

        fromSome.ShouldBe(12);

        int fromNone =
            await NoneInt.With(10)
                         .MapOrAsync(-1, static (v, s) => Task.FromResult(v + s));

        fromNone.ShouldBe(-1);
    }

    [Fact]
    public async Task MapOrDefaultAsyncFallsBackToTheDefaultOfTheOutputType()
    {
        string? fromSome =
            await SomeTwo.With("!")
                         .MapOrDefaultAsync(
                              static (v, s) => Task.FromResult(v + s));

        fromSome.ShouldBe("2!");

        string? fromNone =
            await NoneInt.With("!")
                         .MapOrDefaultAsync(
                              static (v, s) => Task.FromResult(v + s));

        fromNone.ShouldBeNull();
    }

    [Fact]
    public async Task ZipWithAsyncAwaitsOnlyWhenBothSidesHoldAValue()
    {
        var saw = new List<int>();

        Option<int> both =
            await SomeTwo.With(saw)
                         .ZipWithAsync(
                              Option.Some(3),
                              static (v, o, s) =>
                              {
                                  s.Add(v + o);

                                  return Task.FromResult(v + o);
                              });

        both.ShouldBeSomeValue(5);
        saw.ShouldBe(new[] { 5 });

        var otherAbsent = new List<int>();

        Option<int> noOther =
            await SomeTwo.With(otherAbsent)
                         .ZipWithAsync(
                              Option.None<int>(),
                              static (v, o, s) =>
                              {
                                  s.Add(v + o);

                                  return Task.FromResult(v + o);
                              });

        noOther.ShouldBeNone();
        otherAbsent.ShouldBeEmpty();

        var selfAbsent = new List<int>();

        Option<int> noSelf =
            await NoneInt.With(selfAbsent)
                         .ZipWithAsync(
                              Option.Some(3),
                              static (v, o, s) =>
                              {
                                  s.Add(v + o);

                                  return Task.FromResult(v + o);
                              });

        noSelf.ShouldBeNone();
        selfAbsent.ShouldBeEmpty();
    }

    [Fact]
    public async Task ReduceAsyncAwaitsOnlyWhenThereAreTwoValuesToCombine()
    {
        var saw = new List<int>();

        Option<int> both =
            await SomeTwo.With(saw)
                         .ReduceAsync(
                              Option.Some(3),
                              static (a, b, s) =>
                              {
                                  s.Add(a + b);

                                  return Task.FromResult(a + b);
                              });

        both.ShouldBeSomeValue(5);
        saw.ShouldBe(new[] { 5 });

        var otherAbsent = new List<int>();

        Option<int> noOther =
            await SomeTwo.With(otherAbsent)
                         .ReduceAsync(
                              Option.None<int>(),
                              static (a, b, s) =>
                              {
                                  s.Add(a + b);

                                  return Task.FromResult(a + b);
                              });

        noOther.ShouldBeSomeValue(2);
        otherAbsent.ShouldBeEmpty();

        var selfAbsent = new List<int>();

        Option<int> noSelf =
            await NoneInt.With(selfAbsent)
                         .ReduceAsync(
                              Option.Some(3),
                              static (a, b, s) =>
                              {
                                  s.Add(a + b);

                                  return Task.FromResult(a + b);
                              });

        noSelf.ShouldBeSomeValue(3);
        selfAbsent.ShouldBeEmpty();
    }

    [Fact]
    public async Task MapOrNullAsyncAwaitsTheMapOnlyForTheContainedValue()
    {
        var someSaw = new List<int>();

        int? fromSome =
            await SomeTwo.With(someSaw)
                         .MapOrNullAsync(
                              static (v, s) =>
                              {
                                  s.Add(v);

                                  return Task.FromResult(v - 2);
                              });

        fromSome.ShouldBe(0);
        someSaw.ShouldBe(new[] { 2 });

        var noneSaw = new List<int>();

        int? fromNone =
            await NoneInt.With(noneSaw)
                         .MapOrNullAsync(
                              static (v, s) =>
                              {
                                  s.Add(v);

                                  return Task.FromResult(v - 2);
                              });

        fromNone.ShouldBeNull();
        noneSaw.ShouldBeEmpty();
    }

    [Fact]
    public async Task MapOrElseAsyncAwaitsExactlyOneOfItsTwoDelegates()
    {
        var fromSome = new List<string>();

        int some = await SomeTwo.With(fromSome)
                                .MapOrElseAsync(
                                     static s =>
                                     {
                                         s.Add("default");

                                         return Task.FromResult(-1);
                                     },
                                     static (v, s) =>
                                     {
                                         s.Add($"map:{v}");

                                         return Task.FromResult(v);
                                     });

        some.ShouldBe(2);
        fromSome.ShouldBe(["map:2"]);

        var fromNone = new List<string>();

        int none = await NoneInt.With(fromNone)
                                .MapOrElseAsync(
                                     static s =>
                                     {
                                         s.Add("default");

                                         return Task.FromResult(-1);
                                     },
                                     static (v, s) =>
                                     {
                                         s.Add($"map:{v}");

                                         return Task.FromResult(v);
                                     });

        none.ShouldBe(-1);
        fromNone.ShouldBe(["default"]);
    }

    [Fact]
    public async Task MapOrElseAsyncAwaitsTheMapAndCompletesTheFallbackSynchronously()
    {
        var fromSome = new List<string>();

        ValueTask<int> some = SomeTwo.With(fromSome)
                                     .MapOrElseAsync(
                                          static s =>
                                          {
                                              s.Add("default");

                                              return -1;
                                          },
                                          static (v, s) =>
                                          {
                                              s.Add($"map:{v}");

                                              return Task.FromResult(v);
                                          });

        (await some).ShouldBe(2);
        fromSome.ShouldBe(["map:2"]);

        var fromNone = new List<string>();

        ValueTask<int> none = NoneInt.With(fromNone)
                                     .MapOrElseAsync(
                                          static s =>
                                          {
                                              s.Add("default");

                                              return -1;
                                          },
                                          static (v, s) =>
                                          {
                                              s.Add($"map:{v}");

                                              return Task.FromResult(v);
                                          });

        none.IsCompleted.ShouldBeTrue();
        (await none).ShouldBe(-1);
        fromNone.ShouldBe(["default"]);
    }

    [Fact]
    public async Task MapOrElseAsyncAwaitsTheFallbackAndCompletesTheMapSynchronously()
    {
        var fromSome = new List<string>();

        ValueTask<int> some = SomeTwo.With(fromSome)
                                     .MapOrElseAsync(
                                          static s =>
                                          {
                                              s.Add("default");

                                              return Task.FromResult(-1);
                                          },
                                          static (v, s) =>
                                          {
                                              s.Add($"map:{v}");

                                              return v;
                                          });

        some.IsCompleted.ShouldBeTrue();
        (await some).ShouldBe(2);
        fromSome.ShouldBe(["map:2"]);

        var fromNone = new List<string>();

        ValueTask<int> none = NoneInt.With(fromNone)
                                     .MapOrElseAsync(
                                          static s =>
                                          {
                                              s.Add("default");

                                              return Task.FromResult(-1);
                                          },
                                          static (v, s) =>
                                          {
                                              s.Add($"map:{v}");

                                              return v;
                                          });

        (await none).ShouldBe(-1);
        fromNone.ShouldBe(["default"]);
    }

    [Fact]
    public async Task InspectAsyncAwaitsTheActionOnlyForSomeAndReturnsTheOption()
    {
        var fromSome = new List<int>();

        Option<int> some = await SomeTwo.With(fromSome)
                                        .InspectAsync(
                                             static (v, s) =>
                                             {
                                                 s.Add(v);

                                                 return Task.CompletedTask;
                                             });

        some.ShouldBe(SomeTwo);
        fromSome.ShouldBe([2]);

        var fromNone = new List<int>();

        Option<int> none = await NoneInt.With(fromNone)
                                        .InspectAsync(
                                             static (v, s) =>
                                             {
                                                 s.Add(v);

                                                 return Task.CompletedTask;
                                             });

        none.ShouldBe(NoneInt);
        fromNone.ShouldBeEmpty();
    }

    [Fact]
    public async Task FilterAsyncDiscardsAValueThePredicateRejects()
    {
        Option<int> kept =
            await SomeTwo.With(2)
                         .FilterAsync(static (v, s) => Task.FromResult(v == s));

        kept.ShouldBe(Option.Some(2));

        Option<int> dropped =
            await SomeTwo.With(3)
                         .FilterAsync(static (v, s) => Task.FromResult(v == s));

        dropped.ShouldBe(Option.None<int>());

        var invoked = new List<int>();

        Option<int> skipped = await NoneInt.With(invoked)
                                           .FilterAsync(
                                                static (v, s) =>
                                                {
                                                    s.Add(v);

                                                    return Task.FromResult(true);
                                                });

        skipped.ShouldBe(Option.None<int>());
        invoked.ShouldBeEmpty();
    }

    [Fact]
    public async Task OrElseAsyncAwaitsTheFactoryOnlyForNone()
    {
        var invoked = new List<int>();

        Option<int> fromSome =
            await SomeTwo.With(invoked)
                         .OrElseAsync(
                              static s =>
                              {
                                  s.Add(-1);

                                  return new ValueTask<Option<int>>(
                                      Option.Some(99));
                              });

        fromSome.ShouldBe(Option.Some(2));
        invoked.ShouldBeEmpty();

        Option<int> recovered =
            await NoneInt.With(10)
                         .OrElseAsync(
                              static s =>
                                  new ValueTask<Option<int>>(Option.Some(s)));

        recovered.ShouldBe(Option.Some(10));

        Option<int> stillNone =
            await NoneInt.With(10)
                         .OrElseAsync(
                              static _ =>
                                  new ValueTask<Option<int>>(
                                      Option.None<int>()));

        stillNone.ShouldBe(Option.None<int>());
    }

    [Fact]
    public void OrElseAsyncThrowsFromTheCallWhenACompletedFactoryIsNull()
    {
        ArgumentNullException thrown =
            Should.Throw<ArgumentNullException>(
                () => NoneInt.With(10)
                             .OrElseAsync(
                                  static _ =>
                                      new ValueTask<Option<int>>(
                                          default(Option<int>)!)));

        thrown.ParamName.ShouldBe("optionFactory");
    }

    [Fact]
    public async Task OrElseAsyncFaultsTheReturnedTaskWhenAPendingFactoryIsNull()
    {
        var gate = new TaskCompletionSource<bool>();

        ValueTask<Option<int>> pending =
            NoneInt.With(gate)
                   .OrElseAsync(
                        static async ValueTask<Option<int>> (s) =>
                        {
                            await s.Task;

                            return default(Option<int>)!;
                        });

        pending.IsCompleted.ShouldBeFalse();
        gate.SetResult(true);

        ArgumentNullException thrown =
            await Should.ThrowAsync<ArgumentNullException>(
                async () => await pending);

        thrown.ParamName.ShouldBe("optionFactory");
    }

    [Fact]
    public async Task OkOrElseAsyncAwaitsTheErrorFactoryOnlyForNone()
    {
        Result<int, string> fromSome =
            await SomeTwo.With("missing")
                         .OkOrElseAsync(static s => Task.FromResult(s));

        fromSome.ShouldBe(Result.Ok<int, string>(2));

        Result<int, string> fromNone =
            await NoneInt.With("missing")
                         .OkOrElseAsync(static s => Task.FromResult(s));

        fromNone.ShouldBe(Result.Err<int, string>("missing"));
    }

    /// <remarks>
    /// The misuse arrives at the call, not at the await, because the outer
    /// method carries no <see langword="async" /> and so reads
    /// <c>Source</c> eagerly. Asserted with the synchronous
    /// <see cref="Should.Throw{TException}(System.Action)" /> and no
    /// <see langword="await" /> anywhere, which is what makes it a test of
    /// eagerness rather than of the message: were the throw still captured in
    /// a faulted task, nothing would be thrown here at all.
    /// <para>
    /// The message matters too, being the only thing naming the cause. It
    /// would otherwise be easy to lose behind a
    /// <see cref="NullReferenceException" />.
    /// </para>
    /// </remarks>
    [Fact]
    public void ADefaultBoundThrowsFromTheCallRatherThanFromTheAwait()
    {
        Option<int>.Bound<int> bound = default;

        InvalidOperationException thrown =
            Should.Throw<InvalidOperationException>(
                () => bound.MapAsync(
                    static (v, s) => Task.FromResult(v + s)));

        thrown.Message.ShouldContain("Build one by calling With");
    }

    /// <remarks>
    /// The other half of the same property, and the half a consumer is more
    /// likely to meet: a delegate that throws before it ever returns a
    /// <see cref="Task{TResult}" /> throws through the member, because nothing
    /// wraps the invocation. Pinned on a member that does await, so the
    /// eagerness cannot be mistaken for the short-circuit branch simply not
    /// calling the delegate.
    /// </remarks>
    [Fact]
    public void ADelegateThrowingBeforeItsTaskThrowsFromTheCall()
    {
        InvalidOperationException thrown =
            Should.Throw<InvalidOperationException>(
                () => SomeTwo.With(2)
                             .MapAsync<int>(
                                  static (_, _) =>
                                      throw new InvalidOperationException(
                                          "thrown before the task")));

        thrown.Message.ShouldBe("thrown before the task");
    }
}
