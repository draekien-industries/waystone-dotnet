﻿namespace Waystone.Monads.Results;

using System;
using System.Threading.Tasks;
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
    public override bool IsOkAnd(Func<TOk, bool> predicate) => predicate(Value);

    /// <inheritdoc />
    public override async Task<bool> IsOkAnd(Func<TOk, Task<bool>> predicate) =>
        await predicate(Value).ConfigureAwait(false);

    /// <inheritdoc />
    public override async ValueTask<bool> IsOkAnd(
        Func<TOk, ValueTask<bool>> predicate) =>
        await predicate(Value).ConfigureAwait(false);

    /// <inheritdoc />
    public override bool IsErrAnd(Func<TErr, bool> predicate) => false;

    /// <inheritdoc />
    public override Task<bool>
        IsErrAnd(Func<TErr, Task<bool>> predicate) =>
        Task.FromResult(false);

    /// <inheritdoc />
    public override ValueTask<bool> IsErrAnd(
        Func<TErr, ValueTask<bool>> predicate) =>
        new(false);


    /// <inheritdoc />
    public override TOut Match<TOut>(
        Func<TOk, TOut> onOk,
        Func<TErr, TOut> onErr) =>
        onOk(Value);

    /// <inheritdoc />
    public override async Task<TOut> Match<TOut>(
        Func<TOk, Task<TOut>> onOk,
        Func<TErr, Task<TOut>> onErr) =>
        await onOk(Value).ConfigureAwait(false);

    /// <inheritdoc />
    public override async ValueTask<TOut> Match<TOut>(
        Func<TOk, ValueTask<TOut>> onOk,
        Func<TErr, ValueTask<TOut>> onErr) =>
        await onOk(Value).ConfigureAwait(false);

    /// <inheritdoc />
    public override void Match(Action<TOk> onOk, Action<TErr> onErr)
    {
        onOk(Value);
    }

    /// <inheritdoc />
    public override Result<TOut, TErr> And<TOut>(Result<TOut, TErr> other) =>
        other;

    /// <inheritdoc />
    public override Result<TOut, TErr> AndThen<TOut>(
        Func<TOk, Result<TOut, TErr>> createOther) =>
        createOther(Value);

    /// <inheritdoc />
    public override async Task<Result<TOut, TErr>> AndThen<TOut>(
        Func<TOk, Task<Result<TOut, TErr>>> createOther) =>
        await createOther(Value).ConfigureAwait(false);

    /// <inheritdoc />
    public override async ValueTask<Result<TOut, TErr>> AndThen<TOut>(
        Func<TOk, ValueTask<Result<TOut, TErr>>> createOther) =>
        await createOther(Value).ConfigureAwait(false);

    /// <inheritdoc />
    public override Result<TOk, TOut> Or<TOut>(Result<TOk, TOut> other) =>
        Result.Ok<TOk, TOut>(Value);

    /// <inheritdoc />
    public override Result<TOk, TOut>
        OrElse<TOut>(Func<TErr, Result<TOk, TOut>> createOther) =>
        Result.Ok<TOk, TOut>(Value);

    /// <inheritdoc />
    public override Task<Result<TOk, TOut>> OrElse<TOut>(
        Func<TErr, Task<Result<TOk, TOut>>> createOther) =>
        Task.FromResult(Result.Ok<TOk, TOut>(Value));

    /// <inheritdoc />
    public override ValueTask<Result<TOk, TOut>> OrElse<TOut>(
        Func<TErr, ValueTask<Result<TOk, TOut>>> createOther) =>
        new(Value);

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
    public override Task<TOk> UnwrapOrElse(Func<TErr, Task<TOk>> onErr) =>
        Task.FromResult(Value);

    /// <inheritdoc />
    public override ValueTask<TOk> UnwrapOrElse(
        Func<TErr, ValueTask<TOk>> onErr) =>
        new(Value);

    /// <inheritdoc />
    public override TErr UnwrapErr() => throw UnwrapException.For(this);

    /// <inheritdoc />
    public override Result<TOk, TErr> Inspect(Action<TOk> action)
    {
        action(Value);
        return this;
    }

    /// <inheritdoc />
    public override async Task<Result<TOk, TErr>> Inspect(
        Func<TOk, Task> action)
    {
        await action(Value).ConfigureAwait(false);
        return this;
    }

    /// <inheritdoc />
    public override async ValueTask<Result<TOk, TErr>> Inspect(
        Func<TOk, ValueTask> action)
    {
        await action(Value).ConfigureAwait(false);
        return this;
    }

    /// <inheritdoc />
    public override Result<TOk, TErr> InspectErr(Action<TErr> action) => this;

    /// <inheritdoc />
    public override Task<Result<TOk, TErr>> InspectErr(
        Func<TErr, Task> action) =>
        Task.FromResult<Result<TOk, TErr>>(this);

    /// <inheritdoc />
    public override ValueTask<Result<TOk, TErr>> InspectErr(
        Func<TErr, ValueTask> action) =>
        new(this);

    /// <inheritdoc />
    public override Result<TOut, TErr> Map<TOut>(Func<TOk, TOut> map) =>
        map(Value);

    /// <inheritdoc />
    public override async Task<Result<TOut, TErr>> Map<TOut>(
        Func<TOk, Task<TOut>> map) => await map(Value).ConfigureAwait(false);

    /// <inheritdoc />
    public override async ValueTask<Result<TOut, TErr>> Map<TOut>(
        Func<TOk, ValueTask<TOut>> map) =>
        await map(Value).ConfigureAwait(false);

    /// <inheritdoc />
    public override TOut MapOr<TOut>(
        TOut @default,
        Func<TOk, TOut> map) => map(Value);

    /// <inheritdoc />
    public override async Task<TOut> MapOr<TOut>(
        TOut @default,
        Func<TOk, Task<TOut>> map) => await map(Value).ConfigureAwait(false);

    /// <inheritdoc />
    public override async ValueTask<TOut> MapOr<TOut>(
        TOut @default,
        Func<TOk, ValueTask<TOut>> map) =>
        await map(Value).ConfigureAwait(false);

    /// <inheritdoc />
    public override TOut MapOrElse<TOut>(
        Func<TErr, TOut> createDefault,
        Func<TOk, TOut> map) => map(Value);

    /// <inheritdoc />
    public override async Task<TOut> MapOrElse<TOut>(
        Func<TErr, Task<TOut>> createDefault,
        Func<TOk, Task<TOut>> map) => await map(Value).ConfigureAwait(false);

    /// <inheritdoc />
    public override async ValueTask<TOut> MapOrElse<TOut>(
        Func<TErr, ValueTask<TOut>> createDefault,
        Func<TOk, ValueTask<TOut>> map) =>
        await map(Value).ConfigureAwait(false);

    /// <inheritdoc />
    public override Result<TOk, TOut> MapErr<TOut>(Func<TErr, TOut> map) =>
        Value;

    /// <inheritdoc />
    public override Task<Result<TOk, TOut>> MapErr<TOut>(
        Func<TErr, Task<TOut>> map) =>
        Task.FromResult<Result<TOk, TOut>>(Value);

    /// <inheritdoc />
    public override ValueTask<Result<TOk, TOut>> MapErr<TOut>(
        Func<TErr, ValueTask<TOut>> map) =>
        new(Value);

    /// <inheritdoc />
    public override Option<TOk> GetOk() => Option.Some(Value);

    /// <inheritdoc />
    public override Option<TErr> GetErr() => Option.None<TErr>();
}
