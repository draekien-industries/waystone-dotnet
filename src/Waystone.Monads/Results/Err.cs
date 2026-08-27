namespace Waystone.Monads.Results;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Exceptions;
using Options;
#if !DEBUG
using System.Diagnostics;
#endif

/// <summary>A <see cref="Result{TOk,TErr}" /> holding an error.</summary>
/// <remarks>
/// One of the two cases of <see cref="Result{TOk,TErr}" />, so matching both
/// is exhaustive and no third case can be added from outside the library.
/// Build one with <see cref="Result.Err{TOk,TErr}" />. The error is never
/// null, though it may be the default of <typeparamref name="TErr" />.
/// </remarks>
/// <typeparam name="TOk">The ok result value's type</typeparam>
/// <typeparam name="TErr">The error result value's type</typeparam>
#if !DEBUG
[DebuggerStepThrough]
#endif
public sealed record Err<TOk, TErr> : Result<TOk, TErr>
    where TOk : notnull
    where TErr : notnull
{
    internal Err(TErr value)
    {
        if (value is null)
        {
            throw new ArgumentNullException(
                nameof(value),
                "The value of an `Err` result cannot be null.");
        }

        Value = value;
    }

    internal TErr Value { get; }

    /// <inheritdoc />
    public override bool IsOk => false;

    /// <inheritdoc />
    public override bool IsErr => true;

    /// <inheritdoc />
    public override bool IsOkAnd(Func<TOk, bool> predicate) => false;

    /// <inheritdoc />
    public override bool IsOkAnd<TState>(
        TState state,
        Func<TOk, TState, bool> predicate) => false;

    /// <inheritdoc />
    public override ValueTask<bool> IsOkAndAsync(
        Func<TOk, Task<bool>> predicate) => new ValueTask<bool>(false);

    /// <inheritdoc />
    public override bool IsErrAnd(Func<TErr, bool> predicate) =>
        predicate(Value);

    /// <inheritdoc />
    public override bool IsErrAnd<TState>(
        TState state,
        Func<TErr, TState, bool> predicate) => predicate(Value, state);

    /// <inheritdoc />
    public override async ValueTask<bool> IsErrAndAsync(
        Func<TErr, Task<bool>> predicate) =>
        await predicate(Value).ConfigureAwait(false);

    /// <inheritdoc />
    public override TOut Match<TOut>(
        Func<TOk, TOut> onOk,
        Func<TErr, TOut> onErr) =>
        onErr(Value);

    /// <inheritdoc />
    public override TOut Match<TState, TOut>(
        TState state,
        Func<TOk, TState, TOut> onOk,
        Func<TErr, TState, TOut> onErr) =>
        onErr(Value, state);

    /// <inheritdoc />
    public override void Match(Action<TOk> onOk, Action<TErr> onErr)
    {
        onErr(Value);
    }

    /// <inheritdoc />
    public override void Match<TState>(
        TState state,
        Action<TOk, TState> onOk,
        Action<TErr, TState> onErr)
    {
        onErr(Value, state);
    }

    /// <inheritdoc />
    public override async ValueTask<TOut> MatchAsync<TOut>(
        Func<TOk, Task<TOut>> onOk,
        Func<TErr, Task<TOut>> onErr) =>
        await onErr(Value).ConfigureAwait(false);

    /// <inheritdoc />
    public override async ValueTask MatchAsync(
        Func<TOk, Task> onOk,
        Func<TErr, Task> onErr) =>
        await onErr(Value).ConfigureAwait(false);

    /// <inheritdoc />
    public override ValueTask MatchAsync(
        Func<TOk, Task> onOk,
        Action<TErr> onErr)
    {
        onErr(Value);

        return default;
    }

    /// <inheritdoc />
    public override async ValueTask MatchAsync(
        Action<TOk> onOk,
        Func<TErr, Task> onErr) =>
        await onErr(Value).ConfigureAwait(false);

    /// <inheritdoc />
    public override Result<TOut, TErr> And<TOut>(Result<TOut, TErr> other) =>
        Result.Err<TOut, TErr>(Value);

    /// <inheritdoc />
    public override Result<TOut, TErr> AndThen<TOut>(
        Func<TOk, Result<TOut, TErr>> resultFactory) =>
        Result.Err<TOut, TErr>(Value);

    /// <inheritdoc />
    public override ValueTask<Result<TOut, TErr>> AndThenAsync<TOut>(
        Func<TOk, ValueTask<Result<TOut, TErr>>> resultFactory) =>
        new ValueTask<Result<TOut, TErr>>(Result.Err<TOut, TErr>(Value));

    /// <inheritdoc />
    public override Result<TOk, TOut> Or<TOut>(Result<TOk, TOut> other) =>
        other;

    /// <inheritdoc />
    public override Result<TOk, TOut>
        OrElse<TOut>(Func<TErr, Result<TOk, TOut>> resultFactory) =>
        resultFactory(Value);

    /// <inheritdoc />
    public override Result<TOk, TOut> OrElse<TState, TOut>(
        TState state,
        Func<TErr, TState, Result<TOk, TOut>> resultFactory) =>
        resultFactory(Value, state);

    /// <inheritdoc />
    public override ValueTask<Result<TOk, TOut>> OrElseAsync<TOut>(
        Func<TErr, ValueTask<Result<TOk, TOut>>> resultFactory) =>
        resultFactory(Value);

    /// <inheritdoc />
    public override TOk Expect(string message) =>
        throw UnmetExpectationException.For(message, Value);

    /// <inheritdoc />
    public override TErr ExpectErr(string message) => Value;

    /// <inheritdoc />
    public override TOk Unwrap() =>
        throw UnwrapException.For(this);

    /// <inheritdoc />
    public override TOk UnwrapOr(TOk defaultValue) =>
        defaultValue;

    /// <inheritdoc />
    public override TOk? UnwrapOrDefault() => default;

    /// <inheritdoc />
    public override TOk UnwrapOrElse(Func<TErr, TOk> valueFactory) =>
        valueFactory(Value);

    /// <inheritdoc />
    public override TOk UnwrapOrElse<TState>(
        TState state,
        Func<TErr, TState, TOk> valueFactory) => valueFactory(Value, state);

    /// <inheritdoc />
    public override async ValueTask<TOk> UnwrapOrElseAsync(
        Func<TErr, Task<TOk>> valueFactory) =>
        await valueFactory(Value).ConfigureAwait(false);

    /// <inheritdoc />
    public override TErr UnwrapErr() => Value;

    /// <inheritdoc />
    public override Result<TOk, TErr> Inspect(Action<TOk> action) => this;

    /// <inheritdoc />
    public override Result<TOk, TErr> Inspect<TState>(
        TState state,
        Action<TOk, TState> action) => this;

    /// <inheritdoc />
    public override ValueTask<Result<TOk, TErr>> InspectAsync(
        Func<TOk, Task> action) =>
        new ValueTask<Result<TOk, TErr>>(this);

    /// <inheritdoc />
    public override Result<TOk, TErr> InspectErr(Action<TErr> action)
    {
        action(Value);
        return this;
    }

    /// <inheritdoc />
    public override Result<TOk, TErr> InspectErr<TState>(
        TState state,
        Action<TErr, TState> action)
    {
        action(Value, state);
        return this;
    }

    /// <inheritdoc />
    public override async ValueTask<Result<TOk, TErr>> InspectErrAsync(
        Func<TErr, Task> action)
    {
        await action(Value).ConfigureAwait(false);

        return this;
    }

    /// <inheritdoc />
    public override Result<TOut, TErr> Map<TOut>(Func<TOk, TOut> map) =>
        Result.Err<TOut, TErr>(Value);

    /// <inheritdoc />
    public override Result<TOut, TErr> Map<TState, TOut>(
        TState state,
        Func<TOk, TState, TOut> map) => Result.Err<TOut, TErr>(Value);

    /// <inheritdoc />
    public override ValueTask<Result<TOut, TErr>> MapAsync<TOut>(
        Func<TOk, Task<TOut>> map) =>
        new ValueTask<Result<TOut, TErr>>(Result.Err<TOut, TErr>(Value));

    /// <inheritdoc />
    public override TOut MapOr<TOut>(
        TOut defaultValue,
        Func<TOk, TOut> map) => defaultValue;

    /// <inheritdoc />
    public override TOut MapOr<TState, TOut>(
        TState state,
        TOut defaultValue,
        Func<TOk, TState, TOut> map) => defaultValue;

    /// <inheritdoc />
    public override ValueTask<TOut> MapOrAsync<TOut>(
        TOut defaultValue,
        Func<TOk, Task<TOut>> map) => new ValueTask<TOut>(defaultValue);

    /// <inheritdoc />
    public override TOut MapOrDefault<TOut>(Func<TOk, TOut> map) =>
        default!;

    /// <inheritdoc />
    public override TOut MapOrDefault<TState, TOut>(
        TState state,
        Func<TOk, TState, TOut> map) => default!;

    /// <inheritdoc />
    public override TOut? MapOrNull<TOut>(Func<TOk, TOut> map) => null;

    /// <inheritdoc />
    public override ValueTask<TOut?> MapOrNullAsync<TOut>(
        Func<TOk, Task<TOut>> map) =>
        new ValueTask<TOut?>(default(TOut?));

    /// <inheritdoc />
    public override TOut MapOrElse<TOut>(
        Func<TErr, TOut> defaultFactory,
        Func<TOk, TOut> map) => defaultFactory(Value);

    /// <inheritdoc />
    public override TOut MapOrElse<TState, TOut>(
        TState state,
        Func<TErr, TState, TOut> defaultFactory,
        Func<TOk, TState, TOut> map) => defaultFactory(Value, state);

    /// <inheritdoc />
    public override async ValueTask<TOut> MapOrElseAsync<TOut>(
        Func<TErr, Task<TOut>> defaultFactory,
        Func<TOk, Task<TOut>> map) =>
        await defaultFactory(Value).ConfigureAwait(false);

    /// <inheritdoc />
    public override ValueTask<TOut> MapOrElseAsync<TOut>(
        Func<TErr, TOut> defaultFactory,
        Func<TOk, Task<TOut>> map) =>
        new ValueTask<TOut>(defaultFactory(Value));

    /// <inheritdoc />
    public override async ValueTask<TOut> MapOrElseAsync<TOut>(
        Func<TErr, Task<TOut>> defaultFactory,
        Func<TOk, TOut> map) =>
        await defaultFactory(Value).ConfigureAwait(false);

    /// <inheritdoc />
    public override Result<TOk, TOut> MapErr<TOut>(Func<TErr, TOut> map) =>
        Result.Err<TOk, TOut>(map(Value));

    /// <inheritdoc />
    public override Result<TOk, TOut> MapErr<TState, TOut>(
        TState state,
        Func<TErr, TState, TOut> map) =>
        Result.Err<TOk, TOut>(map(Value, state));

    /// <inheritdoc />
    public override async ValueTask<Result<TOk, TOut>> MapErrAsync<TOut>(
        Func<TErr, Task<TOut>> map) =>
        Result.Err<TOk, TOut>(await map(Value).ConfigureAwait(false));

    /// <inheritdoc />
    public override IEnumerable<TOk> AsEnumerable() =>
        Array.Empty<TOk>();

    /// <inheritdoc />
    public override Option<TOk> GetOk() => Option.None<TOk>();

    /// <inheritdoc />
    public override Option<TErr> GetErr() => Option.Some(Value);

    internal override void OnlyThisAssemblyMayDerive()
    { }
}
