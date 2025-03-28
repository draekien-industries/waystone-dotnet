﻿namespace Waystone.Monads.Results;

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
    public override Task<bool> IsOkAnd(Func<TOk, Task<bool>> predicate) =>
        Task.FromResult(false);

    /// <inheritdoc />
    public override ValueTask<bool> IsOkAnd(
        Func<TOk, ValueTask<bool>> predicate) =>
        new(false);

    /// <inheritdoc />
    public override bool IsErrAnd(Func<TErr, bool> predicate) =>
        predicate(Value);

    /// <inheritdoc />
    public override async Task<bool>
        IsErrAnd(Func<TErr, Task<bool>> predicate) =>
        await predicate(Value).ConfigureAwait(false);

    /// <inheritdoc />
    public override async ValueTask<bool> IsErrAnd(
        Func<TErr, ValueTask<bool>> predicate) =>
        await predicate(Value).ConfigureAwait(false);


    /// <inheritdoc />
    public override TOut Match<TOut>(
        Func<TOk, TOut> onOk,
        Func<TErr, TOut> onErr) =>
        onErr(Value);

    /// <inheritdoc />
    public override async Task<TOut> Match<TOut>(
        Func<TOk, Task<TOut>> onOk,
        Func<TErr, Task<TOut>> onErr) =>
        await onErr(Value).ConfigureAwait(false);

    /// <inheritdoc />
    public override async ValueTask<TOut> Match<TOut>(
        Func<TOk, ValueTask<TOut>> onOk,
        Func<TErr, ValueTask<TOut>> onErr) =>
        await onErr(Value).ConfigureAwait(false);

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
    public override Task<Result<TOut, TErr>> AndThen<TOut>(
        Func<TOk, Task<Result<TOut, TErr>>> createOther) =>
        Task.FromResult<Result<TOut, TErr>>(Value);

    /// <inheritdoc />
    public override ValueTask<Result<TOut, TErr>> AndThen<TOut>(
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
    public override async Task<Result<TOk, TOut>> OrElse<TOut>(
        Func<TErr, Task<Result<TOk, TOut>>> createOther) =>
        await createOther(Value).ConfigureAwait(false);

    /// <inheritdoc />
    public override async ValueTask<Result<TOk, TOut>> OrElse<TOut>(
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
    public override async Task<TOk> UnwrapOrElse(Func<TErr, Task<TOk>> onErr) =>
        await onErr(Value).ConfigureAwait(false);

    /// <inheritdoc />
    public override async ValueTask<TOk> UnwrapOrElse(
        Func<TErr, ValueTask<TOk>> onErr) =>
        await onErr(Value).ConfigureAwait(false);

    /// <inheritdoc />
    public override TErr UnwrapErr() => Value;

    /// <inheritdoc />
    public override Result<TOk, TErr> Inspect(Action<TOk> action) => this;

    /// <inheritdoc />
    public override Task<Result<TOk, TErr>> Inspect(
        Func<TOk, Task> action) =>
        Task.FromResult<Result<TOk, TErr>>(this);

    /// <inheritdoc />
    public override ValueTask<Result<TOk, TErr>> Inspect(
        Func<TOk, ValueTask> action) =>
        new(this);

    /// <inheritdoc />
    public override Result<TOk, TErr> InspectErr(Action<TErr> action)
    {
        action(Value);
        return this;
    }

    /// <inheritdoc />
    public override async Task<Result<TOk, TErr>> InspectErr(
        Func<TErr, Task> action)
    {
        await action(Value).ConfigureAwait(false);
        return this;
    }

    /// <inheritdoc />
    public override async ValueTask<Result<TOk, TErr>> InspectErr(
        Func<TErr, ValueTask> action)
    {
        await action(Value).ConfigureAwait(false);
        return this;
    }

    /// <inheritdoc />
    public override Result<TOut, TErr> Map<TOut>(Func<TOk, TOut> map) =>
        Value;

    /// <inheritdoc />
    public override Task<Result<TOut, TErr>> Map<TOut>(
        Func<TOk, Task<TOut>> map) =>
        Task.FromResult<Result<TOut, TErr>>(Value);

    /// <inheritdoc />
    public override ValueTask<Result<TOut, TErr>> Map<TOut>(
        Func<TOk, ValueTask<TOut>> map) =>
        new(Value);

    /// <inheritdoc />
    public override TOut MapOr<TOut>(
        TOut @default,
        Func<TOk, TOut> map) => @default;

    /// <inheritdoc />
    public override Task<TOut> MapOr<TOut>(
        TOut @default,
        Func<TOk, Task<TOut>> map) => Task.FromResult(@default);

    /// <inheritdoc />
    public override ValueTask<TOut> MapOr<TOut>(
        TOut @default,
        Func<TOk, ValueTask<TOut>> map) => new(@default);

    /// <inheritdoc />
    public override TOut MapOrElse<TOut>(
        Func<TErr, TOut> createDefault,
        Func<TOk, TOut> map) => createDefault(Value);

    /// <inheritdoc />
    public override async Task<TOut> MapOrElse<TOut>(
        Func<TErr, Task<TOut>> createDefault,
        Func<TOk, Task<TOut>> map) =>
        await createDefault(Value).ConfigureAwait(false);

    /// <inheritdoc />
    public override async ValueTask<TOut> MapOrElse<TOut>(
        Func<TErr, ValueTask<TOut>> createDefault,
        Func<TOk, ValueTask<TOut>> map) =>
        await createDefault(Value).ConfigureAwait(false);

    /// <inheritdoc />
    public override Result<TOk, TOut> MapErr<TOut>(Func<TErr, TOut> map) =>
        map(Value);

    /// <inheritdoc />
    public override async Task<Result<TOk, TOut>> MapErr<TOut>(
        Func<TErr, Task<TOut>> map) => await map(Value);

    /// <inheritdoc />
    public override async ValueTask<Result<TOk, TOut>> MapErr<TOut>(
        Func<TErr, ValueTask<TOut>> map) => await map(Value);

    /// <inheritdoc />
    public override Option<TOk> GetOk() => Option.None<TOk>();

    /// <inheritdoc />
    public override Option<TErr> GetErr() => Option.Some(Value);
}
