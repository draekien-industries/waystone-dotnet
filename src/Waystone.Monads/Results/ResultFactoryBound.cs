namespace Waystone.Monads.Results;

using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Errors;
#if !DEBUG
using System.Diagnostics;
#endif

public static partial class Result
{
    /// <summary>
    /// Binds a value so that the next <c>Try</c> can hand it to its factory
    /// rather than have the factory capture it.
    /// </summary>
    /// <remarks>
    /// The counterpart of <see cref="Result{TOk,TErr}.Bound{TState}" /> for the
    /// factories here, which have no result yet to call members on, so this
    /// type exists instead.
    /// <para>
    /// Each <c>Try</c> comes in two forms, as it does on
    /// <see cref="Result" /> itself: one that is handed an
    /// <c>onError</c> and decides its own error type, and one that omits it and
    /// reports an <see cref="Error" />.
    /// </para>
    /// <para>
    /// Unlike <see cref="Result{TOk,TErr}.Bound{TState}" />, a
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
        /// Runs <paramref name="factory" /> and turns anything it throws into an
        /// error of your own type.
        /// </summary>
        /// <remarks>
        /// Which exceptions are swallowed is decided by
        /// <see cref="Configs.MonadOptions" />, not by this call, and anything
        /// outside that set propagates.
        /// <para>
        /// A <paramref name="factory" /> that returns <see langword="null" />
        /// is treated as a failure too: <paramref name="onError" /> is invoked
        /// with an <see cref="ArgumentNullException" /> that was never thrown,
        /// so it carries no stack trace and is not logged. An
        /// <see cref="OperationCanceledException" /> is not caught at all and
        /// propagates to the caller unless
        /// <see cref="Configs.MonadOptionsBuilder.UseCancellationAsFailure" />
        /// is configured.
        /// </para>
        /// </remarks>
        /// <param name="factory">
        /// Produces the ok value from the bound state. Its exceptions are what
        /// this method exists to catch.
        /// </param>
        /// <param name="onError">
        /// Describes a caught exception as <typeparamref name="TErr" />. It is
        /// handed the exception rather than the bound state, so it is the one
        /// delegate here that binding does not help.
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
        /// <typeparam name="TOk">
        /// The type <paramref name="factory" /> produces.
        /// </typeparam>
        /// <typeparam name="TErr">
        /// The type <paramref name="onError" /> produces.
        /// </typeparam>
        /// <returns>
        /// <see cref="Ok{TOk,TErr}" /> of what <paramref name="factory" />
        /// produced, or <see cref="Err{TOk,TErr}" /> of what
        /// <paramref name="onError" /> made of the exception.
        /// </returns>
        public Result<TOk, TErr> Try<TOk, TErr>(
            Func<TState, TOk> factory,
            Func<Exception, TErr> onError,
            [CallerMemberName] string callerMemberName = "",
            [CallerLineNumber] int callerLineNumber = 0,
            [CallerArgumentExpression(nameof(factory))]
            string callerArgumentExpression = "")
            where TOk : notnull where TErr : notnull =>
            Result.Try(
                _state,
                factory,
                onError,
                callerMemberName,
                callerLineNumber,
                callerArgumentExpression);

        /// <summary>
        /// Runs <paramref name="factory" /> and turns anything it throws into an
        /// <see cref="Error" />.
        /// </summary>
        /// <remarks>
        /// The shorter of the two: no <c>onError</c>, and the failure is
        /// described by the library's own <see cref="Error" />. Reach for the
        /// overload that takes <c>onError</c> once the caller needs to
        /// distinguish failures by type rather than by message.
        /// <para>
        /// A <paramref name="factory" /> that returns <see langword="null" />
        /// is treated as a failure too, carrying an <see cref="Error" />
        /// converted from an <see cref="ArgumentNullException" /> that was
        /// never thrown, so it carries no stack trace and is not logged. An
        /// <see cref="OperationCanceledException" /> is not caught at all and
        /// propagates to the caller unless
        /// <see cref="Configs.MonadOptionsBuilder.UseCancellationAsFailure" />
        /// is configured.
        /// </para>
        /// </remarks>
        /// <param name="factory">
        /// Produces the ok value from the bound state. Its exceptions are what
        /// this method exists to catch.
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
        /// <typeparam name="TOk">
        /// The type <paramref name="factory" /> produces.
        /// </typeparam>
        /// <returns>
        /// <see cref="Ok{TOk,TErr}" /> of what <paramref name="factory" />
        /// produced, or <see cref="Err{TOk,TErr}" /> of an
        /// <see cref="Error" /> describing the exception.
        /// </returns>
        public Result<TOk, Error> Try<TOk>(
            Func<TState, TOk> factory,
            [CallerMemberName] string callerMemberName = "",
            [CallerLineNumber] int callerLineNumber = 0,
            [CallerArgumentExpression(nameof(factory))]
            string callerArgumentExpression = "")
            where TOk : notnull =>
            Result.Try(
                _state,
                factory,
                callerMemberName,
                callerLineNumber,
                callerArgumentExpression);

        /// <summary>
        /// Awaits <paramref name="asyncFactory" /> and turns anything it throws
        /// into an error of your own type.
        /// </summary>
        /// <remarks>
        /// The task is awaited here, so a fault raised after the delegate
        /// returns is caught as well — which is the whole difference from
        /// handing an async delegate to
        /// <see cref="Try{TOk,TErr}" />, where the task would be
        /// stored unawaited and nothing would be caught at all. <c>WM1011</c>
        /// reports that mistake.
        /// <para>
        /// A <paramref name="asyncFactory" /> that returns
        /// <see langword="null" /> is treated as a failure too:
        /// <paramref name="onError" /> is invoked with an
        /// <see cref="ArgumentNullException" /> that was never thrown, so it
        /// carries no stack trace and is not logged. An
        /// <see cref="OperationCanceledException" /> is not caught at all and
        /// propagates to the caller unless
        /// <see cref="Configs.MonadOptionsBuilder.UseCancellationAsFailure" />
        /// is configured.
        /// </para>
        /// </remarks>
        /// <param name="asyncFactory">
        /// Produces the ok value from the bound state. Its exceptions, and its
        /// task's, are what this method exists to catch.
        /// </param>
        /// <param name="onError">
        /// Describes a caught exception as <typeparamref name="TErr" />. It is
        /// handed the exception rather than the bound state, so it is the one
        /// delegate here that binding does not help.
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
        /// <typeparam name="TOk">
        /// The type <paramref name="asyncFactory" /> produces.
        /// </typeparam>
        /// <typeparam name="TErr">
        /// The type <paramref name="onError" /> produces.
        /// </typeparam>
        /// <returns>
        /// <see cref="Ok{TOk,TErr}" /> of what
        /// <paramref name="asyncFactory" /> produced, or
        /// <see cref="Err{TOk,TErr}" /> of what <paramref name="onError" /> made
        /// of the exception.
        /// </returns>
        public ValueTask<Result<TOk, TErr>> TryAsync<TOk, TErr>(
            Func<TState, Task<TOk>> asyncFactory,
            Func<Exception, TErr> onError,
            [CallerMemberName] string callerMemberName = "",
            [CallerLineNumber] int callerLineNumber = 0,
            [CallerArgumentExpression(nameof(asyncFactory))]
            string callerArgumentExpression = "")
            where TOk : notnull where TErr : notnull =>
            Result.TryAsync(
                _state,
                asyncFactory,
                onError,
                callerMemberName,
                callerLineNumber,
                callerArgumentExpression);

        /// <summary>
        /// Awaits <paramref name="asyncFactory" /> and turns anything it throws
        /// into an <see cref="Error" />.
        /// </summary>
        /// <remarks>
        /// The shorter of the two async forms: no <c>onError</c>, and the
        /// failure is described by the library's own <see cref="Error" />.
        /// <para>
        /// An <paramref name="asyncFactory" /> that returns
        /// <see langword="null" /> is treated as a failure too, carrying an
        /// <see cref="Error" /> converted from an
        /// <see cref="ArgumentNullException" /> that was never thrown, so it
        /// carries no stack trace and is not logged. An
        /// <see cref="OperationCanceledException" /> is not caught at all and
        /// propagates to the caller unless
        /// <see cref="Configs.MonadOptionsBuilder.UseCancellationAsFailure" />
        /// is configured.
        /// </para>
        /// </remarks>
        /// <param name="asyncFactory">
        /// Produces the ok value from the bound state. Its exceptions, and its
        /// task's, are what this method exists to catch.
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
        /// <typeparam name="TOk">
        /// The type <paramref name="asyncFactory" /> produces.
        /// </typeparam>
        /// <returns>
        /// <see cref="Ok{TOk,TErr}" /> of what
        /// <paramref name="asyncFactory" /> produced, or
        /// <see cref="Err{TOk,TErr}" /> of an <see cref="Error" /> describing
        /// the exception.
        /// </returns>
        public ValueTask<Result<TOk, Error>> TryAsync<TOk>(
            Func<TState, Task<TOk>> asyncFactory,
            [CallerMemberName] string callerMemberName = "",
            [CallerLineNumber] int callerLineNumber = 0,
            [CallerArgumentExpression(nameof(asyncFactory))]
            string callerArgumentExpression = "")
            where TOk : notnull =>
            Result.TryAsync(
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
    /// The factory-side spelling of <c>With</c> on a
    /// <see cref="Result{TOk,TErr}" />: there is no result to call it on yet, so
    /// it is called on <see cref="Result" /> instead and the state is bound
    /// before the result exists.
    /// <code>
    /// Result.With(path)
    ///       .Try(static p =&gt; File.ReadAllText(p));
    /// </code>
    /// <para>
    /// Pass a tuple to bind more than one value, and mark the factory
    /// <see langword="static" /> so the compiler rejects a factory that
    /// captures a variable.
    /// </para>
    /// </remarks>
    /// <param name="state">
    /// The value handed to the factory of whichever <c>Try</c> is called next.
    /// It is unconstrained, so a null state is permitted.
    /// </param>
    /// <typeparam name="TState">The type of the bound value.</typeparam>
    /// <returns>
    /// <paramref name="state" /> carrying the four factories that consume it.
    /// </returns>
    public static Bound<TState> With<TState>(TState state) => new(state);
}
