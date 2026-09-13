namespace Waystone.Monads.Options;

using System;
using System.Threading.Tasks;
using Results;
#if !DEBUG
using System.Diagnostics;
#endif

public abstract partial record Option<T> where T : notnull
{
    /// <summary>
    /// An <see cref="Option{T}" /> with a value bound to it, ready to be handed
    /// to a delegate that would otherwise have captured it.
    /// </summary>
    /// <remarks>
    /// Created by <c>With</c> on an <see cref="Option{T}" />. Each member below
    /// takes the same delegate as the <see cref="Option{T}" /> member it shares
    /// a name with, invokes it with the bound state, and returns a plain
    /// <see cref="Option{T}" />. The bound state applies to that one call
    /// only; bind it again for a second call.
    /// <para>
    /// Mark the delegate <see langword="static" /> if it does not otherwise
    /// need to capture anything. A delegate that reads only its parameters
    /// allocates nothing when marked <see langword="static" />; one that also
    /// reads an outer variable allocates a new closure on every call.
    /// </para>
    /// <para>
    /// A <see langword="default" /> instance of this type has no option to
    /// call into, and every member throws
    /// <see cref="InvalidOperationException" />. Only <c>With</c> produces a
    /// usable instance.
    /// </para>
    /// <para>
    /// Where an asynchronous member surfaces a failure depends on the member.
    /// Most evaluate their body eagerly, so a <see langword="default" /> instance
    /// and a delegate that throws before it returns its
    /// <see cref="System.Threading.Tasks.Task" /> both throw at the call rather
    /// than from the returned task. The members that await on both branches are
    /// declared <see langword="async" /> and surface the same two failures from
    /// the returned task instead. Await the call inside the
    /// <see langword="try" /> block and both are caught either way.
    /// </para>
    /// </remarks>
    /// <typeparam name="TState">The type of the bound value.</typeparam>
#if !DEBUG
    [DebuggerStepThrough]
#endif
    public readonly struct Bound<TState>
    {
        private readonly Option<T> _option;
        private readonly TState _state;

        internal Bound(Option<T> option, TState state)
        {
            _option = option;
            _state = state;
        }

        private Option<T> Source =>
            _option
         ?? throw new InvalidOperationException(
                "A default Option<T>.Bound<TState> has no option bound to it. Build one by calling With on an option.");

        /// <summary>
        /// Tests the contained value against <paramref name="predicate" />,
        /// treating <see cref="None{T}" /> as a failure.
        /// </summary>
        /// <param name="predicate">
        /// Decides whether the contained value qualifies. It receives the value
        /// and the bound state, and is not invoked for a
        /// <see cref="None{T}" />.
        /// </param>
        /// <returns>
        /// True if the option holds a value and <paramref name="predicate" />
        /// accepts it; false otherwise.
        /// </returns>
        public bool IsSomeAnd(Func<T, TState, bool> predicate) =>
            Source.IsSomeAnd(_state, predicate);

        /// <summary>
        /// Tests the contained value against <paramref name="predicate" />,
        /// treating <see cref="None{T}" /> as a pass.
        /// </summary>
        /// <remarks>
        /// The mirror of <see cref="IsSomeAnd" /> on the empty case, not on the
        /// predicate: both invoke <paramref name="predicate" /> only when a
        /// value is present, and they disagree about what an absent one means.
        /// </remarks>
        /// <param name="predicate">
        /// Decides whether the contained value qualifies. It receives the value
        /// and the bound state, and is not invoked for a
        /// <see cref="None{T}" />.
        /// </param>
        /// <returns>
        /// True if the option holds no value, or holds one
        /// <paramref name="predicate" /> accepts; false otherwise.
        /// </returns>
        public bool IsNoneOr(Func<T, TState, bool> predicate) =>
            Source.IsNoneOr(_state, predicate);

        /// <summary>
        /// Produces a value from whichever case the option is in, so both cases
        /// are answered in one expression.
        /// </summary>
        /// <remarks>
        /// The bound state reaches both <paramref name="onSome" /> and
        /// <paramref name="onNone" />, so a single bound value can serve both
        /// branches.
        /// </remarks>
        /// <param name="onSome">
        /// Produces the result from the contained value and the bound state.
        /// </param>
        /// <param name="onNone">
        /// Produces the result from the bound state alone, there being no
        /// contained value to hand it.
        /// </param>
        /// <typeparam name="TOut">The type both delegates produce.</typeparam>
        /// <returns>
        /// Whatever the delegate for the option's case returned.
        /// </returns>
        public TOut Match<TOut>(
            Func<T, TState, TOut> onSome,
            Func<TState, TOut> onNone) =>
            Source.Match(_state, onSome, onNone);

        /// <summary>
        /// Runs a side effect for whichever case the option is in, producing
        /// nothing.
        /// </summary>
        /// <remarks>
        /// The counterpart of <see cref="Match{TOut}" /> for work that has no
        /// result to return — writing a log line, incrementing a counter.
        /// Prefer that overload wherever a value can be produced instead, since
        /// a side effect is harder to test than a return.
        /// </remarks>
        /// <param name="onSome">
        /// Acts on the contained value and the bound state.
        /// </param>
        /// <param name="onNone">
        /// Acts on the bound state alone, there being no contained value to
        /// hand it.
        /// </param>
        public void Match(Action<T, TState> onSome, Action<TState> onNone) =>
            Source.Match(_state, onSome, onNone);

        /// <summary>
        /// Returns the contained value, falling back to what
        /// <paramref name="valueFactory" /> produces.
        /// </summary>
        /// <remarks>
        /// The fallback is computed only for a <see cref="None{T}" />, which is
        /// the difference from <see cref="Option{T}.UnwrapOr" /> — pass a
        /// delegate here when producing the replacement allocates or
        /// otherwise does real work.
        /// </remarks>
        /// <param name="valueFactory">
        /// Produces the replacement from the bound state. It receives no value,
        /// there being none to hand it, and is not invoked for a
        /// <see cref="Some{T}" />.
        /// </param>
        /// <returns>
        /// The contained value, or what <paramref name="valueFactory" />
        /// produced.
        /// </returns>
        public T UnwrapOrElse(Func<TState, T> valueFactory) =>
            Source.UnwrapOrElse(_state, valueFactory);

        /// <summary>
        /// Transforms the contained value, leaving <see cref="None{T}" /> alone.
        /// </summary>
        /// <param name="map">
        /// Produces the new value from the contained one and the bound state.
        /// It is not invoked for a <see cref="None{T}" />.
        /// </param>
        /// <typeparam name="TOut">
        /// The type <paramref name="map" /> produces.
        /// </typeparam>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="map" /> returns null. See the remarks on
        /// <see cref="Option{T}" /> for why that throws rather than producing a
        /// <see cref="None{T}" />.
        /// </exception>
        /// <returns>
        /// <see cref="Some{T}" /> of the new value, or <see cref="None{T}" />
        /// if there was nothing to transform.
        /// </returns>
        public Option<TOut> Map<TOut>(Func<T, TState, TOut> map)
            where TOut : notnull =>
            Source.Map(_state, map);

        /// <summary>
        /// Transforms the contained value into another option, leaving
        /// <see cref="None{T}" /> alone.
        /// </summary>
        /// <remarks>
        /// The difference from <see cref="Map{TOut}" /> is that
        /// <paramref name="optionFactory" /> decides the case as well as the
        /// value, so a step that can itself find nothing chains here without
        /// producing a nested option.
        /// </remarks>
        /// <param name="optionFactory">
        /// Produces the next option from the contained value and the bound
        /// state. It is not invoked for a <see cref="None{T}" />.
        /// </param>
        /// <typeparam name="TOut">
        /// The value type of the option <paramref name="optionFactory" />
        /// produces.
        /// </typeparam>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="optionFactory" /> returns a null option. See the
        /// remarks on <see cref="Option{T}" /> for why that throws rather than
        /// producing a <see cref="None{T}" />.
        /// </exception>
        /// <returns>
        /// Whatever <paramref name="optionFactory" /> produced, or
        /// <see cref="None{T}" /> if it was never invoked.
        /// </returns>
        public Option<TOut> AndThen<TOut>(
            Func<T, TState, Option<TOut>> optionFactory) where TOut : notnull =>
            Source.AndThen(_state, optionFactory);

        /// <summary>
        /// Transforms the contained value, or returns
        /// <paramref name="defaultValue" /> for a <see cref="None{T}" />.
        /// </summary>
        /// <remarks>
        /// <paramref name="defaultValue" /> is evaluated at the call site
        /// whether it is needed or not. Where producing it allocates or
        /// otherwise does real work, use <see cref="MapOrElse{TOut}" /> instead.
        /// </remarks>
        /// <param name="defaultValue">Stands in for an absent value.</param>
        /// <param name="map">
        /// Produces the result from the contained value and the bound state. It
        /// is not invoked for a <see cref="None{T}" />.
        /// </param>
        /// <typeparam name="TOut">
        /// The type <paramref name="map" /> produces, and the type of
        /// <paramref name="defaultValue" />.
        /// </typeparam>
        /// <returns>
        /// What <paramref name="map" /> produced, or
        /// <paramref name="defaultValue" />.
        /// </returns>
        public TOut MapOr<TOut>(TOut defaultValue, Func<T, TState, TOut> map) =>
            Source.MapOr(_state, defaultValue, map);

        /// <summary>
        /// Transforms the contained value, or returns the default of
        /// <typeparamref name="TOut" /> for a <see cref="None{T}" />.
        /// </summary>
        /// <remarks>
        /// For a value type the default is indistinguishable from a legitimate
        /// zero, so a caller who needs to tell the two apart wants
        /// <see cref="MapOr{TOut}" /> with a sentinel, or <c>MapOrNull</c>.
        /// </remarks>
        /// <param name="map">
        /// Produces the result from the contained value and the bound state. It
        /// is not invoked for a <see cref="None{T}" />.
        /// </param>
        /// <typeparam name="TOut">
        /// The type <paramref name="map" /> produces.
        /// </typeparam>
        /// <returns>
        /// What <paramref name="map" /> produced, or the default of
        /// <typeparamref name="TOut" />.
        /// </returns>
        public TOut? MapOrDefault<TOut>(Func<T, TState, TOut> map)
            where TOut : notnull =>
            Source.MapOrDefault(_state, map);

        /// <summary>
        /// Transforms the contained value into a nullable value type, using null
        /// for a <see cref="None{T}" />.
        /// </summary>
        /// <remarks>
        /// Converts the option's case into a <see cref="Nullable{T}" />.
        /// Prefer it to <see cref="MapOrDefault{TOut}" /> wherever the produced
        /// type is a value type, since a mapped zero and an absent value are
        /// the same <see langword="default" /> and different nulls.
        /// </remarks>
        /// <param name="map">
        /// Produces the result from the contained value and the bound state. It
        /// is not invoked for a <see cref="None{T}" />.
        /// </param>
        /// <typeparam name="TOut">
        /// The value type <paramref name="map" /> produces.
        /// </typeparam>
        /// <returns>
        /// What <paramref name="map" /> produced, or null for a
        /// <see cref="None{T}" />.
        /// </returns>
        public TOut? MapOrNull<TOut>(Func<T, TState, TOut> map)
            where TOut : struct =>
            Source.MapOrNull(_state, map);

        /// <summary>
        /// Transforms the contained value, or computes a replacement for a
        /// <see cref="None{T}" />.
        /// </summary>
        /// <remarks>
        /// The bound state reaches both delegates. Handing it to
        /// <paramref name="map" /> alone would leave
        /// <paramref name="defaultFactory" /> capturing, and the allocation
        /// this exists to avoid would still be there.
        /// </remarks>
        /// <param name="defaultFactory">
        /// Produces the replacement from the bound state. It receives no value,
        /// there being none to hand it, and is not invoked for a
        /// <see cref="Some{T}" />.
        /// </param>
        /// <param name="map">
        /// Produces the result from the contained value and the bound state. It
        /// is not invoked for a <see cref="None{T}" />.
        /// </param>
        /// <typeparam name="TOut">The type both delegates produce.</typeparam>
        /// <returns>
        /// What <paramref name="map" /> produced, or what
        /// <paramref name="defaultFactory" /> produced.
        /// </returns>
        public TOut MapOrElse<TOut>(
            Func<TState, TOut> defaultFactory,
            Func<T, TState, TOut> map) =>
            Source.MapOrElse(_state, defaultFactory, map);

        /// <summary>
        /// Runs a side effect on the contained value and hands the same option
        /// back, so a chain can observe a step without interrupting it.
        /// </summary>
        /// <param name="action">
        /// Acts on the contained value and the bound state. It is not invoked
        /// for a <see cref="None{T}" />.
        /// </param>
        /// <returns>
        /// The option this was built from, unchanged in either case.
        /// </returns>
        public Option<T> Inspect(Action<T, TState> action) =>
            Source.Inspect(_state, action);

        /// <summary>
        /// Discards the contained value unless it satisfies
        /// <paramref name="predicate" />.
        /// </summary>
        /// <param name="predicate">
        /// Decides whether the contained value is kept. It receives the value
        /// and the bound state, and is not invoked for a
        /// <see cref="None{T}" />.
        /// </param>
        /// <returns>
        /// The same <see cref="Some{T}" /> when <paramref name="predicate" />
        /// accepts the value, otherwise <see cref="None{T}" />.
        /// </returns>
        public Option<T> Filter(Func<T, TState, bool> predicate) =>
            Source.Filter(_state, predicate);

        /// <summary>
        /// Keeps the option when it holds a value, otherwise substitutes the
        /// one <paramref name="optionFactory" /> produces.
        /// </summary>
        /// <remarks>
        /// The replacement may itself be a <see cref="None{T}" />, so this
        /// narrows nothing on its own — it is how a fallback source is tried in
        /// turn.
        /// </remarks>
        /// <param name="optionFactory">
        /// Produces the replacement from the bound state. It receives no value,
        /// there being none to hand it, and is not invoked for a
        /// <see cref="Some{T}" />.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="optionFactory" /> returns a null option. A
        /// recovery that finds nothing is a <see cref="None{T}" />, so null is
        /// no more meaningful here than anywhere else.
        /// </exception>
        /// <returns>
        /// The original option if it held a value, otherwise whatever
        /// <paramref name="optionFactory" /> produced.
        /// </returns>
        public Option<T> OrElse(Func<TState, Option<T>> optionFactory) =>
            Source.OrElse(_state, optionFactory);

        /// <summary>
        /// Converts the option into a <see cref="Result{TOk,TErr}" />,
        /// computing an error to explain an absent value.
        /// </summary>
        /// <remarks>
        /// <paramref name="errorFactory" /> runs only for a
        /// <see cref="None{T}" />; it is never invoked for a
        /// <see cref="Some{T}" />.
        /// </remarks>
        /// <param name="errorFactory">
        /// Produces the error from the bound state. It receives no value, there
        /// being none to hand it, and is not invoked for a
        /// <see cref="Some{T}" />.
        /// </param>
        /// <typeparam name="TErr">
        /// The error type <paramref name="errorFactory" /> produces.
        /// </typeparam>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="errorFactory" /> returns null. An
        /// <see cref="Err{TOk,TErr}" /> cannot hold a null error.
        /// </exception>
        /// <returns>
        /// <see cref="Ok{TOk,TErr}" /> of the contained value, or
        /// <see cref="Err{TOk,TErr}" /> of what
        /// <paramref name="errorFactory" /> produced.
        /// </returns>
        public Result<T, TErr> OkOrElse<TErr>(Func<TState, TErr> errorFactory)
            where TErr : notnull =>
            Source.OkOrElse(_state, errorFactory);

        /// <summary>
        /// Combines the contained value with a second option's, using the bound
        /// state.
        /// </summary>
        /// <remarks>
        /// Both values reach <paramref name="zip" /> as arguments already, so the
        /// bound state is for whatever else the delegate would have closed over.
        /// Where a lone <see cref="Some{T}" /> should survive the other side being
        /// absent, use <see cref="Reduce" /> instead.
        /// </remarks>
        /// <param name="other">The option to combine with.</param>
        /// <param name="zip">
        /// Combines the two contained values with the bound state. It is invoked
        /// only when both options hold a value.
        /// </param>
        /// <typeparam name="TOther">The value type of the other option.</typeparam>
        /// <typeparam name="TOut">The type the delegate produces.</typeparam>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="zip" /> returns null.
        /// </exception>
        /// <returns>
        /// <see cref="Some{T}" /> of what <paramref name="zip" /> produced when
        /// both options hold a value, otherwise <see cref="None{T}" />.
        /// </returns>
        public Option<TOut> ZipWith<TOther, TOut>(
            Option<TOther> other,
            Func<T, TOther, TState, TOut> zip)
            where TOther : notnull
            where TOut : notnull =>
            Source.ZipWith(_state, other, zip);

        /// <summary>
        /// Merges the contained value with a second option's, using the bound
        /// state, and keeps a lone value when only one side has one.
        /// </summary>
        /// <remarks>
        /// Unlike <see cref="ZipWith{TOther,TOut}" />, a <see cref="Some{T}" />
        /// survives a <see cref="None{T}" /> on the other side and is returned
        /// unchanged, so the delegate and the bound state are both consulted only
        /// when there are genuinely two values to combine.
        /// </remarks>
        /// <param name="other">The option to merge with.</param>
        /// <param name="reduce">
        /// Combines the two present values with the bound state.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="reduce" /> returns null.
        /// </exception>
        /// <returns>
        /// The combined value when both options hold one, otherwise whichever
        /// single <see cref="Some{T}" /> there was, otherwise
        /// <see cref="None{T}" />.
        /// </returns>
        public Option<T> Reduce(Option<T> other, Func<T, T, TState, T> reduce) =>
            Source.Reduce(_state, other, reduce);

        /// <summary>
        /// Awaits a predicate against the contained value and the bound state,
        /// answering false when there is no value to test.
        /// </summary>
        /// <remarks>
        /// A <see cref="None{T}" /> never invokes
        /// <paramref name="predicate" />, so the returned
        /// <see cref="ValueTask{TResult}" /> is already complete and allocates
        /// no state machine on the empty branch.
        /// </remarks>
        /// <param name="predicate">
        /// Tests the contained value against the bound state.
        /// </param>
        /// <returns>
        /// True if the option holds a value and <paramref name="predicate" />
        /// accepted it; false otherwise.
        /// </returns>
        public ValueTask<bool> IsSomeAndAsync(
            Func<T, TState, Task<bool>> predicate) =>
            Source is Some<T> some
                ? Awaited(predicate(some.Value, _state))
                : new ValueTask<bool>(false);

        /// <summary>
        /// Awaits a predicate against the contained value and the bound state,
        /// answering true when there is no value to test.
        /// </summary>
        /// <remarks>
        /// The inverse default of <see cref="IsSomeAndAsync" />: absence
        /// satisfies this rather than failing it, which makes it the one to reach
        /// for when the predicate expresses a rule a missing value is exempt
        /// from.
        /// </remarks>
        /// <param name="predicate">
        /// Tests the contained value against the bound state.
        /// </param>
        /// <returns>
        /// True if the option holds no value, or holds one
        /// <paramref name="predicate" /> accepted.
        /// </returns>
        public ValueTask<bool> IsNoneOrAsync(
            Func<T, TState, Task<bool>> predicate) =>
            Source is Some<T> some
                ? Awaited(predicate(some.Value, _state))
                : new ValueTask<bool>(true);

        /// <summary>
        /// Awaits whichever of two branches the option selects, handing the bound
        /// state to both.
        /// </summary>
        /// <remarks>
        /// Both branches await, so both build a state machine and this one keeps
        /// the <c>async</c> keyword where the mixed overloads beside it drop it.
        /// Reach for one of those when only one branch has work to await.
        /// </remarks>
        /// <param name="onSome">
        /// Produces the result from the contained value and the bound state.
        /// </param>
        /// <param name="onNone">
        /// Produces the result from the bound state alone, there being no
        /// contained value to hand it.
        /// </param>
        /// <typeparam name="TOut">The type both delegates produce.</typeparam>
        /// <returns>Whatever the delegate for the option's case returned.</returns>
        public async ValueTask<TOut> MatchAsync<TOut>(
            Func<T, TState, Task<TOut>> onSome,
            Func<TState, Task<TOut>> onNone)
        {
            return Source is Some<T> some
                ? await onSome(some.Value, _state).ConfigureAwait(false)
                : await onNone(_state).ConfigureAwait(false);
        }

        /// <summary>
        /// Awaits the branch for a contained value, answering an absent one
        /// without awaiting anything.
        /// </summary>
        /// <remarks>
        /// Neither branch builds a state machine. A <see cref="None{T}" />
        /// completes synchronously, and the value branch hands its task back
        /// wrapped rather than awaiting it, so the caller's own await is the only
        /// one. Measured at 16 bytes a call against 136 for a private
        /// <c>async</c> helper.
        /// </remarks>
        /// <param name="onSome">
        /// Produces the result from the contained value and the bound state, as
        /// work worth awaiting.
        /// </param>
        /// <param name="onNone">
        /// Produces the result from the bound state alone, without awaiting.
        /// </param>
        /// <typeparam name="TOut">The type both delegates produce.</typeparam>
        /// <returns>Whatever the delegate for the option's case returned.</returns>
        public ValueTask<TOut> MatchAsync<TOut>(
            Func<T, TState, Task<TOut>> onSome,
            Func<TState, TOut> onNone) =>
            Source is Some<T> some
                ? new ValueTask<TOut>(onSome(some.Value, _state))
                : new ValueTask<TOut>(onNone(_state));

        /// <summary>
        /// Awaits the branch for an absent value, answering a contained one
        /// without awaiting anything.
        /// </summary>
        /// <remarks>
        /// Neither branch builds a state machine, for the reason given on the
        /// overload above.
        /// </remarks>
        /// <param name="onSome">
        /// Produces the result from the contained value and the bound state,
        /// without awaiting.
        /// </param>
        /// <param name="onNone">
        /// Produces the result from the bound state alone, as work worth
        /// awaiting.
        /// </param>
        /// <typeparam name="TOut">The type both delegates produce.</typeparam>
        /// <returns>Whatever the delegate for the option's case returned.</returns>
        public ValueTask<TOut> MatchAsync<TOut>(
            Func<T, TState, TOut> onSome,
            Func<TState, Task<TOut>> onNone) =>
            Source is Some<T> some
                ? new ValueTask<TOut>(onSome(some.Value, _state))
                : new ValueTask<TOut>(onNone(_state));

        /// <summary>
        /// Awaits whichever branch the option selects for its side effect alone,
        /// handing the bound state to both.
        /// </summary>
        /// <remarks>
        /// The counterpart of
        /// <see cref="Match(Action{T,TState},Action{TState})" /> for work that has
        /// no result to return. Prefer the value-producing overload wherever one
        /// can be produced, since a side effect is harder to test than a return.
        /// </remarks>
        /// <param name="onSome">
        /// Handles the contained value and the bound state.
        /// </param>
        /// <param name="onNone">
        /// Handles the bound state alone, there being no contained value to hand
        /// it.
        /// </param>
        public ValueTask MatchAsync(
            Func<T, TState, Task> onSome,
            Func<TState, Task> onNone) =>
            Source is Some<T> some
                ? new ValueTask(onSome(some.Value, _state))
                : new ValueTask(onNone(_state));

        /// <summary>
        /// Awaits the side effect for a contained value, running the one for an
        /// absent value without awaiting.
        /// </summary>
        /// <remarks>
        /// A <see cref="None{T}" /> completes synchronously, and neither branch
        /// builds a state machine.
        /// </remarks>
        /// <param name="onSome">
        /// Handles the contained value and the bound state, as work worth
        /// awaiting.
        /// </param>
        /// <param name="onNone">
        /// Handles the bound state alone, without awaiting.
        /// </param>
        public ValueTask MatchAsync(
            Func<T, TState, Task> onSome,
            Action<TState> onNone)
        {
            if (Source is Some<T> some)
            {
                return new ValueTask(onSome(some.Value, _state));
            }

            onNone(_state);

            return default;
        }

        /// <summary>
        /// Awaits the side effect for an absent value, running the one for a
        /// contained value without awaiting.
        /// </summary>
        /// <remarks>
        /// A <see cref="Some{T}" /> completes synchronously, and neither branch
        /// builds a state machine.
        /// </remarks>
        /// <param name="onSome">
        /// Handles the contained value and the bound state, without awaiting.
        /// </param>
        /// <param name="onNone">
        /// Handles the bound state alone, as work worth awaiting.
        /// </param>
        public ValueTask MatchAsync(
            Action<T, TState> onSome,
            Func<TState, Task> onNone)
        {
            if (Source is Some<T> some)
            {
                onSome(some.Value, _state);

                return default;
            }

            return new ValueTask(onNone(_state));
        }

        /// <summary>
        /// Returns the contained value, awaiting a replacement built from the
        /// bound state when there is none.
        /// </summary>
        /// <remarks>
        /// A <see cref="Some{T}" /> completes synchronously, so the factory is
        /// not awaited for an option that already holds a value.
        /// </remarks>
        /// <param name="valueFactory">
        /// Produces the fallback from the bound state. It receives no value,
        /// there being none to hand it.
        /// </param>
        /// <returns>
        /// The contained value, or what <paramref name="valueFactory" />
        /// produced.
        /// </returns>
        public ValueTask<T> UnwrapOrElseAsync(
            Func<TState, Task<T>> valueFactory) =>
            Source is Some<T> some
                ? new ValueTask<T>(some.Value)
                : Awaited(valueFactory(_state));

        /// <summary>
        /// Awaits a transform of the contained value and the bound state, keeping
        /// the option's case.
        /// </summary>
        /// <remarks>
        /// A <see cref="None{T}" /> completes synchronously as a
        /// <see cref="None{T}" /> of the new type, so the transform is never
        /// awaited without a value to hand it.
        /// </remarks>
        /// <param name="map">
        /// Transforms the contained value using the bound state.
        /// </param>
        /// <typeparam name="TOut">The type the transform produces.</typeparam>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="map" /> returns null.
        /// </exception>
        /// <returns>
        /// <see cref="Some{T}" /> of the transformed value, or
        /// <see cref="None{T}" /> of <typeparamref name="TOut" />.
        /// </returns>
        public ValueTask<Option<TOut>> MapAsync<TOut>(
            Func<T, TState, Task<TOut>> map) where TOut : notnull =>
            Source is Some<T> some
                ? AwaitedSome(map(some.Value, _state))
                : new ValueTask<Option<TOut>>(Option.None<TOut>());

        /// <summary>
        /// Awaits an option-producing step against the contained value and the
        /// bound state, keeping whichever case the step returns.
        /// </summary>
        /// <remarks>
        /// The step returns a <see cref="ValueTask{TResult}" /> rather than a
        /// <see cref="Task{TResult}" /> so that a chain of these composes by
        /// name. Declare your own step the same way: a method group returning
        /// <see cref="Task{TResult}" /> does not convert to it, and the call
        /// site fails with <c>CS0411</c> rather than naming the mismatch.
        /// </remarks>
        /// <param name="optionFactory">
        /// Produces the next option from the contained value and the bound state.
        /// </param>
        /// <typeparam name="TOut">
        /// The value type the produced option holds.
        /// </typeparam>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="optionFactory" /> returns a null option. See the
        /// remarks on <see cref="Option{T}" /> for why that throws rather than
        /// producing a <see cref="None{T}" />. It is thrown from the call when
        /// the factory's task had already completed and faults the returned
        /// task otherwise, so await the call to see it either way.
        /// </exception>
        /// <returns>
        /// Whatever <paramref name="optionFactory" /> produced, or
        /// <see cref="None{T}" /> of <typeparamref name="TOut" />.
        /// </returns>
        public ValueTask<Option<TOut>> AndThenAsync<TOut>(
            Func<T, TState, ValueTask<Option<TOut>>> optionFactory)
            where TOut : notnull =>
            Source is Some<T> some
                ? Option.NotNullAsync(
                      optionFactory(some.Value, _state),
                      nameof(optionFactory))
                : new ValueTask<Option<TOut>>(Option.None<TOut>());

        /// <summary>
        /// Awaits a transform of the contained value and the bound state, falling
        /// back to a value already in hand.
        /// </summary>
        /// <remarks>
        /// The fallback is evaluated by the caller either way, so reach for
        /// <see cref="MapOrElseAsync{TOut}(Func{TState,Task{TOut}},Func{T,TState,Task{TOut}})" />
        /// instead once producing it allocates or otherwise does real work.
        /// </remarks>
        /// <param name="defaultValue">
        /// Returned for a <see cref="None{T}" />, and evaluated whether or not it
        /// is used.
        /// </param>
        /// <param name="map">
        /// Transforms the contained value using the bound state.
        /// </param>
        /// <typeparam name="TOut">
        /// The type the transform and the fallback share.
        /// </typeparam>
        /// <returns>
        /// The transformed value, or <paramref name="defaultValue" />.
        /// </returns>
        public ValueTask<TOut> MapOrAsync<TOut>(
            TOut defaultValue,
            Func<T, TState, Task<TOut>> map) =>
            Source is Some<T> some
                ? Awaited(map(some.Value, _state))
                : new ValueTask<TOut>(defaultValue);

        /// <summary>
        /// Awaits a transform of the contained value and the bound state, falling
        /// back to the default of the produced type.
        /// </summary>
        /// <remarks>
        /// The fallback is indistinguishable from a transform that produced the
        /// same default, so this suits a type whose default already reads as the
        /// absent case. <c>WM2015</c> reports the reading where it does not.
        /// </remarks>
        /// <param name="map">
        /// Transforms the contained value using the bound state.
        /// </param>
        /// <typeparam name="TOut">The type the transform produces.</typeparam>
        /// <returns>
        /// The transformed value, or the default of
        /// <typeparamref name="TOut" />.
        /// </returns>
        public ValueTask<TOut?> MapOrDefaultAsync<TOut>(
            Func<T, TState, Task<TOut>> map) where TOut : notnull =>
            Source is Some<T> some
                ? AwaitedNullable(map(some.Value, _state))
                : default;

        /// <summary>
        /// Awaits a transform of the contained value and the bound state into a
        /// nullable value type, using null for a <see cref="None{T}" />.
        /// </summary>
        /// <remarks>
        /// The asynchronous counterpart of <see cref="MapOrNull{TOut}" />.
        /// Unlike <see cref="MapOrDefaultAsync{TOut}" />, the absent case is a
        /// null rather than a <see langword="default" />, so it stays
        /// distinguishable from a transform that produced a zero.
        /// <para>
        /// A <see cref="None{T}" /> returns an already-completed
        /// <see cref="ValueTask{TResult}" /> and never invokes
        /// <paramref name="map" />, so that branch builds no state machine.
        /// </para>
        /// </remarks>
        /// <param name="map">
        /// Transforms the contained value using the bound state.
        /// </param>
        /// <typeparam name="TOut">
        /// The value type the transform produces.
        /// </typeparam>
        /// <returns>
        /// The transformed value, or null for a <see cref="None{T}" />.
        /// </returns>
        public ValueTask<TOut?> MapOrNullAsync<TOut>(
            Func<T, TState, Task<TOut>> map) where TOut : struct =>
            Source is Some<T> some
                ? AwaitedOrNull(map(some.Value, _state))
                : default;

        /// <summary>
        /// Awaits a transform of the contained value and the bound state, or
        /// awaits a fallback built from the state alone.
        /// </summary>
        /// <remarks>
        /// Exactly one of the two runs, which is what separates this from
        /// <see cref="MapOrAsync{TOut}" /> - reach for it when producing the
        /// fallback is itself work worth skipping.
        /// </remarks>
        /// <param name="defaultFactory">
        /// Produces the result for a <see cref="None{T}" /> from the bound state.
        /// </param>
        /// <param name="map">
        /// Transforms the contained value using the bound state.
        /// </param>
        /// <typeparam name="TOut">The type both delegates produce.</typeparam>
        /// <returns>Whatever the delegate selected produced.</returns>
        public async ValueTask<TOut> MapOrElseAsync<TOut>(
            Func<TState, Task<TOut>> defaultFactory,
            Func<T, TState, Task<TOut>> map)
        {
            return Source is Some<T> some
                ? await map(some.Value, _state).ConfigureAwait(false)
                : await defaultFactory(_state).ConfigureAwait(false);
        }

        /// <summary>
        /// Awaits the transform of a contained value, building the fallback
        /// without awaiting.
        /// </summary>
        /// <remarks>
        /// Neither branch builds a state machine, as on
        /// <see cref="MatchAsync{TOut}(Func{T,TState,Task{TOut}},Func{TState,TOut})" />.
        /// </remarks>
        /// <param name="defaultFactory">
        /// Produces the result for a <see cref="None{T}" /> from the bound
        /// state, without awaiting.
        /// </param>
        /// <param name="map">
        /// Transforms the contained value using the bound state, as work worth
        /// awaiting.
        /// </param>
        /// <typeparam name="TOut">The type both delegates produce.</typeparam>
        /// <returns>Whatever the delegate selected produced.</returns>
        public ValueTask<TOut> MapOrElseAsync<TOut>(
            Func<TState, TOut> defaultFactory,
            Func<T, TState, Task<TOut>> map) =>
            Source is Some<T> some
                ? new ValueTask<TOut>(map(some.Value, _state))
                : new ValueTask<TOut>(defaultFactory(_state));

        /// <summary>
        /// Awaits the fallback for an absent value, transforming a contained one
        /// without awaiting.
        /// </summary>
        /// <remarks>
        /// Neither branch builds a state machine, as on
        /// <see cref="MatchAsync{TOut}(Func{T,TState,Task{TOut}},Func{TState,TOut})" />.
        /// </remarks>
        /// <param name="defaultFactory">
        /// Produces the result for a <see cref="None{T}" /> from the bound
        /// state, as work worth awaiting.
        /// </param>
        /// <param name="map">
        /// Transforms the contained value using the bound state, without
        /// awaiting.
        /// </param>
        /// <typeparam name="TOut">The type both delegates produce.</typeparam>
        /// <returns>Whatever the delegate selected produced.</returns>
        public ValueTask<TOut> MapOrElseAsync<TOut>(
            Func<TState, Task<TOut>> defaultFactory,
            Func<T, TState, TOut> map) =>
            Source is Some<T> some
                ? new ValueTask<TOut>(map(some.Value, _state))
                : new ValueTask<TOut>(defaultFactory(_state));

        /// <summary>
        /// Awaits a side effect against the contained value and the bound state,
        /// handing the option back unchanged.
        /// </summary>
        /// <remarks>
        /// The option is returned as it was, so this drops into a chain without
        /// altering what flows through it. A <see cref="None{T}" /> completes
        /// synchronously.
        /// </remarks>
        /// <param name="action">
        /// Acts on the contained value and the bound state.
        /// </param>
        /// <returns>The same option, whichever case it is in.</returns>
        public ValueTask<Option<T>> InspectAsync(
            Func<T, TState, Task> action)
        {
            Option<T> option = Source;

            return option is Some<T> some
                ? AwaitedInspect(action(some.Value, _state), option)
                : new ValueTask<Option<T>>(option);
        }

        /// <summary>
        /// Awaits a predicate against the contained value and the bound state,
        /// discarding a value it rejects.
        /// </summary>
        /// <remarks>
        /// A rejected value becomes a <see cref="None{T}" />, so this narrows the
        /// option rather than reporting on it - which is the difference from
        /// <see cref="IsSomeAndAsync" />. A <see cref="None{T}" /> stays one and
        /// completes synchronously.
        /// </remarks>
        /// <param name="predicate">
        /// Tests the contained value against the bound state.
        /// </param>
        /// <returns>
        /// The option unchanged if <paramref name="predicate" /> accepted its
        /// value, otherwise <see cref="None{T}" />.
        /// </returns>
        public ValueTask<Option<T>> FilterAsync(
            Func<T, TState, Task<bool>> predicate)
        {
            Option<T> option = Source;

            return option is Some<T> some
                ? AwaitedFilter(predicate(some.Value, _state), option)
                : new ValueTask<Option<T>>(option);
        }

        /// <summary>
        /// Awaits a substitute option built from the bound state when there is no
        /// value.
        /// </summary>
        /// <remarks>
        /// The substitute may itself be a <see cref="None{T}" />, so this is one
        /// attempt at recovering a value rather than a guarantee of one. The step
        /// returns a <see cref="ValueTask{TResult}" /> so a chain of these
        /// composes by name.
        /// </remarks>
        /// <param name="optionFactory">
        /// Produces the replacement from the bound state. It receives no value,
        /// there being none to hand it.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="optionFactory" /> returns a null option. A
        /// recovery that finds nothing is a <see cref="None{T}" />, so null is
        /// no more meaningful here than anywhere else. It is thrown from the
        /// call when the factory's task had already completed and faults the
        /// returned task otherwise, so await the call to see it either way.
        /// </exception>
        /// <returns>
        /// The original option if it holds a value, otherwise whatever
        /// <paramref name="optionFactory" /> produced.
        /// </returns>
        public ValueTask<Option<T>> OrElseAsync(
            Func<TState, ValueTask<Option<T>>> optionFactory)
        {
            Option<T> option = Source;

            return option is Some<T>
                ? new ValueTask<Option<T>>(option)
                : Option.NotNullAsync(
                      optionFactory(_state),
                      nameof(optionFactory));
        }

        /// <summary>
        /// Converts the option to a result, awaiting an error built from the
        /// bound state when there is no value.
        /// </summary>
        /// <remarks>
        /// <paramref name="errorFactory" /> runs only for a
        /// <see cref="None{T}" />, so it can build an error using whatever
        /// context caused the absence.
        /// </remarks>
        /// <param name="errorFactory">
        /// Produces the error from the bound state. It receives no value, there
        /// being none to hand it.
        /// </param>
        /// <typeparam name="TErr">
        /// The error type the factory produces.
        /// </typeparam>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="errorFactory" /> returns null. An
        /// <see cref="Err{TOk,TErr}" /> cannot hold a null error.
        /// </exception>
        /// <returns>
        /// <see cref="Ok{TOk,TErr}" /> of the contained value, or
        /// <see cref="Err{TOk,TErr}" /> of what
        /// <paramref name="errorFactory" /> produced.
        /// </returns>
        public ValueTask<Result<T, TErr>> OkOrElseAsync<TErr>(
            Func<TState, Task<TErr>> errorFactory) where TErr : notnull =>
            Source is Some<T> some
                ? new ValueTask<Result<T, TErr>>(
                      Result.Ok<T, TErr>(some.Value))
                : AwaitedErr<TErr>(errorFactory(_state));

        /// <summary>
        /// Awaits a combination of the contained value and a second option's,
        /// using the bound state.
        /// </summary>
        /// <remarks>
        /// A <see cref="None{T}" /> on either side returns an already-completed
        /// <see cref="ValueTask{TResult}" /> and never invokes
        /// <paramref name="zip" />, so neither absent case builds a state machine
        /// or costs an await.
        /// </remarks>
        /// <param name="other">The option to combine with.</param>
        /// <param name="zip">
        /// Combines the two contained values with the bound state. It is invoked
        /// only when both options hold a value.
        /// </param>
        /// <typeparam name="TOther">The value type of the other option.</typeparam>
        /// <typeparam name="TOut">The type the delegate produces.</typeparam>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="zip" /> returns null.
        /// </exception>
        /// <returns>
        /// <see cref="Some{T}" /> of what <paramref name="zip" /> produced when
        /// both options hold a value, otherwise <see cref="None{T}" />.
        /// </returns>
        public ValueTask<Option<TOut>> ZipWithAsync<TOther, TOut>(
            Option<TOther> other,
            Func<T, TOther, TState, Task<TOut>> zip)
            where TOther : notnull
            where TOut : notnull =>
            Source is Some<T> some && other is Some<TOther> otherSome
                ? AwaitedSome(zip(some.Value, otherSome.Value, _state))
                : new ValueTask<Option<TOut>>(Option.None<TOut>());

        /// <summary>
        /// Awaits a merge of the contained value with a second option's, using the
        /// bound state, and keeps a lone value when only one side has one.
        /// </summary>
        /// <remarks>
        /// Unlike <see cref="ZipWithAsync{TOther,TOut}" />, a lone
        /// <see cref="Some{T}" /> is returned unchanged rather than discarded. Only
        /// the branch with two values to combine awaits, so the other three return
        /// an already-completed <see cref="ValueTask{TResult}" />.
        /// </remarks>
        /// <param name="other">The option to merge with.</param>
        /// <param name="reduce">
        /// Combines the two present values with the bound state.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="reduce" /> returns null.
        /// </exception>
        /// <returns>
        /// The combined value when both options hold one, otherwise whichever
        /// single <see cref="Some{T}" /> there was, otherwise
        /// <see cref="None{T}" />.
        /// </returns>
        public ValueTask<Option<T>> ReduceAsync(
            Option<T> other,
            Func<T, T, TState, Task<T>> reduce) =>
            Source is Some<T> some
                ? other is Some<T> otherSome
                    ? AwaitedSome(reduce(some.Value, otherSome.Value, _state))
                    : new ValueTask<Option<T>>(some)
                : new ValueTask<Option<T>>(other);

        private static async ValueTask<TResult> Awaited<TResult>(
            Task<TResult> task) =>
            await task.ConfigureAwait(false);

        private static async ValueTask<Option<TOut>> AwaitedSome<TOut>(
            Task<TOut> task) where TOut : notnull =>
            Option.Some(await task.ConfigureAwait(false));

        private static async ValueTask<TOut?> AwaitedNullable<TOut>(
            Task<TOut> task) where TOut : notnull =>
            await task.ConfigureAwait(false);

        private static async ValueTask<TOut?> AwaitedOrNull<TOut>(
            Task<TOut> task) where TOut : struct =>
            await task.ConfigureAwait(false);

        private static async ValueTask<Option<T>> AwaitedInspect(
            Task task,
            Option<T> option)
        {
            await task.ConfigureAwait(false);

            return option;
        }

        private static async ValueTask<Option<T>> AwaitedFilter(
            Task<bool> task,
            Option<T> option) =>
            await task.ConfigureAwait(false) ? option : Option.None<T>();

        private static async ValueTask<Result<T, TErr>> AwaitedErr<TErr>(
            Task<TErr> task) where TErr : notnull =>
            Result.Err<T, TErr>(await task.ConfigureAwait(false));
    }
}
