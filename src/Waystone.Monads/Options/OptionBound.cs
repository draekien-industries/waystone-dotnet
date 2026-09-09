namespace Waystone.Monads.Options;

using System;
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
    }
}
