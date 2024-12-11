namespace Waystone.Monads;

using System;
using Exceptions;

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
    public override bool IsSomeAnd(Predicate<T> predicate) =>
        false;

    /// <inheritdoc />
    public override bool IsNoneOr(Predicate<T> predicate) =>
        true;

    /// <inheritdoc />
    public override TOut Match<TOut>(
        Func<T, TOut> onSome,
        Func<TOut> onNone) => onNone();

    /// <inheritdoc />
    public override void Match(Action<T> onSome, Action onNone)
    {
        onNone();
    }

    /// <inheritdoc />
    public override T Expect(string message) =>
        throw new UnmetExpectationException(message);

    /// <inheritdoc />
    public override T Unwrap() =>
        throw new UnwrapException("Unwrap called for a `None` value.");

    /// <inheritdoc />
    public override T UnwrapOr(T value) =>
        value;

    /// <inheritdoc />
    public override T? UnwrapOrDefault() =>
        default;

    /// <inheritdoc />
    public override T UnwrapOrElse(Func<T> @else) =>
        @else();

    /// <inheritdoc />
    public override Option<T2> Map<T2>(Func<T, T2> map) =>
        Option.None<T2>();

    /// <inheritdoc />
    public override T2 MapOr<T2>(T2 @default, Func<T, T2> map) =>
        @default;

    /// <inheritdoc />
    public override T2 MapOrElse<T2>(
        Func<T2> createDefault,
        Func<T, T2> map) => createDefault();

    /// <inheritdoc />
    public override Option<T> Inspect(Action<T> action) =>
        this;

    /// <inheritdoc />
    public override Option<T> Filter(Predicate<T> predicate) =>
        this;

    /// <inheritdoc />
    public override Option<T> Or(Option<T> other) =>
        other;

    /// <inheritdoc />
    public override Option<T> OrElse(Func<Option<T>> createElse) =>
        createElse();

    /// <inheritdoc />
    public override Option<T> Xor(Option<T> other) =>
        other.IsSome ? other : this;

    /// <inheritdoc />
    public override Option<(T, T2)> Zip<T2>(Option<T2> other) =>
        Option.None<(T, T2)>();
}
