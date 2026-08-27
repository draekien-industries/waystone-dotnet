namespace Waystone.Monads.Options;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Exceptions;
using Results;
#if !DEBUG
using System.Diagnostics;
#endif

/// <summary>An <see cref="Option{T}" /> holding no value.</summary>
/// <remarks>
/// One of the two cases of <see cref="Option{T}" />, so matching both is
/// exhaustive and no third case can be added from outside the library. Build
/// one with <see cref="Option.None{T}" />, which hands back a cached instance
/// rather than constructing one.
/// </remarks>
/// <typeparam name="T">The option value's type.</typeparam>
#if !DEBUG
[DebuggerStepThrough]
#endif
public sealed record None<T> : Option<T>
    where T : notnull
{
    internal static readonly None<T> Instance = new();

    /// <inheritdoc />
    public override bool IsSome => false;

    /// <inheritdoc />
    public override bool IsNone => true;

    /// <inheritdoc />
    public override bool IsSomeAnd(Func<T, bool> predicate) =>
        false;

    /// <inheritdoc />
    public override bool IsSomeAnd<TState>(
        TState state,
        Func<T, TState, bool> predicate) => false;

    /// <inheritdoc />
    public override ValueTask<bool> IsSomeAndAsync(
        Func<T, Task<bool>> predicate) =>
        new ValueTask<bool>(false);

    /// <inheritdoc />
    public override bool IsNoneOr(Func<T, bool> predicate) =>
        true;

    /// <inheritdoc />
    public override bool IsNoneOr<TState>(
        TState state,
        Func<T, TState, bool> predicate) => true;

    /// <inheritdoc />
    public override ValueTask<bool> IsNoneOrAsync(
        Func<T, Task<bool>> predicate) =>
        new ValueTask<bool>(true);

    /// <inheritdoc />
    public override TOut Match<TOut>(
        Func<T, TOut> onSome,
        Func<TOut> onNone) => onNone();

    /// <inheritdoc />
    public override TOut Match<TState, TOut>(
        TState state,
        Func<T, TState, TOut> onSome,
        Func<TState, TOut> onNone) => onNone(state);

    /// <inheritdoc />
    public override void Match(Action<T> onSome, Action onNone)
    {
        onNone();
    }

    /// <inheritdoc />
    public override void Match<TState>(
        TState state,
        Action<T, TState> onSome,
        Action<TState> onNone)
    {
        onNone(state);
    }

    /// <inheritdoc />
    public override async ValueTask<TOut> MatchAsync<TOut>(
        Func<T, Task<TOut>> onSome,
        Func<Task<TOut>> onNone) =>
        await onNone().ConfigureAwait(false);

    /// <inheritdoc />
    public override async ValueTask<TOut> MatchAsync<TOut>(
        Func<T, TOut> onSome,
        Func<Task<TOut>> onNone) =>
        await onNone().ConfigureAwait(false);

    /// <inheritdoc />
    public override ValueTask<TOut> MatchAsync<TOut>(
        Func<T, Task<TOut>> onSome,
        Func<TOut> onNone) =>
        new ValueTask<TOut>(onNone());

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
    public override T UnwrapOrElse(Func<T> valueFactory) =>
        valueFactory();

    /// <inheritdoc />
    public override T UnwrapOrElse<TState>(TState state, Func<TState, T> valueFactory) =>
        valueFactory(state);

    /// <inheritdoc />
    public override ValueTask<T> UnwrapOrElseAsync(Func<Task<T>> valueFactory) =>
        new ValueTask<T>(valueFactory());

    /// <inheritdoc />
    public override Option<TOut> Map<TOut>(Func<T, TOut> map) =>
        Option.None<TOut>();

    /// <inheritdoc />
    public override Option<TOut> Map<TState, TOut>(
        TState state,
        Func<T, TState, TOut> map) => Option.None<TOut>();

    /// <inheritdoc />
    public override ValueTask<Option<TOut>> MapAsync<TOut>(
        Func<T, Task<TOut>> map) =>
        new ValueTask<Option<TOut>>(Option.None<TOut>());

    /// <inheritdoc />
    public override Option<TOut> And<TOut>(Option<TOut> other) =>
        Option.None<TOut>();

    /// <inheritdoc />
    public override ValueTask<Option<TOut>> AndThenAsync<TOut>(
        Func<T, ValueTask<Option<TOut>>> optionFactory) =>
        new ValueTask<Option<TOut>>(Option.None<TOut>());

    /// <inheritdoc />
    public override TOut MapOr<TOut>(TOut defaultValue, Func<T, TOut> map) =>
        defaultValue;

    /// <inheritdoc />
    public override TOut MapOr<TState, TOut>(
        TState state,
        TOut defaultValue,
        Func<T, TState, TOut> map) => defaultValue;

    /// <inheritdoc />
    public override ValueTask<TOut> MapOrAsync<TOut>(
        TOut defaultValue,
        Func<T, Task<TOut>> map) =>
        new ValueTask<TOut>(defaultValue);

    /// <inheritdoc />
    public override TOut MapOrDefault<TOut>(Func<T, TOut> map) =>
        default!;

    /// <inheritdoc />
    public override TOut MapOrDefault<TState, TOut>(
        TState state,
        Func<T, TState, TOut> map) => default!;

    /// <inheritdoc />
    public override ValueTask<TOut> MapOrDefaultAsync<TOut>(
        Func<T, Task<TOut>> map) =>
        new ValueTask<TOut>(default(TOut)!);

    /// <inheritdoc />
    public override TOut? MapOrNull<TOut>(Func<T, TOut> map) => null;

    /// <inheritdoc />
    public override ValueTask<TOut?> MapOrNullAsync<TOut>(
        Func<T, Task<TOut>> map) =>
        new ValueTask<TOut?>(default(TOut?));

    /// <inheritdoc />
    public override TOut MapOrElse<TOut>(
        Func<TOut> defaultFactory,
        Func<T, TOut> map) => defaultFactory();

    /// <inheritdoc />
    public override TOut MapOrElse<TState, TOut>(
        TState state,
        Func<TState, TOut> defaultFactory,
        Func<T, TState, TOut> map) => defaultFactory(state);

    /// <inheritdoc />
    public override async ValueTask<TOut> MapOrElseAsync<TOut>(
        Func<Task<TOut>> defaultFactory,
        Func<T, Task<TOut>> map) =>
        await defaultFactory().ConfigureAwait(false);

    /// <inheritdoc />
    public override ValueTask<TOut> MapOrElseAsync<TOut>(
        Func<TOut> defaultFactory,
        Func<T, Task<TOut>> map) =>
        new ValueTask<TOut>(defaultFactory());

    /// <inheritdoc />
    public override async ValueTask<TOut> MapOrElseAsync<TOut>(
        Func<Task<TOut>> defaultFactory,
        Func<T, TOut> map) =>
        await defaultFactory().ConfigureAwait(false);

    /// <inheritdoc />
    public override Option<T> Inspect(Action<T> action) =>
        this;

    /// <inheritdoc />
    public override Option<T> Inspect<TState>(
        TState state,
        Action<T, TState> action) => this;

    /// <inheritdoc />
    public override ValueTask<Option<T>> InspectAsync(Func<T, Task> action) =>
        new ValueTask<Option<T>>(this);

    /// <inheritdoc />
    public override Option<T> Filter(Func<T, bool> predicate) =>
        this;

    /// <inheritdoc />
    public override Option<T> Filter<TState>(
        TState state,
        Func<T, TState, bool> predicate) => this;

    /// <inheritdoc />
    public override ValueTask<Option<T>> FilterAsync(
        Func<T, Task<bool>> predicate) =>
        new ValueTask<Option<T>>(this);

    /// <inheritdoc />
    public override Option<T> Or(Option<T> other) =>
        other;

    /// <inheritdoc />
    public override Option<T> OrElse(Func<Option<T>> optionFactory) =>
        optionFactory();

    /// <inheritdoc />
    public override Option<T> OrElse<TState>(
        TState state,
        Func<TState, Option<T>> optionFactory) => optionFactory(state);

    /// <inheritdoc />
    public override ValueTask<Option<T>> OrElseAsync(
        Func<ValueTask<Option<T>>> optionFactory) =>
        optionFactory();

    /// <inheritdoc />
    public override Option<T> Xor(Option<T> other) =>
        other.IsSome ? other : this;

    /// <inheritdoc />
    public override Option<(T, TOther)> Zip<TOther>(Option<TOther> other) =>
        Option.None<(T, TOther)>();

    /// <inheritdoc />
    public override Option<TOut> ZipWith<TOther, TOut>(
        Option<TOther> other,
        Func<T, TOther, TOut> zip) =>
        Option.None<TOut>();

    /// <inheritdoc />
    public override ValueTask<Option<TOut>> ZipWithAsync<TOther, TOut>(
        Option<TOther> other,
        Func<T, TOther, Task<TOut>> zip) =>
        new ValueTask<Option<TOut>>(Option.None<TOut>());

    /// <inheritdoc />
    public override Option<T> Reduce(Option<T> other, Func<T, T, T> reduce) =>
        other;

    /// <inheritdoc />
    public override ValueTask<Option<T>> ReduceAsync(
        Option<T> other,
        Func<T, T, Task<T>> reduce) =>
        new ValueTask<Option<T>>(other);

    /// <inheritdoc />
    public override IEnumerable<T> AsEnumerable() =>
        Array.Empty<T>();

    /// <inheritdoc />
    public override Result<T, TErr> OkOr<TErr>(TErr error) =>
        Result.Err<T, TErr>(error);

    /// <inheritdoc />
    public override Result<T, TErr> OkOrElse<TErr>(Func<TErr> errorFactory) =>
        Result.Err<T, TErr>(errorFactory());

    /// <inheritdoc />
    public override Result<T, TErr> OkOrElse<TState, TErr>(
        TState state,
        Func<TState, TErr> errorFactory) =>
        Result.Err<T, TErr>(errorFactory(state));

    /// <inheritdoc />
    public override async ValueTask<Result<T, TErr>> OkOrElseAsync<TErr>(
        Func<Task<TErr>> errorFactory) =>
        Result.Err<T, TErr>(await errorFactory().ConfigureAwait(false));


    internal override void OnlyThisAssemblyMayDerive()
    { }
}
