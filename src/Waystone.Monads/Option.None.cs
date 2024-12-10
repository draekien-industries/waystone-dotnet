namespace Waystone.Monads;

using System;
using Exceptions;

/// <summary>No value of type <typeparamref name="T" />.</summary>
/// <typeparam name="T">The option value's type.</typeparam>
public sealed record None<T> : IOption<T>
    where T : notnull
{
    /// <inheritdoc />
    public bool IsSome => false;

    /// <inheritdoc />
    public bool IsNone => true;

    /// <inheritdoc />
    public bool IsSomeAnd(Predicate<T> predicate) =>
        false;

    /// <inheritdoc />
    public bool IsNoneOr(Predicate<T> predicate) =>
        true;

    /// <inheritdoc />
    public TOut Match<TOut>(
        Func<T, TOut> onSome,
        Func<TOut> onNone) => onNone();

    /// <inheritdoc />
    public void Match(Action<T> onSome, Action onNone)
    {
        onNone();
    }

    /// <inheritdoc />
    public T Expect(string message) =>
        throw new UnmetExpectationException(message);

    /// <inheritdoc />
    public T Unwrap() =>
        throw new UnwrapException("Unwrap called for a `None` value.");

    /// <inheritdoc />
    public T UnwrapOr(T value) =>
        value;

    /// <inheritdoc />
    public T? UnwrapOrDefault() =>
        default;

    /// <inheritdoc />
    public T UnwrapOrElse(Func<T> @else) =>
        @else();

    /// <inheritdoc />
    public IOption<T2> Map<T2>(Func<T, T2> map) where T2 : notnull =>
        Option.None<T2>();

    /// <inheritdoc />
    public T2 MapOr<T2>(T2 @default, Func<T, T2> map) =>
        @default;

    /// <inheritdoc />
    public T2 MapOrElse<T2>(
        Func<T2> createDefault,
        Func<T, T2> map) => createDefault();

    /// <inheritdoc />
    public IOption<T> Inspect(Action<T> action) =>
        this;

    /// <inheritdoc />
    public IOption<T> Filter(Predicate<T> predicate) =>
        this;

    /// <inheritdoc />
    public IOption<T> Or(IOption<T> other) =>
        other;

    /// <inheritdoc />
    public IOption<T> OrElse(Func<IOption<T>> createElse) =>
        createElse();

    /// <inheritdoc />
    public IOption<T> Xor(IOption<T> other) =>
        other.IsSome ? other : this;

    /// <inheritdoc />
    public IOption<(T, T2)> Zip<T2>(IOption<T2> other) where T2 : notnull =>
        Option.None<(T, T2)>();
}
