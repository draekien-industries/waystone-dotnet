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

    private static readonly Action<int> Record = _ => { };

    private static readonly Action NoOp = () => { };

    private static readonly Action<int, int> RecordWithState = (_, _) => { };

    private static readonly Func<Option<int>> SomeTen = () => Option.Some(10);

    private static readonly Func<int, Option<int>> SomeState = state =>
        Option.Some(state);

    private static readonly Func<string> Boom = () => "boom";

    private static readonly Func<string, string> EchoError = state => state;

    private static readonly Func<string, bool> IsBoom = error => error == "boom";

    private static readonly Func<string, int, bool> ErrorMatches =
        (error, state) => error.Length == state;

    private static readonly Action<string> RecordError = _ => { };

    private static readonly Action<string, int> RecordErrorWithState =
        (_, _) => { };

    private static readonly Func<string, Result<int, int>> ErrZero = _ =>
        Result.Err<int, int>(0);

    private static readonly Func<string, int, Result<int, int>> ErrState =
        (_, state) => Result.Err<int, int>(state);

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

        some.IsSomeAnd(IsOne).ShouldBeTrue();
        some.IsSomeAnd(1, Matches).ShouldBeTrue();

        some.IsNoneOr(IsOne).ShouldBeTrue();
        some.IsNoneOr(1, Matches).ShouldBeTrue();

        some.MapOrDefault(Increment).ShouldBe(2);
        some.MapOrDefault(10, Add).ShouldBe(11);

        some.Inspect(Record).ShouldBe(some);
        some.Inspect(10, RecordWithState).ShouldBe(some);

        some.UnwrapOrElse(Ten).ShouldBe(1);
        some.UnwrapOrElse(10, Identity).ShouldBe(1);

        some.OrElse(SomeTen).ShouldBe(some);
        some.OrElse(10, SomeState).ShouldBe(some);

        some.OkOrElse(Boom).ShouldBe(Result.Ok<int, string>(1));
        some.OkOrElse("boom", EchoError).ShouldBe(Result.Ok<int, string>(1));
    }

    /// <remarks>
    /// Match is the one member with two overloads of the same arity on each
    /// side of the state split: the Func pair and the Action pair both go from
    /// two required arguments to three. Only the delegate types separate them,
    /// and Record binds as the non-state onSome and as the state onNone, which
    /// is the shape that would go wrong first.
    /// </remarks>
    [Fact]
    public void StoredDelegatesBindToTheIntendedMatchOverload()
    {
        Option<int> some = Option.Some(1);

        some.Match(Increment, Ten).ShouldBe(2);
        some.Match(10, Add, Identity).ShouldBe(11);

        some.Match(Record, NoOp);
        some.Match(10, RecordWithState, Record);
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

        ok.IsOkAnd(IsOne).ShouldBeTrue();
        ok.IsOkAnd(1, Matches).ShouldBeTrue();

        ok.IsErrAnd(IsBoom).ShouldBeFalse();
        ok.IsErrAnd(4, ErrorMatches).ShouldBeFalse();

        ok.MapOrDefault(Increment).ShouldBe(2);
        ok.MapOrDefault(10, Add).ShouldBe(11);

        ok.Inspect(Record).ShouldBe(ok);
        ok.Inspect(10, RecordWithState).ShouldBe(ok);

        ok.InspectErr(RecordError).ShouldBe(ok);
        ok.InspectErr(10, RecordErrorWithState).ShouldBe(ok);

        ok.UnwrapOrElse(Zero).ShouldBe(1);
        ok.UnwrapOrElse(10, ErrorState).ShouldBe(1);

        ok.OrElse(ErrZero).ShouldBe(Result.Ok<int, int>(1));
        ok.OrElse(10, ErrState).ShouldBe(Result.Ok<int, int>(1));
    }

    /// <remarks>
    /// The same trap as the Option Match pair, one type argument wider: the
    /// Func and Action forms of the state overload both take three arguments,
    /// and only the delegate types separate them. Record binds as the non-state
    /// onOk and as the state onOk, so passing it either way has to still reach
    /// the Action form.
    /// </remarks>
    [Fact]
    public void StoredDelegatesBindToTheIntendedResultMatchOverload()
    {
        Result<int, string> ok = Result.Ok<int, string>(1);

        ok.Match(Increment, Zero).ShouldBe(2);
        ok.Match(10, Add, ErrorState).ShouldBe(11);

        ok.Match(Record, RecordError);
        ok.Match(10, RecordWithState, RecordErrorWithState);
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

        Result.Try(Ten).ShouldBeOk();
        Result.Try(10, Identity).ShouldBeOk();
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

        await Result.TryAsync(TenAsync).ShouldBeOkAsync();
        await Result.TryAsync(10, IdentityAsync).ShouldBeOkAsync();
    }
}
