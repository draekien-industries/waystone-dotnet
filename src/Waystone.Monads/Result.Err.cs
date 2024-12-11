namespace Waystone.Monads;

using System;
using Exceptions;

/// <summary>An error result</summary>
/// <typeparam name="TOk">The ok result value's type</typeparam>
/// <typeparam name="TErr">The error result value's type</typeparam>
public sealed record Err<TOk, TErr> : Result<TOk, TErr>
    where TOk : notnull
    where TErr : notnull
{
    internal Err(TErr value)
    {
        Value = value;
    }

    internal TErr Value { get; }

    /// <inheritdoc />
    public override bool IsOk => false;

    /// <inheritdoc />
    public override bool IsErr => true;

    /// <inheritdoc />
    public override bool IsOkAnd(Predicate<TOk> predicate) =>
        false;

    /// <inheritdoc />
    public override bool IsErrAnd(Predicate<TErr> predicate) =>
        predicate(Value);

    /// <inheritdoc />
    public override TOut Match<TOut>(
        Func<TOk, TOut> onOk,
        Func<TErr, TOut> onErr) =>
        onErr(Value);

    /// <inheritdoc />
    public override void Match(Action<TOk> onOk, Action<TErr> onErr)
    {
        onErr(Value);
    }

    /// <inheritdoc />
    public override Result<TOk2, TErr> And<TOk2>(Result<TOk2, TErr> other) =>
        Result.Err<TOk2, TErr>(Value);

    /// <inheritdoc />
    public override Result<TOk2, TErr> AndThen<TOk2>(
        Func<TOk, Result<TOk2, TErr>> createOther) =>
        Result.Err<TOk2, TErr>(Value);

    /// <inheritdoc />
    public override Result<TOk, TErr2> Or<TErr2>(Result<TOk, TErr2> other) =>
        other;

    /// <inheritdoc />
    public override Result<TOk, TErr2>
        OrElse<TErr2>(Func<TErr, Result<TOk, TErr2>> createOther) =>
        createOther(Value);

    /// <inheritdoc />
    public override TOk Expect(string message) =>
        throw UnmetExpectationException.For(message, Value);

    /// <inheritdoc />
    public override TErr ExpectErr(string message) => Value;

    /// <inheritdoc />
    public override TOk Unwrap() =>
        throw UnwrapException.For(this);

    /// <inheritdoc />
    public override TOk UnwrapOr(TOk @default) =>
        @default;

    /// <inheritdoc />
    public override TOk? UnwrapOrDefault() => default;

    /// <inheritdoc />
    public override TOk UnwrapOrElse(Func<TErr, TOk> onErr) =>
        onErr(Value);

    /// <inheritdoc />
    public override TErr UnwrapErr() => Value;

    /// <inheritdoc />
    public override Result<TOk, TErr> Inspect(Action<TOk> action) => this;

    /// <inheritdoc />
    public override Result<TOk, TErr> InspectErr(Action<TErr> action)
    {
        action(Value);
        return this;
    }

    /// <inheritdoc />
    public override Result<TOk2, TErr> Map<TOk2>(Func<TOk, TOk2> map) =>
        Result.Err<TOk2, TErr>(Value);

    /// <inheritdoc />
    public override TOk2 MapOr<TOk2>(
        TOk2 @default,
        Func<TOk, TOk2> map) => @default;

    /// <inheritdoc />
    public override TOk2 MapOrElse<TOk2>(
        Func<TErr, TOk2> createDefault,
        Func<TOk, TOk2> map) => createDefault(Value);

    /// <inheritdoc />
    public override Result<TOk, TErr2> MapErr<TErr2>(Func<TErr, TErr2> map) =>
        Result.Err<TOk, TErr2>(map(Value));

    /// <inheritdoc />
    public override IOption<TOk> GetOk() => Option.None<TOk>();

    /// <inheritdoc />
    public override IOption<TErr> GetErr() => Option.Some(Value);

    /// <summary>
    /// Implicitly converts a value of <typeparamref name="TErr" /> into an
    /// <see cref="Err{TOk,TErr}" />
    /// </summary>
    /// <param name="value">The <typeparamref name="TErr" /> value</param>
    /// <returns>
    /// An <see cref="Result{TOk,TErr}" /> of type
    /// <see cref="Err{TOk,TErr}" />
    /// </returns>
    public static implicit operator Err<TOk, TErr>(TErr value) => new(value);
}
