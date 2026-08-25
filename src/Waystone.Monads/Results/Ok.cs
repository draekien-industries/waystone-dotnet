namespace Waystone.Monads.Results;

using System;
using System.Collections.Generic;
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
    public override bool IsErrAnd(Func<TErr, bool> predicate) => false;

    /// <inheritdoc />
    public override bool IsErrAnd<TState>(
        TState state,
        Func<TErr, TState, bool> predicate) => false;


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
    public override Result<TOut, TErr> And<TOut>(Result<TOut, TErr> other) =>
        other;

    /// <inheritdoc />
    public override Result<TOut, TErr> AndThen<TOut>(
        Func<TOk, Result<TOut, TErr>> createOther) =>
        createOther(Value);

    /// <inheritdoc />
    public override Result<TOk, TOut> Or<TOut>(Result<TOk, TOut> other) =>
        Result.Ok<TOk, TOut>(Value);

    /// <inheritdoc />
    public override Result<TOk, TOut>
        OrElse<TOut>(Func<TErr, Result<TOk, TOut>> createOther) =>
        Result.Ok<TOk, TOut>(Value);

    /// <inheritdoc />
    public override Result<TOk, TOut> OrElse<TState, TOut>(
        TState state,
        Func<TErr, TState, Result<TOk, TOut>> createOther) =>
        Result.Ok<TOk, TOut>(Value);

    /// <inheritdoc />
    public override TOk Expect(string message) => Value;

    /// <inheritdoc />
    public override TErr ExpectErr(string message) =>
        throw UnmetExpectationException.For(message, Value);

    /// <inheritdoc />
    public override TOk Unwrap() => Value;

    /// <inheritdoc />
    public override TOk UnwrapOr(TOk @default) =>
        Value;

    /// <inheritdoc />
    public override TOk UnwrapOrDefault() => Value;

    /// <inheritdoc />
    public override TOk UnwrapOrElse(Func<TErr, TOk> onErr) =>
        Value;

    /// <inheritdoc />
    public override TOk UnwrapOrElse<TState>(
        TState state,
        Func<TErr, TState, TOk> onErr) => Value;

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
    public override Result<TOk, TErr> InspectErr(Action<TErr> action) => this;

    /// <inheritdoc />
    public override Result<TOk, TErr> InspectErr<TState>(
        TState state,
        Action<TErr, TState> action) => this;

    /// <inheritdoc />
    public override Result<TOut, TErr> Map<TOut>(Func<TOk, TOut> map) =>
        map(Value);

    /// <inheritdoc />
    public override Result<TOut, TErr> Map<TState, TOut>(
        TState state,
        Func<TOk, TState, TOut> map) => map(Value, state);

    /// <inheritdoc />
    public override TOut MapOr<TOut>(
        TOut @default,
        Func<TOk, TOut> map) => map(Value);

    /// <inheritdoc />
    public override TOut MapOr<TState, TOut>(
        TState state,
        TOut @default,
        Func<TOk, TState, TOut> map) => map(Value, state);

    /// <inheritdoc />
    public override TOut MapOrDefault<TOut>(Func<TOk, TOut> map) =>
        map(Value);

    /// <inheritdoc />
    public override TOut MapOrDefault<TState, TOut>(
        TState state,
        Func<TOk, TState, TOut> map) => map(Value, state);

    /// <inheritdoc />
    public override TOut MapOrElse<TOut>(
        Func<TErr, TOut> createDefault,
        Func<TOk, TOut> map) => map(Value);

    /// <inheritdoc />
    public override TOut MapOrElse<TState, TOut>(
        TState state,
        Func<TErr, TState, TOut> createDefault,
        Func<TOk, TState, TOut> map) => map(Value, state);

    /// <inheritdoc />
    public override Result<TOk, TOut> MapErr<TOut>(Func<TErr, TOut> map) =>
        Value;

    /// <inheritdoc />
    public override Result<TOk, TOut> MapErr<TState, TOut>(
        TState state,
        Func<TErr, TState, TOut> map) => Value;

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
