namespace Waystone.Monads;

using System;
using Exceptions;

/// <summary>
/// A type which can be in two states, a <see cref="Some{T}" /> or a
/// <see cref="None{T}" />.
/// </summary>
/// <typeparam name="T">The option value's type.</typeparam>
public interface IOption<T>
    where T : notnull
{
    /// <summary>
    /// Returns <see langword="true" /> if the option is a
    /// <see cref="Some{T}" /> value.
    /// </summary>
    bool IsSome { get; }

    /// <summary>
    /// Returns <see langword="false" /> if the option is a
    /// <see cref="None{T}" /> value.
    /// </summary>
    bool IsNone { get; }

    /// <summary>
    /// Returns <see langword="true" /> if the option is a
    /// <see cref="Some{T}" /> and the value inside of it matches a predicate.
    /// </summary>
    /// <param name="predicate">A <see cref="Predicate{T}" /></param>
    bool IsSomeAnd(Predicate<T> predicate);

    /// <summary>
    /// Returns <see langword="true" /> if the option is a
    /// <see cref="None{T}" /> or the value inside of it matches a predicate.
    /// </summary>
    /// <param name="predicate">A <see cref="Predicate{T}" /></param>
    bool IsNoneOr(Predicate<T> predicate);

    /// <summary>
    /// Performs a <see langword="switch" /> on the option, invoking the
    /// <paramref name="onSome" /> callback when it is a <see cref="Some{T}" /> and the
    /// <paramref name="onNone" /> callback when it is a  <see cref="None{T}" />.
    /// </summary>
    /// <param name="onSome">
    /// A <see cref="Func{T, TResult}" /> for handling the
    /// <see cref="Some{T}" /> case.
    /// </param>
    /// <param name="onNone">
    /// A <see cref="Func{TResult}" /> for handling the
    /// <see cref="None{T}" /> case.
    /// </param>
    /// <typeparam name="TOut">The returned type.</typeparam>
    /// <returns>
    /// The output of either the <paramref name="onSome" /> or
    /// <paramref name="onNone" /> callback.
    /// </returns>
    TOut Match<TOut>(
        Func<T, TOut> onSome,
        Func<TOut> onNone);

    /// <summary>
    /// Performs a <see langword="switch" /> on the option, invoking the
    /// <paramref name="onSome" /> callback when it is a <see cref="Some{T}" /> and the
    /// <paramref name="onNone" /> callback when it is a  <see cref="None{T}" />.
    /// </summary>
    /// <param name="onSome">
    /// A <see cref="Action{T}" /> for handling the
    /// <see cref="Some{T}" /> case.
    /// </param>
    /// <param name="onNone">
    /// A <see cref="Action" /> for handling the
    /// <see cref="None{T}" /> case.
    /// </param>
    void Match(Action<T> onSome, Action onNone);

    /// <summary>
    /// Returns the contained <see cref="Some{T}" /> value, consuming the
    /// <see cref="IOption{T}" />.
    /// </summary>
    /// <param name="message">A custom exception message</param>
    /// <exception cref="UnmetExpectationException">
    /// Thrown if the value is a
    /// <see cref="None{T}" /> with a custom message provided by
    /// <paramref name="message" />
    /// </exception>
    T Expect(string message);

    /// <summary>
    /// Returns the contained <see cref="Some{T}" /> value, consuming the
    /// <see cref="IOption{T}" />.
    /// </summary>
    /// <remarks>
    /// Because this function may throw an exception, its use is generally
    /// discouraged. Instead, prefer to use the <see cref="IOption{T}.Match{TOut}" />
    /// function and handle the <see cref="None{T}" /> case explicitly, or call
    /// <see cref="IOption{T}.UnwrapOr" />, <see cref="IOption{T}.UnwrapOrElse" />, or
    /// <see cref="IOption{T}.UnwrapOrDefault" />.
    /// </remarks>
    /// <exception cref="UnwrapException">
    /// Throws if the option equals
    /// <see cref="None{T}" />
    /// </exception>
    T Unwrap();

    /// <summary>
    /// Returns the contained <see cref="Some{T}" /> value or a provided
    /// default.
    /// </summary>
    /// <param name="value">
    /// The default value to return on a <see cref="None{T}" />
    /// </param>
    T UnwrapOr(T value);

    /// <summary>
    /// Returns the contained <see cref="Some{T}" /> value or the
    /// <see langword="default" /> of <typeparamref name="T" />.
    /// </summary>
    T? UnwrapOrDefault();

    /// <summary>
    /// Returns the contained <see cref="Some{T}" /> value or computes it from
    /// a delegate.
    /// </summary>
    /// <param name="else">
    /// The delegate which computes the <see cref="None{T}" />
    /// value.
    /// </param>
    T UnwrapOrElse(Func<T> @else);

    /// <summary>
    /// Maps an <c>Option&lt;T&gt;</c> to an <c>Option&lt;T2&gt;</c> by
    /// applying a function to a contained value (if <see cref="Some{T}" />) or returns
    /// <see cref="None{T}" /> (if <see cref="None{T}" />).
    /// </summary>
    /// <param name="map">The map function.</param>
    /// <typeparam name="T2">The return type of the map function.</typeparam>
    IOption<T2> Map<T2>(Func<T, T2> map) where T2 : notnull;

    /// <summary>
    /// Returns the provided default result (if <see cref="None{T}" />), or
    /// applies a function to the contained value (if <see cref="Some{T}" />).
    /// </summary>
    /// <param name="default">The default value for a <see cref="None{T}" />.</param>
    /// <param name="map">The map function.</param>
    /// <typeparam name="T2">The return type of the map function.</typeparam>
    T2 MapOr<T2>(T2 @default, Func<T, T2> map);

    /// <summary>
    /// Computes a default from a function (if <see cref="None{T}" />), or
    /// applies a function to the contained value (if <see cref="Some{T}" />).
    /// </summary>
    /// <param name="createDefault">
    /// The function that will create a default value for a
    /// <see cref="None{T}" />.
    /// </param>
    /// <param name="map">The map function.</param>
    /// <typeparam name="T2">The return type of the map function.</typeparam>
    T2 MapOrElse<T2>(
        Func<T2> createDefault,
        Func<T, T2> map);

    /// <summary>
    /// Calls a function with a reference to the contained value if
    /// <see cref="Some{T}" />
    /// </summary>
    /// <param name="action">The function to execute against the value.</param>
    /// <returns>The original <see cref="IOption{T}" /></returns>
    IOption<T> Inspect(Action<T> action);

    /// <summary>
    /// Returns <see cref="None{T}" /> if the option is <see cref="None{T}" />,
    /// otherwise calls the <paramref name="predicate" /> with the wrapped value and
    /// returns:
    /// <list type="bullet">
    /// <item>
    /// <see cref="Some{T}" /> if the <paramref name="predicate" /> returns
    /// <see langword="true" /> (where <typeparamref name="T" /> is the wrapped value),
    /// and
    /// </item>
    /// <item>
    /// <see cref="None{T}" /> if the <paramref name="predicate" /> returns
    /// <see langword="false" />.
    /// </item>
    /// </list>
    /// </summary>
    /// <param name="predicate">The filter function.</param>
    IOption<T> Filter(Predicate<T> predicate);

    /// <summary>
    /// Returns the option if it contains a value, otherwise returns
    /// <paramref name="other" />
    /// </summary>
    /// <param name="other">The other option.</param>
    IOption<T> Or(IOption<T> other);

    /// <summary>
    /// Returns the option if it contains a value, otherwise invokes the
    /// <paramref name="createElse" /> function and returns the result.
    /// </summary>
    /// <param name="createElse">The function that will create the other option.</param>
    IOption<T> OrElse(Func<IOption<T>> createElse);

    /// <summary>
    /// Returns <see cref="Some{T}" /> if exactly one of
    /// <see langword="this" /> or <paramref name="other" /> is <see cref="Some{T}" />,
    /// otherwise returns <see cref="None{T}" />.
    /// </summary>
    /// <param name="other">The other option.</param>
    IOption<T> Xor(IOption<T> other);

    /// <summary>
    /// Zips the current option with another option, combining the values into
    /// a tuple.
    /// </summary>
    /// <param name="other">The other option.</param>
    /// <typeparam name="T2">The type of the value contained in the other option.</typeparam>
    /// <returns>
    /// If the current option is <see cref="Some{T}" /> and
    /// <paramref name="other" /> is <see cref="Some{T}" />, this method returns
    /// <c>Some&lt;(T, T2)&gt;</c>. Otherwise, <c>None&lt;(T, T2)&gt;</c> is returned.
    /// </returns>
    IOption<(T, T2)> Zip<T2>(IOption<T2> other) where T2 : notnull;
}
