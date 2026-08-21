namespace Waystone.Monads.Options;

using System;
using System.Collections.Generic;
using Results;
#if !DEBUG
using System.Diagnostics;
#endif

/// <summary>Some value of type <typeparamref name="T" /></summary>
/// <typeparam name="T">
/// The type belonging to the value inside the
/// <see cref="Some{T}" />
/// </typeparam>
#if !DEBUG
[DebuggerStepThrough]
#endif
public sealed record Some<T> : Option<T>
    where T : notnull
{
    internal Some(T value)
    {
        if (value is null)
        {
            throw new ArgumentNullException(
                nameof(value),
                "The value of a `Some` option cannot be null.");
        }

        Value = value;
    }

    private T Value { get; }

    /// <inheritdoc />
    public override bool IsSome => true;

    /// <inheritdoc />
    public override bool IsNone => false;

    /// <inheritdoc />
    public override bool IsSomeAnd(Func<T, bool> predicate) =>
        predicate(Value);

    /// <inheritdoc />
    public override bool IsNoneOr(Func<T, bool> predicate) =>
        predicate(Value);

    /// <inheritdoc />
    public override TOut Match<TOut>(
        Func<T, TOut> onSome,
        Func<TOut> onNone) => onSome(Value);

    /// <inheritdoc />
    public override void Match(Action<T> onSome, Action onNone)
    {
        onSome(Value);
    }

    /// <inheritdoc />
    public override T Expect(string message) =>
        Value;

    /// <inheritdoc />
    public override T Unwrap() => Value;

    /// <inheritdoc />
    public override T UnwrapOr(T value) =>
        Value;

    /// <inheritdoc />
    public override T UnwrapOrDefault() =>
        Value;

    /// <inheritdoc />
    public override T UnwrapOrElse(Func<T> @else) =>
        Value;

    /// <inheritdoc />
    public override Option<TOut> And<TOut>(Option<TOut> other) =>
        other;

    /// <inheritdoc />
    public override Option<TOut> Map<TOut>(Func<T, TOut> map) =>
        map(Value);

    /// <inheritdoc />
    public override Option<TOut> Map<TState, TOut>(
        TState state,
        Func<T, TState, TOut> map) => map(Value, state);

    /// <inheritdoc />
    public override TOut MapOr<TOut>(TOut @default, Func<T, TOut> map) =>
        map(Value);

    /// <inheritdoc />
    public override TOut MapOr<TState, TOut>(
        TState state,
        TOut @default,
        Func<T, TState, TOut> map) => map(Value, state);

    /// <inheritdoc />
    public override TOut MapOrDefault<TOut>(Func<T, TOut> map) =>
        map(Value);

    /// <inheritdoc />
    public override TOut MapOrElse<TOut>(
        Func<TOut> createDefault,
        Func<T, TOut> map) => Match(map, createDefault);

    /// <inheritdoc />
    public override TOut MapOrElse<TState, TOut>(
        TState state,
        Func<TState, TOut> createDefault,
        Func<T, TState, TOut> map) => map(Value, state);

    /// <inheritdoc />
    public override Option<T> Inspect(Action<T> action)
    {
        action(Value);
        return this;
    }

    /// <inheritdoc />
    public override Option<T> Filter(Func<T, bool> predicate) =>
        predicate(Value) ? this : Option.None<T>();

    /// <inheritdoc />
    public override Option<T> Filter<TState>(
        TState state,
        Func<T, TState, bool> predicate) =>
        predicate(Value, state) ? this : Option.None<T>();

    /// <inheritdoc />
    public override Option<T> Or(Option<T> other) =>
        this;

    /// <inheritdoc />
    public override Option<T> OrElse(Func<Option<T>> createElse) =>
        this;

    /// <inheritdoc />
    public override Option<T> Xor(Option<T> other) =>
        other.IsSome ? Option.None<T>() : this;

    /// <inheritdoc />
    public override Option<(T, TOther)> Zip<TOther>(Option<TOther> other)
    {
        if (other is Some<TOther> otherSome)
        {
            return Option.Some((Value, otherSome.Value));
        }

        return Option.None<(T, TOther)>();
    }

    /// <inheritdoc />
    public override Option<TOut> ZipWith<TOther, TOut>(
        Option<TOther> other,
        Func<T, TOther, TOut> zip) =>
        other.Map(otherValue => zip(Value, otherValue));

    /// <inheritdoc />
    public override Option<T> Reduce(Option<T> other, Func<T, T, T> reduce) =>
        other.Match<Option<T>>(
            otherValue => reduce(Value, otherValue),
            () => this);

    /// <inheritdoc />
    public override IEnumerable<T> AsEnumerable() =>
        new[] { Value };

    /// <inheritdoc />
    public override Result<T, TErr> OkOr<TErr>(TErr error) =>
        Result.Ok<T, TErr>(Value);

    /// <inheritdoc />
    public override Result<T, TErr> OkOrElse<TErr>(Func<TErr> errorFactory) =>
        Result.Ok<T, TErr>(Value);

    internal override void OnlyThisAssemblyMayDerive()
    { }
}
