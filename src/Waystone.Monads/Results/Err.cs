namespace Waystone.Monads.Results;

using System;
using System.Threading.Tasks;
using Exceptions;
using Options;

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
    public override bool IsOkAnd(Func<TOk, bool> predicate) => false;

    /// <inheritdoc />
    public override bool IsErrAnd(Func<TErr, bool> predicate) =>
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
    public override Result<TOut, TErr> And<TOut>(Result<TOut, TErr> other) =>
        Value;

    /// <inheritdoc />
    public override Result<TOut, TErr> AndThen<TOut>(
        Func<TOk, Result<TOut, TErr>> createOther) =>
        Value;

    /// <inheritdoc />
    public override Task<Result<TOut, TErr>> AndThenAsync<TOut>(
        Func<TOk, Task<Result<TOut, TErr>>> createOther) =>
        Task.FromResult<Result<TOut, TErr>>(Value);

    /// <inheritdoc />
    public override ValueTask<Result<TOut, TErr>> AndThenAsync<TOut>(
        Func<TOk, ValueTask<Result<TOut, TErr>>> createOther) =>
        new(Value);

    /// <inheritdoc />
    public override Result<TOk, TOut> Or<TOut>(Result<TOk, TOut> other) =>
        other;

    /// <inheritdoc />
    public override Result<TOk, TOut>
        OrElse<TOut>(Func<TErr, Result<TOk, TOut>> createOther) =>
        createOther(Value);

    /// <inheritdoc />
    public override async Task<Result<TOk, TOut>> OrElseAsync<TOut>(
        Func<TErr, Task<Result<TOk, TOut>>> createOther) =>
        await createOther(Value).ConfigureAwait(false);

    /// <inheritdoc />
    public override async ValueTask<Result<TOk, TOut>> OrElseAsync<TOut>(
        Func<TErr, ValueTask<Result<TOk, TOut>>> createOther) =>
        await createOther(Value).ConfigureAwait(false);

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
    public override async Task<TOk> UnwrapOrElseAsync(
        Func<TErr, Task<TOk>> onErr) =>
        await onErr(Value).ConfigureAwait(false);

    /// <inheritdoc />
    public override async ValueTask<TOk> UnwrapOrElseAsync(
        Func<TErr, ValueTask<TOk>> onErr) =>
        await onErr(Value).ConfigureAwait(false);

    /// <inheritdoc />
    public override TErr UnwrapErr() => Value;

    /// <inheritdoc />
    public override Result<TOk, TErr> Inspect(Action<TOk> action) => this;

    /// <inheritdoc />
    public override Task<Result<TOk, TErr>> InspectAsync(
        Func<TOk, Task> action) =>
        Task.FromResult<Result<TOk, TErr>>(this);

    /// <inheritdoc />
    public override ValueTask<Result<TOk, TErr>> InspectAsync(
        Func<TOk, ValueTask> action) =>
        new(this);

    /// <inheritdoc />
    public override Result<TOk, TErr> InspectErr(Action<TErr> action)
    {
        action(Value);
        return this;
    }

    /// <inheritdoc />
    public override async Task<Result<TOk, TErr>> InspectErrAsync(
        Func<TErr, Task> action)
    {
        await action(Value).ConfigureAwait(false);
        return this;
    }

    /// <inheritdoc />
    public override async ValueTask<Result<TOk, TErr>> InspectErrAsync(
        Func<TErr, ValueTask> action)
    {
        await action(Value).ConfigureAwait(false);
        return this;
    }

    /// <inheritdoc />
    public override Result<TOut, TErr> Map<TOut>(Func<TOk, TOut> map) =>
        Value;

    /// <inheritdoc />
    public override TOut MapOr<TOut>(
        TOut @default,
        Func<TOk, TOut> map) => @default;

    /// <inheritdoc />
    public override TOut MapOrElse<TOut>(
        Func<TErr, TOut> createDefault,
        Func<TOk, TOut> map) => createDefault(Value);

    /// <inheritdoc />
    public override Result<TOk, TOut> MapErr<TOut>(Func<TErr, TOut> map) =>
        map(Value);

    /// <inheritdoc />
    public override async Task<Result<TOk, TOut>> MapErrAsync<TOut>(
        Func<TErr, Task<TOut>> map) => await map(Value);

    /// <inheritdoc />
    public override async ValueTask<Result<TOk, TOut>> MapErrAsync<TOut>(
        Func<TErr, ValueTask<TOut>> map) => await map(Value);

    /// <inheritdoc />
    public override Option<TOk> GetOk() => Option.None<TOk>();

    /// <inheritdoc />
    public override Option<TErr> GetErr() => Option.Some(Value);
}
