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
    /// a name with, invokes it with the bound state, and returns the plain
    /// <see cref="Option{T}" /> — the state is spent by the call rather than
    /// carried onward, so a chain that needs it twice binds it twice.
    /// <para>
    /// Nested inside <see cref="Option{T}" /> rather than named beside it
    /// because a second type parameter on a monad in this library already means
    /// something else: <see cref="Result{TOk,TErr}" /> uses it for the error
    /// type. Nesting keeps <see cref="Option{T}" /> a one-parameter type, so a
    /// genuine <c>Option&lt;string, int&gt;</c> still fails to compile.
    /// </para>
    /// <para>
    /// The point is the delegate, not this type. A lambda that reads the state
    /// from its parameter captures nothing, so marking it
    /// <see langword="static" /> costs nothing and the compiler caches it; a
    /// lambda that reaches for an outer variable allocates a display class
    /// every time the call site runs. Writing <see langword="static" /> is what
    /// stops a later edit from quietly putting the allocation back.
    /// </para>
    /// <para>
    /// A <see langword="default" /> instance has no option to act on and every
    /// member throws <see cref="InvalidOperationException" />, in the manner of
    /// <c>ImmutableArray&lt;T&gt;</c>. Reach one only by declaring it —
    /// <c>With</c> cannot produce one.
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
        /// <see langword="true" /> only when the option holds a value and
        /// <paramref name="predicate" /> accepts it.
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
        /// <see langword="true" /> when the option is a <see cref="None{T}" />,
        /// or when it holds a value <paramref name="predicate" /> accepts.
        /// </returns>
        public bool IsNoneOr(Func<T, TState, bool> predicate) =>
            Source.IsNoneOr(_state, predicate);

        /// <summary>
        /// Produces a value from whichever case the option is in, so both cases
        /// are answered in one expression.
        /// </summary>
        /// <remarks>
        /// The bound state reaches both delegates, which is what makes this the
        /// most worthwhile member to bind for. Two capturing lambdas share one
        /// display class but need a delegate each, so a capturing <c>Match</c>
        /// allocates one delegate more than a capturing <c>Map</c> does.
        /// <c>StateOverloadBenchmarks</c> measures both.
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
        /// delegate here when producing the replacement costs something.
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
        /// whether it is needed or not. Where producing it costs something, use
        /// <see cref="MapOrElse{TOut}" /> instead.
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
        /// This is the seam where an absence stops being acceptable and has to
        /// be accounted for. <paramref name="errorFactory" /> runs only for a
        /// <see cref="None{T}" />, so building an error that carries context is
        /// affordable here.
        /// </remarks>
        /// <param name="errorFactory">
        /// Produces the error from the bound state. It receives no value, there
        /// being none to hand it, and is not invoked for a
        /// <see cref="Some{T}" />.
        /// </param>
        /// <typeparam name="TErr">
        /// The error type <paramref name="errorFactory" /> produces.
        /// </typeparam>
        /// <returns>
        /// <see cref="Ok{TOk,TErr}" /> of the contained value, or
        /// <see cref="Err{TOk,TErr}" /> of what
        /// <paramref name="errorFactory" /> produced.
        /// </returns>
        public Result<T, TErr> OkOrElse<TErr>(Func<TState, TErr> errorFactory)
            where TErr : notnull =>
            Source.OkOrElse(_state, errorFactory);

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
        public async ValueTask<bool> IsSomeAndAsync(
            Func<T, TState, Task<bool>> predicate)
        {
            return Source is Some<T> some
                && await predicate(some.Value, _state).ConfigureAwait(false);
        }

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
        public async ValueTask<bool> IsNoneOrAsync(
            Func<T, TState, Task<bool>> predicate)
        {
            return Source is not Some<T> some
                || await predicate(some.Value, _state).ConfigureAwait(false);
        }

        /// <summary>
        /// Awaits whichever of two branches the option selects, handing the bound
        /// state to both.
        /// </summary>
        /// <remarks>
        /// Only the both-asynchronous form is offered, where
        /// <see cref="Option{T}" /> itself also carries the two mixed ones. To
        /// await one branch and not the other, call
        /// <see cref="Match{TOut}(Func{T,TState,TOut},Func{TState,TOut})" /> and
        /// await inside the branch that needs it.
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
        public async ValueTask<T> UnwrapOrElseAsync(
            Func<TState, Task<T>> valueFactory)
        {
            return Source is Some<T> some
                ? some.Value
                : await valueFactory(_state).ConfigureAwait(false);
        }

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
        /// <returns>
        /// <see cref="Some{T}" /> of the transformed value, or
        /// <see cref="None{T}" /> of <typeparamref name="TOut" />.
        /// </returns>
        public async ValueTask<Option<TOut>> MapAsync<TOut>(
            Func<T, TState, Task<TOut>> map) where TOut : notnull
        {
            return Source is Some<T> some
                ? Option.Some(
                      await map(some.Value, _state).ConfigureAwait(false))
                : Option.None<TOut>();
        }

        /// <summary>
        /// Awaits an option-producing step against the contained value and the
        /// bound state, keeping whichever case the step returns.
        /// </summary>
        /// <remarks>
        /// The step returns a <see cref="ValueTask{TResult}" /> rather than a
        /// <see cref="Task{TResult}" /> so that a chain of these composes by
        /// name: a method group returning <see cref="Task{TResult}" /> does not
        /// convert to it, and <c>WSG0003</c> reports that shape at the
        /// declaration rather than leaving the caller a <c>CS0411</c>.
        /// </remarks>
        /// <param name="optionFactory">
        /// Produces the next option from the contained value and the bound state.
        /// </param>
        /// <typeparam name="TOut">
        /// The value type the produced option holds.
        /// </typeparam>
        /// <returns>
        /// Whatever <paramref name="optionFactory" /> produced, or
        /// <see cref="None{T}" /> of <typeparamref name="TOut" />.
        /// </returns>
        public async ValueTask<Option<TOut>> AndThenAsync<TOut>(
            Func<T, TState, ValueTask<Option<TOut>>> optionFactory)
            where TOut : notnull
        {
            return Source is Some<T> some
                ? await optionFactory(some.Value, _state).ConfigureAwait(false)
                : Option.None<TOut>();
        }

        /// <summary>
        /// Awaits a transform of the contained value and the bound state, falling
        /// back to a value already in hand.
        /// </summary>
        /// <remarks>
        /// The fallback is evaluated by the caller either way, so reach for
        /// <see cref="MapOrElseAsync{TOut}" /> instead once producing it costs
        /// anything.
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
        public async ValueTask<TOut> MapOrAsync<TOut>(
            TOut defaultValue,
            Func<T, TState, Task<TOut>> map)
        {
            return Source is Some<T> some
                ? await map(some.Value, _state).ConfigureAwait(false)
                : defaultValue;
        }

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
        public async ValueTask<TOut?> MapOrDefaultAsync<TOut>(
            Func<T, TState, Task<TOut>> map) where TOut : notnull
        {
            return Source is Some<T> some
                ? await map(some.Value, _state).ConfigureAwait(false)
                : default;
        }

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
        public async ValueTask<Option<T>> InspectAsync(
            Func<T, TState, Task> action)
        {
            Option<T> option = Source;

            if (option is Some<T> some)
            {
                await action(some.Value, _state).ConfigureAwait(false);
            }

            return option;
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
        public async ValueTask<Option<T>> FilterAsync(
            Func<T, TState, Task<bool>> predicate)
        {
            Option<T> option = Source;

            if (option is not Some<T> some) return option;

            return await predicate(some.Value, _state).ConfigureAwait(false)
                ? option
                : Option.None<T>();
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
        /// <returns>
        /// The original option if it holds a value, otherwise whatever
        /// <paramref name="optionFactory" /> produced.
        /// </returns>
        public async ValueTask<Option<T>> OrElseAsync(
            Func<TState, ValueTask<Option<T>>> optionFactory)
        {
            Option<T> option = Source;

            return option is Some<T>
                ? option
                : await optionFactory(_state).ConfigureAwait(false);
        }

        /// <summary>
        /// Converts the option to a result, awaiting an error built from the
        /// bound state when there is no value.
        /// </summary>
        /// <remarks>
        /// The crossing point from "absent" to "failed for a stated reason",
        /// which is why the error is produced rather than passed - an option
        /// reaching here usually knows why it is empty.
        /// </remarks>
        /// <param name="errorFactory">
        /// Produces the error from the bound state. It receives no value, there
        /// being none to hand it.
        /// </param>
        /// <typeparam name="TErr">
        /// The error type the factory produces.
        /// </typeparam>
        /// <returns>
        /// <see cref="Ok{TOk,TErr}" /> of the contained value, or
        /// <see cref="Err{TOk,TErr}" /> of what
        /// <paramref name="errorFactory" /> produced.
        /// </returns>
        public async ValueTask<Result<T, TErr>> OkOrElseAsync<TErr>(
            Func<TState, Task<TErr>> errorFactory) where TErr : notnull
        {
            return Source is Some<T> some
                ? Result.Ok<T, TErr>(some.Value)
                : Result.Err<T, TErr>(
                      await errorFactory(_state).ConfigureAwait(false));
        }
    }
}
