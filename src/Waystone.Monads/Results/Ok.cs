namespace Waystone.Monads.Results;

using System;
using Exceptions;
using Options;

/// <summary>An ok result type</summary>
/// <typeparam name="TOk">The ok result value's type</typeparam>
/// <typeparam name="TErr">The error result value's type</typeparam>
public sealed record Ok<TOk, TErr> : Result<TOk, TErr>
    where TOk : notnull where TErr : notnull
{
    internal Ok(TOk value)
    {
        Value = value;
    }

    internal TOk Value { get; }

    /// <inheritdoc />
    public override bool IsOk => true;

    /// <inheritdoc />
    public override bool IsErr => false;

    /// <inheritdoc />
    public override bool IsOkAnd(Predicate<TOk> predicate) =>
        predicate(Value);

    /// <inheritdoc />
    public override bool IsErrAnd(Predicate<TErr> predicate) =>
        false;

    /// <inheritdoc />
    public override TOut Match<TOut>(
        Func<TOk, TOut> onOk,
        Func<TErr, TOut> onErr) =>
        onOk(Value);

    /// <inheritdoc />
    public override void Match(Action<TOk> onOk, Action<TErr> onErr)
    {
        onOk(Value);
    }

    /// <inheritdoc />
    public override Result<TOk2, TErr> And<TOk2>(Result<TOk2, TErr> other) =>
        other;

    /// <inheritdoc />
    public override Result<TOk2, TErr> AndThen<TOk2>(
        Func<TOk, Result<TOk2, TErr>> createOther) =>
        createOther(Value);

    /// <inheritdoc />
    public override Result<TOk, TErr2> Or<TErr2>(Result<TOk, TErr2> other) =>
        Result.Ok<TOk, TErr2>(Value);

    /// <inheritdoc />
    public override Result<TOk, TErr2>
        OrElse<TErr2>(Func<TErr, Result<TOk, TErr2>> createOther) =>
        Result.Ok<TOk, TErr2>(Value);

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
    public override TErr UnwrapErr() => throw UnwrapException.For(this);

    /// <inheritdoc />
    public override Result<TOk, TErr> Inspect(Action<TOk> action)
    {
        action(Value);
        return this;
    }

    /// <inheritdoc />
    public override Result<TOk, TErr> InspectErr(Action<TErr> action) => this;

    /// <inheritdoc />
    public override Result<TOk2, TErr> Map<TOk2>(Func<TOk, TOk2> map) =>
        Result.Ok<TOk2, TErr>(map(Value));

    /// <inheritdoc />
    public override TOk2 MapOr<TOk2>(
        TOk2 @default,
        Func<TOk, TOk2> map) => map(Value);

    /// <inheritdoc />
    public override TOk2 MapOrElse<TOk2>(
        Func<TErr, TOk2> createDefault,
        Func<TOk, TOk2> map) => map(Value);

    /// <inheritdoc />
    public override Result<TOk, TErr2> MapErr<TErr2>(Func<TErr, TErr2> map) =>
        Result.Ok<TOk, TErr2>(Value);

    /// <inheritdoc />
    public override Option<TOk> GetOk() => Option.Some(Value);

    /// <inheritdoc />
    public override Option<TErr> GetErr() => Option.None<TErr>();
}
