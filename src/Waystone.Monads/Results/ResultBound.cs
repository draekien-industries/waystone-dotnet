namespace Waystone.Monads.Results;

using System;
#if !DEBUG
using System.Diagnostics;
#endif

public abstract partial record Result<TOk, TErr>
    where TOk : notnull where TErr : notnull
{
    /// <summary>
    /// A <see cref="Result{TOk,TErr}" /> with a value bound to it, ready to be
    /// handed to a delegate that would otherwise have captured it.
    /// </summary>
    /// <remarks>
    /// Created by <c>With</c> on a <see cref="Result{TOk,TErr}" />. Each member
    /// below takes the same delegate as the <see cref="Result{TOk,TErr}" />
    /// member it shares a name with, invokes it with the bound state, and
    /// returns the plain <see cref="Result{TOk,TErr}" /> — the state is spent by
    /// the call rather than carried onward, so a chain that needs it twice binds
    /// it twice.
    /// <para>
    /// Every delegate here receives a value as well as the state, which is
    /// where this differs from <see cref="Options.Option{T}.Bound{TState}" />:
    /// a result always holds something, so there is no branch that has only the
    /// state to give. Which value arrives depends on the branch — the ok value
    /// or the error.
    /// </para>
    /// <para>
    /// The point is the delegate, not this type. A lambda that reads the state
    /// from its parameter captures nothing, so marking it
    /// <see langword="static" /> costs nothing and the compiler caches it; a
    /// lambda that reaches for an outer variable allocates a display class every
    /// time the call site runs. Writing <see langword="static" /> is what stops
    /// a later edit from quietly putting the allocation back.
    /// </para>
    /// <para>
    /// A <see langword="default" /> instance has no result to act on and every
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
        private readonly Result<TOk, TErr> _result;
        private readonly TState _state;

        internal Bound(Result<TOk, TErr> result, TState state)
        {
            _result = result;
            _state = state;
        }

        private Result<TOk, TErr> Source =>
            _result
         ?? throw new InvalidOperationException(
                "A default Result<TOk, TErr>.Bound<TState> has no result bound to it. Build one by calling With on a result.");

        /// <summary>
        /// Tests the contained ok value against <paramref name="predicate" />,
        /// treating a failure as a failure.
        /// </summary>
        /// <param name="predicate">
        /// Decides whether the ok value qualifies. It receives that value and
        /// the bound state, and is not invoked for an
        /// <see cref="Err{TOk,TErr}" />.
        /// </param>
        /// <returns>
        /// <see langword="true" /> only when the result succeeded and
        /// <paramref name="predicate" /> accepts its value.
        /// </returns>
        public bool IsOkAnd(Func<TOk, TState, bool> predicate) =>
            Source.IsOkAnd(_state, predicate);

        /// <summary>
        /// Tests the contained error against <paramref name="predicate" />,
        /// treating a success as a failure to match.
        /// </summary>
        /// <remarks>
        /// Reaches the *error*, not the ok value, so this is how a caller asks
        /// which kind of failure it got without unwrapping and without a
        /// <see cref="Match{TOut}" /> whose success branch has nothing to do.
        /// </remarks>
        /// <param name="predicate">
        /// Decides whether the error qualifies. It receives that error and the
        /// bound state, and is not invoked for an
        /// <see cref="Ok{TOk,TErr}" />.
        /// </param>
        /// <returns>
        /// <see langword="true" /> only when the result failed and
        /// <paramref name="predicate" /> accepts its error.
        /// </returns>
        public bool IsErrAnd(Func<TErr, TState, bool> predicate) =>
            Source.IsErrAnd(_state, predicate);

        /// <summary>
        /// Produces a value from whichever case the result is in, so both cases
        /// are answered in one expression.
        /// </summary>
        /// <remarks>
        /// The bound state reaches both delegates, which is what makes this the
        /// most worthwhile member to bind for. Two capturing lambdas share one
        /// display class but need a delegate each, so a capturing <c>Match</c>
        /// allocates one delegate more than a capturing <c>Map</c> does.
        /// <c>StateOverloadBenchmarks</c> measures both.
        /// </remarks>
        /// <param name="onOk">
        /// Produces the result from the contained ok value and the bound state.
        /// </param>
        /// <param name="onErr">
        /// Produces the result from the contained error and the bound state.
        /// </param>
        /// <typeparam name="TOut">The type both delegates produce.</typeparam>
        /// <returns>
        /// Whatever the delegate for the result's case returned.
        /// </returns>
        public TOut Match<TOut>(
            Func<TOk, TState, TOut> onOk,
            Func<TErr, TState, TOut> onErr) =>
            Source.Match(_state, onOk, onErr);

        /// <summary>
        /// Runs a side effect for whichever case the result is in, producing
        /// nothing.
        /// </summary>
        /// <remarks>
        /// The counterpart of <see cref="Match{TOut}" /> for work that has no
        /// result to return — writing a log line, incrementing a counter.
        /// Prefer that overload wherever a value can be produced instead, since
        /// a side effect is harder to test than a return.
        /// </remarks>
        /// <param name="onOk">
        /// Acts on the contained ok value and the bound state.
        /// </param>
        /// <param name="onErr">
        /// Acts on the contained error and the bound state.
        /// </param>
        public void Match(Action<TOk, TState> onOk, Action<TErr, TState> onErr) =>
            Source.Match(_state, onOk, onErr);

        /// <summary>
        /// Continues the chain with another result, leaving a failure alone.
        /// </summary>
        /// <remarks>
        /// The error type is fixed across the step, so
        /// <paramref name="resultFactory" /> can fail but not change how failure
        /// is described. Use <see cref="OrElse{TOut}" /> to work on the error
        /// side instead.
        /// </remarks>
        /// <param name="resultFactory">
        /// Produces the next result from the contained ok value and the bound
        /// state. It is not invoked for an <see cref="Err{TOk,TErr}" />.
        /// </param>
        /// <typeparam name="TOut">
        /// The ok type of the result <paramref name="resultFactory" /> produces.
        /// </typeparam>
        /// <returns>
        /// Whatever <paramref name="resultFactory" /> produced, or the original
        /// error unchanged.
        /// </returns>
        public Result<TOut, TErr> AndThen<TOut>(
            Func<TOk, TState, Result<TOut, TErr>> resultFactory)
            where TOut : notnull =>
            Source.AndThen(_state, resultFactory);

        /// <summary>
        /// Recovers from a failure with another result, leaving a success alone.
        /// </summary>
        /// <remarks>
        /// The mirror of <see cref="AndThen{TOut}" /> across the two cases: the
        /// ok type is fixed and the error type is what the step may change, so
        /// this is where a failure is retried, translated, or turned into a
        /// success.
        /// </remarks>
        /// <param name="resultFactory">
        /// Produces the replacement from the contained error and the bound
        /// state. It is not invoked for an <see cref="Ok{TOk,TErr}" />.
        /// </param>
        /// <typeparam name="TOut">
        /// The error type of the result <paramref name="resultFactory" />
        /// produces.
        /// </typeparam>
        /// <returns>
        /// The original ok value unchanged, or whatever
        /// <paramref name="resultFactory" /> produced.
        /// </returns>
        public Result<TOk, TOut> OrElse<TOut>(
            Func<TErr, TState, Result<TOk, TOut>> resultFactory)
            where TOut : notnull =>
            Source.OrElse(_state, resultFactory);

        /// <summary>
        /// Returns the contained ok value, falling back to what
        /// <paramref name="valueFactory" /> produces.
        /// </summary>
        /// <remarks>
        /// The fallback sees the error, so the replacement can depend on what
        /// went wrong rather than being a blanket default. That is the
        /// difference from <see cref="Result{TOk,TErr}.UnwrapOr" />, which never
        /// learns the error at all.
        /// </remarks>
        /// <param name="valueFactory">
        /// Produces the replacement from the contained error and the bound
        /// state. It is not invoked for an <see cref="Ok{TOk,TErr}" />.
        /// </param>
        /// <returns>
        /// The contained ok value, or what <paramref name="valueFactory" />
        /// produced.
        /// </returns>
        public TOk UnwrapOrElse(Func<TErr, TState, TOk> valueFactory) =>
            Source.UnwrapOrElse(_state, valueFactory);

        /// <summary>
        /// Runs a side effect on the contained ok value and hands the same
        /// result back, so a chain can observe a step without interrupting it.
        /// </summary>
        /// <param name="action">
        /// Acts on the contained ok value and the bound state. It is not invoked
        /// for an <see cref="Err{TOk,TErr}" />.
        /// </param>
        /// <returns>
        /// The result this was built from, unchanged in either case.
        /// </returns>
        public Result<TOk, TErr> Inspect(Action<TOk, TState> action) =>
            Source.Inspect(_state, action);

        /// <summary>
        /// Runs a side effect on the contained error and hands the same result
        /// back, so a failure can be recorded without being handled here.
        /// </summary>
        /// <remarks>
        /// The usual reason to reach for this rather than
        /// <see cref="Match(System.Action{TOk,TState},System.Action{TErr,TState})" />
        /// is that the failure is being logged at this level but decided at a
        /// higher one, and the result has to continue on unchanged.
        /// </remarks>
        /// <param name="action">
        /// Acts on the contained error and the bound state. It is not invoked
        /// for an <see cref="Ok{TOk,TErr}" />.
        /// </param>
        /// <returns>
        /// The result this was built from, unchanged in either case.
        /// </returns>
        public Result<TOk, TErr> InspectErr(Action<TErr, TState> action) =>
            Source.InspectErr(_state, action);

        /// <summary>
        /// Transforms the contained ok value, leaving a failure alone.
        /// </summary>
        /// <param name="map">
        /// Produces the new value from the contained ok value and the bound
        /// state. It is not invoked for an <see cref="Err{TOk,TErr}" />.
        /// </param>
        /// <typeparam name="TOut">
        /// The type <paramref name="map" /> produces.
        /// </typeparam>
        /// <returns>
        /// <see cref="Ok{TOk,TErr}" /> of the new value, or the original error
        /// unchanged.
        /// </returns>
        public Result<TOut, TErr> Map<TOut>(Func<TOk, TState, TOut> map)
            where TOut : notnull =>
            Source.Map(_state, map);

        /// <summary>
        /// Transforms the contained ok value, or returns
        /// <paramref name="defaultValue" /> for a failure.
        /// </summary>
        /// <remarks>
        /// The error is discarded rather than inspected, so reach for this only
        /// where every failure deserves the same answer. Where it does not, use
        /// <see cref="MapOrElse{TOut}" />, which is handed the error.
        /// </remarks>
        /// <param name="defaultValue">Stands in for a failure.</param>
        /// <param name="map">
        /// Produces the result from the contained ok value and the bound state.
        /// It is not invoked for an <see cref="Err{TOk,TErr}" />.
        /// </param>
        /// <typeparam name="TOut">
        /// The type <paramref name="map" /> produces, and the type of
        /// <paramref name="defaultValue" />.
        /// </typeparam>
        /// <returns>
        /// What <paramref name="map" /> produced, or
        /// <paramref name="defaultValue" />.
        /// </returns>
        public TOut MapOr<TOut>(TOut defaultValue, Func<TOk, TState, TOut> map) =>
            Source.MapOr(_state, defaultValue, map);

        /// <summary>
        /// Transforms the contained ok value, or returns the default of
        /// <typeparamref name="TOut" /> for a failure.
        /// </summary>
        /// <remarks>
        /// For a value type the default is indistinguishable from a legitimate
        /// zero, so a caller who needs to tell a failure apart from a genuine
        /// result wants <see cref="MapOr{TOut}" /> with a sentinel, or
        /// <c>MapOrNull</c>.
        /// </remarks>
        /// <param name="map">
        /// Produces the result from the contained ok value and the bound state.
        /// It is not invoked for an <see cref="Err{TOk,TErr}" />.
        /// </param>
        /// <typeparam name="TOut">
        /// The type <paramref name="map" /> produces.
        /// </typeparam>
        /// <returns>
        /// What <paramref name="map" /> produced, or the default of
        /// <typeparamref name="TOut" />.
        /// </returns>
        public TOut? MapOrDefault<TOut>(Func<TOk, TState, TOut> map)
            where TOut : notnull =>
            Source.MapOrDefault(_state, map);

        /// <summary>
        /// Transforms the contained ok value, or computes a replacement from the
        /// error.
        /// </summary>
        /// <remarks>
        /// The bound state reaches both delegates. Handing it to
        /// <paramref name="map" /> alone would leave
        /// <paramref name="defaultFactory" /> capturing, and the allocation this
        /// exists to avoid would still be there.
        /// </remarks>
        /// <param name="defaultFactory">
        /// Produces the replacement from the contained error and the bound
        /// state. It is not invoked for an <see cref="Ok{TOk,TErr}" />.
        /// </param>
        /// <param name="map">
        /// Produces the result from the contained ok value and the bound state.
        /// It is not invoked for an <see cref="Err{TOk,TErr}" />.
        /// </param>
        /// <typeparam name="TOut">The type both delegates produce.</typeparam>
        /// <returns>
        /// What <paramref name="map" /> produced, or what
        /// <paramref name="defaultFactory" /> produced.
        /// </returns>
        public TOut MapOrElse<TOut>(
            Func<TErr, TState, TOut> defaultFactory,
            Func<TOk, TState, TOut> map) =>
            Source.MapOrElse(_state, defaultFactory, map);

        /// <summary>
        /// Transforms the contained error, leaving a success alone.
        /// </summary>
        /// <remarks>
        /// How a failure crosses a boundary: an error from one layer is restated
        /// in the vocabulary of the next without the success path being touched
        /// or the chain being broken open.
        /// </remarks>
        /// <param name="map">
        /// Produces the new error from the contained one and the bound state. It
        /// is not invoked for an <see cref="Ok{TOk,TErr}" />.
        /// </param>
        /// <typeparam name="TOut">
        /// The error type <paramref name="map" /> produces.
        /// </typeparam>
        /// <returns>
        /// The original ok value unchanged, or <see cref="Err{TOk,TErr}" /> of
        /// the new error.
        /// </returns>
        public Result<TOk, TOut> MapErr<TOut>(Func<TErr, TState, TOut> map)
            where TOut : notnull =>
            Source.MapErr(_state, map);
    }
}
