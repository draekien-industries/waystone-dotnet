namespace Waystone.Monads.Options;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Results;
#if !DEBUG
using System.Diagnostics;
#endif

/// <summary>An <see cref="Option{T}" /> holding a value.</summary>
/// <remarks>
/// One of the two cases of <see cref="Option{T}" />, so matching both is
/// exhaustive and no third case can be added from outside the library. Build
/// one with <see cref="Option.Some{T}" />. The value is never null, though it
/// may be the default of <typeparamref name="T" />.
/// </remarks>
/// <typeparam name="T">
/// The type belonging to the value inside the
/// <see cref="Some{T}" />
/// </typeparam>
#if !DEBUG
[DebuggerStepThrough]
#endif
public sealed record Some<T> : Option<T>
    where T : notnull
{
    internal Some(T value)
    {
        if (value is null)
        {
            throw new ArgumentNullException(
                nameof(value),
                "The value of a `Some` option cannot be null.");
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
    public override bool IsSomeAnd<TState>(
        TState state,
        Func<T, TState, bool> predicate) => predicate(Value, state);

    /// <inheritdoc />
    public override async ValueTask<bool> IsSomeAndAsync(
        Func<T, Task<bool>> predicate) =>
        await predicate(Value).ConfigureAwait(false);

    /// <inheritdoc />
    public override bool IsNoneOr(Func<T, bool> predicate) =>
        predicate(Value);

    /// <inheritdoc />
    public override bool IsNoneOr<TState>(
        TState state,
        Func<T, TState, bool> predicate) => predicate(Value, state);

    /// <inheritdoc />
    public override async ValueTask<bool> IsNoneOrAsync(
        Func<T, Task<bool>> predicate) =>
        await predicate(Value).ConfigureAwait(false);

    /// <inheritdoc />
    public override TOut Match<TOut>(
        Func<T, TOut> onSome,
        Func<TOut> onNone) => onSome(Value);

    /// <inheritdoc />
    public override TOut Match<TState, TOut>(
        TState state,
        Func<T, TState, TOut> onSome,
        Func<TState, TOut> onNone) => onSome(Value, state);

    /// <inheritdoc />
    public override void Match(Action<T> onSome, Action onNone)
    {
        onSome(Value);
    }

    /// <inheritdoc />
    public override void Match<TState>(
        TState state,
        Action<T, TState> onSome,
        Action<TState> onNone)
    {
        onSome(Value, state);
    }

    /// <inheritdoc />
    public override async ValueTask<TOut> MatchAsync<TOut>(
        Func<T, Task<TOut>> onSome,
        Func<Task<TOut>> onNone) =>
        await onSome(Value).ConfigureAwait(false);

    /// <inheritdoc />
    public override ValueTask<TOut> MatchAsync<TOut>(
        Func<T, TOut> onSome,
        Func<Task<TOut>> onNone) =>
        new ValueTask<TOut>(onSome(Value));

    /// <inheritdoc />
    public override async ValueTask<TOut> MatchAsync<TOut>(
        Func<T, Task<TOut>> onSome,
        Func<TOut> onNone) =>
        await onSome(Value).ConfigureAwait(false);

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
    public override T UnwrapOrElse(Func<T> valueFactory) =>
        Value;

    /// <inheritdoc />
    public override T UnwrapOrElse<TState>(TState state, Func<TState, T> valueFactory) =>
        Value;

    /// <inheritdoc />
    public override ValueTask<T> UnwrapOrElseAsync(Func<Task<T>> valueFactory) =>
        new ValueTask<T>(Value);

    /// <inheritdoc />
    public override Option<TOut> Map<TOut>(Func<T, TOut> map) =>
        Option.NoneIfNull(map(Value));

    /// <inheritdoc />
    public override Option<TOut> Map<TState, TOut>(
        TState state,
        Func<T, TState, TOut> map) =>
        Option.NoneIfNull(map(Value, state));

    /// <inheritdoc />
    public override async ValueTask<Option<TOut>> MapAsync<TOut>(
        Func<T, Task<TOut>> map) =>
        Option.Some(await map(Value).ConfigureAwait(false));

    /// <inheritdoc />
    public override Option<TOut> And<TOut>(Option<TOut> other) =>
        other;

    /// <inheritdoc />
    public override ValueTask<Option<TOut>> AndThenAsync<TOut>(
        Func<T, ValueTask<Option<TOut>>> optionFactory) =>
        optionFactory(Value);

    /// <inheritdoc />
    public override TOut MapOr<TOut>(TOut defaultValue, Func<T, TOut> map) =>
        map(Value);

    /// <inheritdoc />
    public override TOut MapOr<TState, TOut>(
        TState state,
        TOut defaultValue,
        Func<T, TState, TOut> map) => map(Value, state);

    /// <inheritdoc />
    public override async ValueTask<TOut> MapOrAsync<TOut>(
        TOut defaultValue,
        Func<T, Task<TOut>> map) =>
        await map(Value).ConfigureAwait(false);

    /// <inheritdoc />
    public override TOut MapOrDefault<TOut>(Func<T, TOut> map) =>
        map(Value);

    /// <inheritdoc />
    public override TOut MapOrDefault<TState, TOut>(
        TState state,
        Func<T, TState, TOut> map) => map(Value, state);

    /// <inheritdoc />
    public override async ValueTask<TOut> MapOrDefaultAsync<TOut>(
        Func<T, Task<TOut>> map) =>
        await map(Value).ConfigureAwait(false);

    /// <inheritdoc />
    public override TOut? MapOrNull<TOut>(Func<T, TOut> map) =>
        map(Value);

    /// <inheritdoc />
    public override async ValueTask<TOut?> MapOrNullAsync<TOut>(
        Func<T, Task<TOut>> map) =>
        await map(Value).ConfigureAwait(false);

    /// <inheritdoc />
    public override TOut MapOrElse<TOut>(
        Func<TOut> defaultFactory,
        Func<T, TOut> map) => Match(map, defaultFactory);

    /// <inheritdoc />
    public override TOut MapOrElse<TState, TOut>(
        TState state,
        Func<TState, TOut> defaultFactory,
        Func<T, TState, TOut> map) => map(Value, state);

    /// <inheritdoc />
    public override async ValueTask<TOut> MapOrElseAsync<TOut>(
        Func<Task<TOut>> defaultFactory,
        Func<T, Task<TOut>> map) =>
        await map(Value).ConfigureAwait(false);

    /// <inheritdoc />
    public override async ValueTask<TOut> MapOrElseAsync<TOut>(
        Func<TOut> defaultFactory,
        Func<T, Task<TOut>> map) =>
        await map(Value).ConfigureAwait(false);

    /// <inheritdoc />
    public override ValueTask<TOut> MapOrElseAsync<TOut>(
        Func<Task<TOut>> defaultFactory,
        Func<T, TOut> map) =>
        new ValueTask<TOut>(map(Value));

    /// <inheritdoc />
    public override Option<T> Inspect(Action<T> action)
    {
        action(Value);
        return this;
    }

    /// <inheritdoc />
    public override Option<T> Inspect<TState>(
        TState state,
        Action<T, TState> action)
    {
        action(Value, state);
        return this;
    }

    /// <inheritdoc />
    public override async ValueTask<Option<T>> InspectAsync(Func<T, Task> action)
    {
        await action(Value).ConfigureAwait(false);

        return this;
    }

    /// <inheritdoc />
    public override Option<T> Filter(Func<T, bool> predicate) =>
        predicate(Value) ? this : Option.None<T>();

    /// <inheritdoc />
    public override Option<T> Filter<TState>(
        TState state,
        Func<T, TState, bool> predicate) =>
        predicate(Value, state) ? this : Option.None<T>();

    /// <inheritdoc />
    public override async ValueTask<Option<T>> FilterAsync(
        Func<T, Task<bool>> predicate) =>
        await predicate(Value).ConfigureAwait(false)
            ? this
            : Option.None<T>();

    /// <inheritdoc />
    public override Option<T> Or(Option<T> other) =>
        this;

    /// <inheritdoc />
    public override Option<T> OrElse(Func<Option<T>> optionFactory) =>
        this;

    /// <inheritdoc />
    public override Option<T> OrElse<TState>(
        TState state,
        Func<TState, Option<T>> optionFactory) => this;

    /// <inheritdoc />
    public override ValueTask<Option<T>> OrElseAsync(
        Func<ValueTask<Option<T>>> optionFactory) =>
        new ValueTask<Option<T>>(this);

    /// <inheritdoc />
    public override Option<T> Xor(Option<T> other) =>
        other.IsSome ? Option.None<T>() : this;

    /// <inheritdoc />
    public override Option<(T, TOther)> Zip<TOther>(Option<TOther> other)
    {
        if (other is Some<TOther> otherSome)
        {
            return Option.Some((Value, otherSome.Value));
        }

        return Option.None<(T, TOther)>();
    }

    /// <inheritdoc />
    public override Option<TOut> ZipWith<TOther, TOut>(
        Option<TOther> other,
        Func<T, TOther, TOut> zip) =>
        other.Map(otherValue => zip(Value, otherValue));

    /// <inheritdoc />
    public override ValueTask<Option<TOut>> ZipWithAsync<TOther, TOut>(
        Option<TOther> other,
        Func<T, TOther, Task<TOut>> zip) =>
        other.MapAsync(otherValue => zip(Value, otherValue));

    /// <inheritdoc />
    public override Option<T> Reduce(Option<T> other, Func<T, T, T> reduce) =>
        other.Match<Option<T>>(
            otherValue => Option.NoneIfNull(reduce(Value, otherValue)),
            () => this);

    /// <inheritdoc />
    public override async ValueTask<Option<T>> ReduceAsync(
        Option<T> other,
        Func<T, T, Task<T>> reduce)
    {
        if (other is not Some<T> otherSome) return this;

        return Option.NoneIfNull(
            await reduce(Value, otherSome.Value).ConfigureAwait(false));
    }

    /// <inheritdoc />
    public override IEnumerable<T> AsEnumerable() =>
        new[] { Value };

    /// <inheritdoc />
    public override Result<T, TErr> OkOr<TErr>(TErr error) =>
        Result.Ok<T, TErr>(Value);

    /// <inheritdoc />
    public override Result<T, TErr> OkOrElse<TErr>(Func<TErr> errorFactory) =>
        Result.Ok<T, TErr>(Value);

    /// <inheritdoc />
    public override Result<T, TErr> OkOrElse<TState, TErr>(
        TState state,
        Func<TState, TErr> errorFactory) => Result.Ok<T, TErr>(Value);

    /// <inheritdoc />
    public override ValueTask<Result<T, TErr>> OkOrElseAsync<TErr>(
        Func<Task<TErr>> errorFactory) =>
        new ValueTask<Result<T, TErr>>(Result.Ok<T, TErr>(Value));


    internal override void OnlyThisAssemblyMayDerive()
    { }
}
