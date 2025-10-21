namespace Waystone.Monads.Options;

using System;
using Exceptions;
using Results;

/// <summary>No value of type <typeparamref name="T" />.</summary>
/// <typeparam name="T">The option value's type.</typeparam>
public sealed record None<T> : Option<T>
    where T : notnull
{
    /// <inheritdoc />
    public override bool IsSome => false;

    /// <inheritdoc />
    public override bool IsNone => true;

    /// <inheritdoc />
#if !DEBUG
    [DebuggerStepThrough]
    [StackTraceHidden]
#endif
    public override bool IsSomeAnd(Func<T, bool> predicate) =>
        false;

    /// <inheritdoc />
#if !DEBUG
    [DebuggerStepThrough]
    [StackTraceHidden]
#endif
    public override bool IsNoneOr(Func<T, bool> predicate) =>
        true;

    /// <inheritdoc />
#if !DEBUG
    [DebuggerStepThrough]
    [StackTraceHidden]
#endif
    public override TOut Match<TOut>(
        Func<T, TOut> onSome,
        Func<TOut> onNone) => onNone();

    /// <inheritdoc />
#if !DEBUG
    [DebuggerStepThrough]
    [StackTraceHidden]
#endif
    public override void Match(Action<T> onSome, Action onNone)
    {
        onNone();
    }

    /// <inheritdoc />
#if !DEBUG
    [DebuggerStepThrough]
    [StackTraceHidden]
#endif
    public override T Expect(string message) =>
        throw new UnmetExpectationException(message);

    /// <inheritdoc />
#if !DEBUG
    [DebuggerStepThrough]
    [StackTraceHidden]
#endif
    public override T Unwrap() =>
        throw new UnwrapException("Unwrap called for a `None` value.");

    /// <inheritdoc />
#if !DEBUG
    [DebuggerStepThrough]
    [StackTraceHidden]
#endif
    public override T UnwrapOr(T value) =>
        value;

    /// <inheritdoc />
#if !DEBUG
    [DebuggerStepThrough]
    [StackTraceHidden]
#endif
    public override T? UnwrapOrDefault() =>
        default;

    /// <inheritdoc />
#if !DEBUG
    [DebuggerStepThrough]
    [StackTraceHidden]
#endif
    public override T UnwrapOrElse(Func<T> @else) =>
        @else();

    /// <inheritdoc />
#if !DEBUG
    [DebuggerStepThrough]
    [StackTraceHidden]
#endif
    public override Option<T2> Map<T2>(Func<T, T2> map) =>
        Option.None<T2>();

    /// <inheritdoc />
#if !DEBUG
    [DebuggerStepThrough]
    [StackTraceHidden]
#endif
    public override T2 MapOr<T2>(T2 @default, Func<T, T2> map) =>
        @default;

    /// <inheritdoc />
#if !DEBUG
    [DebuggerStepThrough]
    [StackTraceHidden]
#endif
    public override T2 MapOrElse<T2>(
        Func<T2> createDefault,
        Func<T, T2> map) => createDefault();

    /// <inheritdoc />
#if !DEBUG
    [DebuggerStepThrough]
    [StackTraceHidden]
#endif
    public override Option<T> Inspect(Action<T> action) =>
        this;

    /// <inheritdoc />
#if !DEBUG
    [DebuggerStepThrough]
    [StackTraceHidden]
#endif
    public override Option<T> Filter(Func<T, bool> predicate) =>
        this;

    /// <inheritdoc />
#if !DEBUG
    [DebuggerStepThrough]
    [StackTraceHidden]
#endif
    public override Option<T> Or(Option<T> other) =>
        other;

    /// <inheritdoc />
#if !DEBUG
    [DebuggerStepThrough]
    [StackTraceHidden]
#endif
    public override Option<T> OrElse(Func<Option<T>> createElse) =>
        createElse();

    /// <inheritdoc />
#if !DEBUG
    [DebuggerStepThrough]
    [StackTraceHidden]
#endif
    public override Option<T> Xor(Option<T> other) =>
        other.IsSome ? other : this;

    /// <inheritdoc />
#if !DEBUG
    [DebuggerStepThrough]
    [StackTraceHidden]
#endif
    public override Option<(T, T2)> Zip<T2>(Option<T2> other) =>
        Option.None<(T, T2)>();

    /// <inheritdoc />
#if !DEBUG
    [DebuggerStepThrough]
    [StackTraceHidden]
#endif
    public override Option<TOut> ZipWith<TOther, TOut>(
        Option<TOther> other,
        Func<T, TOther, TOut> zip) =>
        Option.None<TOut>();

    /// <inheritdoc />
#if !DEBUG
    [DebuggerStepThrough]
    [StackTraceHidden]
#endif
    public override Result<T, TErr> OkOr<TErr>(TErr error) =>
        Result.Err<T, TErr>(error);

    /// <inheritdoc />
#if !DEBUG
    [DebuggerStepThrough]
    [StackTraceHidden]
#endif
    public override Result<T, TErr> OkOrElse<TErr>(Func<TErr> errorFactory) =>
        Result.Err<T, TErr>(errorFactory());
}
