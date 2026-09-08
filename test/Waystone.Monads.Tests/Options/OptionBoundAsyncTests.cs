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
    /// The sync members throw from the call itself; an async one returns a
    /// faulted task instead, so the same misuse surfaces at the await. Pinned
    /// because the message is the only thing naming the cause, and it would be
    /// easy to lose it behind a <see cref="NullReferenceException" />.
    /// </remarks>
    [Fact]
    public async Task ADefaultBoundFaultsRatherThanDereferencingNothing()
    {
        Option<int>.Bound<int> bound = default;

        InvalidOperationException thrown =
            await Should.ThrowAsync<InvalidOperationException>(
                async () => await bound.MapAsync(
                    static (v, s) => Task.FromResult(v + s)));

        thrown.Message.ShouldContain("Build one by calling With");
    }
}
