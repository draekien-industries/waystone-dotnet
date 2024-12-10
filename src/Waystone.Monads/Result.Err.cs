namespace Waystone.Monads;

using System;
using Exceptions;

/// <summary>An error result</summary>
/// <typeparam name="TOk">The ok result value's type</typeparam>
/// <typeparam name="TErr">The error result value's type</typeparam>
public sealed record Err<TOk, TErr> : IResult<TOk, TErr>
    where TOk : notnull
    where TErr : notnull
{
    internal Err(TErr value)
    {
        Value = value;
    }

    internal TErr Value { get; }

    /// <inheritdoc />
    public bool IsOk => false;

    /// <inheritdoc />
    public bool IsErr => true;

    /// <inheritdoc />
    public bool IsOkAnd(Predicate<TOk> predicate) =>
        false;

    /// <inheritdoc />
    public bool IsErrAnd(Predicate<TErr> predicate) =>
        predicate(Value);

    /// <inheritdoc />
    public TOut Match<TOut>(Func<TOk, TOut> onOk, Func<TErr, TOut> onErr) =>
        onErr(Value);

    /// <inheritdoc />
    public void Match(Action<TOk> onOk, Action<TErr> onErr)
    {
        onErr(Value);
    }

    /// <inheritdoc />
    public IResult<TOk2, TErr> And<TOk2>(IResult<TOk2, TErr> other)
        where TOk2 : notnull =>
        Result.Err<TOk2, TErr>(Value);

    /// <inheritdoc />
    public IResult<TOk2, TErr> AndThen<TOk2>(
        Func<TOk, IResult<TOk2, TErr>> createOther) where TOk2 : notnull =>
        Result.Err<TOk2, TErr>(Value);

    /// <inheritdoc />
    public IResult<TOk, TErr2> Or<TErr2>(IResult<TOk, TErr2> other)
        where TErr2 : notnull =>
        other;

    /// <inheritdoc />
    public IResult<TOk, TErr2>
        OrElse<TErr2>(Func<TErr, IResult<TOk, TErr2>> createOther)
        where TErr2 : notnull =>
        createOther(Value);

    /// <inheritdoc />
    public TOk Expect(string message) =>
        throw UnmetExpectationException.For(message, Value);

    /// <inheritdoc />
    public TErr ExpectErr(string message) => Value;

    /// <inheritdoc />
    public TOk Unwrap() =>
        throw UnwrapException.For(this);

    /// <inheritdoc />
    public TOk UnwrapOr(TOk @default) =>
        @default;

    /// <inheritdoc />
    public TOk? UnwrapOrDefault() => default;

    /// <inheritdoc />
    public TOk UnwrapOrElse(Func<TErr, TOk> onErr) =>
        onErr(Value);

    /// <inheritdoc />
    public TErr UnwrapErr() => Value;

    /// <inheritdoc />
    public IResult<TOk, TErr> Inspect(Action<TOk> action) => this;

    /// <inheritdoc />
    public IResult<TOk, TErr> InspectErr(Action<TErr> action)
    {
        action(Value);
        return this;
    }

    /// <inheritdoc />
    public IResult<TOk2, TErr> Map<TOk2>(Func<TOk, TOk2> map)
        where TOk2 : notnull =>
        Result.Err<TOk2, TErr>(Value);

    /// <inheritdoc />
    public TOk2 MapOr<TOk2>(
        TOk2 @default,
        Func<TOk, TOk2> map) => @default;

    /// <inheritdoc />
    public TOk2 MapOrElse<TOk2>(
        Func<TErr, TOk2> createDefault,
        Func<TOk, TOk2> map) => createDefault(Value);

    /// <inheritdoc />
    public IResult<TOk, TErr2> MapErr<TErr2>(Func<TErr, TErr2> map)
        where TErr2 : notnull =>
        Result.Err<TOk, TErr2>(map(Value));

    /// <inheritdoc />
    public IOption<TOk> GetOk() => Option.None<TOk>();

    /// <inheritdoc />
    public IOption<TErr> GetErr() => Option.Some(Value);
}
