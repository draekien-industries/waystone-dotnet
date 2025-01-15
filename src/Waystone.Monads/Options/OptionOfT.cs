﻿namespace Waystone.Monads.Options;

using System;
using System.Threading.Tasks;
using Exceptions;

/// <summary>
/// A type which can be in two states, a <see cref="Some{T}" /> or a
/// <see cref="None{T}" />.
/// </summary>
/// <typeparam name="T">The option value's type.</typeparam>
public abstract record Option<T> where T : notnull
{
    /// <summary>
    /// Returns <see langword="true" /> if the option is a
    /// <see cref="Some{T}" /> value.
    /// </summary>
    public abstract bool IsSome { get; }

    /// <summary>
    /// Returns <see langword="false" /> if the option is a
    /// <see cref="None{T}" /> value.
    /// </summary>
    public abstract bool IsNone { get; }

    /// <summary>
    /// Returns <see langword="true" /> if the option is a
    /// <see cref="Some{T}" /> and the value inside of it matches a predicate.
    /// </summary>
    /// <param name="predicate">The condition to evaluate the option against</param>
    public abstract bool IsSomeAnd(Func<T, bool> predicate);

    /// <inheritdoc cref="IsSomeAnd(System.Func{T,bool})" />
    public abstract Task<bool> IsSomeAnd(
        Func<T, Task<bool>> predicate);

    /// <inheritdoc cref="IsSomeAnd(System.Func{T,bool})" />
    public abstract ValueTask<bool> IsSomeAnd(
        Func<T, ValueTask<bool>> predicate);

    /// <summary>
    /// Returns <see langword="true" /> if the option is a
    /// <see cref="None{T}" /> or the value inside of it matches a predicate.
    /// </summary>
    /// <param name="predicate">The condition to evaluate the option against</param>
    public abstract bool IsNoneOr(Func<T, bool> predicate);

    /// <inheritdoc cref="IsNoneOr(System.Func{T,bool})" />
    public abstract Task<bool> IsNoneOr(Func<T, Task<bool>> predicate);

    /// <inheritdoc cref="IsNoneOr(System.Func{T,bool})" />
    public abstract ValueTask<bool> IsNoneOr(
        Func<T, ValueTask<bool>> predicate);

    /// <summary>
    /// Performs a <see langword="switch" /> on the option, invoking the
    /// <paramref name="onSome" /> callback when it is a <see cref="Some{T}" /> and the
    /// <paramref name="onNone" /> callback when it is a  <see cref="None{T}" />.
    /// </summary>
    /// <param name="onSome">A callback for handling the <see cref="Some{T}" /> case.</param>
    /// <param name="onNone">A callback for handling the <see cref="None{T}" /> case.</param>
    /// <typeparam name="TOut">The returned type.</typeparam>
    /// <returns>
    /// The output of either the <paramref name="onSome" /> or
    /// <paramref name="onNone" /> callback.
    /// </returns>
    public abstract TOut Match<TOut>(Func<T, TOut> onSome, Func<TOut> onNone);

    /// <summary>
    /// Performs a <see langword="switch" /> on the option, invoking the
    /// <paramref name="onSome" /> callback when it is a <see cref="Some{T}" /> and the
    /// <paramref name="onNone" /> callback when it is a  <see cref="None{T}" />.
    /// </summary>
    /// <param name="onSome">A callback for handling the <see cref="Some{T}" /> case.</param>
    /// <param name="onNone">A callback for handling the <see cref="None{T}" /> case.</param>
    /// <typeparam name="TOut">The returned type.</typeparam>
    /// <returns>
    /// The output of either the <paramref name="onSome" /> or
    /// <paramref name="onNone" /> callback.
    /// </returns>
    public abstract Task<TOut> Match<TOut>(
        Func<T, Task<TOut>> onSome,
        Func<Task<TOut>> onNone);

    /// <summary>
    /// Performs a <see langword="switch" /> on the option, invoking the
    /// <paramref name="onSome" /> callback when it is a <see cref="Some{T}" /> and the
    /// <paramref name="onNone" /> callback when it is a  <see cref="None{T}" />.
    /// </summary>
    /// <param name="onSome">A callback for handling the <see cref="Some{T}" /> case.</param>
    /// <param name="onNone">A callback for handling the <see cref="None{T}" /> case.</param>
    /// <typeparam name="TOut">The returned type.</typeparam>
    /// <returns>
    /// The output of either the <paramref name="onSome" /> or
    /// <paramref name="onNone" /> callback.
    /// </returns>
    public abstract ValueTask<TOut> Match<TOut>(
        Func<T, ValueTask<TOut>> onSome,
        Func<ValueTask<TOut>> onNone);

    /// <summary>
    /// Performs a <see langword="switch" /> on the option, invoking the
    /// <paramref name="onSome" /> callback when it is a <see cref="Some{T}" /> and the
    /// <paramref name="onNone" /> callback when it is a  <see cref="None{T}" />.
    /// </summary>
    /// <param name="onSome">A callback for handling the <see cref="Some{T}" /> case.</param>
    /// <param name="onNone">A callback for handling the <see cref="None{T}" /> case.</param>
    public abstract void Match(Action<T> onSome, Action onNone);

    /// <summary>
    /// Returns the contained <see cref="Some{T}" /> value, consuming the
    /// <see cref="Option{T}" />.
    /// </summary>
    /// <param name="message">A custom exception message</param>
    /// <exception cref="UnmetExpectationException">
    /// Thrown if the value is a
    /// <see cref="None{T}" /> with a custom message provided by
    /// <paramref name="message" />
    /// </exception>
    public abstract T Expect(string message);

    /// <summary>
    /// Returns the contained <see cref="Some{T}" /> value, consuming the
    /// <see cref="Option{T}" />.
    /// </summary>
    /// <remarks>
    /// Because this function may throw an exception, its use is generally
    /// discouraged. Instead, prefer to use the <code>Match</code> function and handle
    /// the <see cref="None{T}" /> case explicitly, or call <code>UnwrapOr</code>.
    /// <code>UnwrapOrElse</code>, or <code>UnwrapOrDefault</code>
    /// </remarks>
    /// <exception cref="UnwrapException">
    /// Throws if the option equals
    /// <see cref="None{T}" />
    /// </exception>
    public abstract T Unwrap();

    /// <summary>
    /// Returns the contained <see cref="Some{T}" /> value or a provided
    /// default.
    /// </summary>
    /// <param name="value">
    /// The default value to return on a <see cref="None{T}" />
    /// </param>
    public abstract T UnwrapOr(T value);

    /// <summary>
    /// Returns the contained <see cref="Some{T}" /> value or the
    /// <see langword="default" /> of <typeparamref name="T" />.
    /// </summary>
    public abstract T? UnwrapOrDefault();

    /// <summary>
    /// Returns the contained <see cref="Some{T}" /> value or computes it from
    /// a delegate.
    /// </summary>
    /// <param name="else">
    /// The delegate which computes the <see cref="None{T}" />
    /// value.
    /// </param>
    public abstract T UnwrapOrElse(Func<T> @else);

    /// <summary>
    /// Returns the contained <see cref="Some{T}" /> value or computes it from
    /// a delegate.
    /// </summary>
    /// <param name="else">
    /// The delegate which computes the <see cref="None{T}" />
    /// value.
    /// </param>
    public abstract Task<T> UnwrapOrElse(Func<Task<T>> @else);

    /// <summary>
    /// Returns the contained <see cref="Some{T}" /> value or computes it from
    /// a delegate.
    /// </summary>
    /// <param name="else">
    /// The delegate which computes the <see cref="None{T}" />
    /// value.
    /// </param>
    public abstract ValueTask<T> UnwrapOrElse(Func<ValueTask<T>> @else);

    /// <summary>
    /// Maps an <c>Option&lt;T&gt;</c> to an <c>Option&lt;T2&gt;</c> by
    /// applying a function to a contained value (if <see cref="Some{T}" />) or returns
    /// <see cref="None{T}" /> (if <see cref="None{T}" />).
    /// </summary>
    /// <param name="map">The map function.</param>
    /// <typeparam name="T2">The return type of the map function.</typeparam>
    public abstract Option<T2> Map<T2>(Func<T, T2> map) where T2 : notnull;

    /// <summary>
    /// Maps an <c>Option&lt;T&gt;</c> to an <c>Option&lt;T2&gt;</c> by
    /// applying a function to a contained value (if <see cref="Some{T}" />) or returns
    /// <see cref="None{T}" /> (if <see cref="None{T}" />).
    /// </summary>
    /// <param name="map">The map function.</param>
    /// <typeparam name="T2">The return type of the map function.</typeparam>
    public abstract Task<Option<T2>> Map<T2>(Func<T, Task<T2>> map)
        where T2 : notnull;

    /// <summary>
    /// Maps an <c>Option&lt;T&gt;</c> to an <c>Option&lt;T2&gt;</c> by
    /// applying a function to a contained value (if <see cref="Some{T}" />) or returns
    /// <see cref="None{T}" /> (if <see cref="None{T}" />).
    /// </summary>
    /// <param name="map">The map function.</param>
    /// <typeparam name="T2">The return type of the map function.</typeparam>
    public abstract ValueTask<Option<T2>> Map<T2>(Func<T, ValueTask<T2>> map)
        where T2 : notnull;

    /// <summary>
    /// Returns the provided default result (if <see cref="None{T}" />), or
    /// applies a function to the contained value (if <see cref="Some{T}" />).
    /// </summary>
    /// <param name="default">The default value for a <see cref="None{T}" />.</param>
    /// <param name="map">The map function.</param>
    /// <typeparam name="T2">The return type of the map function.</typeparam>
    public abstract T2 MapOr<T2>(T2 @default, Func<T, T2> map);

    /// <summary>
    /// Returns the provided default result (if <see cref="None{T}" />), or
    /// applies a function to the contained value (if <see cref="Some{T}" />).
    /// </summary>
    /// <param name="default">The default value for a <see cref="None{T}" />.</param>
    /// <param name="map">The map function.</param>
    /// <typeparam name="T2">The return type of the map function.</typeparam>
    public abstract Task<T2> MapOr<T2>(T2 @default, Func<T, Task<T2>> map);

    /// <summary>
    /// Returns the provided default result (if <see cref="None{T}" />), or
    /// applies a function to the contained value (if <see cref="Some{T}" />).
    /// </summary>
    /// <param name="default">The default value for a <see cref="None{T}" />.</param>
    /// <param name="map">The map function.</param>
    /// <typeparam name="T2">The return type of the map function.</typeparam>
    public abstract ValueTask<T2> MapOr<T2>(
        T2 @default,
        Func<T, ValueTask<T2>> map);

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
    public abstract T2 MapOrElse<T2>(Func<T2> createDefault, Func<T, T2> map);

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
    public abstract Task<T2> MapOrElse<T2>(
        Func<Task<T2>> createDefault,
        Func<T, Task<T2>> map);

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
    public abstract ValueTask<T2> MapOrElse<T2>(
        Func<ValueTask<T2>> createDefault,
        Func<T, ValueTask<T2>> map);

    /// <summary>
    /// Calls a function with a reference to the contained value if
    /// <see cref="Some{T}" />
    /// </summary>
    /// <param name="action">The function to execute against the value.</param>
    /// <returns>The original <see cref="Option{T}" /></returns>
    public abstract Option<T> Inspect(Action<T> action);

    /// <summary>
    /// Calls a function with a reference to the contained value if
    /// <see cref="Some{T}" />
    /// </summary>
    /// <param name="action">The function to execute against the value.</param>
    /// <returns>The original <see cref="Option{T}" /></returns>
    public abstract Task<Option<T>> Inspect(Func<T, Task> action);

    /// <summary>
    /// Calls a function with a reference to the contained value if
    /// <see cref="Some{T}" />
    /// </summary>
    /// <param name="action">The function to execute against the value.</param>
    /// <returns>The original <see cref="Option{T}" /></returns>
    public abstract ValueTask<Option<T>> Inspect(Func<T, ValueTask> action);

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
    public abstract Option<T> Filter(Func<T, bool> predicate);

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
    public abstract Task<Option<T>> Filter(Func<T, Task<bool>> predicate);

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
    public abstract ValueTask<Option<T>> Filter(
        Func<T, ValueTask<bool>> predicate);

    /// <summary>
    /// Returns the option if it contains a value, otherwise returns
    /// <paramref name="other" />
    /// </summary>
    /// <param name="other">The other option.</param>
    public abstract Option<T> Or(Option<T> other);

    /// <summary>
    /// Returns the option if it contains a value, otherwise invokes the
    /// <paramref name="createElse" /> function and returns the result.
    /// </summary>
    /// <param name="createElse">The function that will create the other option.</param>
    public abstract Option<T> OrElse(Func<Option<T>> createElse);

    /// <summary>
    /// Returns the option if it contains a value, otherwise invokes the
    /// <paramref name="createElse" /> function and returns the result.
    /// </summary>
    /// <param name="createElse">The function that will create the other option.</param>
    public abstract Task<Option<T>> OrElse(Func<Task<Option<T>>> createElse);

    /// <summary>
    /// Returns the option if it contains a value, otherwise invokes the
    /// <paramref name="createElse" /> function and returns the result.
    /// </summary>
    /// <param name="createElse">The function that will create the other option.</param>
    public abstract ValueTask<Option<T>> OrElse(
        Func<ValueTask<Option<T>>> createElse);

    /// <summary>
    /// Returns <see cref="Some{T}" /> if exactly one of
    /// <see langword="this" /> or <paramref name="other" /> is <see cref="Some{T}" />,
    /// otherwise returns <see cref="None{T}" />.
    /// </summary>
    /// <param name="other">The other option.</param>
    public abstract Option<T> Xor(Option<T> other);

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
    public abstract Option<(T, T2)> Zip<T2>(Option<T2> other)
        where T2 : notnull;

    /// <summary>
    /// Implicitly converts a value of type <typeparamref name="T" /> into an
    /// <see cref="Option{T}" />
    /// </summary>
    /// <param name="value">The value of the option</param>
    /// <returns>
    /// A <see cref="Some{T}" /> when the value is not the default of its
    /// type, otherwise a <see cref="None{T}" />
    /// </returns>
    public static implicit operator Option<T>(T value) =>
        Equals(value, default(T)) ? new None<T>() : new Some<T>(value);
}
