namespace Waystone.Monads.Options;

using System;
using System.Threading.Tasks;
using Exceptions;

/// <summary>No value of type <typeparamref name="T" />.</summary>
/// <typeparam name="T">The option value's type.</typeparam>
public sealed record None<T> : Option<T>
    where T : notnull
{
    /// <inheritdoc />
    public override bool IsSome => false;

    /// <inheritdoc />
    public override bool IsNone => true;

    /// <inheritdoc />
    public override bool IsSomeAnd(Func<T, bool> predicate) =>
        false;

    /// <inheritdoc />
    public override Task<bool> IsSomeAnd(Func<T, Task<bool>> predicate) =>
        Task.FromResult(false);

    /// <inheritdoc />
    public override ValueTask<bool> IsSomeAnd(
        Func<T, ValueTask<bool>> predicate) => ValueTask.FromResult(false);

    /// <inheritdoc />
    public override bool IsNoneOr(Func<T, bool> predicate) =>
        true;

    /// <inheritdoc />
    public override Task<bool> IsNoneOr(Func<T, Task<bool>> predicate) =>
        Task.FromResult(true);

    /// <inheritdoc />
    public override ValueTask<bool>
        IsNoneOr(Func<T, ValueTask<bool>> predicate) =>
        ValueTask.FromResult(true);

    /// <inheritdoc />
    public override TOut Match<TOut>(
        Func<T, TOut> onSome,
        Func<TOut> onNone) => onNone();

    /// <inheritdoc />
    public override async Task<TOut> Match<TOut>(
        Func<T, Task<TOut>> onSome,
        Func<Task<TOut>> onNone) => await onNone();

    /// <inheritdoc />
    public override async ValueTask<TOut> Match<TOut>(
        Func<T, ValueTask<TOut>> onSome,
        Func<ValueTask<TOut>> onNone) => await onNone();

    /// <inheritdoc />
    public override void Match(Action<T> onSome, Action onNone)
    {
        onNone();
    }

    /// <inheritdoc />
    public override T Expect(string message) =>
        throw new UnmetExpectationException(message);

    /// <inheritdoc />
    public override T Unwrap() =>
        throw new UnwrapException("Unwrap called for a `None` value.");

    /// <inheritdoc />
    public override T UnwrapOr(T value) =>
        value;

    /// <inheritdoc />
    public override T? UnwrapOrDefault() =>
        default;

    /// <inheritdoc />
    public override T UnwrapOrElse(Func<T> @else) =>
        @else();

    /// <inheritdoc />
    public override async Task<T> UnwrapOrElse(Func<Task<T>> @else) =>
        await @else();

    /// <inheritdoc />
    public override async ValueTask<T> UnwrapOrElse(Func<ValueTask<T>> @else) =>
        await @else();

    /// <inheritdoc />
    public override Option<T2> Map<T2>(Func<T, T2> map) =>
        Option.None<T2>();

    /// <inheritdoc />
    public override Task<Option<T2>> Map<T2>(Func<T, Task<T2>> map) =>
        Task.FromResult(Option.None<T2>());

    /// <inheritdoc />
    public override ValueTask<Option<T2>> Map<T2>(Func<T, ValueTask<T2>> map) =>
        ValueTask.FromResult(Option.None<T2>());

    /// <inheritdoc />
    public override T2 MapOr<T2>(T2 @default, Func<T, T2> map) =>
        @default;

    /// <inheritdoc />
    public override Task<T2>
        MapOr<T2>(T2 @default, Func<T, Task<T2>> map) =>
        Task.FromResult(@default);

    /// <inheritdoc />
    public override ValueTask<T2> MapOr<T2>(
        T2 @default,
        Func<T, ValueTask<T2>> map) => ValueTask.FromResult(@default);

    /// <inheritdoc />
    public override T2 MapOrElse<T2>(
        Func<T2> createDefault,
        Func<T, T2> map) => createDefault();

    /// <inheritdoc />
    public override async Task<T2> MapOrElse<T2>(
        Func<Task<T2>> createDefault,
        Func<T, Task<T2>> map) => await createDefault();

    /// <inheritdoc />
    public override async ValueTask<T2> MapOrElse<T2>(
        Func<ValueTask<T2>> createDefault,
        Func<T, ValueTask<T2>> map) => await createDefault();

    /// <inheritdoc />
    public override Option<T> Inspect(Action<T> action) =>
        this;

    /// <inheritdoc />
    public override Task<Option<T>> Inspect(Func<T, Task> action) =>
        Task.FromResult<Option<T>>(this);

    /// <inheritdoc />
    public override ValueTask<Option<T>> Inspect(Func<T, ValueTask> action) =>
        ValueTask.FromResult<Option<T>>(this);

    /// <inheritdoc />
    public override Option<T> Filter(Func<T, bool> predicate) =>
        this;

    /// <inheritdoc />
    public override Task<Option<T>> Filter(Func<T, Task<bool>> predicate) =>
        Task.FromResult<Option<T>>(this);

    /// <inheritdoc />
    public override ValueTask<Option<T>> Filter(
        Func<T, ValueTask<bool>> predicate) =>
        ValueTask.FromResult<Option<T>>(this);

    /// <inheritdoc />
    public override Option<T> Or(Option<T> other) =>
        other;

    /// <inheritdoc />
    public override Option<T> OrElse(Func<Option<T>> createElse) =>
        createElse();

    /// <inheritdoc />
    public override async Task<Option<T>> OrElse(
        Func<Task<Option<T>>> createElse) => await createElse();

    /// <inheritdoc />
    public override async ValueTask<Option<T>> OrElse(
        Func<ValueTask<Option<T>>> createElse) => await createElse();

    /// <inheritdoc />
    public override Option<T> Xor(Option<T> other) =>
        other.IsSome ? other : this;

    /// <inheritdoc />
    public override Option<(T, T2)> Zip<T2>(Option<T2> other) =>
        Option.None<(T, T2)>();
}
