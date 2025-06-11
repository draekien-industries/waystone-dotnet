namespace Waystone.Monads.Results;

using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Configs;

/// <summary>Static methods for <see cref="Result{TOk,TErr}" /></summary>
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
    /// An <see cref="Ok{TOk,TErr}" /> if the factory executes successfully,
    /// otherwise a <see cref="Err{TOk,TErr}" />
    /// </returns>
    public static Result<TOk, TErr> Try<TOk, TErr>(
        Func<TOk> factory,
        Func<Exception, TErr> onError,
        [CallerMemberName] string callerMemberName = "",
        [CallerLineNumber] int callerLineNumber = 0,
        [CallerArgumentExpression(nameof(factory))]
        string callerArgumentExpression = "")
        where TOk : notnull where TErr : notnull
    {
        try
        {
            return Ok<TOk, TErr>(factory());
        }
        catch (Exception ex)
        {
            var caller = new CallerInfo(
                callerMemberName,
                callerArgumentExpression,
                callerLineNumber);
            MonadsGlobalConfig.Log(ex, caller);
            return Err<TOk, TErr>(onError(ex));
        }
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
    /// An <see cref="Ok{TOk,TErr}" /> if the factory executes successfully,
    /// otherwise a <see cref="Err{TOk,TErr}" />
    /// </returns>
    public static async Task<Result<TOk, TErr>> Try<TOk, TErr>(
        Func<Task<TOk>> asyncFactory,
        Func<Exception, TErr> onError,
        [CallerMemberName] string callerMemberName = "",
        [CallerLineNumber] int callerLineNumber = 0,
        [CallerArgumentExpression(nameof(asyncFactory))]
        string callerArgumentExpression = "")
        where TOk : notnull where TErr : notnull
    {
        try
        {
            return Ok<TOk, TErr>(await asyncFactory());
        }
        catch (Exception ex)
        {
            var caller = new CallerInfo(
                callerMemberName,
                callerArgumentExpression,
                callerLineNumber);
            MonadsGlobalConfig.Log(ex, caller);
            return Err<TOk, TErr>(onError(ex));
        }
    }


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
}
