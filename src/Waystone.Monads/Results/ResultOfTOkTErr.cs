namespace Waystone.Monads.Results;

using System;
using System.Collections.Generic;
using Exceptions;
using Extensions;
using Options;
#if !DEBUG
using System.Diagnostics;
#endif

/// <summary>
/// A type that represents either a success (<see cref="Ok{TOk,TErr}" />)
/// or failure (<see cref="Err{TOk,TErr}" />).
/// </summary>
/// <typeparam name="TOk">The type of the <see cref="Ok{TOk,TErr}" /> value.</typeparam>
/// <typeparam name="TErr">The type of the <see cref="Err{TOk,TErr}" /> value.</typeparam>
#if !DEBUG
[DebuggerStepThrough]
#endif
public abstract record Result<TOk, TErr>
    where TOk : notnull where TErr : notnull
{
    internal Result()
    { }

    internal abstract void OnlyThisAssemblyMayDerive();

    /// <summary>
    /// Returns <see langword="true" /> if the result is
    /// <see cref="Ok{TOk,TErr}" />.
    /// </summary>
    public abstract bool IsOk { get; }

    /// <summary>
    /// Returns <see langword="true" /> if the result is
    /// <see cref="Err{TOk,TErr}" />.
    /// </summary>
    public abstract bool IsErr { get; }

    /// <summary>
    /// Returns <see langword="true" /> if the result is
    /// <see cref="Ok{TOk,TErr}" /> and the value inside of it matches a predicate.
    /// </summary>
    /// <param name="predicate">The condition that the ok value must satisfy</param>
    public abstract bool IsOkAnd(Func<TOk, bool> predicate);

    /// <summary>
    /// Returns <see langword="true" /> if the result is
    /// <see cref="Ok{TOk,TErr}" /> and the value inside of it matches a predicate
    /// that takes state instead of capturing it.
    /// </summary>
    /// <remarks>
    /// Handing the <paramref name="state" /> to the delegate rather than
    /// capturing it lets the delegate be <see langword="static" />, so the call
    /// allocates no closure. <c>WM2017</c> reports a capturing call that could
    /// use this overload.
    /// </remarks>
    /// <param name="state">
    /// The value the delegate would otherwise capture. It is passed through
    /// unchanged and is never inspected.
    /// </param>
    /// <param name="predicate">The condition that the ok value must satisfy</param>
    /// <typeparam name="TState">
    /// The type of the state passed to the predicate. It is unconstrained, so a
    /// null state is permitted.
    /// </typeparam>
    public abstract bool IsOkAnd<TState>(
        TState state,
        Func<TOk, TState, bool> predicate);

    /// <summary>
    /// Returns <see langword="true" /> if the result is
    /// <see cref="Err{TOk,TErr}" /> and the value inside of it matches a predicate.
    /// </summary>
    /// <param name="predicate">The condition that the error value must satisfy</param>
    public abstract bool IsErrAnd(Func<TErr, bool> predicate);

    /// <summary>
    /// Returns <see langword="true" /> if the result is
    /// <see cref="Err{TOk,TErr}" /> and the value inside of it matches a
    /// predicate that takes state instead of capturing it.
    /// </summary>
    /// <remarks>
    /// Handing the <paramref name="state" /> to the delegate rather than
    /// capturing it lets the delegate be <see langword="static" />, so the call
    /// allocates no closure. <c>WM2017</c> reports a capturing call that could
    /// use this overload.
    /// </remarks>
    /// <param name="state">
    /// The value the delegate would otherwise capture. It is passed through
    /// unchanged and is never inspected.
    /// </param>
    /// <param name="predicate">The condition that the error value must satisfy</param>
    /// <typeparam name="TState">
    /// The type of the state passed to the predicate. It is unconstrained, so a
    /// null state is permitted.
    /// </typeparam>
    public abstract bool IsErrAnd<TState>(
        TState state,
        Func<TErr, TState, bool> predicate);

    /// <summary>
    /// Performs a <see langword="switch" /> on the result, invoking the
    /// <paramref name="onOk" /> callback when it is a <see cref="Ok{TOk,TErr}" /> and
    /// the <paramref name="onErr" /> callback when it is a
    /// <see cref="Err{TOk,TErr}" />.
    /// </summary>
    /// <param name="onOk">
    /// A callback for handling the <see cref="Ok{TOk,TErr}" />
    /// case.
    /// </param>
    /// <param name="onErr">
    /// A callback for handling the <see cref="Err{TOk,TErr}" />
    /// case.
    /// </param>
    /// <typeparam name="TOut">The returned type.</typeparam>
    public abstract TOut Match<TOut>(
        Func<TOk, TOut> onOk,
        Func<TErr, TOut> onErr);

    /// <summary>
    /// Performs a <see langword="switch" /> on the result and returns what the
    /// callback for its case produces, with state passed to the callbacks
    /// rather than captured by them.
    /// </summary>
    /// <remarks>
    /// Handing the <paramref name="state" /> to the delegates rather than
    /// capturing it lets them be <see langword="static" />, so the call
    /// allocates no closure. <c>WM2017</c> reports a capturing call that could
    /// use this overload. A capturing <c>Match</c> allocates more than the
    /// single-delegate members do, because its two branches share one display
    /// class but need a delegate each.
    /// </remarks>
    /// <param name="state">
    /// The value the delegate would otherwise capture. It is passed through
    /// unchanged and is never inspected.
    /// </param>
    /// <param name="onOk">
    /// A callback for handling the <see cref="Ok{TOk,TErr}" />
    /// case.
    /// </param>
    /// <param name="onErr">
    /// A callback for handling the <see cref="Err{TOk,TErr}" />
    /// case.
    /// </param>
    /// <typeparam name="TState">
    /// The type of the state passed to the callbacks. It is unconstrained, so a
    /// null state is permitted.
    /// </typeparam>
    /// <typeparam name="TOut">The returned type.</typeparam>
    public abstract TOut Match<TState, TOut>(
        TState state,
        Func<TOk, TState, TOut> onOk,
        Func<TErr, TState, TOut> onErr);

    /// <summary>
    /// Performs a <see langword="switch" /> on the result, invoking the
    /// <paramref name="onOk" /> callback when it is a <see cref="Ok{TOk,TErr}" /> and
    /// the <paramref name="onErr" /> callback when it is a
    /// <see cref="Err{TOk,TErr}" />.
    /// </summary>
    /// <param name="onOk">
    /// A callback for handling the <see cref="Ok{TOk,TErr}" />
    /// case.
    /// </param>
    /// <param name="onErr">
    /// A callback for handling the <see cref="Err{TOk,TErr}" />
    /// case.
    /// </param>
    public abstract void Match(Action<TOk> onOk, Action<TErr> onErr);

    /// <summary>
    /// Performs a <see langword="switch" /> on the result for its side effect,
    /// with state passed to the callbacks rather than captured by them.
    /// </summary>
    /// <remarks>
    /// Handing the <paramref name="state" /> to the delegates rather than
    /// capturing it lets them be <see langword="static" />, so the call
    /// allocates no closure. <c>WM2017</c> reports a capturing call that could
    /// use this overload. A capturing <c>Match</c> allocates more than the
    /// single-delegate members do, because its two branches share one display
    /// class but need a delegate each.
    /// </remarks>
    /// <param name="state">
    /// The value the delegate would otherwise capture. It is passed through
    /// unchanged and is never inspected.
    /// </param>
    /// <param name="onOk">
    /// A callback for handling the <see cref="Ok{TOk,TErr}" />
    /// case.
    /// </param>
    /// <param name="onErr">
    /// A callback for handling the <see cref="Err{TOk,TErr}" />
    /// case.
    /// </param>
    /// <typeparam name="TState">
    /// The type of the state passed to the callbacks. It is unconstrained, so a
    /// null state is permitted.
    /// </typeparam>
    public abstract void Match<TState>(
        TState state,
        Action<TOk, TState> onOk,
        Action<TErr, TState> onErr);

    /// <summary>
    /// Returns <paramref name="other" /> if the <see langword="this" />
    /// instance is <see cref="Ok{TOk,TErr}" />, otherwise returns the
    /// <see cref="Err{TOk,TErr}" /> value of <see langword="this" /> instance.
    /// </summary>
    /// <example>
    /// <code>
    /// var x = Result.Ok(2);
    /// var y = Result.Err("late error");
    /// Assert.Equal(x.And(y), Result.Err("late error"));
    /// </code>
    /// </example>
    /// <example>
    /// <code>
    /// var x = Result.Err("early error");
    /// var y = Result.Ok(2);
    /// Assert.Equal(x.And(y), Result.Err("early error"));
    /// </code>
    /// </example>
    /// <example>
    /// <code>
    /// var x = Result.Err("first error");
    /// var y = Result.Err("second error");
    /// Assert.Equal(x.And(y), Result.Err("first error"));
    /// </code>
    /// </example>
    /// <example>
    /// <code>
    /// var x = Result.Ok(2);
    /// var y = Result.Ok("different result type");
    /// Assert.Equal(x.And(y), Result.Ok("different result type"));
    /// </code>
    /// </example>
    /// <param name="other">The other result type.</param>
    /// <typeparam name="TOut">
    /// The <see cref="Ok{TOk,TErr}" /> value's type of the
    /// other result.
    /// </typeparam>
    public abstract Result<TOut, TErr> And<TOut>(Result<TOut, TErr> other)
        where TOut : notnull;

    /// <summary>
    /// Calls the <paramref name="resultFactory" /> if the result is
    /// <see cref="Ok{TOk,TErr}" />, otherwise returns the <see cref="Err{TOk,TErr}" />
    /// value of <see langword="this" /> instance.
    /// </summary>
    /// <param name="resultFactory">A function that creates the other result.</param>
    /// <typeparam name="TOut">
    /// The <see cref="Ok{TOk,TErr}" /> value's type of the
    /// other result.
    /// </typeparam>
    public abstract Result<TOut, TErr> AndThen<TOut>(
        Func<TOk, Result<TOut, TErr>> resultFactory) where TOut : notnull;

    /// <summary>
    /// Calls the <paramref name="resultFactory" /> with <paramref name="state" />
    /// if the result is <see cref="Ok{TOk,TErr}" />, otherwise returns the
    /// <see cref="Err{TOk,TErr}" /> value of <see langword="this" /> instance.
    /// </summary>
    /// <remarks>
    /// Handing the <paramref name="state" /> to the delegate rather than
    /// capturing it lets the delegate be <see langword="static" />, so the call
    /// allocates no closure. <c>WM2017</c> reports a capturing call that could
    /// use this overload.
    /// </remarks>
    /// <param name="state">
    /// The value the delegate would otherwise capture. It is passed through
    /// unchanged and is never inspected.
    /// </param>
    /// <param name="resultFactory">A function that creates the other result.</param>
    /// <typeparam name="TState">
    /// The type of the state passed to the function. It is unconstrained, so a
    /// null state is permitted.
    /// </typeparam>
    /// <typeparam name="TOut">
    /// The <see cref="Ok{TOk,TErr}" /> value's type of the
    /// other result.
    /// </typeparam>
#if !DEBUG
    [DebuggerStepThrough]
#endif
    public Result<TOut, TErr> AndThen<TState, TOut>(
        TState state,
        Func<TOk, TState, Result<TOut, TErr>> resultFactory)
        where TOut : notnull =>
        Map(state, resultFactory).Flatten();

    /// <summary>
    /// Returns <paramref name="other" /> if the result is
    /// <see cref="Err{TOk,TErr}" />, otherwise returns the <see cref="Ok{TOk,TErr}" />
    /// value of this result instance.
    /// </summary>
    /// <param name="other">The other result.</param>
    /// <typeparam name="TOut">The other result's error value type</typeparam>
    public abstract Result<TOk, TOut> Or<TOut>(Result<TOk, TOut> other)
        where TOut : notnull;

    /// <summary>
    /// Calls <paramref name="resultFactory" /> if the result is
    /// <see cref="Err{TOk,TErr}" />, otherwise returns the <see cref="Ok{TOk,TErr}" />
    /// value of this result instance.
    /// </summary>
    /// <remarks>
    /// The delegate is not invoked on an <see cref="Ok{TOk,TErr}" />, so it is
    /// the lazy counterpart to <see cref="Or{TOut}" />, which evaluates its
    /// argument either way.
    /// </remarks>
    /// <param name="resultFactory">A function which creates the other result.</param>
    /// <typeparam name="TOut">The other result's error value type.</typeparam>
    public abstract Result<TOk, TOut> OrElse<TOut>(
        Func<TErr, Result<TOk, TOut>> resultFactory) where TOut : notnull;

    /// <summary>
    /// Calls a function that takes state instead of capturing it if the result
    /// is <see cref="Err{TOk,TErr}" />, otherwise returns the
    /// <see cref="Ok{TOk,TErr}" /> value of this result instance.
    /// </summary>
    /// <remarks>
    /// Handing the <paramref name="state" /> to the delegate rather than
    /// capturing it lets the delegate be <see langword="static" />, so the call
    /// allocates no closure. <c>WM2017</c> reports a capturing call that could
    /// use this overload. The delegate is not invoked on an
    /// <see cref="Ok{TOk,TErr}" />, so a capturing call allocates a closure it
    /// then discards.
    /// </remarks>
    /// <param name="state">
    /// The value the delegate would otherwise capture. It is passed through
    /// unchanged and is never inspected.
    /// </param>
    /// <param name="resultFactory">A function which creates the other result.</param>
    /// <typeparam name="TState">
    /// The type of the state passed to the function. It is unconstrained, so a
    /// null state is permitted.
    /// </typeparam>
    /// <typeparam name="TOut">The other result's error value type.</typeparam>
    public abstract Result<TOk, TOut> OrElse<TState, TOut>(
        TState state,
        Func<TErr, TState, Result<TOk, TOut>> resultFactory)
        where TOut : notnull;

    /// <summary>
    /// Returns the contained <see cref="Ok{TOk,TErr}" /> value, consuming the
    /// result instance.
    /// </summary>
    /// <remarks>
    /// Throws on an <see cref="Err{TOk,TErr}" />, differing from
    /// <see cref="Unwrap" /> only in that the thrown message leads with
    /// <paramref name="message" />. Prefer a member that cannot throw:
    /// <see cref="Match{TOut}(Func{TOk,TOut},Func{TErr,TOut})" /> to handle both
    /// cases explicitly, or <see cref="UnwrapOr" />,
    /// <see cref="UnwrapOrElse(Func{TErr,TOk})" /> or
    /// <see cref="UnwrapOrDefault" /> to supply a fallback.
    /// </remarks>
    /// <exception cref="UnmetExpectationException">
    /// Throws if the value is an
    /// <see cref="Err{TOk,TErr}" />, with an exception message including the passed
    /// <paramref name="message" />, and the content of the
    /// <see cref="Err{TOk,TErr}" />
    /// </exception>
    /// <param name="message">The custom exception message.</param>
    public abstract TOk Expect(string message);

    /// <summary>
    /// Returns the contained <see cref="Err{TOk,TErr}" /> value, consuming
    /// the result instance.
    /// </summary>
    /// <param name="message">The custom exception message.</param>
    /// <exception cref="UnmetExpectationException">
    /// Throws if the value is an
    /// <see cref="Ok{TOk,TErr}" />, with a message including the passed
    /// <paramref name="message" />, and the content of the <see cref="Ok{TOk,TErr}" />
    /// </exception>
    public abstract TErr ExpectErr(string message);

    /// <summary>
    /// Returns the contained <see cref="Ok{TOk,TErr}" /> value, consuming the
    /// result instance.
    /// </summary>
    /// <remarks>
    /// Throws on an <see cref="Err{TOk,TErr}" />, so prefer a member that
    /// cannot: <see cref="Match{TOut}(Func{TOk,TOut},Func{TErr,TOut})" /> to
    /// handle both cases explicitly, or <see cref="UnwrapOr" />,
    /// <see cref="UnwrapOrElse(Func{TErr,TOk})" /> or
    /// <see cref="UnwrapOrDefault" /> to supply a fallback.
    /// </remarks>
    /// <exception cref="UnwrapException">
    /// Throws if the value is an
    /// <see cref="Err{TOk,TErr}" />, with an exception message provided by the
    /// <see cref="Err{TOk,TErr}" /> value.
    /// </exception>
    public abstract TOk Unwrap();

    /// <summary>
    /// Returns the contained <see cref="Ok{TOk,TErr}" /> value or a provided
    /// default.
    /// </summary>
    /// <param name="defaultValue">
    /// The default value to return on an
    /// <see cref="Err{TOk,TErr}" />
    /// </param>
    public abstract TOk UnwrapOr(TOk defaultValue);

    /// <summary>
    /// Returns the contained <see cref="Ok{TOk,TErr}" /> value or the default
    /// value for <typeparamref name="TOk" />
    /// </summary>
    public abstract TOk? UnwrapOrDefault();

    /// <summary>
    /// Returns the contained <see cref="Ok{TOk,TErr}" /> value or computes it
    /// from the callback function.
    /// </summary>
    /// <param name="valueFactory">
    /// Produces the returned value from the contained error. It runs only on an
    /// <see cref="Err{TOk,TErr}" />.
    /// </param>
    public abstract TOk UnwrapOrElse(Func<TErr, TOk> valueFactory);

    /// <summary>
    /// Returns the contained <see cref="Ok{TOk,TErr}" /> value or computes it
    /// from a callback that takes state instead of capturing it.
    /// </summary>
    /// <remarks>
    /// Handing the <paramref name="state" /> to the delegate rather than
    /// capturing it lets the delegate be <see langword="static" />, so the call
    /// allocates no closure. <c>WM2017</c> reports a capturing call that could
    /// use this overload. The delegate is not invoked on an
    /// <see cref="Ok{TOk,TErr}" />, so a capturing call allocates a closure it
    /// then discards.
    /// </remarks>
    /// <param name="state">
    /// The value the delegate would otherwise capture. It is passed through
    /// unchanged and is never inspected.
    /// </param>
    /// <param name="valueFactory">
    /// Produces the returned value from the contained error. It runs only on an
    /// <see cref="Err{TOk,TErr}" />.
    /// </param>
    /// <typeparam name="TState">
    /// The type of the state passed to the callback. It is unconstrained, so a
    /// null state is permitted.
    /// </typeparam>
    public abstract TOk UnwrapOrElse<TState>(
        TState state,
        Func<TErr, TState, TOk> valueFactory);

    /// <summary>
    /// Returns the contained <see cref="Err{TOk,TErr}" /> value, consuming
    /// the result instance.
    /// </summary>
    /// <remarks>
    /// Throws on an <see cref="Ok{TOk,TErr}" />, so reach for it only where the
    /// result is already known to be an <see cref="Err{TOk,TErr}" />. Prefer
    /// <see cref="GetErr" />, which returns a <see cref="None{T}" /> instead of
    /// throwing.
    /// </remarks>
    /// <exception cref="UnwrapException">
    /// Throws if the result is an <see cref="Ok{TOk,TErr}" />, with an exception
    /// message provided by the <see cref="Ok{TOk,TErr}" />'s value.
    /// </exception>
    public abstract TErr UnwrapErr();

    /// <summary>
    /// Calls a function with a reference to the contained value if
    /// <see cref="Ok{TOk,TErr}" />
    /// </summary>
    /// <param name="action">The function to be invoked.</param>
    /// <returns>The original <see cref="Result{TOk,TErr}" />, unchanged.</returns>
    public abstract Result<TOk, TErr> Inspect(Action<TOk> action);

    /// <summary>
    /// Calls a function with the contained value and the state if
    /// <see cref="Ok{TOk,TErr}" />, so the function need not capture.
    /// </summary>
    /// <remarks>
    /// Handing the <paramref name="state" /> to the delegate rather than
    /// capturing it lets the delegate be <see langword="static" />, so the call
    /// allocates no closure. <c>WM2017</c> reports a capturing call that could
    /// use this overload.
    /// </remarks>
    /// <param name="state">
    /// The value the delegate would otherwise capture. It is passed through
    /// unchanged and is never inspected.
    /// </param>
    /// <param name="action">The function to be invoked.</param>
    /// <typeparam name="TState">
    /// The type of the state passed to the function. It is unconstrained, so a
    /// null state is permitted.
    /// </typeparam>
    /// <returns>The original <see cref="Result{TOk,TErr}" />, unchanged.</returns>
    public abstract Result<TOk, TErr> Inspect<TState>(
        TState state,
        Action<TOk, TState> action);

    /// <summary>
    /// Calls a function with a reference to the contained value if
    /// <see cref="Err{TOk,TErr}" />
    /// </summary>
    /// <param name="action">The function to be invoked.</param>
    /// <returns>The original <see cref="Result{TOk,TErr}" />, unchanged.</returns>
    public abstract Result<TOk, TErr> InspectErr(Action<TErr> action);

    /// <summary>
    /// Calls a function with the contained error and the state if
    /// <see cref="Err{TOk,TErr}" />, so the function need not capture.
    /// </summary>
    /// <remarks>
    /// Handing the <paramref name="state" /> to the delegate rather than
    /// capturing it lets the delegate be <see langword="static" />, so the call
    /// allocates no closure. <c>WM2017</c> reports a capturing call that could
    /// use this overload.
    /// </remarks>
    /// <param name="state">
    /// The value the delegate would otherwise capture. It is passed through
    /// unchanged and is never inspected.
    /// </param>
    /// <param name="action">The function to be invoked.</param>
    /// <typeparam name="TState">
    /// The type of the state passed to the function. It is unconstrained, so a
    /// null state is permitted.
    /// </typeparam>
    /// <returns>The original <see cref="Result{TOk,TErr}" />, unchanged.</returns>
    public abstract Result<TOk, TErr> InspectErr<TState>(
        TState state,
        Action<TErr, TState> action);

    /// <summary>
    /// Maps a <c>Result&lt;TOk, TErr&gt;</c> to
    /// <c>Result&lt;TOut, TErr&gt;</c> by applying a function to a contained
    /// <see cref="Ok{TOk,TErr}" /> value, leaving an <see cref="Err{TOk,TErr}" />
    /// untouched.
    /// </summary>
    /// <param name="map">The map function.</param>
    /// <typeparam name="TOut">The output value type.</typeparam>
    public abstract Result<TOut, TErr> Map<TOut>(Func<TOk, TOut> map)
        where TOut : notnull;

    /// <summary>
    /// Maps a <c>Result&lt;TOk, TErr&gt;</c> to
    /// <c>Result&lt;TOut, TErr&gt;</c> by applying a function to a contained
    /// <see cref="Ok{TOk,TErr}" /> value, leaving an <see cref="Err{TOk,TErr}" />
    /// untouched.
    /// </summary>
    /// <remarks>
    /// Handing the <paramref name="state" /> to the delegate rather than
    /// capturing it lets the delegate be <see langword="static" />, so the call
    /// allocates no closure. <c>WM2017</c> reports a capturing call that could
    /// use this overload.
    /// </remarks>
    /// <param name="state">
    /// The value the delegate would otherwise capture. It is passed through
    /// unchanged and is never inspected.
    /// </param>
    /// <param name="map">The map function.</param>
    /// <typeparam name="TState">
    /// The type of the state passed to the map function. It is unconstrained,
    /// so a null state is permitted.
    /// </typeparam>
    /// <typeparam name="TOut">The output value type.</typeparam>
    public abstract Result<TOut, TErr> Map<TState, TOut>(
        TState state,
        Func<TOk, TState, TOut> map) where TOut : notnull;

    /// <summary>
    /// Returns the provided default (if <see cref="Err{TOk,TErr}" />), or
    /// applies a function to the contained value (if <see cref="Ok{TOk,TErr}" />).
    /// </summary>
    /// <param name="defaultValue">
    /// The default value for an <see cref="Err{TOk,TErr}" />
    /// </param>
    /// <param name="map">The map function for an <see cref="Ok{TOk,TErr}" /></param>
    /// <typeparam name="TOut">The mapped result value type</typeparam>
    public abstract TOut MapOr<TOut>(TOut defaultValue, Func<TOk, TOut> map);

    /// <summary>
    /// Returns the provided default (if <see cref="Err{TOk,TErr}" />), or
    /// applies a function to the contained value (if <see cref="Ok{TOk,TErr}" />).
    /// </summary>
    /// <remarks>
    /// Handing the <paramref name="state" /> to the delegate rather than
    /// capturing it lets the delegate be <see langword="static" />, so the call
    /// allocates no closure. <c>WM2017</c> reports a capturing call that could
    /// use this overload.
    /// </remarks>
    /// <param name="state">
    /// The value the delegate would otherwise capture. It is passed through
    /// unchanged and is never inspected.
    /// </param>
    /// <param name="defaultValue">
    /// The default value for an <see cref="Err{TOk,TErr}" />
    /// </param>
    /// <param name="map">The map function for an <see cref="Ok{TOk,TErr}" /></param>
    /// <typeparam name="TState">
    /// The type of the state passed to the map function. It is unconstrained,
    /// so a null state is permitted.
    /// </typeparam>
    /// <typeparam name="TOut">The mapped result value type</typeparam>
    public abstract TOut MapOr<TState, TOut>(
        TState state,
        TOut defaultValue,
        Func<TOk, TState, TOut> map);

    /// <summary>
    /// Returns the <see langword="default" /> of <typeparamref name="TOut" /> (if
    /// <see cref="Err{TOk,TErr}" />), or applies a function to the contained value
    /// (if <see cref="Ok{TOk,TErr}" />).
    /// </summary>
    /// <param name="map">The map function for an <see cref="Ok{TOk,TErr}" /></param>
    /// <typeparam name="TOut">The mapped result value type</typeparam>
    public abstract TOut? MapOrDefault<TOut>(Func<TOk, TOut> map)
        where TOut : notnull;

    /// <summary>
    /// Returns the <see langword="default" /> of <typeparamref name="TOut" /> (if
    /// <see cref="Err{TOk,TErr}" />), or applies a function that takes state
    /// instead of capturing it to the contained value (if
    /// <see cref="Ok{TOk,TErr}" />).
    /// </summary>
    /// <remarks>
    /// Handing the <paramref name="state" /> to the delegate rather than
    /// capturing it lets the delegate be <see langword="static" />, so the call
    /// allocates no closure. <c>WM2017</c> reports a capturing call that could
    /// use this overload.
    /// </remarks>
    /// <param name="state">
    /// The value the delegate would otherwise capture. It is passed through
    /// unchanged and is never inspected.
    /// </param>
    /// <param name="map">The map function for an <see cref="Ok{TOk,TErr}" /></param>
    /// <typeparam name="TState">
    /// The type of the state passed to the map function. It is unconstrained, so a
    /// null state is permitted.
    /// </typeparam>
    /// <typeparam name="TOut">The mapped result value type</typeparam>
    public abstract TOut? MapOrDefault<TState, TOut>(
        TState state,
        Func<TOk, TState, TOut> map) where TOut : notnull;

    /// <summary>
    /// Maps a <c>Result&lt;TOk, TErr&gt;</c> to <typeparamref name="TOut" />
    /// by applying fallback function <paramref name="defaultFactory" /> to a contained
    /// <see cref="Err{TOk,TErr}" /> value, or the <paramref name="map" /> function to
    /// a contained <see cref="Ok{TOk,TErr}" /> value.
    /// </summary>
    /// <param name="defaultFactory">
    /// A function to create the default value for an
    /// <see cref="Err{TOk,TErr}" />
    /// </param>
    /// <param name="map">The map function for an <see cref="Ok{TOk,TErr}" /></param>
    /// <typeparam name="TOut">The mapped result value type</typeparam>
    /// <returns>
    /// What <paramref name="map" /> produces from the contained value on an
    /// <see cref="Ok{TOk,TErr}" />, otherwise what
    /// <paramref name="defaultFactory" /> produces from the contained error.
    /// </returns>
    public abstract TOut MapOrElse<TOut>(
        Func<TErr, TOut> defaultFactory,
        Func<TOk, TOut> map);

    /// <summary>
    /// Maps a <c>Result&lt;TOk, TErr&gt;</c> to <typeparamref name="TOut" />
    /// by applying fallback function <paramref name="defaultFactory" /> to a contained
    /// <see cref="Err{TOk,TErr}" /> value, or the <paramref name="map" /> function to
    /// a contained <see cref="Ok{TOk,TErr}" /> value.
    /// </summary>
    /// <remarks>
    /// Handing the <paramref name="state" /> to the delegates rather than
    /// capturing it lets them be <see langword="static" />, so the call
    /// allocates no closure. <c>WM2017</c> reports a capturing call that could
    /// use this overload.
    /// </remarks>
    /// <param name="state">
    /// The value the delegates would otherwise capture. It is passed through
    /// unchanged and is never inspected.
    /// </param>
    /// <param name="defaultFactory">
    /// A function to create the default value for an
    /// <see cref="Err{TOk,TErr}" />
    /// </param>
    /// <param name="map">The map function for an <see cref="Ok{TOk,TErr}" /></param>
    /// <typeparam name="TState">
    /// The type of the state passed to both functions. It is unconstrained, so
    /// a null state is permitted.
    /// </typeparam>
    /// <typeparam name="TOut">The mapped result value type</typeparam>
    public abstract TOut MapOrElse<TState, TOut>(
        TState state,
        Func<TErr, TState, TOut> defaultFactory,
        Func<TOk, TState, TOut> map);

    /// <summary>
    /// Maps a <c>Result&lt;TOk, TErr&gt;</c> to
    /// <c>Result&lt;TOk, TOut&gt;</c> by applying a function to a contained
    /// <see cref="Err{TOk,TErr}" /> value, leaving an <see cref="Ok{TOk,TErr}" />
    /// value untouched.
    /// </summary>
    /// <param name="map">
    /// The map function to apply to the <see cref="Err{TOk,TErr}" />
    /// </param>
    /// <typeparam name="TOut">The output error value type</typeparam>
    public abstract Result<TOk, TOut> MapErr<TOut>(Func<TErr, TOut> map)
        where TOut : notnull;

    /// <summary>
    /// Maps a <c>Result&lt;TOk, TErr&gt;</c> to
    /// <c>Result&lt;TOk, TOut&gt;</c> by applying a function to a contained
    /// <see cref="Err{TOk,TErr}" /> value, leaving an <see cref="Ok{TOk,TErr}" />
    /// value untouched.
    /// </summary>
    /// <remarks>
    /// Handing the <paramref name="state" /> to the delegate rather than
    /// capturing it lets the delegate be <see langword="static" />, so the call
    /// allocates no closure. <c>WM2017</c> reports a capturing call that could
    /// use this overload.
    /// </remarks>
    /// <param name="state">
    /// The value the delegate would otherwise capture. It is passed through
    /// unchanged and is never inspected.
    /// </param>
    /// <param name="map">
    /// The map function to apply to the <see cref="Err{TOk,TErr}" />
    /// </param>
    /// <typeparam name="TState">
    /// The type of the state passed to the map function. It is unconstrained,
    /// so a null state is permitted.
    /// </typeparam>
    /// <typeparam name="TOut">The output error value type</typeparam>
    public abstract Result<TOk, TOut> MapErr<TState, TOut>(
        TState state,
        Func<TErr, TState, TOut> map) where TOut : notnull;

    /// <summary>
    /// Converts from a <see cref="Result{TOk,TErr}" /> into an
    /// <c>Option&lt;TOk&gt;</c>
    /// </summary>
    /// <returns>
    /// A <see cref="Some{T}" /> holding the success value on an
    /// <see cref="Ok{TOk,TErr}" />, otherwise a <see cref="None{T}" />. The
    /// error is discarded, so call <see cref="GetErr" /> first if you need it.
    /// </returns>
    public abstract Option<TOk> GetOk();

    /// <summary>
    /// Converts from a <see cref="Result{TOk,TErr}" /> to
    /// <c>Option&lt;TErr&gt;</c>
    /// </summary>
    /// <returns>
    /// A <see cref="Some{T}" /> holding the error on an
    /// <see cref="Err{TOk,TErr}" />, otherwise a <see cref="None{T}" />. The
    /// success value is discarded, so call <see cref="GetOk" /> first if you
    /// need it.
    /// </returns>
    public abstract Option<TErr> GetErr();

    /// <summary>
    /// Returns a sequence over the possibly contained
    /// <see cref="Ok{TOk,TErr}" /> value.
    /// </summary>
    /// <returns>
    /// A sequence yielding the contained value once if the result is an
    /// <see cref="Ok{TOk,TErr}" />, otherwise an empty sequence. The error of an
    /// <see cref="Err{TOk,TErr}" /> is discarded.
    /// </returns>
    public abstract IEnumerable<TOk> AsEnumerable();

    /// <summary>
    /// Implicitly creates an <see cref="Ok{TOk,TErr}" /> result from a value
    /// of type <typeparamref name="TOk" />
    /// </summary>
    /// <param name="value">The <typeparamref name="TOk" /> value</param>
    /// <returns>The created <see cref="Result{TOk,TErr}" /></returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value" /> is null. Use
    /// <c>Result.Try</c> when a null is a possible outcome
    /// you would rather handle than have thrown at you.
    /// </exception>
    public static implicit operator Result<TOk, TErr>(TOk value) =>
        Result.Ok<TOk, TErr>(value);

    /// <summary>
    /// Implicitly creates an <see cref="Err{TOk,TErr}" /> result from a value
    /// of type <typeparamref name="TErr" />
    /// </summary>
    /// <param name="value">The <typeparamref name="TErr" /> value</param>
    /// <returns>The created <see cref="Result{TOk,TErr}" /></returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value" /> is null.
    /// </exception>
    public static implicit operator Result<TOk, TErr>(TErr value) =>
        Result.Err<TOk, TErr>(value);
}
