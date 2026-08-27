namespace Waystone.Monads.Results;

using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Configs;
using Diagnostics;
using Errors;
#if !DEBUG
using System.Diagnostics;
#endif

/// <summary>Creates <see cref="Result{TOk,TErr}" /> values</summary>
/// <remarks>
/// The <see cref="Ok{TOk,TErr}" /> and <see cref="Err{TOk,TErr}" />
/// constructors are both internal, so this class is the only way to build a
/// <see cref="Result{TOk,TErr}" /> from outside the library.
/// </remarks>
#if !DEBUG
[DebuggerStepThrough]
#endif
public static class Result
{
    /// <summary>
    /// Tries to store the result of a <paramref name="factory" /> into a
    /// <see cref="Result{TOk,TErr}" />, invoking <paramref name="onError" /> if the
    /// factory throws an exception.
    /// </summary>
    /// <param name="factory">
    /// A method which when executed will return the value
    /// contained in the <see cref="Result{TOk,TErr}" />
    /// </param>
    /// <param name="onError">
    /// A callback method that will be invoked for any exceptions
    /// thrown by the <paramref name="factory" />
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
    /// <typeparam name="TOk">The factory method return value's type</typeparam>
    /// <typeparam name="TErr">The error handler return value's type</typeparam>
    /// <returns>
    /// An <see cref="Ok{TOk,TErr}" /> if the factory produces a non-null
    /// value, otherwise an <see cref="Err{TOk,TErr}" />.
    /// </returns>
    /// <remarks>
    /// <para>
    /// An <see cref="Err{TOk,TErr}" /> is returned both when the factory throws
    /// and when it returns null, and <paramref name="onError" /> is invoked in
    /// either case. A factory that returns null is handed an
    /// <see cref="ArgumentNullException" /> that was never thrown, so it carries
    /// no stack trace. Only the thrown case reaches the exception logger
    /// configured on <see cref="MonadOptions" />, which also writes to the
    /// console while a debugger is attached, whether or not a logger is
    /// configured.
    /// </para>
    /// <para>
    /// An <see cref="OperationCanceledException" /> is not caught. It leaves
    /// this method untouched, so it is neither logged nor passed to
    /// <paramref name="onError" />, and the caller observes the cancellation it
    /// asked for. Call
    /// <see cref="MonadOptions.UseCancellationAsFailure" /> to catch it like
    /// any other exception.
    /// </para>
    /// </remarks>
    public static Result<TOk, TErr> Try<TOk, TErr>(
        Func<TOk> factory,
        Func<Exception, TErr> onError,
        [CallerMemberName] string callerMemberName = "",
        [CallerLineNumber] int callerLineNumber = 0,
        [CallerArgumentExpression(nameof(factory))]
        string callerArgumentExpression = "")
        where TOk : notnull where TErr : notnull
    {
        TOk value;

        try
        {
            value = factory();
        }
        catch (Exception ex) when (MonadOptions.Current.Catches(ex))
        {
            var caller = new CallerInfo(
                callerMemberName,
                callerArgumentExpression,
                callerLineNumber);
            MonadOptions.Current.Log(ex, caller, MonadKind.Result);
            return Err<TOk, TErr>(onError(ex));
        }

        return value is null
            ? Err<TOk, TErr>(onError(FactoryReturnedNull(nameof(factory))))
            : Ok<TOk, TErr>(value);
    }

    /// <summary>
    /// Tries to store the result of an <paramref name="asyncFactory" /> into
    /// a <see cref="Result{TOk, TErr}" />, invoking <paramref name="onError" /> if the
    /// factory throws an exception.
    /// </summary>
    /// <param name="asyncFactory">
    /// An asynchronous method which when executed will
    /// produce the value of the <see cref="Result{TOk,TErr}" />
    /// </param>
    /// <param name="onError">
    /// A callback method that will be invoked for any exceptions
    /// thrown by the <paramref name="asyncFactory" />
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
    /// <typeparam name="TOk">The factory method return value's type</typeparam>
    /// <typeparam name="TErr">The error handler return value's type</typeparam>
    /// <returns>
    /// An <see cref="Ok{TOk,TErr}" /> if the factory produces a non-null
    /// value, otherwise an <see cref="Err{TOk,TErr}" />.
    /// </returns>
    /// <remarks>
    /// <para>
    /// An <see cref="Err{TOk,TErr}" /> is returned both when the factory throws
    /// and when it returns null, and <paramref name="onError" /> is invoked in
    /// either case. A factory that returns null is handed an
    /// <see cref="ArgumentNullException" /> that was never thrown, so it carries
    /// no stack trace. Only the thrown case reaches the exception logger
    /// configured on <see cref="MonadOptions" />, which also writes to the
    /// console while a debugger is attached, whether or not a logger is
    /// configured.
    /// </para>
    /// <para>
    /// An <see cref="OperationCanceledException" /> is not caught. It leaves
    /// this method untouched, so it is neither logged nor passed to
    /// <paramref name="onError" />, and the caller observes the cancellation it
    /// asked for. Call
    /// <see cref="MonadOptions.UseCancellationAsFailure" /> to catch it like
    /// any other exception.
    /// </para>
    /// </remarks>
    public static async ValueTask<Result<TOk, TErr>> TryAsync<TOk, TErr>(
        Func<Task<TOk>> asyncFactory,
        Func<Exception, TErr> onError,
        [CallerMemberName] string callerMemberName = "",
        [CallerLineNumber] int callerLineNumber = 0,
        [CallerArgumentExpression(nameof(asyncFactory))]
        string callerArgumentExpression = "")
        where TOk : notnull where TErr : notnull
    {
        TOk value;

        try
        {
            value = await asyncFactory();
        }
        catch (Exception ex) when (MonadOptions.Current.Catches(ex))
        {
            var caller = new CallerInfo(
                callerMemberName,
                callerArgumentExpression,
                callerLineNumber);
            MonadOptions.Current.Log(ex, caller, MonadKind.Result);
            return Err<TOk, TErr>(onError(ex));
        }

        return value is null
            ? Err<TOk, TErr>(
                onError(FactoryReturnedNull(nameof(asyncFactory))))
            : Ok<TOk, TErr>(value);
    }

    /// <summary>
    /// Tries to store the result of a <paramref name="factory" /> into a
    /// <see cref="Result{TOk,TErr}" />, handing it the provided
    /// <paramref name="state" /> and invoking <paramref name="onError" /> if the
    /// factory throws an exception.
    /// </summary>
    /// <param name="state">
    /// The value the factory would otherwise capture. It is passed through
    /// unchanged and is never inspected.
    /// </param>
    /// <param name="factory">
    /// A method which when executed will return the value
    /// contained in the <see cref="Result{TOk,TErr}" />
    /// </param>
    /// <param name="onError">
    /// A callback method that will be invoked for any exceptions
    /// thrown by the <paramref name="factory" />
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
    /// <typeparam name="TState">
    /// The state's type. It is unconstrained, so a null state is permitted.
    /// </typeparam>
    /// <typeparam name="TOk">The factory method return value's type</typeparam>
    /// <typeparam name="TErr">The error handler return value's type</typeparam>
    /// <returns>
    /// An <see cref="Ok{TOk,TErr}" /> if the factory produces a non-null
    /// value, otherwise an <see cref="Err{TOk,TErr}" />.
    /// </returns>
    /// <remarks>
    /// <para>
    /// The <paramref name="state" /> is handed to the factory rather than
    /// captured by it, so the factory can be <c>static</c> and the call
    /// allocates no closure. A
    /// <see cref="System.Threading.CancellationToken" /> is the state this
    /// exists for. The <paramref name="onError" /> callback is not handed the
    /// state, so a handler that needs it still captures.
    /// </para>
    /// <para>
    /// An <see cref="Err{TOk,TErr}" /> is returned both when the factory throws
    /// and when it returns null, and <paramref name="onError" /> is invoked in
    /// either case. A factory that returns null is handed an
    /// <see cref="ArgumentNullException" /> that was never thrown, so it carries
    /// no stack trace. Only the thrown case reaches the exception logger
    /// configured on <see cref="MonadOptions" />, which also writes to the
    /// console while a debugger is attached, whether or not a logger is
    /// configured.
    /// </para>
    /// <para>
    /// An <see cref="OperationCanceledException" /> is not caught. It leaves
    /// this method untouched, so it is neither logged nor passed to
    /// <paramref name="onError" />, and the caller observes the cancellation it
    /// asked for. Call
    /// <see cref="MonadOptions.UseCancellationAsFailure" /> to catch it like
    /// any other exception.
    /// </para>
    /// </remarks>
    public static Result<TOk, TErr> Try<TState, TOk, TErr>(
        TState state,
        Func<TState, TOk> factory,
        Func<Exception, TErr> onError,
        [CallerMemberName] string callerMemberName = "",
        [CallerLineNumber] int callerLineNumber = 0,
        [CallerArgumentExpression(nameof(factory))]
        string callerArgumentExpression = "")
        where TOk : notnull where TErr : notnull
    {
        TOk value;

        try
        {
            value = factory(state);
        }
        catch (Exception ex) when (MonadOptions.Current.Catches(ex))
        {
            var caller = new CallerInfo(
                callerMemberName,
                callerArgumentExpression,
                callerLineNumber);
            MonadOptions.Current.Log(ex, caller, MonadKind.Result);
            return Err<TOk, TErr>(onError(ex));
        }

        return value is null
            ? Err<TOk, TErr>(onError(FactoryReturnedNull(nameof(factory))))
            : Ok<TOk, TErr>(value);
    }

    /// <summary>
    /// Tries to store the result of an <paramref name="asyncFactory" /> into
    /// a <see cref="Result{TOk, TErr}" />, handing it the provided
    /// <paramref name="state" /> and invoking <paramref name="onError" /> if the
    /// factory throws an exception.
    /// </summary>
    /// <param name="state">
    /// The value the factory would otherwise capture. It is passed through
    /// unchanged and is never inspected.
    /// </param>
    /// <param name="asyncFactory">
    /// An asynchronous method which when executed will
    /// produce the value of the <see cref="Result{TOk,TErr}" />
    /// </param>
    /// <param name="onError">
    /// A callback method that will be invoked for any exceptions
    /// thrown by the <paramref name="asyncFactory" />
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
    /// <typeparam name="TState">
    /// The state's type. It is unconstrained, so a null state is permitted.
    /// </typeparam>
    /// <typeparam name="TOk">The factory method return value's type</typeparam>
    /// <typeparam name="TErr">The error handler return value's type</typeparam>
    /// <returns>
    /// An <see cref="Ok{TOk,TErr}" /> if the factory produces a non-null
    /// value, otherwise an <see cref="Err{TOk,TErr}" />.
    /// </returns>
    /// <remarks>
    /// <para>
    /// The <paramref name="state" /> is handed to the factory rather than
    /// captured by it, so the factory can be <c>static</c> and the call
    /// allocates no closure. A
    /// <see cref="System.Threading.CancellationToken" /> is the state this
    /// exists for. The <paramref name="onError" /> callback is not handed the
    /// state, so a handler that needs it still captures.
    /// </para>
    /// <para>
    /// An <see cref="Err{TOk,TErr}" /> is returned both when the factory throws
    /// and when it returns null, and <paramref name="onError" /> is invoked in
    /// either case. A factory that returns null is handed an
    /// <see cref="ArgumentNullException" /> that was never thrown, so it carries
    /// no stack trace. Only the thrown case reaches the exception logger
    /// configured on <see cref="MonadOptions" />, which also writes to the
    /// console while a debugger is attached, whether or not a logger is
    /// configured.
    /// </para>
    /// <para>
    /// An <see cref="OperationCanceledException" /> is not caught. It leaves
    /// this method untouched, so it is neither logged nor passed to
    /// <paramref name="onError" />, and the caller observes the cancellation it
    /// asked for. Call
    /// <see cref="MonadOptions.UseCancellationAsFailure" /> to catch it like
    /// any other exception.
    /// </para>
    /// </remarks>
    public static async ValueTask<Result<TOk, TErr>> TryAsync<TState, TOk, TErr>(
        TState state,
        Func<TState, Task<TOk>> asyncFactory,
        Func<Exception, TErr> onError,
        [CallerMemberName] string callerMemberName = "",
        [CallerLineNumber] int callerLineNumber = 0,
        [CallerArgumentExpression(nameof(asyncFactory))]
        string callerArgumentExpression = "")
        where TOk : notnull where TErr : notnull
    {
        TOk value;

        try
        {
            value = await asyncFactory(state);
        }
        catch (Exception ex) when (MonadOptions.Current.Catches(ex))
        {
            var caller = new CallerInfo(
                callerMemberName,
                callerArgumentExpression,
                callerLineNumber);
            MonadOptions.Current.Log(ex, caller, MonadKind.Result);
            return Err<TOk, TErr>(onError(ex));
        }

        return value is null
            ? Err<TOk, TErr>(
                onError(FactoryReturnedNull(nameof(asyncFactory))))
            : Ok<TOk, TErr>(value);
    }

    private static ArgumentNullException FactoryReturnedNull(
        string parameterName) =>
        new(parameterName, "The factory returned null.");

    /// <summary>
    /// Creates an <see cref="Ok{TOk,TErr}" /> result containing the provided
    /// value.
    /// </summary>
    /// <param name="value">The success value the result will hold.</param>
    /// <typeparam name="TOk">The ok result value's type</typeparam>
    /// <typeparam name="TErr">The error result value's type</typeparam>
    /// <returns>
    /// A <see cref="Result{TOk,TErr}" /> that is always an
    /// <see cref="Ok{TOk,TErr}" />. The static type is
    /// <see cref="Result{TOk,TErr}" />, so match on it to reach the value.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value" /> is null. An <see cref="Ok{TOk,TErr}" /> cannot
    /// hold null, matching the <c>notnull</c> constraint on
    /// <typeparamref name="TOk" />.
    /// </exception>
    public static Result<TOk, TErr> Ok<TOk, TErr>(TOk value)
        where TOk : notnull
        where TErr : notnull =>
        new Ok<TOk, TErr>(value);

    /// <summary>
    /// Creates an <see cref="Err{TOk,TErr}" /> result containing the provided
    /// value.
    /// </summary>
    /// <param name="value">The error value the result will hold.</param>
    /// <typeparam name="TOk">The ok result value's type</typeparam>
    /// <typeparam name="TErr">The error result value's type</typeparam>
    /// <returns>
    /// A <see cref="Result{TOk,TErr}" /> that is always an
    /// <see cref="Err{TOk,TErr}" />. The static type is
    /// <see cref="Result{TOk,TErr}" />, so match on it to reach the error.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value" /> is null. An <see cref="Err{TOk,TErr}" /> cannot
    /// hold null, matching the <c>notnull</c> constraint on
    /// <typeparamref name="TErr" />.
    /// </exception>
    public static Result<TOk, TErr> Err<TOk, TErr>(TErr value)
        where TOk : notnull
        where TErr : notnull =>
        new Err<TOk, TErr>(value);

    /// <summary>
    /// Tries to store the result of a <paramref name="factory" /> into a
    /// <see cref="Result{TOk,TErr}" /> which uses <see cref="Error" /> as its error
    /// type, converting any thrown exception using
    /// <see cref="Error.FromException" />.
    /// </summary>
    /// <param name="factory">
    /// A method which when executed will return the value
    /// contained in the <see cref="Result{TOk,TErr}" />
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
    /// <typeparam name="TOk">The factory method return value's type</typeparam>
    /// <returns>
    /// An <see cref="Ok{TOk,TErr}" /> if the factory produces a non-null
    /// value, otherwise an <see cref="Err{TOk,TErr}" />.
    /// </returns>
    /// <remarks>
    /// <para>
    /// A factory that returns null also produces an
    /// <see cref="Err{TOk,TErr}" />, carrying an <see cref="Error" /> converted
    /// from an <see cref="ArgumentNullException" /> that was never thrown and so
    /// has no stack trace. Only the thrown case reaches the exception logger
    /// configured on <see cref="MonadOptions" />, which also writes to the
    /// console while a debugger is attached, whether or not a logger is
    /// configured.
    /// </para>
    /// <para>
    /// An <see cref="OperationCanceledException" /> is not caught. Call
    /// <see cref="MonadOptions.UseCancellationAsFailure" /> to catch it like
    /// any other exception.
    /// </para>
    /// </remarks>
    public static Result<TOk, Error> Try<TOk>(
        Func<TOk> factory,
        [CallerMemberName] string callerMemberName = "",
        [CallerLineNumber] int callerLineNumber = 0,
        [CallerArgumentExpression(nameof(factory))]
        string callerArgumentExpression = "")
        where TOk : notnull =>
        Try(
            factory,
            Error.FromException,
            callerMemberName,
            callerLineNumber,
            callerArgumentExpression);

    /// <summary>
    /// Tries to store the result of an <paramref name="asyncFactory" /> into
    /// a <see cref="Result{TOk,TErr}" /> which uses <see cref="Error" /> as its error
    /// type, converting any thrown exception using
    /// <see cref="Error.FromException" />.
    /// </summary>
    /// <param name="asyncFactory">
    /// An asynchronous method which when executed will
    /// produce the value of the <see cref="Result{TOk,TErr}" />
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
    /// <typeparam name="TOk">The factory method return value's type</typeparam>
    /// <returns>
    /// An <see cref="Ok{TOk,TErr}" /> if the factory produces a non-null
    /// value, otherwise an <see cref="Err{TOk,TErr}" />.
    /// </returns>
    /// <remarks>
    /// <para>
    /// A factory that returns null also produces an
    /// <see cref="Err{TOk,TErr}" />, carrying an <see cref="Error" /> converted
    /// from an <see cref="ArgumentNullException" /> that was never thrown and so
    /// has no stack trace. Only the thrown case reaches the exception logger
    /// configured on <see cref="MonadOptions" />, which also writes to the
    /// console while a debugger is attached, whether or not a logger is
    /// configured.
    /// </para>
    /// <para>
    /// An <see cref="OperationCanceledException" /> is not caught. Call
    /// <see cref="MonadOptions.UseCancellationAsFailure" /> to catch it like
    /// any other exception.
    /// </para>
    /// </remarks>
    public static ValueTask<Result<TOk, Error>> TryAsync<TOk>(
        Func<Task<TOk>> asyncFactory,
        [CallerMemberName] string callerMemberName = "",
        [CallerLineNumber] int callerLineNumber = 0,
        [CallerArgumentExpression(nameof(asyncFactory))]
        string callerArgumentExpression = "")
        where TOk : notnull =>
        TryAsync(
            asyncFactory,
            Error.FromException,
            callerMemberName,
            callerLineNumber,
            callerArgumentExpression);

    /// <summary>
    /// Tries to store the result of a <paramref name="factory" /> into a
    /// <see cref="Result{TOk,TErr}" /> which uses <see cref="Error" /> as its error
    /// type, handing the factory the provided <paramref name="state" /> and
    /// converting any thrown exception using <see cref="Error.FromException" />.
    /// </summary>
    /// <param name="state">
    /// The value the factory would otherwise capture. It is passed through
    /// unchanged and is never inspected.
    /// </param>
    /// <param name="factory">
    /// A method which when executed will return the value
    /// contained in the <see cref="Result{TOk,TErr}" />
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
    /// <typeparam name="TState">
    /// The state's type. It is unconstrained, so a null state is permitted.
    /// </typeparam>
    /// <typeparam name="TOk">The factory method return value's type</typeparam>
    /// <returns>
    /// An <see cref="Ok{TOk,TErr}" /> if the factory produces a non-null
    /// value, otherwise an <see cref="Err{TOk,TErr}" />.
    /// </returns>
    /// <remarks>
    /// <para>
    /// The <paramref name="state" /> is handed to the factory rather than
    /// captured by it, so the factory can be <c>static</c> and the call
    /// allocates no closure. A
    /// <see cref="System.Threading.CancellationToken" /> is the state this
    /// exists for.
    /// </para>
    /// <para>
    /// A factory that returns null also produces an
    /// <see cref="Err{TOk,TErr}" />, carrying an <see cref="Error" /> converted
    /// from an <see cref="ArgumentNullException" /> that was never thrown and so
    /// has no stack trace. Only the thrown case reaches the exception logger
    /// configured on <see cref="MonadOptions" />, which also writes to the
    /// console while a debugger is attached, whether or not a logger is
    /// configured.
    /// </para>
    /// <para>
    /// An <see cref="OperationCanceledException" /> is not caught. Call
    /// <see cref="MonadOptions.UseCancellationAsFailure" /> to catch it like
    /// any other exception.
    /// </para>
    /// </remarks>
    public static Result<TOk, Error> Try<TState, TOk>(
        TState state,
        Func<TState, TOk> factory,
        [CallerMemberName] string callerMemberName = "",
        [CallerLineNumber] int callerLineNumber = 0,
        [CallerArgumentExpression(nameof(factory))]
        string callerArgumentExpression = "")
        where TOk : notnull =>
        Try(
            state,
            factory,
            Error.FromException,
            callerMemberName,
            callerLineNumber,
            callerArgumentExpression);

    /// <summary>
    /// Tries to store the result of an <paramref name="asyncFactory" /> into
    /// a <see cref="Result{TOk,TErr}" /> which uses <see cref="Error" /> as its error
    /// type, handing the factory the provided <paramref name="state" /> and
    /// converting any thrown exception using <see cref="Error.FromException" />.
    /// </summary>
    /// <param name="state">
    /// The value the factory would otherwise capture. It is passed through
    /// unchanged and is never inspected.
    /// </param>
    /// <param name="asyncFactory">
    /// An asynchronous method which when executed will
    /// produce the value of the <see cref="Result{TOk,TErr}" />
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
    /// <typeparam name="TState">
    /// The state's type. It is unconstrained, so a null state is permitted.
    /// </typeparam>
    /// <typeparam name="TOk">The factory method return value's type</typeparam>
    /// <returns>
    /// An <see cref="Ok{TOk,TErr}" /> if the factory produces a non-null
    /// value, otherwise an <see cref="Err{TOk,TErr}" />.
    /// </returns>
    /// <remarks>
    /// <para>
    /// The <paramref name="state" /> is handed to the factory rather than
    /// captured by it, so the factory can be <c>static</c> and the call
    /// allocates no closure. A
    /// <see cref="System.Threading.CancellationToken" /> is the state this
    /// exists for.
    /// </para>
    /// <para>
    /// A factory that returns null also produces an
    /// <see cref="Err{TOk,TErr}" />, carrying an <see cref="Error" /> converted
    /// from an <see cref="ArgumentNullException" /> that was never thrown and so
    /// has no stack trace. Only the thrown case reaches the exception logger
    /// configured on <see cref="MonadOptions" />, which also writes to the
    /// console while a debugger is attached, whether or not a logger is
    /// configured.
    /// </para>
    /// <para>
    /// An <see cref="OperationCanceledException" /> is not caught. Call
    /// <see cref="MonadOptions.UseCancellationAsFailure" /> to catch it like
    /// any other exception.
    /// </para>
    /// </remarks>
    public static ValueTask<Result<TOk, Error>> TryAsync<TState, TOk>(
        TState state,
        Func<TState, Task<TOk>> asyncFactory,
        [CallerMemberName] string callerMemberName = "",
        [CallerLineNumber] int callerLineNumber = 0,
        [CallerArgumentExpression(nameof(asyncFactory))]
        string callerArgumentExpression = "")
        where TOk : notnull =>
        TryAsync(
            state,
            asyncFactory,
            Error.FromException,
            callerMemberName,
            callerLineNumber,
            callerArgumentExpression);

    /// <summary>
    /// Creates an <see cref="Ok{TOk,TErr}" /> result containing the provided
    /// value, using <see cref="Error" /> as the error type.
    /// </summary>
    /// <param name="value">The success value the result will hold.</param>
    /// <typeparam name="TOk">The ok result value's type</typeparam>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value" /> is null.
    /// </exception>
    public static Result<TOk, Error> Ok<TOk>(TOk value)
        where TOk : notnull =>
        new Ok<TOk, Error>(value);

    /// <summary>
    /// Creates an <see cref="Err{TOk,TErr}" /> result containing the provided
    /// <see cref="Error" />.
    /// </summary>
    /// <param name="error">The error contained in the result.</param>
    /// <typeparam name="TOk">The ok result value's type</typeparam>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="error" /> is null.
    /// </exception>
    public static Result<TOk, Error> Err<TOk>(Error error)
        where TOk : notnull =>
        new Err<TOk, Error>(error);

    internal static Result<TOk, TErr> NotNull<TOk, TErr>(
        Result<TOk, TErr> result,
        string delegateName)
        where TOk : notnull
        where TErr : notnull =>
        result
     ?? throw new ArgumentNullException(
            delegateName,
            $"The `{delegateName}` delegate returned a null result. Return "
          + "`Result.Err<TOk, TErr>(error)` to express a failure; a null result "
          + "is never valid, and the next call against it would throw a "
          + "`NullReferenceException` far from here.");

    internal static ValueTask<Result<TOk, TErr>> NotNullAsync<TOk, TErr>(
        ValueTask<Result<TOk, TErr>> result,
        string delegateName)
        where TOk : notnull
        where TErr : notnull =>
        result.IsCompletedSuccessfully
            ? new ValueTask<Result<TOk, TErr>>(
                NotNull(result.Result, delegateName))
            : AwaitNotNull(result, delegateName);

    private static async ValueTask<Result<TOk, TErr>> AwaitNotNull<TOk, TErr>(
        ValueTask<Result<TOk, TErr>> result,
        string delegateName)
        where TOk : notnull
        where TErr : notnull =>
        NotNull(await result.ConfigureAwait(false), delegateName);
}
