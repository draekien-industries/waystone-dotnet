namespace Waystone.Monads.Options;

using System;
using Results;

/// <summary>Some value of type <typeparamref name="T" /></summary>
/// <typeparam name="T">
/// The type belonging to the value inside the
/// <see cref="Some{T}" />
/// </typeparam>
public sealed record Some<T> : Option<T>
    where T : notnull
{
    internal Some(T value)
    {
        if (Equals(value, default(T)))
        {
            throw new InvalidOperationException(
                $"The value of a `Some` option cannot be it's default value `{default(T)}`");
        }

        Value = value;
    }

    private T Value { get; }

    /// <inheritdoc />
    public override bool IsSome => true;

    /// <inheritdoc />
    public override bool IsNone => false;

    /// <inheritdoc />
#if !DEBUG
    [DebuggerStepThrough]
    [StackTraceHidden]
#endif
    public override bool IsSomeAnd(Func<T, bool> predicate) =>
        predicate(Value);

    /// <inheritdoc />
#if !DEBUG
    [DebuggerStepThrough]
    [StackTraceHidden]
#endif
    public override bool IsNoneOr(Func<T, bool> predicate) =>
        predicate(Value);

    /// <inheritdoc />
#if !DEBUG
    [DebuggerStepThrough]
    [StackTraceHidden]
#endif
    public override TOut Match<TOut>(
        Func<T, TOut> onSome,
        Func<TOut> onNone) => onSome(Value);

    /// <inheritdoc />
#if !DEBUG
    [DebuggerStepThrough]
    [StackTraceHidden]
#endif
    public override void Match(Action<T> onSome, Action onNone)
    {
        onSome(Value);
    }

    /// <inheritdoc />
#if !DEBUG
    [DebuggerStepThrough]
    [StackTraceHidden]
#endif
    public override T Expect(string message) =>
        Value;

    /// <inheritdoc />
#if !DEBUG
    [DebuggerStepThrough]
    [StackTraceHidden]
#endif
    public override T Unwrap() => Value;

    /// <inheritdoc />
#if !DEBUG
    [DebuggerStepThrough]
    [StackTraceHidden]
#endif
    public override T UnwrapOr(T value) =>
        Value;

    /// <inheritdoc />
#if !DEBUG
    [DebuggerStepThrough]
    [StackTraceHidden]
#endif
    public override T UnwrapOrDefault() =>
        Value;

    /// <inheritdoc />
#if !DEBUG
    [DebuggerStepThrough]
    [StackTraceHidden]
#endif
    public override T UnwrapOrElse(Func<T> @else) =>
        Value;

    /// <inheritdoc />
#if !DEBUG
    [DebuggerStepThrough]
    [StackTraceHidden]
#endif
    public override Option<T2> Map<T2>(Func<T, T2> map) =>
        map(Value);

    /// <inheritdoc />
#if !DEBUG
    [DebuggerStepThrough]
    [StackTraceHidden]
#endif
    public override T2 MapOr<T2>(T2 @default, Func<T, T2> map) =>
        map(Value);

    /// <inheritdoc />
#if !DEBUG
    [DebuggerStepThrough]
    [StackTraceHidden]
#endif
    public override T2 MapOrElse<T2>(
        Func<T2> createDefault,
        Func<T, T2> map) => Match(map, createDefault);

    /// <inheritdoc />
#if !DEBUG
    [DebuggerStepThrough]
    [StackTraceHidden]
#endif
    public override Option<T> Inspect(Action<T> action)
    {
        action(Value);
        return this;
    }

    /// <inheritdoc />
#if !DEBUG
    [DebuggerStepThrough]
    [StackTraceHidden]
#endif
    public override Option<T> Filter(Func<T, bool> predicate) =>
        predicate(Value) ? this : Option.None<T>();

    /// <inheritdoc />
#if !DEBUG
    [DebuggerStepThrough]
    [StackTraceHidden]
#endif
    public override Option<T> Or(Option<T> other) =>
        this;

    /// <inheritdoc />
#if !DEBUG
    [DebuggerStepThrough]
    [StackTraceHidden]
#endif
    public override Option<T> OrElse(Func<Option<T>> createElse) =>
        this;

    /// <inheritdoc />
#if !DEBUG
    [DebuggerStepThrough]
    [StackTraceHidden]
#endif
    public override Option<T> Xor(Option<T> other) =>
        other.IsSome ? Option.None<T>() : this;

    /// <inheritdoc />
#if !DEBUG
    [DebuggerStepThrough]
    [StackTraceHidden]
#endif
    public override Option<(T, T2)> Zip<T2>(Option<T2> other)
    {
        if (other is Some<T2> otherSome)
        {
            return Option.Some((Value, otherSome.Value));
        }

        return Option.None<(T, T2)>();
    }

    /// <inheritdoc />
#if !DEBUG
    [DebuggerStepThrough]
    [StackTraceHidden]
#endif
    public override Option<TOut> ZipWith<TOther, TOut>(
        Option<TOther> other,
        Func<T, TOther, TOut> zip) =>
        other.Map(otherValue => zip(Value, otherValue));

    /// <inheritdoc />
#if !DEBUG
    [DebuggerStepThrough]
    [StackTraceHidden]
#endif
    public override Result<T, TErr> OkOr<TErr>(TErr error) =>
        Result.Ok<T, TErr>(Value);

    /// <inheritdoc />
#if !DEBUG
    [DebuggerStepThrough]
    [StackTraceHidden]
#endif
    public override Result<T, TErr> OkOrElse<TErr>(Func<TErr> errorFactory) =>
        Result.Ok<T, TErr>(Value);
}
