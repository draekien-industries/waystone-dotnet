namespace Waystone.Monads;

using System;
using Exceptions;

/// <summary>An ok result type</summary>
/// <typeparam name="TOk">The ok result value's type</typeparam>
/// <typeparam name="TErr">The error result value's type</typeparam>
public sealed record Ok<TOk, TErr> : IResult<TOk, TErr>
    where TOk : notnull where TErr : notnull
{
    internal Ok(TOk value)
    {
        Value = value;
    }

    internal TOk Value { get; }

    /// <inheritdoc />
    public bool IsOk => true;

    /// <inheritdoc />
    public bool IsErr => false;

    /// <inheritdoc />
    public bool IsOkAnd(Predicate<TOk> predicate) =>
        predicate(Value);

    /// <inheritdoc />
    public bool IsErrAnd(Predicate<TErr> predicate) =>
        false;

    /// <inheritdoc />
    public TOut Match<TOut>(Func<TOk, TOut> onOk, Func<TErr, TOut> onErr) =>
        onOk(Value);

    /// <inheritdoc />
    public void Match(Action<TOk> onOk, Action<TErr> onErr)
    {
        onOk(Value);
    }

    /// <inheritdoc />
    public IResult<TOk2, TErr> And<TOk2>(IResult<TOk2, TErr> other)
        where TOk2 : notnull =>
        other;

    /// <inheritdoc />
    public IResult<TOk2, TErr> AndThen<TOk2>(
        Func<TOk, IResult<TOk2, TErr>> createOther) where TOk2 : notnull =>
        createOther(Value);

    /// <inheritdoc />
    public IResult<TOk, TErr2> Or<TErr2>(IResult<TOk, TErr2> other)
        where TErr2 : notnull =>
        Result.Ok<TOk, TErr2>(Value);

    /// <inheritdoc />
    public IResult<TOk, TErr2>
        OrElse<TErr2>(Func<TErr, IResult<TOk, TErr2>> createOther)
        where TErr2 : notnull =>
        Result.Ok<TOk, TErr2>(Value);

    /// <inheritdoc />
    public TOk Expect(string message) => Value;

    /// <inheritdoc />
    public TErr ExpectErr(string message) =>
        throw UnmetExpectationException.For(message, Value);

    /// <inheritdoc />
    public TOk Unwrap() => Value;

    /// <inheritdoc />
    public TOk UnwrapOr(TOk @default) =>
        Value;

    /// <inheritdoc />
    public TOk UnwrapOrDefault() => Value;

    /// <inheritdoc />
    public TOk UnwrapOrElse(Func<TErr, TOk> onErr) =>
        Value;

    /// <inheritdoc />
    public TErr UnwrapErr() => throw UnwrapException.For(this);

    /// <inheritdoc />
    public IResult<TOk, TErr> Inspect(Action<TOk> action)
    {
        action(Value);
        return this;
    }

    /// <inheritdoc />
    public IResult<TOk, TErr> InspectErr(Action<TErr> action) => this;

    /// <inheritdoc />
    public IResult<TOk2, TErr> Map<TOk2>(Func<TOk, TOk2> map)
        where TOk2 : notnull =>
        Result.Ok<TOk2, TErr>(map(Value));

    /// <inheritdoc />
    public TOk2 MapOr<TOk2>(
        TOk2 @default,
        Func<TOk, TOk2> map) => map(Value);

    /// <inheritdoc />
    public TOk2 MapOrElse<TOk2>(
        Func<TErr, TOk2> createDefault,
        Func<TOk, TOk2> map) => map(Value);

    /// <inheritdoc />
    public IResult<TOk, TErr2> MapErr<TErr2>(Func<TErr, TErr2> map)
        where TErr2 : notnull =>
        Result.Ok<TOk, TErr2>(Value);

    /// <inheritdoc />
    public IOption<TOk> GetOk() => Option.Some(Value);

    /// <inheritdoc />
    public IOption<TErr> GetErr() => Option.None<TErr>();
}
