namespace Waystone.Monads.Results;

using System;
using System.Collections.Generic;
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
    public override bool IsErrAnd(Func<TErr, bool> predicate) =>
        predicate(Value);

    /// <inheritdoc />
    public override bool IsErrAnd<TState>(
        TState state,
        Func<TErr, TState, bool> predicate) => predicate(Value, state);

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
    public override Result<TOut, TErr> And<TOut>(Result<TOut, TErr> other) =>
        Result.Err<TOut, TErr>(Value);

    /// <inheritdoc />
    public override Result<TOut, TErr> AndThen<TOut>(
        Func<TOk, Result<TOut, TErr>> resultFactory) =>
        Result.Err<TOut, TErr>(Value);

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
    public override TErr UnwrapErr() => Value;

    /// <inheritdoc />
    public override Result<TOk, TErr> Inspect(Action<TOk> action) => this;

    /// <inheritdoc />
    public override Result<TOk, TErr> Inspect<TState>(
        TState state,
        Action<TOk, TState> action) => this;

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
    public override Result<TOut, TErr> Map<TOut>(Func<TOk, TOut> map) =>
        Result.Err<TOut, TErr>(Value);

    /// <inheritdoc />
    public override Result<TOut, TErr> Map<TState, TOut>(
        TState state,
        Func<TOk, TState, TOut> map) => Result.Err<TOut, TErr>(Value);

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
    public override TOut MapOrDefault<TOut>(Func<TOk, TOut> map) =>
        default!;

    /// <inheritdoc />
    public override TOut MapOrDefault<TState, TOut>(
        TState state,
        Func<TOk, TState, TOut> map) => default!;

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
    public override Result<TOk, TOut> MapErr<TOut>(Func<TErr, TOut> map) =>
        Result.Err<TOk, TOut>(map(Value));

    /// <inheritdoc />
    public override Result<TOk, TOut> MapErr<TState, TOut>(
        TState state,
        Func<TErr, TState, TOut> map) =>
        Result.Err<TOk, TOut>(map(Value, state));

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
