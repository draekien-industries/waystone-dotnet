namespace Waystone.Monads.Results;

using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Configs;
using Errors;
#if !DEBUG
using System.Diagnostics;
#endif

/// <summary>Static methods for <see cref="Result{TOk,TErr}" /></summary>
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
    /// <param name="callerMemberName">The method name of the caller.</param>
    /// <param name="callerLineNumber">The line number of the caller.</param>
    /// <param name="callerArgumentExpression">
    /// The argument expression used as the
    /// factory.
    /// </param>
    /// <typeparam name="TOk">The factory method return value's type</typeparam>
    /// <typeparam name="TErr">The error handler return value's type</typeparam>
    /// <returns>
    /// An <see cref="Ok{TOk,TErr}" /> if the factory produces a non-null
    /// value, otherwise an <see cref="Err{TOk,TErr}" />.
    /// </returns>
    /// <remarks>
    /// An <see cref="Err{TOk,TErr}" /> is returned both when the factory throws
    /// and when it returns null, and <paramref name="onError" /> is invoked in
    /// either case. A factory that returns null is handed an
    /// <see cref="ArgumentNullException" /> that was never thrown, so it carries
    /// no stack trace, and only the thrown case is reported to the exception
    /// logger configured on <see cref="MonadOptions" />.
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
        catch (Exception ex)
        {
            var caller = new CallerInfo(
                callerMemberName,
                callerArgumentExpression,
                callerLineNumber);
            MonadOptions.Current.Log(ex, caller);
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
    /// <param name="callerMemberName">The method name of the caller.</param>
    /// <param name="callerLineNumber">The line number of the caller.</param>
    /// <param name="callerArgumentExpression">
    /// The argument expression used as the
    /// factory.
    /// </param>
    /// <typeparam name="TOk">The factory method return value's type</typeparam>
    /// <typeparam name="TErr">The error handler return value's type</typeparam>
    /// <returns>
    /// An <see cref="Ok{TOk,TErr}" /> if the factory produces a non-null
    /// value, otherwise an <see cref="Err{TOk,TErr}" />.
    /// </returns>
    [Obsolete(
        "Use TryAsync instead. This overload will be removed in v6 of Waystone.Monads.")]
    public static Task<Result<TOk, TErr>> Try<TOk, TErr>(
        Func<Task<TOk>> asyncFactory,
        Func<Exception, TErr> onError,
        [CallerMemberName] string callerMemberName = "",
        [CallerLineNumber] int callerLineNumber = 0,
        [CallerArgumentExpression(nameof(asyncFactory))]
        string callerArgumentExpression = "")
        where TOk : notnull where TErr : notnull =>
        TryAsync(
            asyncFactory,
            onError,
            callerMemberName,
            callerLineNumber,
            callerArgumentExpression);

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
    /// <param name="callerMemberName">The method name of the caller.</param>
    /// <param name="callerLineNumber">The line number of the caller.</param>
    /// <param name="callerArgumentExpression">
    /// The argument expression used as the
    /// factory.
    /// </param>
    /// <typeparam name="TOk">The factory method return value's type</typeparam>
    /// <typeparam name="TErr">The error handler return value's type</typeparam>
    /// <returns>
    /// An <see cref="Ok{TOk,TErr}" /> if the factory produces a non-null
    /// value, otherwise an <see cref="Err{TOk,TErr}" />.
    /// </returns>
    /// <remarks>
    /// An <see cref="Err{TOk,TErr}" /> is returned both when the factory throws
    /// and when it returns null, and <paramref name="onError" /> is invoked in
    /// either case. A factory that returns null is handed an
    /// <see cref="ArgumentNullException" /> that was never thrown, so it carries
    /// no stack trace, and only the thrown case is reported to the exception
    /// logger configured on <see cref="MonadOptions" />.
    /// </remarks>
    public static async Task<Result<TOk, TErr>> TryAsync<TOk, TErr>(
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
        catch (Exception ex)
        {
            var caller = new CallerInfo(
                callerMemberName,
                callerArgumentExpression,
                callerLineNumber);
            MonadOptions.Current.Log(ex, caller);
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
    /// <param name="value">The value of the result type.</param>
    public static Result<TOk, TErr> Ok<TOk, TErr>(TOk value)
        where TOk : notnull
        where TErr : notnull =>
        new Ok<TOk, TErr>(value);

    /// <summary>
    /// Creates an <see cref="Err{TOk,TErr}" /> result containing the provided
    /// value.
    /// </summary>
    /// <param name="value">The value of the result type.</param>
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
    /// <param name="callerMemberName">The method name of the caller.</param>
    /// <param name="callerLineNumber">The line number of the caller.</param>
    /// <param name="callerArgumentExpression">
    /// The argument expression used as the
    /// factory.
    /// </param>
    /// <typeparam name="TOk">The factory method return value's type</typeparam>
    /// <returns>
    /// An <see cref="Ok{TOk,TErr}" /> if the factory produces a non-null
    /// value, otherwise an <see cref="Err{TOk,TErr}" />.
    /// </returns>
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
    /// <param name="callerMemberName">The method name of the caller.</param>
    /// <param name="callerLineNumber">The line number of the caller.</param>
    /// <param name="callerArgumentExpression">
    /// The argument expression used as the
    /// factory.
    /// </param>
    /// <typeparam name="TOk">The factory method return value's type</typeparam>
    /// <returns>
    /// An <see cref="Ok{TOk,TErr}" /> if the factory produces a non-null
    /// value, otherwise an <see cref="Err{TOk,TErr}" />.
    /// </returns>
    public static Task<Result<TOk, Error>> TryAsync<TOk>(
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
    /// Creates an <see cref="Ok{TOk,TErr}" /> result containing the provided
    /// value, using <see cref="Error" /> as the error type.
    /// </summary>
    /// <param name="value">The value of the result type.</param>
    /// <typeparam name="TOk">The ok result value's type</typeparam>
    public static Result<TOk, Error> Ok<TOk>(TOk value)
        where TOk : notnull =>
        new Ok<TOk, Error>(value);

    /// <summary>
    /// Creates an <see cref="Err{TOk,TErr}" /> result containing the provided
    /// <see cref="Error" />.
    /// </summary>
    /// <param name="error">The error contained in the result.</param>
    /// <typeparam name="TOk">The ok result value's type</typeparam>
    public static Result<TOk, Error> Err<TOk>(Error error)
        where TOk : notnull =>
        new Err<TOk, Error>(error);

    /// <summary>
    /// Creates an <see cref="Err{TOk,TErr}" /> result containing an
    /// <see cref="Error" /> whose code is derived from the provided enum value.
    /// </summary>
    /// <remarks>
    /// Uses the <see cref="ErrorCodeFactory" /> configured in
    /// <see cref="MonadOptions" /> to create the error code.
    /// </remarks>
    /// <param name="code">The enum value to create the error code from.</param>
    /// <param name="message">
    /// A descriptive error message providing more context
    /// about the error.
    /// </param>
    /// <typeparam name="TOk">The ok result value's type</typeparam>
    public static Result<TOk, Error> Err<TOk>(Enum code, string message)
        where TOk : notnull =>
        new Err<TOk, Error>(Error.FromEnum(code, message));
}
