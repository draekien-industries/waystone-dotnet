namespace Waystone.Monads;

using Options;
using Results;
using Shouldly;
using System;
using System.Threading.Tasks;
using Xunit;

/// <remarks>
/// Guards the constraint that a state overload adds the state parameter rather
/// than reusing an existing slot. Every case here passes a stored delegate
/// rather than a lambda, because a lambda lets the compiler disambiguate on
/// inferred arity and would hide a collision that a method group or a
/// <c>Func</c> variable walks straight into.
/// </remarks>
public sealed class StateOverloadResolutionTests
{
    private static readonly Func<int, int> Increment = x => x + 1;

    private static readonly Func<int, int, int> Add = (x, state) => x + state;

    private static readonly Func<int, bool> IsOne = x => x == 1;

    private static readonly Func<int, int, bool> Matches = (x, state) =>
        x == state;

    private static readonly Func<int> Ten = () => 10;

    private static readonly Func<int, int> Identity = state => state;

    private static readonly Func<string, int> Zero = _ => 0;

    private static readonly Func<string, int, int> ErrorState =
        (_, state) => state;

    private static readonly Func<int, Option<int>> SomeIncrement = x =>
        Option.Some(x + 1);

    private static readonly Func<int, int, Option<int>> SomeAdd = (x, state) =>
        Option.Some(x + state);

    private static readonly Func<int, Result<int, string>> OkIncrement = x =>
        Result.Ok<int, string>(x + 1);

    private static readonly Func<int, int, Result<int, string>> OkAdd =
        (x, state) => Result.Ok<int, string>(x + state);

    private static readonly Func<Exception, int> OnError = _ => -1;

    private static readonly Func<Task<int>> TenAsync = () => Task.FromResult(10);

    private static readonly Func<int, Task<int>> IdentityAsync = state =>
        Task.FromResult(state);

    [Fact]
    public void StoredDelegatesBindToTheIntendedOptionOverload()
    {
        Option<int> some = Option.Some(1);

        some.Map(Increment).ShouldBe(Option.Some(2));
        some.Map(10, Add).ShouldBe(Option.Some(11));

        some.MapOr(100, Increment).ShouldBe(2);
        some.MapOr(10, 100, Add).ShouldBe(11);

        some.MapOrElse(Ten, Increment).ShouldBe(2);
        some.MapOrElse(10, Identity, Add).ShouldBe(11);

        some.Filter(IsOne).ShouldBe(some);
        some.Filter(1, Matches).ShouldBe(some);

        some.AndThen(SomeIncrement).ShouldBe(Option.Some(2));
        some.AndThen(10, SomeAdd).ShouldBe(Option.Some(11));
    }

    [Fact]
    public void StoredDelegatesBindToTheIntendedResultOverload()
    {
        Result<int, string> ok = Result.Ok<int, string>(1);

        ok.Map(Increment).ShouldBe(Result.Ok<int, string>(2));
        ok.Map(10, Add).ShouldBe(Result.Ok<int, string>(11));

        ok.MapOr(100, Increment).ShouldBe(2);
        ok.MapOr(10, 100, Add).ShouldBe(11);

        ok.MapOrElse(Zero, Increment).ShouldBe(2);
        ok.MapOrElse(10, ErrorState, Add).ShouldBe(11);

        ok.MapErr(Zero).ShouldBe(Result.Ok<int, int>(1));
        ok.MapErr(10, ErrorState).ShouldBe(Result.Ok<int, int>(1));

        ok.AndThen(OkIncrement).ShouldBe(Result.Ok<int, string>(2));
        ok.AndThen(10, OkAdd).ShouldBe(Result.Ok<int, string>(11));
    }

    /// <remarks>
    /// The factory pairs are the one place where arity does not separate the
    /// overloads on its own: <c>Result.Try(factory, onError)</c> and
    /// <c>Result.Try(state, factory)</c> both take two required arguments, and
    /// only inference tells them apart. A stored delegate is the shape that
    /// would collide.
    /// </remarks>
    [Fact]
    public void StoredDelegatesBindToTheIntendedFactoryOverload()
    {
        Option.Try(Ten).ShouldBe(Option.Some(10));
        Option.Try(10, Identity).ShouldBe(Option.Some(10));

        Result.Try(Ten, OnError).ShouldBe(Result.Ok<int, int>(10));
        Result.Try(10, Identity, OnError).ShouldBe(Result.Ok<int, int>(10));

        Result.Try(Ten).IsOk.ShouldBeTrue();
        Result.Try(10, Identity).IsOk.ShouldBeTrue();
    }

    [Fact]
    public async Task StoredDelegatesBindToTheIntendedAsyncFactoryOverload()
    {
        (await Option.TryAsync(TenAsync)).ShouldBe(Option.Some(10));
        (await Option.TryAsync(10, IdentityAsync)).ShouldBe(Option.Some(10));

        (await Result.TryAsync(TenAsync, OnError))
           .ShouldBe(Result.Ok<int, int>(10));

        (await Result.TryAsync(10, IdentityAsync, OnError))
           .ShouldBe(Result.Ok<int, int>(10));

        (await Result.TryAsync(TenAsync)).IsOk.ShouldBeTrue();
        (await Result.TryAsync(10, IdentityAsync)).IsOk.ShouldBeTrue();
    }
}
