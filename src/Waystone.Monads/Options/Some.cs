namespace Waystone.Monads.Options;

using System;
using System.Threading.Tasks;

/// <summary>Some value of type <typeparamref name="T" /></summary>
/// <typeparam name="T">
/// The type belonging to the value inside the
/// <see cref="Some{T}" />
/// </typeparam>
public sealed record Some<T> : Option<T>
    where T : notnull
{
    internal Some(T value)
    {
        if (Equals(value, default(T)))
        {
            throw new InvalidOperationException(
                $"The value of a `Some` option cannot be it's default value `{default(T)}`");
        }

        Value = value;
    }

    private T Value { get; }

    /// <inheritdoc />
    public override bool IsSome => true;

    /// <inheritdoc />
    public override bool IsNone => false;

    /// <inheritdoc />
    public override bool IsSomeAnd(Func<T, bool> predicate) =>
        predicate(Value);

    /// <inheritdoc />
    public override bool IsNoneOr(Func<T, bool> predicate) =>
        predicate(Value);

    /// <inheritdoc />
    public override async Task<bool> IsNoneOr(Func<T, Task<bool>> predicate) =>
        await predicate(Value);

    /// <inheritdoc />
    public override async ValueTask<bool> IsNoneOr(
        Func<T, ValueTask<bool>> predicate) => await predicate(Value);

    /// <inheritdoc />
    public override TOut Match<TOut>(
        Func<T, TOut> onSome,
        Func<TOut> onNone) => onSome(Value);

    /// <inheritdoc />
    public override void Match(Action<T> onSome, Action onNone)
    {
        onSome(Value);
    }

    /// <inheritdoc />
    public override T Expect(string message) =>
        Value;

    /// <inheritdoc />
    public override T Unwrap() => Value;

    /// <inheritdoc />
    public override T UnwrapOr(T value) =>
        Value;

    /// <inheritdoc />
    public override T UnwrapOrDefault() =>
        Value;

    /// <inheritdoc />
    public override T UnwrapOrElse(Func<T> @else) =>
        Value;

    /// <inheritdoc />
    public override Task<T> UnwrapOrElse(Func<Task<T>> @else) =>
        Task.FromResult(Value);

    /// <inheritdoc />
    public override ValueTask<T> UnwrapOrElse(Func<ValueTask<T>> @else) =>
        new(Value);

    /// <inheritdoc />
    public override Option<T2> Map<T2>(Func<T, T2> map) =>
        map(Value);

    /// <inheritdoc />
    public override async Task<Option<T2>> Map<T2>(Func<T, Task<T2>> map) =>
        await map(Value);

    /// <inheritdoc />
    public override async ValueTask<Option<T2>> Map<T2>(
        Func<T, ValueTask<T2>> map) => await map(Value);

    /// <inheritdoc />
    public override T2 MapOr<T2>(T2 @default, Func<T, T2> map) =>
        map(Value);

    /// <inheritdoc />
    public override async Task<T2>
        MapOr<T2>(T2 @default, Func<T, Task<T2>> map) => await map(Value);

    /// <inheritdoc />
    public override async ValueTask<T2> MapOr<T2>(
        T2 @default,
        Func<T, ValueTask<T2>> map) => await map(Value);

    /// <inheritdoc />
    public override T2 MapOrElse<T2>(
        Func<T2> createDefault,
        Func<T, T2> map) => Match(map, createDefault);

    /// <inheritdoc />
    public override async Task<T2> MapOrElse<T2>(
        Func<Task<T2>> createDefault,
        Func<T, Task<T2>> map) => await map(Value);

    /// <inheritdoc />
    public override async ValueTask<T2> MapOrElse<T2>(
        Func<ValueTask<T2>> createDefault,
        Func<T, ValueTask<T2>> map) => await map(Value);

    /// <inheritdoc />
    public override Option<T> Inspect(Action<T> action)
    {
        action(Value);
        return this;
    }

    /// <inheritdoc />
    public override async Task<Option<T>> Inspect(Func<T, Task> action)
    {
        await action(Value);
        return this;
    }


    /// <inheritdoc />
    public override async ValueTask<Option<T>> Inspect(
        Func<T, ValueTask> action)
    {
        await action(Value);
        return this;
    }

    /// <inheritdoc />
    public override Option<T> Filter(Func<T, bool> predicate) =>
        predicate(Value) ? this : Option.None<T>();

    /// <inheritdoc />
    public override async Task<Option<T>>
        Filter(Func<T, Task<bool>> predicate) =>
        await predicate(Value) ? this : Option.None<T>();

    /// <inheritdoc />
    public override async ValueTask<Option<T>> Filter(
        Func<T, ValueTask<bool>> predicate) =>
        await predicate(Value) ? this : Option.None<T>();

    /// <inheritdoc />
    public override Option<T> Or(Option<T> other) =>
        this;

    /// <inheritdoc />
    public override Option<T> OrElse(Func<Option<T>> createElse) =>
        this;

    /// <inheritdoc />
    public override Task<Option<T>> OrElse(
        Func<Task<Option<T>>> createElse) => Task.FromResult<Option<T>>(this);

    /// <inheritdoc />
    public override ValueTask<Option<T>> OrElse(
        Func<ValueTask<Option<T>>> createElse) =>
        new(this);

    /// <inheritdoc />
    public override Option<T> Xor(Option<T> other) =>
        other.IsSome ? Option.None<T>() : this;

    /// <inheritdoc />
    public override Option<(T, T2)> Zip<T2>(Option<T2> other)
    {
        if (other is Some<T2> otherSome)
        {
            return Option.Some((Value, otherSome.Value));
        }

        return Option.None<(T, T2)>();
    }
}
