namespace Waystone.Monads.Results;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Exceptions;
using Options;
#if !DEBUG
using System.Diagnostics;
#endif

/// <summary>A <see cref="Result{TOk,TErr}" /> holding a success value.</summary>
/// <remarks>
/// One of the two cases of <see cref="Result{TOk,TErr}" />, so matching both
/// is exhaustive and no third case can be added from outside the library.
/// Build one with <see cref="Result.Ok{TOk,TErr}" />. The value is never null,
/// though it may be the default of <typeparamref name="TOk" />.
/// </remarks>
/// <typeparam name="TOk">The ok result value's type</typeparam>
/// <typeparam name="TErr">The error result value's type</typeparam>
#if !DEBUG
[DebuggerStepThrough]
#endif
public sealed record Ok<TOk, TErr> : Result<TOk, TErr>
    where TOk : notnull where TErr : notnull
{
    internal Ok(TOk value)
    {
        if (value is null)
        {
            throw new ArgumentNullException(
                nameof(value),
                "The value of an `Ok` result cannot be null.");
        }

        Value = value;
    }

    internal TOk Value { get; }

    /// <summary>Binds the success value in a positional pattern.</summary>
    /// <remarks>
    /// What makes <c>result is Ok&lt;TOk, TErr&gt;(var value)</c> and an arm of
    /// <c>result switch { Ok&lt;TOk, TErr&gt;(var value) => …,
    /// Err&lt;TOk, TErr&gt;(var error) => … }</c> compile. A pattern over a
    /// result names both type arguments even though only one is bound, which is
    /// the cost of the case types being generic in both.
    /// <para>
    /// This is the only way to read the value off an
    /// <see cref="Ok{TOk,TErr}" /> directly; the property behind it is internal,
    /// so a caller who wants the value without naming the case type goes through
    /// <see cref="Result{TOk,TErr}.TryUnwrap" /> or
    /// <see cref="Result{TOk,TErr}.Unwrap" /> instead.
    /// </para>
    /// </remarks>
    /// <param name="value">Receives the success value, which is never null.</param>
    public void Deconstruct(out TOk value)
    {
        value = Value;
    }

    /// <inheritdoc />
    public override bool IsOk => true;

    /// <inheritdoc />
    public override bool IsErr => false;

    /// <inheritdoc />
    public override bool IsOkAnd(Func<TOk, bool> predicate) => predicate(Value);

    /// <inheritdoc />
    public override bool IsOkAnd<TState>(
        TState state,
        Func<TOk, TState, bool> predicate) => predicate(Value, state);

    /// <inheritdoc />
    public override async ValueTask<bool> IsOkAndAsync(
        Func<TOk, Task<bool>> predicate) =>
        await predicate(Value).ConfigureAwait(false);

    /// <inheritdoc />
    public override bool IsErrAnd(Func<TErr, bool> predicate) => false;

    /// <inheritdoc />
    public override bool IsErrAnd<TState>(
        TState state,
        Func<TErr, TState, bool> predicate) => false;

    /// <inheritdoc />
    public override ValueTask<bool> IsErrAndAsync(
        Func<TErr, Task<bool>> predicate) => new ValueTask<bool>(false);

    /// <inheritdoc />
    public override TOut Match<TOut>(
        Func<TOk, TOut> onOk,
        Func<TErr, TOut> onErr) =>
        onOk(Value);

    /// <inheritdoc />
    public override TOut Match<TState, TOut>(
        TState state,
        Func<TOk, TState, TOut> onOk,
        Func<TErr, TState, TOut> onErr) =>
        onOk(Value, state);

    /// <inheritdoc />
    public override void Match(Action<TOk> onOk, Action<TErr> onErr)
    {
        onOk(Value);
    }

    /// <inheritdoc />
    public override void Match<TState>(
        TState state,
        Action<TOk, TState> onOk,
        Action<TErr, TState> onErr)
    {
        onOk(Value, state);
    }

    /// <inheritdoc />
    public override async ValueTask<TOut> MatchAsync<TOut>(
        Func<TOk, Task<TOut>> onOk,
        Func<TErr, Task<TOut>> onErr) =>
        await onOk(Value).ConfigureAwait(false);

    /// <inheritdoc />
    public override async ValueTask MatchAsync(
        Func<TOk, Task> onOk,
        Func<TErr, Task> onErr) =>
        await onOk(Value).ConfigureAwait(false);

    /// <inheritdoc />
    public override async ValueTask MatchAsync(
        Func<TOk, Task> onOk,
        Action<TErr> onErr) =>
        await onOk(Value).ConfigureAwait(false);

    /// <inheritdoc />
    public override ValueTask MatchAsync(
        Action<TOk> onOk,
        Func<TErr, Task> onErr)
    {
        onOk(Value);

        return default;
    }

    /// <inheritdoc />
    public override Result<TOut, TErr> And<TOut>(Result<TOut, TErr> other) =>
        other;

    /// <inheritdoc />
    public override Result<TOut, TErr> AndThen<TOut>(
        Func<TOk, Result<TOut, TErr>> resultFactory) =>
        Result.NotNull(resultFactory(Value), nameof(resultFactory));

    /// <inheritdoc />
    public override Result<TOut, TErr> AndThen<TState, TOut>(
        TState state,
        Func<TOk, TState, Result<TOut, TErr>> resultFactory) =>
        Result.NotNull(resultFactory(Value, state), nameof(resultFactory));

    /// <inheritdoc />
    public override ValueTask<Result<TOut, TErr>> AndThenAsync<TOut>(
        Func<TOk, ValueTask<Result<TOut, TErr>>> resultFactory) =>
        Result.NotNullAsync(resultFactory(Value), nameof(resultFactory));

    /// <inheritdoc />
    public override Result<TOk, TOut> Or<TOut>(Result<TOk, TOut> other) =>
        Result.Ok<TOk, TOut>(Value);

    /// <inheritdoc />
    public override Result<TOk, TOut>
        OrElse<TOut>(Func<TErr, Result<TOk, TOut>> resultFactory) =>
        Result.Ok<TOk, TOut>(Value);

    /// <inheritdoc />
    public override Result<TOk, TOut> OrElse<TState, TOut>(
        TState state,
        Func<TErr, TState, Result<TOk, TOut>> resultFactory) =>
        Result.Ok<TOk, TOut>(Value);

    /// <inheritdoc />
    public override ValueTask<Result<TOk, TOut>> OrElseAsync<TOut>(
        Func<TErr, ValueTask<Result<TOk, TOut>>> resultFactory) =>
        new ValueTask<Result<TOk, TOut>>(Result.Ok<TOk, TOut>(Value));

    /// <inheritdoc />
    public override TOk Expect(string message) => Value;

    /// <inheritdoc />
    public override TErr ExpectErr(string message) =>
        throw UnmetExpectationException.For(message, Value);

    /// <inheritdoc />
    public override TOk Unwrap() => Value;

    /// <inheritdoc />
    public override TOk UnwrapOr(TOk defaultValue) =>
        Value;

    /// <inheritdoc />
    public override TOk UnwrapOrDefault() => Value;

    /// <inheritdoc />
    public override TOk UnwrapOrElse(Func<TErr, TOk> valueFactory) =>
        Value;

    /// <inheritdoc />
    public override TOk UnwrapOrElse<TState>(
        TState state,
        Func<TErr, TState, TOk> valueFactory) => Value;

    /// <inheritdoc />
    public override ValueTask<TOk> UnwrapOrElseAsync(
        Func<TErr, Task<TOk>> valueFactory) => new ValueTask<TOk>(Value);

    /// <inheritdoc />
    public override TErr UnwrapErr() => throw UnwrapException.For(this);

    /// <inheritdoc />
    public override Result<TOk, TErr> Inspect(Action<TOk> action)
    {
        action(Value);
        return this;
    }

    /// <inheritdoc />
    public override Result<TOk, TErr> Inspect<TState>(
        TState state,
        Action<TOk, TState> action)
    {
        action(Value, state);
        return this;
    }

    /// <inheritdoc />
    public override async ValueTask<Result<TOk, TErr>> InspectAsync(
        Func<TOk, Task> action)
    {
        await action(Value).ConfigureAwait(false);

        return this;
    }

    /// <inheritdoc />
    public override Result<TOk, TErr> InspectErr(Action<TErr> action) => this;

    /// <inheritdoc />
    public override Result<TOk, TErr> InspectErr<TState>(
        TState state,
        Action<TErr, TState> action) => this;

    /// <inheritdoc />
    public override ValueTask<Result<TOk, TErr>> InspectErrAsync(
        Func<TErr, Task> action) =>
        new ValueTask<Result<TOk, TErr>>(this);

    /// <inheritdoc />
    public override Result<TOut, TErr> Map<TOut>(Func<TOk, TOut> map) =>
        Result.Ok<TOut, TErr>(map(Value));

    /// <inheritdoc />
    public override Result<TOut, TErr> Map<TState, TOut>(
        TState state,
        Func<TOk, TState, TOut> map) =>
        Result.Ok<TOut, TErr>(map(Value, state));

    /// <inheritdoc />
    public override async ValueTask<Result<TOut, TErr>> MapAsync<TOut>(
        Func<TOk, Task<TOut>> map) =>
        Result.Ok<TOut, TErr>(await map(Value).ConfigureAwait(false));

    /// <inheritdoc />
    public override TOut MapOr<TOut>(
        TOut defaultValue,
        Func<TOk, TOut> map) => map(Value);

    /// <inheritdoc />
    public override TOut MapOr<TState, TOut>(
        TState state,
        TOut defaultValue,
        Func<TOk, TState, TOut> map) => map(Value, state);

    /// <inheritdoc />
    public override async ValueTask<TOut> MapOrAsync<TOut>(
        TOut defaultValue,
        Func<TOk, Task<TOut>> map) =>
        await map(Value).ConfigureAwait(false);

    /// <inheritdoc />
    public override TOut MapOrDefault<TOut>(Func<TOk, TOut> map) =>
        map(Value);

    /// <inheritdoc />
    public override TOut MapOrDefault<TState, TOut>(
        TState state,
        Func<TOk, TState, TOut> map) => map(Value, state);

    /// <inheritdoc />
    public override TOut? MapOrNull<TOut>(Func<TOk, TOut> map) => map(Value);

    /// <inheritdoc />
    public override async ValueTask<TOut?> MapOrNullAsync<TOut>(
        Func<TOk, Task<TOut>> map) =>
        await map(Value).ConfigureAwait(false);

    /// <inheritdoc />
    public override TOut MapOrElse<TOut>(
        Func<TErr, TOut> defaultFactory,
        Func<TOk, TOut> map) => map(Value);

    /// <inheritdoc />
    public override TOut MapOrElse<TState, TOut>(
        TState state,
        Func<TErr, TState, TOut> defaultFactory,
        Func<TOk, TState, TOut> map) => map(Value, state);

    /// <inheritdoc />
    public override async ValueTask<TOut> MapOrElseAsync<TOut>(
        Func<TErr, Task<TOut>> defaultFactory,
        Func<TOk, Task<TOut>> map) =>
        await map(Value).ConfigureAwait(false);

    /// <inheritdoc />
    public override async ValueTask<TOut> MapOrElseAsync<TOut>(
        Func<TErr, TOut> defaultFactory,
        Func<TOk, Task<TOut>> map) =>
        await map(Value).ConfigureAwait(false);

    /// <inheritdoc />
    public override ValueTask<TOut> MapOrElseAsync<TOut>(
        Func<TErr, Task<TOut>> defaultFactory,
        Func<TOk, TOut> map) =>
        new ValueTask<TOut>(map(Value));

    /// <inheritdoc />
    public override Result<TOk, TOut> MapErr<TOut>(Func<TErr, TOut> map) =>
        Result.Ok<TOk, TOut>(Value);

    /// <inheritdoc />
    public override Result<TOk, TOut> MapErr<TState, TOut>(
        TState state,
        Func<TErr, TState, TOut> map) => Result.Ok<TOk, TOut>(Value);

    /// <inheritdoc />
    public override ValueTask<Result<TOk, TOut>> MapErrAsync<TOut>(
        Func<TErr, Task<TOut>> map) =>
        new ValueTask<Result<TOk, TOut>>(Result.Ok<TOk, TOut>(Value));

    /// <inheritdoc />
    public override IEnumerable<TOk> AsEnumerable() =>
        new[] { Value };

    /// <inheritdoc />
    public override Option<TOk> GetOk() => Option.Some(Value);

    /// <inheritdoc />
    public override Option<TErr> GetErr() => Option.None<TErr>();

    internal override void OnlyThisAssemblyMayDerive()
    { }
}
