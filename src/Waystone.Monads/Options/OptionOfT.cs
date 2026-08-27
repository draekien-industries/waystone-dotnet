namespace Waystone.Monads.Options;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
/// <remarks>
/// <para>
/// A projection that returns null throws <see cref="ArgumentNullException" />
/// rather than producing a <see cref="None{T}" />. Every projection here is
/// constrained to a non-nullable output, so a null is a broken contract and not
/// an absent value, and collapsing it to <see cref="None{T}" /> would make the
/// two indistinguishable — the caller would read "no value" and never learn the
/// projection was wrong. <see cref="Result{TOk,TErr}" /> has always behaved this
/// way; this type was the outlier until 7.0.0.
/// </para>
/// <para>
/// When a projection genuinely may yield nothing, say so: project into an option
/// with <c>AndThen</c> and <see cref="Option.FromNullable{T}(T)" />. That is the
/// difference between mapping and binding, and it is deliberately explicit. The
/// two lenient entry points are <see cref="Option.Try{T}" />, whose whole purpose
/// is to absorb a failure, and <see cref="Option.FromNullable{T}(T)" /> itself.
/// </para>
/// </remarks>
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
    /// <see cref="Some{T}" /> and the value inside of it matches a predicate
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
    /// <param name="predicate">The condition to evaluate the option against</param>
    /// <typeparam name="TState">
    /// The type of the state passed to the predicate. It is unconstrained, so a
    /// null state is permitted.
    /// </typeparam>
    public abstract bool IsSomeAnd<TState>(
        TState state,
        Func<T, TState, bool> predicate);

    /// <summary>
    /// Checks whether the option is a <see cref="Some{T}" /> whose value satisfies
    /// an asynchronous condition.
    /// </summary>
    /// <param name="predicate">
    /// The condition to evaluate the contained value against. It is not invoked on
    /// a <see cref="None{T}" />, so a <see cref="None{T}" /> costs no await.
    /// </param>
    /// <returns>
    /// True if the option is a <see cref="Some{T}" /> and the awaited predicate
    /// returned true; false otherwise, including for every
    /// <see cref="None{T}" />.
    /// </returns>
    public abstract ValueTask<bool> IsSomeAndAsync(
        Func<T, Task<bool>> predicate);

    /// <summary>
    /// Returns <see langword="true" /> if the option is a
    /// <see cref="None{T}" /> or the value inside of it matches a predicate.
    /// </summary>
    /// <param name="predicate">The condition to evaluate the option against</param>
    public abstract bool IsNoneOr(Func<T, bool> predicate);

    /// <summary>
    /// Returns <see langword="true" /> if the option is a
    /// <see cref="None{T}" /> or the value inside of it matches a predicate
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
    /// <param name="predicate">The condition to evaluate the option against</param>
    /// <typeparam name="TState">
    /// The type of the state passed to the predicate. It is unconstrained, so a
    /// null state is permitted.
    /// </typeparam>
    public abstract bool IsNoneOr<TState>(
        TState state,
        Func<T, TState, bool> predicate);

    /// <summary>
    /// Checks whether the option is a <see cref="None{T}" />, or its contained
    /// value satisfies an asynchronous condition.
    /// </summary>
    /// <remarks>
    /// The inverse of <see cref="IsSomeAndAsync" /> in the case it lets through
    /// free: this one treats an absent value as passing, which is what makes it the
    /// right shape for a validation that only rejects a value it actually has.
    /// </remarks>
    /// <param name="predicate">
    /// The condition to evaluate the contained value against. It is not invoked on
    /// a <see cref="None{T}" />.
    /// </param>
    /// <returns>
    /// True if the option is a <see cref="None{T}" />, or the awaited predicate
    /// returned true for the contained value; false otherwise.
    /// </returns>
    public abstract ValueTask<bool> IsNoneOrAsync(
        Func<T, Task<bool>> predicate);

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
    /// Performs a <see langword="switch" /> on the option and returns what the
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
    /// <param name="onSome">A callback for handling the <see cref="Some{T}" /> case.</param>
    /// <param name="onNone">A callback for handling the <see cref="None{T}" /> case.</param>
    /// <typeparam name="TState">
    /// The type of the state passed to the callbacks. It is unconstrained, so a
    /// null state is permitted.
    /// </typeparam>
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
    /// Performs a <see langword="switch" /> on the option for its side effect,
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
    /// <param name="onSome">A callback for handling the <see cref="Some{T}" /> case.</param>
    /// <param name="onNone">A callback for handling the <see cref="None{T}" /> case.</param>
    /// <typeparam name="TState">
    /// The type of the state passed to the callbacks. It is unconstrained, so a
    /// null state is permitted.
    /// </typeparam>
    public abstract void Match<TState>(
        TState state,
        Action<T, TState> onSome,
        Action<TState> onNone);

    /// <summary>Matches the option, awaiting whichever branch is taken.</summary>
    /// <remarks>
    /// The overload to reach for when both branches do real asynchronous work.
    /// Where only one does, prefer the overload taking the other branch
    /// synchronously — it avoids wrapping a value in an already-completed task.
    /// </remarks>
    /// <param name="onSome">Produces the result from the contained value.</param>
    /// <param name="onNone">Produces the result when there is no value.</param>
    /// <typeparam name="TOut">The type both branches produce.</typeparam>
    /// <returns>Whatever the branch taken produced.</returns>
    public abstract ValueTask<TOut> MatchAsync<TOut>(
        Func<T, Task<TOut>> onSome,
        Func<Task<TOut>> onNone);

    /// <summary>
    /// Matches the option where only the absent branch is asynchronous.
    /// </summary>
    /// <param name="onSome">
    /// Produces the result from the contained value, synchronously.
    /// </param>
    /// <param name="onNone">Produces the result when there is no value.</param>
    /// <typeparam name="TOut">The type both branches produce.</typeparam>
    /// <returns>
    /// Whatever the branch taken produced. A <see cref="Some{T}" /> completes
    /// synchronously.
    /// </returns>
    public abstract ValueTask<TOut> MatchAsync<TOut>(
        Func<T, TOut> onSome,
        Func<Task<TOut>> onNone);

    /// <summary>
    /// Matches the option where only the present branch is asynchronous.
    /// </summary>
    /// <param name="onSome">Produces the result from the contained value.</param>
    /// <param name="onNone">
    /// Produces the result when there is no value, synchronously.
    /// </param>
    /// <typeparam name="TOut">The type both branches produce.</typeparam>
    /// <returns>
    /// Whatever the branch taken produced. A <see cref="None{T}" /> completes
    /// synchronously.
    /// </returns>
    public abstract ValueTask<TOut> MatchAsync<TOut>(
        Func<T, Task<TOut>> onSome,
        Func<TOut> onNone);

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
    /// Throws on a <see cref="None{T}" />, so prefer a member that cannot:
    /// <see cref="Match{TOut}(Func{T,TOut},Func{TOut})" /> to handle both cases
    /// explicitly, or
    /// <see cref="UnwrapOr" />, <see cref="UnwrapOrElse" /> or
    /// <see cref="UnwrapOrDefault" /> to supply a fallback.
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
    /// <param name="valueFactory">
    /// The delegate which computes the <see cref="None{T}" />
    /// value.
    /// </param>
    public abstract T UnwrapOrElse(Func<T> valueFactory);

    /// <summary>
    /// Returns the contained <see cref="Some{T}" /> value, or computes it from
    /// a delegate that takes state instead of capturing it.
    /// </summary>
    /// <remarks>
    /// Handing the <paramref name="state" /> to the delegate rather than
    /// capturing it lets the delegate be <see langword="static" />, so the call
    /// allocates no closure. <c>WM2017</c> reports a capturing call that could
    /// use this overload. The delegate is not invoked on a
    /// <see cref="Some{T}" />, so a capturing call allocates a closure it then
    /// discards.
    /// </remarks>
    /// <param name="state">
    /// The value the delegate would otherwise capture. It is passed through
    /// unchanged and is never inspected.
    /// </param>
    /// <param name="valueFactory">
    /// The delegate which computes the <see cref="None{T}" /> value from the
    /// state.
    /// </param>
    /// <typeparam name="TState">
    /// The type of the state passed to the delegate. It is unconstrained, so a
    /// null state is permitted.
    /// </typeparam>
    public abstract T UnwrapOrElse<TState>(TState state, Func<TState, T> valueFactory);

    /// <summary>
    /// Returns the contained value, or awaits <paramref name="valueFactory" /> for
    /// a replacement.
    /// </summary>
    /// <remarks>
    /// A <see cref="Some{T}" /> completes synchronously — the returned
    /// <see cref="ValueTask{TResult}" /> wraps the value directly rather than
    /// running a state machine — so this is safe to call on a hot path where the
    /// option is usually present.
    /// </remarks>
    /// <param name="valueFactory">
    /// Produces the value to return in place of a <see cref="None{T}" />. It is not
    /// invoked on a <see cref="Some{T}" />.
    /// </param>
    /// <returns>
    /// The contained value if the option is a <see cref="Some{T}" />, otherwise
    /// whatever <paramref name="valueFactory" /> produced.
    /// </returns>
    public abstract ValueTask<T> UnwrapOrElseAsync(Func<Task<T>> valueFactory);

    /// <summary>
    /// Maps an <c>Option&lt;T&gt;</c> to an <c>Option&lt;TOut&gt;</c> by
    /// applying a function to a contained value (if <see cref="Some{T}" />) or returns
    /// <see cref="None{T}" /> (if <see cref="None{T}" />).
    /// </summary>
    /// <param name="map">The map function.</param>
    /// <typeparam name="TOut">The return type of the map function.</typeparam>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="map" /> returns null. See the remarks on
    /// <see cref="Option{T}" /> for why that throws rather than producing a
    /// <see cref="None{T}" />.
    /// </exception>
    public abstract Option<TOut> Map<TOut>(Func<T, TOut> map) where TOut : notnull;

    /// <summary>
    /// Maps an <c>Option&lt;T&gt;</c> to an <c>Option&lt;TOut&gt;</c> by
    /// applying a function to a contained value (if <see cref="Some{T}" />) or returns
    /// <see cref="None{T}" /> (if <see cref="None{T}" />).
    /// </summary>
    /// <remarks>
    /// Handing the <paramref name="state" /> to the delegate rather than
    /// capturing it lets the delegate be <see langword="static" />, so the call
    /// allocates no closure. <c>WM2017</c> reports a capturing call that could
    /// use this overload.
    /// </remarks>
    /// <param name="state">The value passed to the map function.</param>
    /// <param name="map">The map function.</param>
    /// <typeparam name="TState">The type of the state passed to the map function.</typeparam>
    /// <typeparam name="TOut">The return type of the map function.</typeparam>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="map" /> returns null. See the remarks on
    /// <see cref="Option{T}" /> for why that throws rather than producing a
    /// <see cref="None{T}" />.
    /// </exception>
    public abstract Option<TOut> Map<TState, TOut>(
        TState state,
        Func<T, TState, TOut> map) where TOut : notnull;

    /// <summary>
    /// Awaits <paramref name="map" /> against the contained value and wraps what it
    /// produces, or short-circuits to <see cref="None{T}" /> without invoking it.
    /// </summary>
    /// <remarks>
    /// The delegate returns a <see cref="Task{TResult}" /> rather than a
    /// <see cref="ValueTask{TResult}" /> so that an ordinary <c>async</c> method
    /// group converts to it by name. Only a chain <em>step</em> — one producing
    /// another monad — takes a <see cref="ValueTask{TResult}" />, which
    /// <c>WSG0003</c> enforces.
    /// </remarks>
    /// <param name="map">
    /// Transforms the contained value. It is not invoked on a
    /// <see cref="None{T}" />, so it may safely be expensive.
    /// </param>
    /// <typeparam name="TOut">The type the delegate produces.</typeparam>
    /// <returns>
    /// <see cref="Some{T}" /> of what <paramref name="map" /> produced, or
    /// <see cref="None{T}" /> when this option is a <see cref="None{T}" /> or the
    /// delegate produced null.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="map" /> returns null. See the remarks on
    /// <see cref="Option{T}" /> for why that throws rather than producing a
    /// <see cref="None{T}" />.
    /// </exception>
    public abstract ValueTask<Option<TOut>> MapAsync<TOut>(
        Func<T, Task<TOut>> map) where TOut : notnull;

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
    /// otherwise calls <paramref name="optionFactory" /> with the wrapped
    /// value and returns the result.
    /// </summary>
    /// <remarks>
    /// Often used to chain fallible operations that may return
    /// <see cref="None{T}" />.
    /// </remarks>
    /// <param name="optionFactory">
    /// Produces the option to return from the wrapped value. It runs only on
    /// a <see cref="Some{T}" />.
    /// </param>
    /// <typeparam name="TOut">The type of the value contained in the resulting option.</typeparam>
    /// <returns>
    /// The option <paramref name="optionFactory" /> produced, or
    /// <see cref="None{T}" /> when this option is a <see cref="None{T}" />.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="optionFactory" /> returns a null option. Returning
    /// null rather than <see cref="Option.None{T}" /> is never meaningful, and
    /// left alone it would surface as a <see cref="NullReferenceException" />
    /// at whatever called into the option next.
    /// </exception>
#if !DEBUG
    [DebuggerStepThrough]
#endif
    public Option<TOut> AndThen<TOut>(Func<T, Option<TOut>> optionFactory)
        where TOut : notnull =>
        Match(
            value => Option.NotNull(
                optionFactory(value),
                nameof(optionFactory)),
            Option.None<TOut>);

    /// <summary>
    /// Returns <see cref="None{T}" /> if the option is a <see cref="None{T}" />,
    /// otherwise calls <paramref name="optionFactory" /> with the wrapped
    /// value and the <paramref name="state" /> and returns the result.
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
    /// <param name="optionFactory">
    /// Produces the option to return from the wrapped value. It runs only on
    /// a <see cref="Some{T}" />.
    /// </param>
    /// <typeparam name="TState">
    /// The type of the state handed to <paramref name="optionFactory" />. It is
    /// unconstrained, so a null state is permitted.
    /// </typeparam>
    /// <typeparam name="TOut">The type of the value contained in the resulting option.</typeparam>
    /// <returns>
    /// The option <paramref name="optionFactory" /> produced, or
    /// <see cref="None{T}" /> when this option is a <see cref="None{T}" />.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="optionFactory" /> returns a null option. Returning
    /// null rather than <see cref="Option.None{T}" /> is never meaningful, and
    /// left alone it would surface as a <see cref="NullReferenceException" />
    /// at whatever called into the option next.
    /// </exception>
#if !DEBUG
    [DebuggerStepThrough]
#endif
    public Option<TOut> AndThen<TState, TOut>(
        TState state,
        Func<T, TState, Option<TOut>> optionFactory) where TOut : notnull =>
        Match(
            (state, optionFactory),
            static (value, s) => Option.NotNull(
                s.optionFactory(value, s.state),
                nameof(optionFactory)),
            static _ => Option.None<TOut>());

    /// <summary>
    /// Awaits <paramref name="optionFactory" /> against the contained value and
    /// returns the option it produces, without nesting.
    /// </summary>
    /// <remarks>
    /// The delegate returns a <see cref="ValueTask{TResult}" /> rather than a
    /// <see cref="Task{TResult}" /> because it produces another
    /// <see cref="Option{T}" /> and is therefore a chain step, which lets an
    /// existing async chain be handed to it by name. <c>WSG0003</c> enforces that.
    /// </remarks>
    /// <param name="optionFactory">
    /// Produces the next option from the contained value. It is not invoked on a
    /// <see cref="None{T}" />.
    /// </param>
    /// <typeparam name="TOut">The value type of the option produced.</typeparam>
    /// <returns>
    /// Whatever <paramref name="optionFactory" /> produced, or <see cref="None{T}" />
    /// when this option is a <see cref="None{T}" />.
    /// </returns>
    public abstract ValueTask<Option<TOut>> AndThenAsync<TOut>(
        Func<T, ValueTask<Option<TOut>>> optionFactory) where TOut : notnull;

    /// <summary>
    /// Returns the provided default result (if <see cref="None{T}" />), or
    /// applies a function to the contained value (if <see cref="Some{T}" />).
    /// </summary>
    /// <param name="defaultValue">The default value for a <see cref="None{T}" />.</param>
    /// <param name="map">The map function.</param>
    /// <typeparam name="TOut">The return type of the map function.</typeparam>
    public abstract TOut MapOr<TOut>(TOut defaultValue, Func<T, TOut> map);

    /// <summary>
    /// Returns the provided default result (if <see cref="None{T}" />), or
    /// applies a function to the contained value (if <see cref="Some{T}" />).
    /// </summary>
    /// <remarks>
    /// Handing the <paramref name="state" /> to the delegate rather than
    /// capturing it lets the delegate be <see langword="static" />, so the call
    /// allocates no closure. <c>WM2017</c> reports a capturing call that could
    /// use this overload.
    /// </remarks>
    /// <param name="state">The value passed to the map function.</param>
    /// <param name="defaultValue">The default value for a <see cref="None{T}" />.</param>
    /// <param name="map">The map function.</param>
    /// <typeparam name="TState">The type of the state passed to the map function.</typeparam>
    /// <typeparam name="TOut">The return type of the map function.</typeparam>
    public abstract TOut MapOr<TState, TOut>(
        TState state,
        TOut defaultValue,
        Func<T, TState, TOut> map);

    /// <summary>
    /// Awaits <paramref name="map" /> against the contained value, or returns
    /// <paramref name="defaultValue" /> unchanged.
    /// </summary>
    /// <remarks>
    /// <paramref name="defaultValue" /> is evaluated by the caller whether or not it
    /// is used. Where producing it is not free, prefer the <c>MapOrElseAsync</c>
    /// overload taking a delegate, which runs only on the branch that needs it.
    /// <c>WM2016</c> reports the difference.
    /// </remarks>
    /// <param name="defaultValue">The value to return for a <see cref="None{T}" />.</param>
    /// <param name="map">
    /// Transforms the contained value. It is not invoked on a <see cref="None{T}" />.
    /// </param>
    /// <typeparam name="TOut">The type the delegate produces.</typeparam>
    /// <returns>
    /// What <paramref name="map" /> produced, or <paramref name="defaultValue" />.
    /// </returns>
    public abstract ValueTask<TOut> MapOrAsync<TOut>(
        TOut defaultValue,
        Func<T, Task<TOut>> map);

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
    /// <see cref="None{T}" />), or applies to the contained value a map
    /// function that takes state instead of capturing it (if
    /// <see cref="Some{T}" />).
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
    /// The type of the state passed to the map function. It is unconstrained, so a
    /// null state is permitted.
    /// </typeparam>
    /// <typeparam name="TOut">The return type of the map function.</typeparam>
    public abstract TOut? MapOrDefault<TState, TOut>(
        TState state,
        Func<T, TState, TOut> map) where TOut : notnull;

    /// <summary>
    /// Awaits <paramref name="map" /> against the contained value, or returns the
    /// default of <typeparamref name="TOut" />.
    /// </summary>
    /// <remarks>
    /// <paramref name="map" /> is not invoked on a <see cref="None{T}" />. When
    /// <typeparamref name="TOut" /> is a value type the returned default is
    /// indistinguishable from a mapped zero; use
    /// <see cref="MapOrNullAsync{TOut}" /> if the caller must tell the two apart.
    /// </remarks>
    /// <param name="map">
    /// Transforms the contained value. It is not invoked on a <see cref="None{T}" />.
    /// </param>
    /// <typeparam name="TOut">The type the delegate produces.</typeparam>
    /// <returns>
    /// What <paramref name="map" /> produced, or the default of
    /// <typeparamref name="TOut" />.
    /// </returns>
    public async ValueTask<TOut?> MapOrDefaultAsync<TOut>(Func<T, Task<TOut>> map)
        where TOut : notnull =>
        this is Some<T> some
            ? await map(some.Value).ConfigureAwait(false)
            : default;

    /// <summary>
    /// Maps the contained value to a nullable value type, using null for a
    /// <see cref="None{T}" />.
    /// </summary>
    /// <remarks>
    /// The bridge out of <see cref="Option{T}" /> into <see cref="Nullable{T}" />,
    /// for handing a value to an API that speaks the latter.
    /// <typeparamref name="TOut" /> is constrained to a value type precisely so
    /// that null cannot also be a mapped result, which is what keeps the return
    /// unambiguous.
    /// </remarks>
    /// <param name="map">
    /// Transforms the contained value. It is not invoked on a <see cref="None{T}" />.
    /// </param>
    /// <typeparam name="TOut">The value type the delegate produces.</typeparam>
    /// <returns>
    /// What <paramref name="map" /> produced, or null for a <see cref="None{T}" />.
    /// </returns>
    public abstract TOut? MapOrNull<TOut>(Func<T, TOut> map) where TOut : struct;

    /// <summary>
    /// Awaits <paramref name="map" /> against the contained value and returns it as
    /// a nullable value type, using null for a <see cref="None{T}" />.
    /// </summary>
    /// <param name="map">
    /// Transforms the contained value. It is not invoked on a <see cref="None{T}" />.
    /// </param>
    /// <typeparam name="TOut">The value type the delegate produces.</typeparam>
    /// <returns>
    /// What <paramref name="map" /> produced, or null for a <see cref="None{T}" />.
    /// </returns>
    public abstract ValueTask<TOut?> MapOrNullAsync<TOut>(
        Func<T, Task<TOut>> map) where TOut : struct;

    /// <summary>
    /// Computes a default from a function (if <see cref="None{T}" />), or
    /// applies a function to the contained value (if <see cref="Some{T}" />).
    /// </summary>
    /// <param name="defaultFactory">
    /// The function that will create a default value for a
    /// <see cref="None{T}" />.
    /// </param>
    /// <param name="map">The map function.</param>
    /// <typeparam name="TOut">The return type of the map function.</typeparam>
    public abstract TOut MapOrElse<TOut>(Func<TOut> defaultFactory, Func<T, TOut> map);

    /// <summary>
    /// Computes a default from a function (if <see cref="None{T}" />), or
    /// applies a function to the contained value (if <see cref="Some{T}" />).
    /// </summary>
    /// <remarks>
    /// Handing the <paramref name="state" /> to the delegate rather than
    /// capturing it lets the delegate be <see langword="static" />, so the call
    /// allocates no closure. <c>WM2017</c> reports a capturing call that could
    /// use this overload.
    /// </remarks>
    /// <param name="state">The value passed to both functions.</param>
    /// <param name="defaultFactory">
    /// The function that will create a default value for a
    /// <see cref="None{T}" />.
    /// </param>
    /// <param name="map">The map function.</param>
    /// <typeparam name="TState">The type of the state passed to both functions.</typeparam>
    /// <typeparam name="TOut">The return type of the map function.</typeparam>
    public abstract TOut MapOrElse<TState, TOut>(
        TState state,
        Func<TState, TOut> defaultFactory,
        Func<T, TState, TOut> map);

    /// <summary>
    /// Awaits whichever of <paramref name="map" /> and
    /// <paramref name="defaultFactory" /> the option selects.
    /// </summary>
    /// <param name="defaultFactory">
    /// Produces the result for a <see cref="None{T}" />. It is not invoked on a
    /// <see cref="Some{T}" />.
    /// </param>
    /// <param name="map">
    /// Transforms the contained value. It is not invoked on a <see cref="None{T}" />.
    /// </param>
    /// <typeparam name="TOut">The type both delegates produce.</typeparam>
    /// <returns>Whatever the delegate selected produced.</returns>
    public abstract ValueTask<TOut> MapOrElseAsync<TOut>(
        Func<Task<TOut>> defaultFactory,
        Func<T, Task<TOut>> map);

    /// <summary>
    /// Awaits <paramref name="map" /> against the contained value, falling back to a
    /// synchronous default.
    /// </summary>
    /// <param name="defaultFactory">
    /// Produces the result for a <see cref="None{T}" />, synchronously.
    /// </param>
    /// <param name="map">Transforms the contained value.</param>
    /// <typeparam name="TOut">The type both delegates produce.</typeparam>
    /// <returns>
    /// Whatever the delegate selected produced. A <see cref="None{T}" /> completes
    /// synchronously.
    /// </returns>
    public abstract ValueTask<TOut> MapOrElseAsync<TOut>(
        Func<TOut> defaultFactory,
        Func<T, Task<TOut>> map);

    /// <summary>
    /// Maps the contained value synchronously, falling back to an awaited default.
    /// </summary>
    /// <param name="defaultFactory">Produces the result for a <see cref="None{T}" />.</param>
    /// <param name="map">
    /// Transforms the contained value, synchronously. It is not invoked on a
    /// <see cref="None{T}" />.
    /// </param>
    /// <typeparam name="TOut">The type both delegates produce.</typeparam>
    /// <returns>
    /// Whatever the delegate selected produced. A <see cref="Some{T}" /> completes
    /// synchronously.
    /// </returns>
    public abstract ValueTask<TOut> MapOrElseAsync<TOut>(
        Func<Task<TOut>> defaultFactory,
        Func<T, TOut> map);

    /// <summary>
    /// Calls a function with a reference to the contained value if
    /// <see cref="Some{T}" />
    /// </summary>
    /// <param name="action">The function to execute against the value.</param>
    /// <returns>The original <see cref="Option{T}" /></returns>
    public abstract Option<T> Inspect(Action<T> action);

    /// <summary>
    /// Calls a function with the contained value and the state if
    /// <see cref="Some{T}" />, so the function need not capture.
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
    /// <param name="action">The function to execute against the value.</param>
    /// <typeparam name="TState">
    /// The type of the state passed to the function. It is unconstrained, so a
    /// null state is permitted.
    /// </typeparam>
    /// <returns>The original <see cref="Option{T}" />, unchanged.</returns>
    public abstract Option<T> Inspect<TState>(
        TState state,
        Action<T, TState> action);

    /// <summary>
    /// Awaits <paramref name="action" /> against the contained value and returns
    /// the option unchanged.
    /// </summary>
    /// <remarks>
    /// For a side effect in the middle of a chain — logging or a metric — where the
    /// side effect itself is asynchronous. The option is passed through whatever
    /// the action does, so this can be dropped into a chain without altering it.
    /// </remarks>
    /// <param name="action">
    /// The side effect to run against the contained value. It is not invoked on a
    /// <see cref="None{T}" />.
    /// </param>
    /// <returns>This option, unchanged.</returns>
    public abstract ValueTask<Option<T>> InspectAsync(Func<T, Task> action);

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
    /// Handing the <paramref name="state" /> to the delegate rather than
    /// capturing it lets the delegate be <see langword="static" />, so the call
    /// allocates no closure. <c>WM2017</c> reports a capturing call that could
    /// use this overload.
    /// </remarks>
    /// <param name="state">The value passed to the predicate.</param>
    /// <param name="predicate">The filter function.</param>
    /// <typeparam name="TState">The type of the state passed to the predicate.</typeparam>
    public abstract Option<T> Filter<TState>(
        TState state,
        Func<T, TState, bool> predicate);

    /// <summary>
    /// Keeps the option when its contained value satisfies an asynchronous
    /// condition, and discards it otherwise.
    /// </summary>
    /// <param name="predicate">
    /// The condition the contained value must satisfy to be kept. It is not invoked
    /// on a <see cref="None{T}" />.
    /// </param>
    /// <returns>
    /// This option if it is a <see cref="Some{T}" /> whose value passed, otherwise
    /// <see cref="None{T}" />. A <see cref="None{T}" /> passes through unchanged.
    /// </returns>
    public abstract ValueTask<Option<T>> FilterAsync(
        Func<T, Task<bool>> predicate);

    /// <summary>
    /// Returns the option if it contains a value, otherwise returns
    /// <paramref name="other" />
    /// </summary>
    /// <param name="other">The other option.</param>
    public abstract Option<T> Or(Option<T> other);

    /// <summary>
    /// Returns the option if it contains a value, otherwise invokes the
    /// <paramref name="optionFactory" /> function and returns the result.
    /// </summary>
    /// <param name="optionFactory">The function that will create the other option.</param>
    public abstract Option<T> OrElse(Func<Option<T>> optionFactory);

    /// <summary>
    /// Returns the option if it contains a value, otherwise invokes a function
    /// that takes state instead of capturing it and returns the result.
    /// </summary>
    /// <remarks>
    /// Handing the <paramref name="state" /> to the delegate rather than
    /// capturing it lets the delegate be <see langword="static" />, so the call
    /// allocates no closure. <c>WM2017</c> reports a capturing call that could
    /// use this overload. The delegate is not invoked on a
    /// <see cref="Some{T}" />, so a capturing call allocates a closure it then
    /// discards.
    /// </remarks>
    /// <param name="state">
    /// The value the delegate would otherwise capture. It is passed through
    /// unchanged and is never inspected.
    /// </param>
    /// <param name="optionFactory">
    /// The function that will create the other option from the state.
    /// </param>
    /// <typeparam name="TState">
    /// The type of the state passed to the function. It is unconstrained, so a
    /// null state is permitted.
    /// </typeparam>
    public abstract Option<T> OrElse<TState>(
        TState state,
        Func<TState, Option<T>> optionFactory);

    /// <summary>
    /// Returns the option if it contains a value, otherwise awaits
    /// <paramref name="optionFactory" /> for a replacement option.
    /// </summary>
    /// <remarks>
    /// The delegate returns a <see cref="ValueTask{TResult}" /> rather than a
    /// <see cref="Task{TResult}" /> because it produces another
    /// <see cref="Option{T}" /> and is therefore a chain step: an existing async
    /// chain can be handed to it by name. <c>WSG0003</c> enforces the distinction.
    /// </remarks>
    /// <param name="optionFactory">
    /// Produces the fallback option. It is not invoked on a
    /// <see cref="Some{T}" />, so a present value costs no await.
    /// </param>
    /// <returns>
    /// This option if it is a <see cref="Some{T}" />, otherwise whatever
    /// <paramref name="optionFactory" /> produced.
    /// </returns>
    public abstract ValueTask<Option<T>> OrElseAsync(
        Func<ValueTask<Option<T>>> optionFactory);

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
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="zip" /> returns null. See the remarks on
    /// <see cref="Option{T}" /> for why that throws rather than producing a
    /// <see cref="None{T}" />.
    /// </exception>
    public abstract Option<TOut> ZipWith<TOther, TOut>(
        Option<TOther> other,
        Func<T, TOther, TOut> zip)
        where TOther : notnull
        where TOut : notnull;

    /// <summary>
    /// Combines this option with <paramref name="other" /> by awaiting
    /// <paramref name="zip" /> against both contained values.
    /// </summary>
    /// <remarks>
    /// Both options must hold a value. Where a single <see cref="Some{T}" /> should
    /// survive the other side being absent, use <see cref="ReduceAsync" /> instead.
    /// </remarks>
    /// <param name="other">The option to combine with.</param>
    /// <param name="zip">
    /// Combines the two contained values. It is invoked only when both options are a
    /// <see cref="Some{T}" />, so a <see cref="None{T}" /> on either side costs no
    /// await.
    /// </param>
    /// <typeparam name="TOther">The value type of the other option.</typeparam>
    /// <typeparam name="TOut">The type the delegate produces.</typeparam>
    /// <returns>
    /// <see cref="Some{T}" /> of what <paramref name="zip" /> produced when both
    /// options hold a value, otherwise <see cref="None{T}" />. A null from
    /// <paramref name="zip" /> becomes a <see cref="None{T}" />.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="zip" /> returns null. See the remarks on
    /// <see cref="Option{T}" /> for why that throws rather than producing a
    /// <see cref="None{T}" />.
    /// </exception>
    public abstract ValueTask<Option<TOut>> ZipWithAsync<TOther, TOut>(
        Option<TOther> other,
        Func<T, TOther, Task<TOut>> zip)
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
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="reduce" /> returns null. See the remarks on
    /// <see cref="Option{T}" /> for why that throws rather than producing a
    /// <see cref="None{T}" />.
    /// </exception>
    public abstract Option<T> Reduce(Option<T> other, Func<T, T, T> reduce);

    /// <summary>
    /// Merges this option with <paramref name="other" />, awaiting
    /// <paramref name="reduce" /> only when both hold a value.
    /// </summary>
    /// <remarks>
    /// Unlike <c>ZipWithAsync</c>, a <see cref="Some{T}" /> survives a
    /// <see cref="None{T}" /> on the other side and is returned unchanged, so the
    /// delegate runs only when there are genuinely two values to combine.
    /// </remarks>
    /// <param name="other">The option to merge with.</param>
    /// <param name="reduce">
    /// Combines the two contained values. It is invoked only when both options are a
    /// <see cref="Some{T}" />.
    /// </param>
    /// <returns>
    /// <see cref="Some{T}" /> of the combined value when both hold one, otherwise
    /// whichever single <see cref="Some{T}" /> there was, otherwise
    /// <see cref="None{T}" />. A null from <paramref name="reduce" /> becomes a
    /// <see cref="None{T}" />.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="reduce" /> returns null. See the remarks on
    /// <see cref="Option{T}" /> for why that throws rather than producing a
    /// <see cref="None{T}" />.
    /// </exception>
    public abstract ValueTask<Option<T>> ReduceAsync(
        Option<T> other,
        Func<T, T, Task<T>> reduce);

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
    /// <paramref name="error" /> is eagerly evaluated. If you are passing the
    /// result of a function call, prefer <see cref="OkOrElse{TErr}" />, which is
    /// lazily evaluated.
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
    /// Transforms the current <see cref="Option{T}" /> into a
    /// <see cref="Result{TOk, TErr}" />, mapping <see cref="Some{T}" /> to
    /// <see cref="Ok{TOk, TErr}" /> and <see cref="None{T}" /> to an
    /// <see cref="Err{TOk, TErr}" /> built by a factory that takes state
    /// instead of capturing it.
    /// </summary>
    /// <remarks>
    /// Handing the <paramref name="state" /> to the delegate rather than
    /// capturing it lets the delegate be <see langword="static" />, so the call
    /// allocates no closure. <c>WM2017</c> reports a capturing call that could
    /// use this overload. The delegate is not invoked on a
    /// <see cref="Some{T}" />, so a capturing call allocates a closure it then
    /// discards.
    /// </remarks>
    /// <param name="state">
    /// The value the delegate would otherwise capture. It is passed through
    /// unchanged and is never inspected.
    /// </param>
    /// <param name="errorFactory">
    /// The function, which when invoked with the state, will return the error
    /// value.
    /// </param>
    /// <typeparam name="TState">
    /// The type of the state passed to the factory. It is unconstrained, so a
    /// null state is permitted.
    /// </typeparam>
    /// <typeparam name="TErr">The type of the error value returned by the factory.</typeparam>
    /// <returns>
    /// An <see cref="Ok{TOk, TErr}" /> if the current option is a
    /// <see cref="Some{T}" />, otherwise an <see cref="Err{TOk, TErr}" />.
    /// </returns>
    public abstract Result<T, TErr> OkOrElse<TState, TErr>(
        TState state,
        Func<TState, TErr> errorFactory) where TErr : notnull;

    /// <summary>
    /// Converts the option into a <see cref="Result{TOk, TErr}" />, awaiting
    /// <paramref name="errorFactory" /> only when there is no value.
    /// </summary>
    /// <param name="errorFactory">
    /// Produces the error for a <see cref="None{T}" />. It is not invoked on a
    /// <see cref="Some{T}" />, so a present value costs no await.
    /// </param>
    /// <typeparam name="TErr">The error type of the result produced.</typeparam>
    /// <returns>
    /// An <see cref="Ok{TOk, TErr}" /> holding the contained value, or an
    /// <see cref="Err{TOk, TErr}" /> holding what <paramref name="errorFactory" />
    /// produced.
    /// </returns>
    public abstract ValueTask<Result<T, TErr>> OkOrElseAsync<TErr>(
        Func<Task<TErr>> errorFactory) where TErr : notnull;
}
