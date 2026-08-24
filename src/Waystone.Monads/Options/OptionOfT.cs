namespace Waystone.Monads.Options;

using System;
using System.Collections.Generic;
using Exceptions;
using Extensions;
using Results;
#if !DEBUG
using System.Diagnostics;
#endif

/// <summary>
/// A type which can be in two states, a <see cref="Some{T}" /> or a
/// <see cref="None{T}" />.
/// </summary>
/// <typeparam name="T">The option value's type.</typeparam>
#if !DEBUG
[DebuggerStepThrough]
#endif
public abstract record Option<T> where T : notnull
{
    internal Option()
    { }

    internal abstract void OnlyThisAssemblyMayDerive();

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

    /// <summary>
    /// Returns <see langword="true" /> if the option is a
    /// <see cref="Some{T}" /> and the value inside of it matches a predicate.
    /// </summary>
    /// <remarks>
    /// The <paramref name="state" /> is handed to the delegate rather than
    /// captured by it, so the delegate can be <see langword="static" /> and the
    /// call allocates no closure.
    /// </remarks>
    /// <param name="state">The value passed to the predicate.</param>
    /// <param name="predicate">The condition to evaluate the option against</param>
    /// <typeparam name="TState">The type of the state passed to the predicate.</typeparam>
    public abstract bool IsSomeAnd<TState>(
        TState state,
        Func<T, TState, bool> predicate);

    /// <summary>
    /// Returns <see langword="true" /> if the option is a
    /// <see cref="None{T}" /> or the value inside of it matches a predicate.
    /// </summary>
    /// <param name="predicate">The condition to evaluate the option against</param>
    public abstract bool IsNoneOr(Func<T, bool> predicate);

    /// <summary>
    /// Returns <see langword="true" /> if the option is a
    /// <see cref="None{T}" /> or the value inside of it matches a predicate.
    /// </summary>
    /// <remarks>
    /// The <paramref name="state" /> is handed to the delegate rather than
    /// captured by it, so the delegate can be <see langword="static" /> and the
    /// call allocates no closure.
    /// </remarks>
    /// <param name="state">The value passed to the predicate.</param>
    /// <param name="predicate">The condition to evaluate the option against</param>
    /// <typeparam name="TState">The type of the state passed to the predicate.</typeparam>
    public abstract bool IsNoneOr<TState>(
        TState state,
        Func<T, TState, bool> predicate);

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
    /// <remarks>
    /// The <paramref name="state" /> is handed to both delegates rather than
    /// captured by them, so they can be <see langword="static" /> and the call
    /// allocates no closure. A capturing <c>Match</c> is the most expensive
    /// call in the library: the two branches share one display class but need a
    /// delegate each.
    /// </remarks>
    /// <param name="state">The value passed to both callbacks.</param>
    /// <param name="onSome">A callback for handling the <see cref="Some{T}" /> case.</param>
    /// <param name="onNone">A callback for handling the <see cref="None{T}" /> case.</param>
    /// <typeparam name="TState">The type of the state passed to both callbacks.</typeparam>
    /// <typeparam name="TOut">The returned type.</typeparam>
    /// <returns>
    /// The output of either the <paramref name="onSome" /> or
    /// <paramref name="onNone" /> callback.
    /// </returns>
    public abstract TOut Match<TState, TOut>(
        TState state,
        Func<T, TState, TOut> onSome,
        Func<TState, TOut> onNone);

    /// <summary>
    /// Performs a <see langword="switch" /> on the option, invoking the
    /// <paramref name="onSome" /> callback when it is a <see cref="Some{T}" /> and the
    /// <paramref name="onNone" /> callback when it is a  <see cref="None{T}" />.
    /// </summary>
    /// <param name="onSome">A callback for handling the <see cref="Some{T}" /> case.</param>
    /// <param name="onNone">A callback for handling the <see cref="None{T}" /> case.</param>
    public abstract void Match(Action<T> onSome, Action onNone);

    /// <summary>
    /// Performs a <see langword="switch" /> on the option, invoking the
    /// <paramref name="onSome" /> callback when it is a <see cref="Some{T}" /> and the
    /// <paramref name="onNone" /> callback when it is a  <see cref="None{T}" />.
    /// </summary>
    /// <remarks>
    /// The <paramref name="state" /> is handed to both delegates rather than
    /// captured by them, so they can be <see langword="static" /> and the call
    /// allocates no closure. A capturing <c>Match</c> is the most expensive
    /// call in the library: the two branches share one display class but need a
    /// delegate each.
    /// </remarks>
    /// <param name="state">The value passed to both callbacks.</param>
    /// <param name="onSome">A callback for handling the <see cref="Some{T}" /> case.</param>
    /// <param name="onNone">A callback for handling the <see cref="None{T}" /> case.</param>
    /// <typeparam name="TState">The type of the state passed to both callbacks.</typeparam>
    public abstract void Match<TState>(
        TState state,
        Action<T, TState> onSome,
        Action<TState> onNone);

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
    /// <remarks>
    /// The <paramref name="state" /> is handed to the delegate rather than
    /// captured by it, so the delegate can be <see langword="static" /> and the
    /// call allocates no closure. On a <see cref="Some{T}" /> the delegate is
    /// never invoked, so a capturing call pays for a closure it does not use.
    /// </remarks>
    /// <param name="state">The value passed to the delegate.</param>
    /// <param name="else">
    /// The delegate which computes the <see cref="None{T}" />
    /// value.
    /// </param>
    /// <typeparam name="TState">The type of the state passed to the delegate.</typeparam>
    public abstract T UnwrapOrElse<TState>(TState state, Func<TState, T> @else);

    /// <summary>
    /// Maps an <c>Option&lt;T&gt;</c> to an <c>Option&lt;TOut&gt;</c> by
    /// applying a function to a contained value (if <see cref="Some{T}" />) or returns
    /// <see cref="None{T}" /> (if <see cref="None{T}" />).
    /// </summary>
    /// <param name="map">The map function.</param>
    /// <typeparam name="TOut">The return type of the map function.</typeparam>
    public abstract Option<TOut> Map<TOut>(Func<T, TOut> map) where TOut : notnull;

    /// <summary>
    /// Maps an <c>Option&lt;T&gt;</c> to an <c>Option&lt;TOut&gt;</c> by
    /// applying a function to a contained value (if <see cref="Some{T}" />) or returns
    /// <see cref="None{T}" /> (if <see cref="None{T}" />).
    /// </summary>
    /// <remarks>
    /// The <paramref name="state" /> is handed to the delegate rather than
    /// captured by it, so the delegate can be <see langword="static" /> and the
    /// call allocates no closure.
    /// </remarks>
    /// <param name="state">The value passed to the map function.</param>
    /// <param name="map">The map function.</param>
    /// <typeparam name="TState">The type of the state passed to the map function.</typeparam>
    /// <typeparam name="TOut">The return type of the map function.</typeparam>
    public abstract Option<TOut> Map<TState, TOut>(
        TState state,
        Func<T, TState, TOut> map) where TOut : notnull;

    /// <summary>
    /// Returns <see cref="None{T}" /> if the option is a <see cref="None{T}" />,
    /// otherwise returns <paramref name="other" />.
    /// </summary>
    /// <remarks>
    /// <paramref name="other" /> is eagerly evaluated. If you are passing the
    /// result of a function call, prefer <see cref="AndThen{TOut}" />, which is
    /// lazily evaluated.
    /// </remarks>
    /// <param name="other">The option to return when this one is a <see cref="Some{T}" />.</param>
    /// <typeparam name="TOut">The type of the value contained in the other option.</typeparam>
    public abstract Option<TOut> And<TOut>(Option<TOut> other) where TOut : notnull;

    /// <summary>
    /// Returns <see cref="None{T}" /> if the option is a <see cref="None{T}" />,
    /// otherwise calls <paramref name="map" /> with the wrapped value and returns
    /// the result.
    /// </summary>
    /// <remarks>
    /// Often used to chain fallible operations that may return
    /// <see cref="None{T}" />.
    /// </remarks>
    /// <param name="map">
    /// A transform function to apply to the inner value if the
    /// option is a <see cref="Some{T}" />.
    /// </param>
    /// <typeparam name="TOut">The type of the value contained in the resulting option.</typeparam>
    /// <returns>
    /// A flattened <see cref="Option{TOut}" /> resulting from applying the
    /// transform function and flattening the nested option.
    /// </returns>
#if !DEBUG
    [DebuggerStepThrough]
#endif
    public Option<TOut> AndThen<TOut>(Func<T, Option<TOut>> map) where TOut : notnull =>
        Map(map).Flatten();

    /// <summary>
    /// Returns <see cref="None{T}" /> if the option is a <see cref="None{T}" />,
    /// otherwise calls <paramref name="map" /> with the wrapped value and the
    /// <paramref name="state" /> and returns the result.
    /// </summary>
    /// <remarks>
    /// The <paramref name="state" /> is handed to the delegate rather than
    /// captured by it, so the delegate can be <see langword="static" /> and the
    /// call allocates no closure.
    /// </remarks>
    /// <param name="state">The value passed to the map function.</param>
    /// <param name="map">
    /// A transform function to apply to the inner value if the
    /// option is a <see cref="Some{T}" />.
    /// </param>
    /// <typeparam name="TState">The type of the state passed to the map function.</typeparam>
    /// <typeparam name="TOut">The type of the value contained in the resulting option.</typeparam>
    /// <returns>
    /// A flattened <see cref="Option{TOut}" /> resulting from applying the
    /// transform function and flattening the nested option.
    /// </returns>
#if !DEBUG
    [DebuggerStepThrough]
#endif
    public Option<TOut> AndThen<TState, TOut>(
        TState state,
        Func<T, TState, Option<TOut>> map) where TOut : notnull =>
        Map(state, map).Flatten();

    /// <summary>
    /// Returns the provided default result (if <see cref="None{T}" />), or
    /// applies a function to the contained value (if <see cref="Some{T}" />).
    /// </summary>
    /// <param name="default">The default value for a <see cref="None{T}" />.</param>
    /// <param name="map">The map function.</param>
    /// <typeparam name="TOut">The return type of the map function.</typeparam>
    public abstract TOut MapOr<TOut>(TOut @default, Func<T, TOut> map);

    /// <summary>
    /// Returns the provided default result (if <see cref="None{T}" />), or
    /// applies a function to the contained value (if <see cref="Some{T}" />).
    /// </summary>
    /// <remarks>
    /// The <paramref name="state" /> is handed to the delegate rather than
    /// captured by it, so the delegate can be <see langword="static" /> and the
    /// call allocates no closure.
    /// </remarks>
    /// <param name="state">The value passed to the map function.</param>
    /// <param name="default">The default value for a <see cref="None{T}" />.</param>
    /// <param name="map">The map function.</param>
    /// <typeparam name="TState">The type of the state passed to the map function.</typeparam>
    /// <typeparam name="TOut">The return type of the map function.</typeparam>
    public abstract TOut MapOr<TState, TOut>(
        TState state,
        TOut @default,
        Func<T, TState, TOut> map);

    /// <summary>
    /// Returns the <see langword="default" /> of <typeparamref name="TOut" /> (if
    /// <see cref="None{T}" />), or applies a function to the contained value (if
    /// <see cref="Some{T}" />).
    /// </summary>
    /// <param name="map">The map function.</param>
    /// <typeparam name="TOut">The return type of the map function.</typeparam>
    public abstract TOut? MapOrDefault<TOut>(Func<T, TOut> map) where TOut : notnull;

    /// <summary>
    /// Returns the <see langword="default" /> of <typeparamref name="TOut" /> (if
    /// <see cref="None{T}" />), or applies a function to the contained value (if
    /// <see cref="Some{T}" />).
    /// </summary>
    /// <remarks>
    /// The <paramref name="state" /> is handed to the delegate rather than
    /// captured by it, so the delegate can be <see langword="static" /> and the
    /// call allocates no closure.
    /// </remarks>
    /// <param name="state">The value passed to the map function.</param>
    /// <param name="map">The map function.</param>
    /// <typeparam name="TState">The type of the state passed to the map function.</typeparam>
    /// <typeparam name="TOut">The return type of the map function.</typeparam>
    public abstract TOut? MapOrDefault<TState, TOut>(
        TState state,
        Func<T, TState, TOut> map) where TOut : notnull;

    /// <summary>
    /// Computes a default from a function (if <see cref="None{T}" />), or
    /// applies a function to the contained value (if <see cref="Some{T}" />).
    /// </summary>
    /// <param name="createDefault">
    /// The function that will create a default value for a
    /// <see cref="None{T}" />.
    /// </param>
    /// <param name="map">The map function.</param>
    /// <typeparam name="TOut">The return type of the map function.</typeparam>
    public abstract TOut MapOrElse<TOut>(Func<TOut> createDefault, Func<T, TOut> map);

    /// <summary>
    /// Computes a default from a function (if <see cref="None{T}" />), or
    /// applies a function to the contained value (if <see cref="Some{T}" />).
    /// </summary>
    /// <remarks>
    /// The <paramref name="state" /> is handed to the delegate rather than
    /// captured by it, so the delegate can be <see langword="static" /> and the
    /// call allocates no closure.
    /// </remarks>
    /// <param name="state">The value passed to both functions.</param>
    /// <param name="createDefault">
    /// The function that will create a default value for a
    /// <see cref="None{T}" />.
    /// </param>
    /// <param name="map">The map function.</param>
    /// <typeparam name="TState">The type of the state passed to both functions.</typeparam>
    /// <typeparam name="TOut">The return type of the map function.</typeparam>
    public abstract TOut MapOrElse<TState, TOut>(
        TState state,
        Func<TState, TOut> createDefault,
        Func<T, TState, TOut> map);

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
    /// <remarks>
    /// The <paramref name="state" /> is handed to the delegate rather than
    /// captured by it, so the delegate can be <see langword="static" /> and the
    /// call allocates no closure.
    /// </remarks>
    /// <param name="state">The value passed to the function.</param>
    /// <param name="action">The function to execute against the value.</param>
    /// <typeparam name="TState">The type of the state passed to the function.</typeparam>
    /// <returns>The original <see cref="Option{T}" /></returns>
    public abstract Option<T> Inspect<TState>(
        TState state,
        Action<T, TState> action);

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
    /// the <paramref name="state" />, returning <see cref="Some{T}" /> when it
    /// returns <see langword="true" /> and <see cref="None{T}" /> when it returns
    /// <see langword="false" />.
    /// </summary>
    /// <remarks>
    /// The <paramref name="state" /> is handed to the delegate rather than
    /// captured by it, so the delegate can be <see langword="static" /> and the
    /// call allocates no closure.
    /// </remarks>
    /// <param name="state">The value passed to the predicate.</param>
    /// <param name="predicate">The filter function.</param>
    /// <typeparam name="TState">The type of the state passed to the predicate.</typeparam>
    public abstract Option<T> Filter<TState>(
        TState state,
        Func<T, TState, bool> predicate);

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
    /// <remarks>
    /// The <paramref name="state" /> is handed to the delegate rather than
    /// captured by it, so the delegate can be <see langword="static" /> and the
    /// call allocates no closure. On a <see cref="Some{T}" /> the delegate is
    /// never invoked, so a capturing call pays for a closure it does not use.
    /// </remarks>
    /// <param name="state">The value passed to the function.</param>
    /// <param name="createElse">The function that will create the other option.</param>
    /// <typeparam name="TState">The type of the state passed to the function.</typeparam>
    public abstract Option<T> OrElse<TState>(
        TState state,
        Func<TState, Option<T>> createElse);

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
    /// <typeparam name="TOther">The type of the value contained in the other option.</typeparam>
    /// <returns>
    /// If the current option is <see cref="Some{T}" /> and
    /// <paramref name="other" /> is <see cref="Some{T}" />, this method returns
    /// <c>Some&lt;(T, TOther)&gt;</c>. Otherwise, <c>None&lt;(T, TOther)&gt;</c> is returned.
    /// </returns>
    public abstract Option<(T, TOther)> Zip<TOther>(Option<TOther> other)
        where TOther : notnull;

    /// <summary>
    /// Zips the current option with another option using the provided
    /// function.
    /// </summary>
    /// <typeparam name="TOther">The type of the value contained in the other option.</typeparam>
    /// <typeparam name="TOut">The output value's type.</typeparam>
    /// <param name="other">The option to zip.</param>
    /// <param name="zip">The function that will perform the zip operation.</param>
    /// <returns>
    /// If the current option is <see cref="Some{T}" /> and
    /// <paramref name="other" /> is <see cref="Some{T}" />, this method returns
    /// <c>Some&lt;TOut&gt;</c> where <c>TOut</c> is the result of applying
    /// <paramref name="zip" /> to the values of both options. Otherwise,
    /// <c>None&lt;TOut&gt;</c> is returned.
    /// </returns>
    public abstract Option<TOut> ZipWith<TOther, TOut>(
        Option<TOther> other,
        Func<T, TOther, TOut> zip)
        where TOther : notnull
        where TOut : notnull;

    /// <summary>Merges the current option with another option.</summary>
    /// <remarks>
    /// Unlike <see cref="ZipWith{TOther,TOut}" />, an option that is a
    /// <see cref="Some{T}" /> survives a <see cref="None{T}" /> on the other side.
    /// </remarks>
    /// <param name="other">The option to merge with.</param>
    /// <param name="reduce">The function that combines two present values.</param>
    /// <returns>
    /// <c>Some(reduce(a, b))</c> when both options are <see cref="Some{T}" />,
    /// whichever option is <see cref="Some{T}" /> when only one of them is, and
    /// <see cref="None{T}" /> when neither is.
    /// </returns>
    public abstract Option<T> Reduce(Option<T> other, Func<T, T, T> reduce);

    /// <summary>Returns a sequence over the possibly contained value.</summary>
    /// <returns>
    /// A sequence yielding the contained value once if the option is a
    /// <see cref="Some{T}" />, otherwise an empty sequence.
    /// </returns>
    public abstract IEnumerable<T> AsEnumerable();

    /// <summary>
    /// Transforms the current <see cref="Option{T}" /> into a
    /// <see cref="Result{TOk, TErr}" />, mapping <see cref="Some{T}" /> to
    /// <see cref="Ok{TOk, TErr}" /> and <see cref="None{T}" /> to
    /// <see cref="Err{TOk, TErr}" />.
    /// </summary>
    /// <remarks>
    /// Arguments passed to this method must be eagerly evauated. If you are
    /// passing the result of a function call, it is recommended to use
    /// <see cref="OkOrElse{TErr}" />, which is lazily evaluated.
    /// </remarks>
    /// <typeparam name="TErr">The type of the error value.</typeparam>
    /// <param name="error">
    /// The error to return when the current option is a
    /// <see cref="None{T}" />.
    /// </param>
    /// <returns>
    /// An <see cref="Ok{TOk, TErr}" /> if the current option is a
    /// <see cref="Some{T}" />, otherwise an <see cref="Err{TOk, TErr}" />.
    /// </returns>
    public abstract Result<T, TErr> OkOr<TErr>(TErr error)
        where TErr : notnull;

    /// <summary>
    /// Transforms the current <see cref="Option{T}" /> into a
    /// <see cref="Result{TOk, TErr}" />, mapping <see cref="Some{T}" /> to
    /// <see cref="Ok{TOk, TErr}" /> and <see cref="None{T}" /> to
    /// <see cref="Err{TOk, TErr}" />.
    /// </summary>
    /// <remarks>
    /// The <paramref name="errorFactory" /> is lazily evaluated, meaning it
    /// will only be invoked if the current option is a <see cref="None{T}" />.
    /// </remarks>
    /// <typeparam name="TErr">The type of the error value returned by the factory.</typeparam>
    /// <param name="errorFactory">
    /// The function, which when invoked, will return the
    /// error value.
    /// </param>
    /// <returns>
    /// An <see cref="Ok{TOk, TErr}" /> if the current option is a
    /// <see cref="Some{T}" />, otherwise an <see cref="Err{TOk, TErr}" />.
    /// </returns>
    public abstract Result<T, TErr> OkOrElse<TErr>(Func<TErr> errorFactory)
        where TErr : notnull;

    /// <summary>
    /// Transforms the <see cref="Option{T}" /> into a
    /// <see cref="Result{TOk, TErr}" />, mapping <see cref="Some{T}" /> to an
    /// <see cref="Ok{TOk, TErr}" /> and <see cref="None{T}" /> to an
    /// <see cref="Err{TOk, TErr}" /> built by the factory.
    /// </summary>
    /// <remarks>
    /// The <paramref name="errorFactory" /> is lazily evaluated, meaning it
    /// will only be invoked if the current option is a <see cref="None{T}" />.
    /// The <paramref name="state" /> is handed to it rather than captured by
    /// it, so it can be <see langword="static" /> and the call allocates no
    /// closure — which on a <see cref="Some{T}" /> is a closure that would
    /// never have been used.
    /// </remarks>
    /// <param name="state">The value passed to the factory.</param>
    /// <param name="errorFactory">
    /// The function, which when invoked, will return the
    /// error value.
    /// </param>
    /// <typeparam name="TState">The type of the state passed to the factory.</typeparam>
    /// <typeparam name="TErr">The type of the error value returned by the factory.</typeparam>
    /// <returns>
    /// An <see cref="Ok{TOk, TErr}" /> if the current option is a
    /// <see cref="Some{T}" />, otherwise an <see cref="Err{TOk, TErr}" />.
    /// </returns>
    public abstract Result<T, TErr> OkOrElse<TState, TErr>(
        TState state,
        Func<TState, TErr> errorFactory) where TErr : notnull;

    /// <summary>
    /// Implicitly converts a value of type <typeparamref name="T" /> into an
    /// <see cref="Option{T}" />
    /// </summary>
    /// <param name="value">The value of the option</param>
    /// <returns>
    /// A <see cref="Some{T}" /> when the value is not null, otherwise a
    /// <see cref="None{T}" />
    /// </returns>
#if !DEBUG
    [DebuggerStepThrough]
#endif
    public static implicit operator Option<T>(T value) =>
        value is null ? Option.None<T>() : new Some<T>(value);
}
