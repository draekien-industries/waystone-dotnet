namespace Waystone.Monads.Results;

using System;
using System.Threading.Tasks;
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
        /// Transforms the contained ok value into a nullable value type, using
        /// null for a failure.
        /// </summary>
        /// <remarks>
        /// The bridge out of <see cref="Result{TOk,TErr}" /> into
        /// <see cref="Nullable{T}" />. Prefer it to
        /// <see cref="MapOrDefault{TOut}" /> wherever the produced type is a value
        /// type, since a mapped zero and a failure are the same
        /// <see langword="default" /> and different nulls. The error itself is
        /// discarded either way.
        /// </remarks>
        /// <param name="map">
        /// Produces the result from the contained ok value and the bound state.
        /// It is not invoked for an <see cref="Err{TOk,TErr}" />.
        /// </param>
        /// <typeparam name="TOut">
        /// The value type <paramref name="map" /> produces.
        /// </typeparam>
        /// <returns>
        /// What <paramref name="map" /> produced, or null for an
        /// <see cref="Err{TOk,TErr}" />.
        /// </returns>
        public TOut? MapOrNull<TOut>(Func<TOk, TState, TOut> map)
            where TOut : struct =>
            Source.MapOrNull(_state, map);

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

        /// <summary>
        /// Awaits a predicate against the contained ok value and the bound state,
        /// answering false for an error.
        /// </summary>
        /// <remarks>
        /// An <see cref="Err{TOk,TErr}" /> never invokes
        /// <paramref name="predicate" />, so the returned
        /// <see cref="ValueTask{TResult}" /> is already complete and allocates
        /// no state machine on the failed branch.
        /// </remarks>
        /// <param name="predicate">
        /// Tests the contained ok value against the bound state.
        /// </param>
        /// <returns>
        /// True if the result succeeded and <paramref name="predicate" />
        /// accepted its value; false otherwise.
        /// </returns>
        public ValueTask<bool> IsOkAndAsync(
            Func<TOk, TState, Task<bool>> predicate) =>
            Source is Ok<TOk, TErr> ok
                ? Awaited(predicate(ok.Value, _state))
                : new ValueTask<bool>(false);

        /// <summary>
        /// Awaits a predicate against the contained error and the bound state,
        /// answering false for a success.
        /// </summary>
        /// <remarks>
        /// Not the negation of <see cref="IsOkAndAsync" />: both answer false for
        /// the case they do not describe, so a success and a rejected error are
        /// indistinguishable here.
        /// </remarks>
        /// <param name="predicate">
        /// Tests the contained error against the bound state.
        /// </param>
        /// <returns>
        /// True if the result failed and <paramref name="predicate" /> accepted
        /// its error; false otherwise.
        /// </returns>
        public ValueTask<bool> IsErrAndAsync(
            Func<TErr, TState, Task<bool>> predicate)
        {
            Result<TOk, TErr> result = Source;

            return result is Ok<TOk, TErr>
                ? new ValueTask<bool>(false)
                : Awaited(predicate(result.UnwrapErr(), _state));
        }

        /// <summary>
        /// Awaits whichever of two branches the result selects, handing the bound
        /// state and that branch's value to it.
        /// </summary>
        /// <remarks>
        /// Both branches await, so both build a state machine and this one keeps
        /// the <c>async</c> keyword where the mixed overloads beside it drop it.
        /// Reach for one of those when only one branch has work to await.
        /// </remarks>
        /// <param name="onOk">
        /// Produces the result from the contained ok value and the bound state.
        /// </param>
        /// <param name="onErr">
        /// Produces the result from the contained error and the bound state.
        /// </param>
        /// <typeparam name="TOut">The type both delegates produce.</typeparam>
        /// <returns>Whatever the delegate for the result's case returned.</returns>
        public async ValueTask<TOut> MatchAsync<TOut>(
            Func<TOk, TState, Task<TOut>> onOk,
            Func<TErr, TState, Task<TOut>> onErr)
        {
            Result<TOk, TErr> result = Source;

            return result is Ok<TOk, TErr> ok
                ? await onOk(ok.Value, _state).ConfigureAwait(false)
                : await onErr(result.UnwrapErr(), _state).ConfigureAwait(false);
        }

        /// <summary>
        /// Awaits the branch for a success, answering a failure without awaiting.
        /// </summary>
        /// <remarks>
        /// An <see cref="Err{TOk,TErr}" /> completes synchronously, and neither
        /// branch builds a state machine.
        /// </remarks>
        /// <param name="onOk">
        /// Produces the result from the contained ok value and the bound state,
        /// as work worth awaiting.
        /// </param>
        /// <param name="onErr">
        /// Produces the result from the contained error and the bound state,
        /// without awaiting.
        /// </param>
        /// <typeparam name="TOut">The type both delegates produce.</typeparam>
        /// <returns>Whatever the delegate for the result's case returned.</returns>
        public ValueTask<TOut> MatchAsync<TOut>(
            Func<TOk, TState, Task<TOut>> onOk,
            Func<TErr, TState, TOut> onErr)
        {
            Result<TOk, TErr> result = Source;

            return result is Ok<TOk, TErr> ok
                ? new ValueTask<TOut>(onOk(ok.Value, _state))
                : new ValueTask<TOut>(onErr(result.UnwrapErr(), _state));
        }

        /// <summary>
        /// Awaits the branch for a failure, answering a success without awaiting.
        /// </summary>
        /// <remarks>
        /// An <see cref="Ok{TOk,TErr}" /> completes synchronously, and neither
        /// branch builds a state machine.
        /// <para>
        /// The body is character-for-character the same as the overload above,
        /// which is correct and reads like a copy-paste slip. The delegates are
        /// the other way round, so each <c>new ValueTask&lt;TOut&gt;(…)</c> binds
        /// to the other constructor — the one taking a <c>Task&lt;TOut&gt;</c>
        /// here where it took a <c>TOut</c> there. Making the two bodies *look*
        /// different is what would actually break one of them.
        /// </para>
        /// </remarks>
        /// <param name="onOk">
        /// Produces the result from the contained ok value and the bound state,
        /// without awaiting.
        /// </param>
        /// <param name="onErr">
        /// Produces the result from the contained error and the bound state, as
        /// work worth awaiting.
        /// </param>
        /// <typeparam name="TOut">The type both delegates produce.</typeparam>
        /// <returns>Whatever the delegate for the result's case returned.</returns>
        public ValueTask<TOut> MatchAsync<TOut>(
            Func<TOk, TState, TOut> onOk,
            Func<TErr, TState, Task<TOut>> onErr)
        {
            Result<TOk, TErr> result = Source;

            return result is Ok<TOk, TErr> ok
                ? new ValueTask<TOut>(onOk(ok.Value, _state))
                : new ValueTask<TOut>(onErr(result.UnwrapErr(), _state));
        }

        /// <summary>
        /// Awaits whichever of two branches the result selects, for their side
        /// effect alone.
        /// </summary>
        /// <remarks>
        /// The counterpart of
        /// <see cref="Match(Action{TOk,TState},Action{TErr,TState})" /> for work
        /// that has no result to return. Prefer the value-producing overload
        /// wherever one can be produced, since a side effect is harder to test
        /// than a return.
        /// </remarks>
        /// <param name="onOk">
        /// Handles the contained ok value and the bound state.
        /// </param>
        /// <param name="onErr">
        /// Handles the contained error and the bound state.
        /// </param>
        public async ValueTask MatchAsync(
            Func<TOk, TState, Task> onOk,
            Func<TErr, TState, Task> onErr)
        {
            Result<TOk, TErr> result = Source;

            if (result is Ok<TOk, TErr> ok)
            {
                await onOk(ok.Value, _state).ConfigureAwait(false);

                return;
            }

            await onErr(result.UnwrapErr(), _state).ConfigureAwait(false);
        }

        /// <summary>
        /// Awaits the side effect for a success, running the one for a failure
        /// without awaiting.
        /// </summary>
        /// <remarks>
        /// An <see cref="Err{TOk,TErr}" /> completes synchronously, so nothing is
        /// awaited and no state machine is built on that branch.
        /// </remarks>
        /// <param name="onOk">
        /// Handles the contained ok value and the bound state, as work worth
        /// awaiting.
        /// </param>
        /// <param name="onErr">
        /// Handles the contained error and the bound state, without awaiting.
        /// </param>
        public ValueTask MatchAsync(
            Func<TOk, TState, Task> onOk,
            Action<TErr, TState> onErr)
        {
            Result<TOk, TErr> result = Source;

            if (result is Ok<TOk, TErr> ok)
            {
                return new ValueTask(onOk(ok.Value, _state));
            }

            onErr(result.UnwrapErr(), _state);

            return default;
        }

        /// <summary>
        /// Awaits the side effect for a failure, running the one for a success
        /// without awaiting.
        /// </summary>
        /// <remarks>
        /// An <see cref="Ok{TOk,TErr}" /> completes synchronously, so nothing is
        /// awaited and no state machine is built on that branch.
        /// </remarks>
        /// <param name="onOk">
        /// Handles the contained ok value and the bound state, without awaiting.
        /// </param>
        /// <param name="onErr">
        /// Handles the contained error and the bound state, as work worth
        /// awaiting.
        /// </param>
        public ValueTask MatchAsync(
            Action<TOk, TState> onOk,
            Func<TErr, TState, Task> onErr)
        {
            Result<TOk, TErr> result = Source;

            if (result is Ok<TOk, TErr> ok)
            {
                onOk(ok.Value, _state);

                return default;
            }

            return new ValueTask(onErr(result.UnwrapErr(), _state));
        }

        /// <summary>
        /// Awaits a result-producing step against the contained ok value and the
        /// bound state, leaving an error untouched.
        /// </summary>
        /// <remarks>
        /// The step may fail in its own right, so this is where a chain's error
        /// can first appear. It returns a <see cref="ValueTask{TResult}" /> so a
        /// chain of these composes by name: a method group returning
        /// <see cref="Task{TResult}" /> does not convert to it, and
        /// <c>WSG0003</c> reports that shape at the declaration.
        /// </remarks>
        /// <param name="resultFactory">
        /// Produces the next result from the contained ok value and the bound
        /// state.
        /// </param>
        /// <typeparam name="TOut">
        /// The ok type the produced result holds.
        /// </typeparam>
        /// <returns>
        /// Whatever <paramref name="resultFactory" /> produced, or the original
        /// error.
        /// </returns>
        public ValueTask<Result<TOut, TErr>> AndThenAsync<TOut>(
            Func<TOk, TState, ValueTask<Result<TOut, TErr>>> resultFactory)
            where TOut : notnull
        {
            Result<TOk, TErr> result = Source;

            return result is Ok<TOk, TErr> ok
                ? resultFactory(ok.Value, _state)
                : new ValueTask<Result<TOut, TErr>>(
                      Result.Err<TOut, TErr>(result.UnwrapErr()));
        }

        /// <summary>
        /// Awaits a recovery step against the contained error and the bound
        /// state, leaving a success untouched.
        /// </summary>
        /// <remarks>
        /// The recovery chooses a new error type, so this is how a chain
        /// translates one failure vocabulary into another. It may also fail
        /// again, so this is an attempt at recovery rather than a guarantee of
        /// one.
        /// </remarks>
        /// <param name="resultFactory">
        /// Produces the replacement from the contained error and the bound state.
        /// </param>
        /// <typeparam name="TOut">
        /// The error type the produced result carries.
        /// </typeparam>
        /// <returns>
        /// The original ok value, or whatever
        /// <paramref name="resultFactory" /> produced.
        /// </returns>
        public ValueTask<Result<TOk, TOut>> OrElseAsync<TOut>(
            Func<TErr, TState, ValueTask<Result<TOk, TOut>>> resultFactory)
            where TOut : notnull
        {
            Result<TOk, TErr> result = Source;

            return result is Ok<TOk, TErr> ok
                ? new ValueTask<Result<TOk, TOut>>(
                      Result.Ok<TOk, TOut>(ok.Value))
                : resultFactory(result.UnwrapErr(), _state);
        }

        /// <summary>
        /// Returns the contained ok value, awaiting a replacement built from the
        /// error and the bound state when there is none.
        /// </summary>
        /// <remarks>
        /// The factory receives the error, so the fallback can depend on what
        /// went wrong rather than being a single blanket value.
        /// </remarks>
        /// <param name="valueFactory">
        /// Produces the fallback from the contained error and the bound state.
        /// </param>
        /// <returns>
        /// The contained ok value, or what <paramref name="valueFactory" />
        /// produced.
        /// </returns>
        public ValueTask<TOk> UnwrapOrElseAsync(
            Func<TErr, TState, Task<TOk>> valueFactory)
        {
            Result<TOk, TErr> result = Source;

            return result is Ok<TOk, TErr> ok
                ? new ValueTask<TOk>(ok.Value)
                : Awaited(valueFactory(result.UnwrapErr(), _state));
        }

        /// <summary>
        /// Awaits a side effect against the contained ok value and the bound
        /// state, handing the result back unchanged.
        /// </summary>
        /// <remarks>
        /// The result is returned as it was, so this drops into a chain without
        /// altering what flows through it. An <see cref="Err{TOk,TErr}" />
        /// completes synchronously.
        /// </remarks>
        /// <param name="action">
        /// Acts on the contained ok value and the bound state.
        /// </param>
        /// <returns>The same result, whichever case it is in.</returns>
        public ValueTask<Result<TOk, TErr>> InspectAsync(
            Func<TOk, TState, Task> action)
        {
            Result<TOk, TErr> result = Source;

            return result is Ok<TOk, TErr> ok
                ? AwaitedInspect(action(ok.Value, _state), result)
                : new ValueTask<Result<TOk, TErr>>(result);
        }

        /// <summary>
        /// Awaits a side effect against the contained error and the bound state,
        /// handing the result back unchanged.
        /// </summary>
        /// <remarks>
        /// The mirror of <see cref="InspectAsync" /> on the failed branch, and the
        /// usual place to log a failure without handling it. An
        /// <see cref="Ok{TOk,TErr}" /> completes synchronously.
        /// </remarks>
        /// <param name="action">
        /// Acts on the contained error and the bound state.
        /// </param>
        /// <returns>The same result, whichever case it is in.</returns>
        public ValueTask<Result<TOk, TErr>> InspectErrAsync(
            Func<TErr, TState, Task> action)
        {
            Result<TOk, TErr> result = Source;

            return result is Ok<TOk, TErr>
                ? new ValueTask<Result<TOk, TErr>>(result)
                : AwaitedInspect(
                      action(result.UnwrapErr(), _state),
                      result);
        }

        /// <summary>
        /// Awaits a transform of the contained ok value and the bound state,
        /// leaving an error untouched.
        /// </summary>
        /// <remarks>
        /// The error type is carried across unchanged, so this changes what
        /// success looks like without touching what failure means.
        /// </remarks>
        /// <param name="map">
        /// Transforms the contained ok value using the bound state.
        /// </param>
        /// <typeparam name="TOut">The type the transform produces.</typeparam>
        /// <returns>
        /// <see cref="Ok{TOk,TErr}" /> of the transformed value, or the original
        /// error.
        /// </returns>
        public ValueTask<Result<TOut, TErr>> MapAsync<TOut>(
            Func<TOk, TState, Task<TOut>> map) where TOut : notnull
        {
            Result<TOk, TErr> result = Source;

            return result is Ok<TOk, TErr> ok
                ? AwaitedOk<TOut>(map(ok.Value, _state))
                : new ValueTask<Result<TOut, TErr>>(
                      Result.Err<TOut, TErr>(result.UnwrapErr()));
        }

        /// <summary>
        /// Awaits a transform of the contained ok value and the bound state,
        /// falling back to a value already in hand.
        /// </summary>
        /// <remarks>
        /// The error is discarded rather than reported, so reach for
        /// <see cref="MapOrElseAsync{TOut}(Func{TErr,TState,Task{TOut}},Func{TOk,TState,Task{TOut}})" />
        /// when the fallback should depend on
        /// what went wrong.
        /// </remarks>
        /// <param name="defaultValue">
        /// Returned for an <see cref="Err{TOk,TErr}" />, and evaluated whether or
        /// not it is used.
        /// </param>
        /// <param name="map">
        /// Transforms the contained ok value using the bound state.
        /// </param>
        /// <typeparam name="TOut">
        /// The type the transform and the fallback share.
        /// </typeparam>
        /// <returns>
        /// The transformed value, or <paramref name="defaultValue" />.
        /// </returns>
        public ValueTask<TOut> MapOrAsync<TOut>(
            TOut defaultValue,
            Func<TOk, TState, Task<TOut>> map) where TOut : notnull =>
            Source is Ok<TOk, TErr> ok
                ? Awaited(map(ok.Value, _state))
                : new ValueTask<TOut>(defaultValue);

        /// <summary>
        /// Awaits a transform of the contained ok value and the bound state,
        /// falling back to the default of the produced type.
        /// </summary>
        /// <remarks>
        /// The error is discarded and the fallback is indistinguishable from a
        /// transform that produced the same default, so this suits a type whose
        /// default already reads as failure.
        /// </remarks>
        /// <param name="map">
        /// Transforms the contained ok value using the bound state.
        /// </param>
        /// <typeparam name="TOut">The type the transform produces.</typeparam>
        /// <returns>
        /// The transformed value, or the default of
        /// <typeparamref name="TOut" />.
        /// </returns>
        public ValueTask<TOut?> MapOrDefaultAsync<TOut>(
            Func<TOk, TState, Task<TOut>> map) where TOut : notnull =>
            Source is Ok<TOk, TErr> ok
                ? AwaitedNullable(map(ok.Value, _state))
                : default;

        /// <summary>
        /// Awaits a transform of the contained ok value and the bound state into a
        /// nullable value type, using null for a failure.
        /// </summary>
        /// <remarks>
        /// The asynchronous half of the bridge into <see cref="Nullable{T}" />.
        /// Unlike <see cref="MapOrDefaultAsync{TOut}" />, a failure is a null
        /// rather than a <see langword="default" />, so it stays distinguishable
        /// from a transform that produced a zero.
        /// <para>
        /// An <see cref="Err{TOk,TErr}" /> returns an already-completed
        /// <see cref="ValueTask{TResult}" /> and never invokes
        /// <paramref name="map" />, so that branch builds no state machine.
        /// </para>
        /// </remarks>
        /// <param name="map">
        /// Transforms the contained ok value using the bound state.
        /// </param>
        /// <typeparam name="TOut">
        /// The value type the transform produces.
        /// </typeparam>
        /// <returns>
        /// The transformed value, or null for an <see cref="Err{TOk,TErr}" />.
        /// </returns>
        public ValueTask<TOut?> MapOrNullAsync<TOut>(
            Func<TOk, TState, Task<TOut>> map) where TOut : struct =>
            Source is Ok<TOk, TErr> ok
                ? AwaitedOrNull(map(ok.Value, _state))
                : default;

        /// <summary>
        /// Awaits a transform of the contained ok value, or awaits a fallback
        /// built from the error, handing the bound state to whichever runs.
        /// </summary>
        /// <remarks>
        /// Exactly one of the two runs, and the fallback receives the error, so
        /// this is the overload that answers both cases without discarding why
        /// the failed one failed.
        /// </remarks>
        /// <param name="defaultFactory">
        /// Produces the result from the contained error and the bound state.
        /// </param>
        /// <param name="map">
        /// Transforms the contained ok value using the bound state.
        /// </param>
        /// <typeparam name="TOut">The type both delegates produce.</typeparam>
        /// <returns>Whatever the delegate selected produced.</returns>
        public async ValueTask<TOut> MapOrElseAsync<TOut>(
            Func<TErr, TState, Task<TOut>> defaultFactory,
            Func<TOk, TState, Task<TOut>> map) where TOut : notnull
        {
            Result<TOk, TErr> result = Source;

            return result is Ok<TOk, TErr> ok
                ? await map(ok.Value, _state).ConfigureAwait(false)
                : await defaultFactory(result.UnwrapErr(), _state)
                     .ConfigureAwait(false);
        }

        /// <summary>
        /// Awaits the transform of a success, building the fallback from the
        /// error without awaiting.
        /// </summary>
        /// <remarks>
        /// Neither branch builds a state machine. An <see cref="Err{TOk,TErr}" />
        /// completes synchronously, and the ok branch hands its task back wrapped
        /// rather than awaiting it, so the caller's own await is the only one.
        /// Measured at 16 bytes a call against 136 for a private <c>async</c>
        /// helper.
        /// </remarks>
        /// <param name="defaultFactory">
        /// Produces the result from the contained error and the bound state,
        /// without awaiting.
        /// </param>
        /// <param name="map">
        /// Transforms the contained ok value using the bound state, as work
        /// worth awaiting.
        /// </param>
        /// <typeparam name="TOut">The type both delegates produce.</typeparam>
        /// <returns>Whatever the delegate selected produced.</returns>
        public ValueTask<TOut> MapOrElseAsync<TOut>(
            Func<TErr, TState, TOut> defaultFactory,
            Func<TOk, TState, Task<TOut>> map) where TOut : notnull
        {
            Result<TOk, TErr> result = Source;

            return result is Ok<TOk, TErr> ok
                ? new ValueTask<TOut>(map(ok.Value, _state))
                : new ValueTask<TOut>(
                    defaultFactory(result.UnwrapErr(), _state));
        }

        /// <summary>
        /// Awaits the fallback built from the error, transforming a success
        /// without awaiting.
        /// </summary>
        /// <remarks>
        /// Neither branch builds a state machine, as on
        /// <see cref="MapOrElseAsync{TOut}(Func{TErr,TState,TOut},Func{TOk,TState,Task{TOut}})" />.
        /// </remarks>
        /// <param name="defaultFactory">
        /// Produces the result from the contained error and the bound state, as
        /// work worth awaiting.
        /// </param>
        /// <param name="map">
        /// Transforms the contained ok value using the bound state, without
        /// awaiting.
        /// </param>
        /// <typeparam name="TOut">The type both delegates produce.</typeparam>
        /// <returns>Whatever the delegate selected produced.</returns>
        public ValueTask<TOut> MapOrElseAsync<TOut>(
            Func<TErr, TState, Task<TOut>> defaultFactory,
            Func<TOk, TState, TOut> map) where TOut : notnull
        {
            Result<TOk, TErr> result = Source;

            return result is Ok<TOk, TErr> ok
                ? new ValueTask<TOut>(map(ok.Value, _state))
                : new ValueTask<TOut>(
                    defaultFactory(result.UnwrapErr(), _state));
        }

        /// <summary>
        /// Awaits a restatement of the contained error and the bound state,
        /// leaving a success untouched.
        /// </summary>
        /// <remarks>
        /// The ok type is carried across unchanged, so this is the mirror of
        /// <see cref="MapAsync{TOut}" /> and the usual way to translate a failure
        /// into the vocabulary of the caller above.
        /// </remarks>
        /// <param name="map">
        /// Restates the contained error using the bound state.
        /// </param>
        /// <typeparam name="TOut">The error type the transform produces.</typeparam>
        /// <returns>
        /// The original ok value, or <see cref="Err{TOk,TErr}" /> of the restated
        /// error.
        /// </returns>
        public ValueTask<Result<TOk, TOut>> MapErrAsync<TOut>(
            Func<TErr, TState, Task<TOut>> map) where TOut : notnull
        {
            Result<TOk, TErr> result = Source;

            return result is Ok<TOk, TErr> ok
                ? new ValueTask<Result<TOk, TOut>>(
                      Result.Ok<TOk, TOut>(ok.Value))
                : AwaitedErr<TOut>(map(result.UnwrapErr(), _state));
        }

        private static async ValueTask<TResult> Awaited<TResult>(
            Task<TResult> task) =>
            await task.ConfigureAwait(false);

        private static async ValueTask<TOut?> AwaitedNullable<TOut>(
            Task<TOut> task) where TOut : notnull =>
            await task.ConfigureAwait(false);

        private static async ValueTask<TOut?> AwaitedOrNull<TOut>(
            Task<TOut> task) where TOut : struct =>
            await task.ConfigureAwait(false);

        private static async ValueTask<Result<TOk, TErr>> AwaitedInspect(
            Task task,
            Result<TOk, TErr> result)
        {
            await task.ConfigureAwait(false);

            return result;
        }

        private static async ValueTask<Result<TOut, TErr>> AwaitedOk<TOut>(
            Task<TOut> task) where TOut : notnull =>
            Result.Ok<TOut, TErr>(await task.ConfigureAwait(false));

        private static async ValueTask<Result<TOk, TOut>> AwaitedErr<TOut>(
            Task<TOut> task) where TOut : notnull =>
            Result.Err<TOk, TOut>(await task.ConfigureAwait(false));
    }
}
