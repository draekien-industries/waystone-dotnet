namespace Waystone.Monads.Options;

using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
#if !DEBUG
using System.Diagnostics;
#endif

public static partial class Option
{
    /// <summary>
    /// A value bound for whichever <c>Try</c> is called on it next.
    /// </summary>
    /// <remarks>
    /// The counterpart of <see cref="Option{T}.Bound{TState}" /> for the two
    /// factories here, which have no option to be called on and so cannot be
    /// reached the same way. The members mirror
    /// <see cref="Option.Try{TState,T}" /> and
    /// <see cref="Option.TryAsync{TState,T}" /> minus the state argument.
    /// <para>
    /// Unlike <see cref="Option{T}.Bound{TState}" />, a
    /// <see langword="default" /> instance here is usable rather than broken:
    /// it binds <see langword="default" /> of
    /// <typeparamref name="TState" />, and a null state is permitted anyway.
    /// </para>
    /// </remarks>
    /// <typeparam name="TState">The type of the bound value.</typeparam>
#if !DEBUG
    [DebuggerStepThrough]
#endif
    public readonly struct Bound<TState>
    {
        private readonly TState _state;

        internal Bound(TState state) => _state = state;

        /// <summary>
        /// Runs <paramref name="factory" />, turning a thrown exception or a
        /// null result into a <see cref="None{T}" />.
        /// </summary>
        /// <remarks>
        /// Which exceptions are swallowed is decided by
        /// <see cref="Configs.MonadOptions" />, not by this call, and anything
        /// outside that set propagates. A handled one is reported to
        /// <see cref="Diagnostics.MonadDiagnostics" /> with the caller
        /// information below, so an absence is traceable to the line that
        /// produced it.
        /// </remarks>
        /// <param name="factory">
        /// Produces the value from the bound state. Its exceptions are what this
        /// method exists to absorb.
        /// </param>
        /// <param name="callerMemberName">
        /// Compiler-supplied for the exception logger. Do not pass it.
        /// </param>
        /// <param name="callerLineNumber">
        /// Compiler-supplied for the exception logger. Do not pass it.
        /// </param>
        /// <param name="callerArgumentExpression">
        /// Compiler-supplied for the exception logger. Do not pass it.
        /// </param>
        /// <typeparam name="T">
        /// The type <paramref name="factory" /> produces.
        /// </typeparam>
        /// <returns>
        /// A <see cref="Some{T}" /> of what <paramref name="factory" />
        /// produced, or a <see cref="None{T}" /> if it returned null or threw.
        /// </returns>
        public Option<T> Try<T>(
            Func<TState, T> factory,
            [CallerMemberName] string callerMemberName = "",
            [CallerLineNumber] int callerLineNumber = 0,
            [CallerArgumentExpression(nameof(factory))]
            string callerArgumentExpression = "")
            where T : notnull =>
            Option.Try(
                _state,
                factory,
                callerMemberName,
                callerLineNumber,
                callerArgumentExpression);

        /// <summary>
        /// Awaits <paramref name="asyncFactory" />, turning a thrown exception
        /// or a null result into a <see cref="None{T}" />.
        /// </summary>
        /// <remarks>
        /// The task is awaited here, so a fault raised after the delegate
        /// returns is caught as well — which is the whole difference from
        /// handing an async delegate to <see cref="Try{T}" />, where the task
        /// would be stored unawaited and nothing would be caught at all.
        /// <c>WM1011</c> reports that mistake.
        /// </remarks>
        /// <param name="asyncFactory">
        /// Produces the value from the bound state. Its exceptions, and its
        /// task's, are what this method exists to absorb.
        /// </param>
        /// <param name="callerMemberName">
        /// Compiler-supplied for the exception logger. Do not pass it.
        /// </param>
        /// <param name="callerLineNumber">
        /// Compiler-supplied for the exception logger. Do not pass it.
        /// </param>
        /// <param name="callerArgumentExpression">
        /// Compiler-supplied for the exception logger. Do not pass it.
        /// </param>
        /// <typeparam name="T">
        /// The type <paramref name="asyncFactory" /> produces.
        /// </typeparam>
        /// <returns>
        /// A <see cref="Some{T}" /> of what <paramref name="asyncFactory" />
        /// produced, or a <see cref="None{T}" /> if it returned null or threw.
        /// </returns>
        public ValueTask<Option<T>> TryAsync<T>(
            Func<TState, Task<T>> asyncFactory,
            [CallerMemberName] string callerMemberName = "",
            [CallerLineNumber] int callerLineNumber = 0,
            [CallerArgumentExpression(nameof(asyncFactory))]
            string callerArgumentExpression = "")
            where T : notnull =>
            Option.TryAsync(
                _state,
                asyncFactory,
                callerMemberName,
                callerLineNumber,
                callerArgumentExpression);
    }

    /// <summary>
    /// Binds a value so that the next <c>Try</c> can hand it to its factory
    /// rather than have the factory capture it.
    /// </summary>
    /// <remarks>
    /// The factory-side spelling of <c>With</c> on an
    /// <see cref="Option{T}" />: there is no option to call it on yet, so it is
    /// called on <see cref="Option" /> instead and the state is bound before
    /// the option exists.
    /// <code>
    /// Option.With(text)
    ///       .Try(static s =&gt; int.Parse(s));
    /// </code>
    /// <para>
    /// Pass a tuple to bind more than one value. Mark the factory
    /// <see langword="static" /> so the compiler reports an error if it
    /// captures a variable.
    /// </para>
    /// </remarks>
    /// <param name="state">
    /// The value handed to the factory of whichever <c>Try</c> is called next.
    /// It is unconstrained, so a null state is permitted.
    /// </param>
    /// <typeparam name="TState">The type of the bound value.</typeparam>
    /// <returns>
    /// A <see cref="Bound{TState}" /> ready to hand <paramref name="state" />
    /// to whichever <c>Try</c> is called on it.
    /// </returns>
    public static Bound<TState> With<TState>(TState state) => new(state);
}
