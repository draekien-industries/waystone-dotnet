namespace Waystone.Monads.Results;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Exceptions;
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
    /// Checks whether the result is an <see cref="Ok{TOk,TErr}" /> whose value
    /// satisfies an asynchronous predicate.
    /// </summary>
    /// <remarks>
    /// <paramref name="predicate" /> is not invoked on an
    /// <see cref="Err{TOk,TErr}" />, so any side effect it carries does not run in
    /// that case and the call completes synchronously.
    /// </remarks>
    /// <param name="predicate">
    /// The asynchronous condition to evaluate against the contained ok value.
    /// </param>
    /// <returns>
    /// True if the result is an <see cref="Ok{TOk,TErr}" /> and
    /// <paramref name="predicate" /> returned true; false otherwise.
    /// </returns>
    public abstract ValueTask<bool> IsOkAndAsync(Func<TOk, Task<bool>> predicate);

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
    /// Checks whether the result is an <see cref="Err{TOk,TErr}" /> whose error
    /// satisfies an asynchronous predicate.
    /// </summary>
    /// <remarks>
    /// <paramref name="predicate" /> is not invoked on an
    /// <see cref="Ok{TOk,TErr}" />, so any side effect it carries does not run in
    /// that case and the call completes synchronously.
    /// </remarks>
    /// <param name="predicate">
    /// The asynchronous condition to evaluate against the contained error.
    /// </param>
    /// <returns>
    /// True if the result is an <see cref="Err{TOk,TErr}" /> and
    /// <paramref name="predicate" /> returned true; false otherwise.
    /// </returns>
    public abstract ValueTask<bool> IsErrAndAsync(
        Func<TErr, Task<bool>> predicate);

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
    /// Awaits whichever of two asynchronous branches the result selects, and
    /// returns what it produced.
    /// </summary>
    /// <remarks>
    /// The overload to reach for when both branches do real asynchronous work.
    /// Where only one does, prefer the overload taking the other branch
    /// synchronously — it avoids wrapping a value in an already-completed task.
    /// </remarks>
    /// <param name="onOk">Produces the result from the contained ok value.</param>
    /// <param name="onErr">Produces the result from the contained error.</param>
    /// <typeparam name="TOut">The type both branches produce.</typeparam>
    /// <returns>Whatever the branch taken produced.</returns>
    public abstract ValueTask<TOut> MatchAsync<TOut>(
        Func<TOk, Task<TOut>> onOk,
        Func<TErr, Task<TOut>> onErr);

    /// <summary>
    /// Runs whichever of two asynchronous branches the result selects, for its
    /// side effect alone.
    /// </summary>
    /// <remarks>
    /// The overload to reach for when both branches do real asynchronous work.
    /// Neither branch returns a value, so this is the asynchronous counterpart of
    /// the <see cref="Match(Action{TOk},Action{TErr})" /> switch rather than of the
    /// mapping one.
    /// </remarks>
    /// <param name="onOk">Handles the contained ok value.</param>
    /// <param name="onErr">Handles the contained error.</param>
    public abstract ValueTask MatchAsync(
        Func<TOk, Task> onOk,
        Func<TErr, Task> onErr);

    /// <summary>
    /// Runs the result's two branches for their side effect, where only the ok
    /// branch is asynchronous.
    /// </summary>
    /// <param name="onOk">Handles the contained ok value.</param>
    /// <param name="onErr">Handles the contained error, synchronously.</param>
    /// <returns>
    /// A <see cref="ValueTask" /> that has already completed when the result is an
    /// <see cref="Err{TOk,TErr}" />.
    /// </returns>
    public abstract ValueTask MatchAsync(
        Func<TOk, Task> onOk,
        Action<TErr> onErr);

    /// <summary>
    /// Runs the result's two branches for their side effect, where only the error
    /// branch is asynchronous.
    /// </summary>
    /// <param name="onOk">Handles the contained ok value, synchronously.</param>
    /// <param name="onErr">Handles the contained error.</param>
    /// <returns>
    /// A <see cref="ValueTask" /> that has already completed when the result is an
    /// <see cref="Ok{TOk,TErr}" />.
    /// </returns>
    public abstract ValueTask MatchAsync(
        Action<TOk> onOk,
        Func<TErr, Task> onErr);

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
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="resultFactory" /> returns a null result. Returning
    /// null rather than an <see cref="Err{TOk,TErr}" /> is never meaningful, and
    /// left alone it would surface as a <see cref="NullReferenceException" />
    /// at whatever called into the result next.
    /// </exception>
    public abstract Result<TOut, TErr> AndThen<TOut>(
        Func<TOk, Result<TOut, TErr>> resultFactory) where TOut : notnull;

    /// <summary>
    /// Chains an asynchronous operation onto an <see cref="Ok{TOk,TErr}" />,
    /// carrying an <see cref="Err{TOk,TErr}" /> straight through.
    /// </summary>
    /// <remarks>
    /// <paramref name="resultFactory" /> is not invoked on an
    /// <see cref="Err{TOk,TErr}" />; the contained error is re-wrapped for the new
    /// ok type instead. Any exception the returned task faults with surfaces to the
    /// caller unchanged rather than becoming an <see cref="Err{TOk,TErr}" />.
    /// </remarks>
    /// <param name="resultFactory">
    /// Produces the next result from the contained ok value.
    /// </param>
    /// <typeparam name="TOut">
    /// The ok value type of the result <paramref name="resultFactory" /> produces.
    /// </typeparam>
    /// <returns>
    /// The result <paramref name="resultFactory" /> produced, or the original error
    /// as an <see cref="Err{TOk,TErr}" /> of <typeparamref name="TOut" />.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="resultFactory" /> returns a null result. Returning
    /// null rather than an <see cref="Err{TOk,TErr}" /> is never meaningful, and
    /// left alone it would surface as a <see cref="NullReferenceException" />
    /// at whatever called into the result next. It is thrown from the call when
    /// the factory's task had already completed and faults the returned task
    /// otherwise, so await the result to see it either way.
    /// </exception>
    public abstract ValueTask<Result<TOut, TErr>> AndThenAsync<TOut>(
        Func<TOk, ValueTask<Result<TOut, TErr>>> resultFactory)
        where TOut : notnull;

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
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="resultFactory" /> returns a null result. Returning
    /// null rather than an <see cref="Err{TOk,TErr}" /> is never meaningful, and
    /// left alone it would surface as a <see cref="NullReferenceException" />
    /// at whatever called into the result next.
    /// </exception>
#if !DEBUG
    [DebuggerStepThrough]
#endif
    public Result<TOut, TErr> AndThen<TState, TOut>(
        TState state,
        Func<TOk, TState, Result<TOut, TErr>> resultFactory)
        where TOut : notnull =>
        Match(
            (state, resultFactory),
            static (value, s) => Result.NotNull(
                s.resultFactory(value, s.state),
                nameof(resultFactory)),
            static (error, _) => Result.Err<TOut, TErr>(error));

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
    /// Recovers from an <see cref="Err{TOk,TErr}" /> asynchronously, leaving an
    /// <see cref="Ok{TOk,TErr}" /> untouched.
    /// </summary>
    /// <remarks>
    /// The error-side counterpart of <c>AndThenAsync</c>:
    /// <paramref name="resultFactory" /> is not invoked on an
    /// <see cref="Ok{TOk,TErr}" />, whose value is re-wrapped for the new error type
    /// instead. The recovery may itself fail, which is why it returns a result
    /// rather than a value.
    /// </remarks>
    /// <param name="resultFactory">
    /// Produces the replacement result from the contained error.
    /// </param>
    /// <typeparam name="TOut">
    /// The error value type of the result <paramref name="resultFactory" />
    /// produces.
    /// </typeparam>
    /// <returns>
    /// The result <paramref name="resultFactory" /> produced, or the original ok
    /// value as an <see cref="Ok{TOk,TErr}" /> of <typeparamref name="TOut" />.
    /// </returns>
    public abstract ValueTask<Result<TOk, TOut>> OrElseAsync<TOut>(
        Func<TErr, ValueTask<Result<TOk, TOut>>> resultFactory)
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
    /// Returns the contained ok value, or awaits a replacement computed from the
    /// error.
    /// </summary>
    /// <remarks>
    /// The fallback that cannot throw, unlike <see cref="Unwrap" />:
    /// <paramref name="valueFactory" /> sees the error and supplies a value for it.
    /// It is not invoked on an <see cref="Ok{TOk,TErr}" />, so that case completes
    /// synchronously.
    /// </remarks>
    /// <param name="valueFactory">
    /// Produces the returned value from the contained error.
    /// </param>
    /// <returns>
    /// The contained ok value, or what <paramref name="valueFactory" /> produced
    /// from the error.
    /// </returns>
    public abstract ValueTask<TOk> UnwrapOrElseAsync(
        Func<TErr, Task<TOk>> valueFactory);

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
    /// Awaits an asynchronous side effect against the contained ok value, then
    /// returns the result unchanged.
    /// </summary>
    /// <remarks>
    /// <paramref name="action" /> is not invoked on an
    /// <see cref="Err{TOk,TErr}" />. Use this to observe an ok value — logging or
    /// metrics — without altering the pipeline; any exception the action faults with
    /// surfaces to the caller.
    /// </remarks>
    /// <param name="action">
    /// The asynchronous side effect to run against the contained ok value.
    /// </param>
    /// <returns>The receiver itself, never a new instance.</returns>
    public abstract ValueTask<Result<TOk, TErr>> InspectAsync(
        Func<TOk, Task> action);

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
    /// Awaits an asynchronous side effect against the contained error, then returns
    /// the result unchanged.
    /// </summary>
    /// <remarks>
    /// <paramref name="action" /> is not invoked on an
    /// <see cref="Ok{TOk,TErr}" />. Use this to observe a failure — logging or
    /// metrics — without handling it; the error is still carried forward.
    /// </remarks>
    /// <param name="action">
    /// The asynchronous side effect to run against the contained error.
    /// </param>
    /// <returns>The receiver itself, never a new instance.</returns>
    public abstract ValueTask<Result<TOk, TErr>> InspectErrAsync(
        Func<TErr, Task> action);

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
    /// Awaits a transformation of the contained ok value, leaving an
    /// <see cref="Err{TOk,TErr}" /> untouched.
    /// </summary>
    /// <remarks>
    /// <paramref name="map" /> is not invoked on an <see cref="Err{TOk,TErr}" />,
    /// whose error is re-wrapped for the new ok type instead. Use
    /// <c>AndThenAsync</c> where the transformation may itself fail.
    /// </remarks>
    /// <param name="map">
    /// Asynchronously produces the mapped value from the contained ok value.
    /// </param>
    /// <typeparam name="TOut">The output value type.</typeparam>
    /// <returns>
    /// An <see cref="Ok{TOk,TErr}" /> holding what <paramref name="map" />
    /// produced, or the original error.
    /// </returns>
    public abstract ValueTask<Result<TOut, TErr>> MapAsync<TOut>(
        Func<TOk, Task<TOut>> map) where TOut : notnull;

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
    /// Awaits a transformation of the contained ok value, falling back to a value
    /// the caller already has.
    /// </summary>
    /// <remarks>
    /// <paramref name="defaultValue" /> is evaluated by the caller before the call,
    /// so reach for <c>MapOrElseAsync</c> where computing it is expensive or
    /// depends on the error. <paramref name="map" /> is not invoked on an
    /// <see cref="Err{TOk,TErr}" />.
    /// </remarks>
    /// <param name="defaultValue">
    /// The value returned for an <see cref="Err{TOk,TErr}" />.
    /// </param>
    /// <param name="map">
    /// Asynchronously produces the mapped value from the contained ok value.
    /// </param>
    /// <typeparam name="TOut">The mapped result value type</typeparam>
    /// <returns>
    /// What <paramref name="map" /> produced, or <paramref name="defaultValue" />
    /// on an <see cref="Err{TOk,TErr}" />.
    /// </returns>
    public abstract ValueTask<TOut> MapOrAsync<TOut>(
        TOut defaultValue,
        Func<TOk, Task<TOut>> map) where TOut : notnull;

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
    /// Awaits a transformation of the contained ok value, falling back to the
    /// <see langword="default" /> of <typeparamref name="TOut" />.
    /// </summary>
    /// <remarks>
    /// <paramref name="map" /> is not invoked on an <see cref="Err{TOk,TErr}" />.
    /// When <typeparamref name="TOut" /> is a value type the returned default is
    /// indistinguishable from a mapped zero; use
    /// <see cref="MapOrNullAsync{TOut}" /> if the caller must tell the two apart.
    /// </remarks>
    /// <param name="map">
    /// Asynchronously produces the mapped value from the contained ok value.
    /// </param>
    /// <typeparam name="TOut">The mapped result value type</typeparam>
    /// <returns>
    /// What <paramref name="map" /> produced, or the
    /// <see langword="default" /> of <typeparamref name="TOut" /> on an
    /// <see cref="Err{TOk,TErr}" />.
    /// </returns>
    public async ValueTask<TOut?> MapOrDefaultAsync<TOut>(Func<TOk, Task<TOut>> map)
        where TOut : notnull =>
        this is Ok<TOk, TErr> ok
            ? await map(ok.Value).ConfigureAwait(false)
            : default;

    /// <summary>
    /// Applies a transformation to the contained ok value, using
    /// <see langword="null" /> rather than <see langword="default" /> for the error
    /// case.
    /// </summary>
    /// <remarks>
    /// Prefer this to <see cref="MapOrDefault{TOut}" /> when
    /// <typeparamref name="TOut" /> is a value type. <c>MapOrDefault</c> returns the
    /// default of <typeparamref name="TOut" /> for an <see cref="Err{TOk,TErr}" />,
    /// which is indistinguishable from a legitimate zero.
    /// </remarks>
    /// <param name="map">
    /// Produces the mapped value from the contained ok value. Not invoked on an
    /// <see cref="Err{TOk,TErr}" />.
    /// </param>
    /// <typeparam name="TOut">The mapped result value type</typeparam>
    /// <returns>
    /// The transformed value, or <see langword="null" /> on an
    /// <see cref="Err{TOk,TErr}" />.
    /// </returns>
    public abstract TOut? MapOrNull<TOut>(Func<TOk, TOut> map)
        where TOut : struct;

    /// <summary>
    /// Awaits a transformation of the contained ok value, using
    /// <see langword="null" /> rather than <see langword="default" /> for the error
    /// case.
    /// </summary>
    /// <remarks>
    /// Prefer this to <c>MapOrDefaultAsync</c> when <typeparamref name="TOut" /> is
    /// a value type, for the same reason as the synchronous pair: a returned zero
    /// would be indistinguishable from a mapped one.
    /// </remarks>
    /// <param name="map">
    /// Asynchronously produces the mapped value from the contained ok value. Not
    /// invoked on an <see cref="Err{TOk,TErr}" />.
    /// </param>
    /// <typeparam name="TOut">The mapped result value type</typeparam>
    /// <returns>
    /// The transformed value, or <see langword="null" /> on an
    /// <see cref="Err{TOk,TErr}" />.
    /// </returns>
    public abstract ValueTask<TOut?> MapOrNullAsync<TOut>(
        Func<TOk, Task<TOut>> map) where TOut : struct;

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
    /// Awaits whichever of two asynchronous delegates the result selects.
    /// </summary>
    /// <remarks>
    /// The overload to reach for when both delegates do real asynchronous work.
    /// Where only one does, prefer the overload taking the other synchronously — it
    /// avoids wrapping a value in an already-completed task.
    /// </remarks>
    /// <param name="defaultFactory">
    /// Produces the result from the contained error. It is not invoked on an
    /// <see cref="Ok{TOk,TErr}" />.
    /// </param>
    /// <param name="map">
    /// Transforms the contained ok value. It is not invoked on an
    /// <see cref="Err{TOk,TErr}" />.
    /// </param>
    /// <typeparam name="TOut">The type both delegates produce.</typeparam>
    /// <returns>Whatever the delegate selected produced.</returns>
    public abstract ValueTask<TOut> MapOrElseAsync<TOut>(
        Func<TErr, Task<TOut>> defaultFactory,
        Func<TOk, Task<TOut>> map) where TOut : notnull;

    /// <summary>
    /// Awaits a transformation of the contained ok value, falling back to a
    /// synchronous default computed from the error.
    /// </summary>
    /// <param name="defaultFactory">
    /// Produces the result from the contained error, synchronously.
    /// </param>
    /// <param name="map">Transforms the contained ok value.</param>
    /// <typeparam name="TOut">The type both delegates produce.</typeparam>
    /// <returns>
    /// Whatever the delegate selected produced. An <see cref="Err{TOk,TErr}" />
    /// completes synchronously.
    /// </returns>
    public abstract ValueTask<TOut> MapOrElseAsync<TOut>(
        Func<TErr, TOut> defaultFactory,
        Func<TOk, Task<TOut>> map) where TOut : notnull;

    /// <summary>
    /// Transforms the contained ok value synchronously, falling back to an awaited
    /// default computed from the error.
    /// </summary>
    /// <param name="defaultFactory">
    /// Produces the result from the contained error.
    /// </param>
    /// <param name="map">
    /// Transforms the contained ok value, synchronously. It is not invoked on an
    /// <see cref="Err{TOk,TErr}" />.
    /// </param>
    /// <typeparam name="TOut">The type both delegates produce.</typeparam>
    /// <returns>
    /// Whatever the delegate selected produced. An <see cref="Ok{TOk,TErr}" />
    /// completes synchronously.
    /// </returns>
    public abstract ValueTask<TOut> MapOrElseAsync<TOut>(
        Func<TErr, Task<TOut>> defaultFactory,
        Func<TOk, TOut> map) where TOut : notnull;

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
    /// Awaits a transformation of the contained error, leaving an
    /// <see cref="Ok{TOk,TErr}" /> untouched.
    /// </summary>
    /// <remarks>
    /// <paramref name="map" /> is not invoked on an <see cref="Ok{TOk,TErr}" />,
    /// whose value is re-wrapped for the new error type instead. Use this to
    /// translate an error into the vocabulary of the calling layer without deciding
    /// whether the operation succeeded.
    /// </remarks>
    /// <param name="map">
    /// Asynchronously produces the mapped error from the contained error.
    /// </param>
    /// <typeparam name="TOut">The output error value type</typeparam>
    /// <returns>
    /// An <see cref="Err{TOk,TErr}" /> holding what <paramref name="map" />
    /// produced, or the original ok value.
    /// </returns>
    public abstract ValueTask<Result<TOk, TOut>> MapErrAsync<TOut>(
        Func<TErr, Task<TOut>> map) where TOut : notnull;

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
}
