namespace Waystone.Monads.Results.Extensions;

using System;
using System.Threading.Tasks;

/// <summary>Async extension methods for <see cref="Result{TOk,TErr}" />.</summary>
public static partial class ResultOfTOkTErrAsyncExtensions
{
    /// <summary>
    /// Processes a result asynchronously by executing the appropriate
    /// function based on the result's state.
    /// </summary>
    /// <param name="result">The result to be processed.</param>
    /// <param name="onOk">
    /// A function to execute if the result is in an "ok" state. The
    /// function will be passed the successful value.
    /// </param>
    /// <param name="onErr">
    /// A function to execute if the result is in an "error" state.
    /// The function will be passed the error value.
    /// </param>
    /// <typeparam name="TOk">The type of the successful value contained in the result.</typeparam>
    /// <typeparam name="TErr">The type of the error value contained in the result.</typeparam>
    /// <typeparam name="TOut">
    /// The type of the return value after executing the
    /// functions.
    /// </typeparam>
    /// <returns>
    /// A task that represents the asynchronous operation, containing the
    /// value returned by the executed function.
    /// </returns>
    public static Task<TOut> MatchAsync<TOk, TErr, TOut>(
        this Result<TOk, TErr> result,
        Func<TOk, Task<TOut>> onOk,
        Func<TErr, Task<TOut>> onErr)
        where TOk : notnull where TErr : notnull =>
        result.Match(onOk, onErr);

    /// <summary>
    /// Asynchronously processes a result by executing the appropriate
    /// function based on the result's state.
    /// </summary>
    /// <param name="result">The result to be processed.</param>
    /// <param name="onOk">
    /// A function to execute if the result is in an "ok" state. The
    /// function will be passed the successful value.
    /// </param>
    /// <param name="onErr">
    /// A function to execute if the result is in an "error" state.
    /// The function will be passed the error value.
    /// </param>
    /// <typeparam name="TOk">The type of the successful value contained in the result.</typeparam>
    /// <typeparam name="TErr">The type of the error value contained in the result.</typeparam>
    /// <typeparam name="TOut">The type of the return value from the executed function.</typeparam>
    /// <returns>
    /// A task that represents the asynchronous operation, containing the
    /// value returned by the executed function.
    /// </returns>
    public static ValueTask<TOut> MatchAsync<TOk, TErr, TOut>(
        this Result<TOk, TErr> result,
        Func<TOk, ValueTask<TOut>> onOk,
        Func<TErr, ValueTask<TOut>> onErr)
        where TOk : notnull where TErr : notnull =>
        result.Match(onOk, onErr);

    /// <summary>
    /// Processes a result asynchronously by executing the appropriate
    /// function based on the result's state.
    /// </summary>
    /// <param name="resultTask">
    /// A task that represents the asynchronous result to be
    /// processed.
    /// </param>
    /// <param name="onOk">
    /// A function to execute if the result is in an "ok" state. The
    /// function will be passed the successful value.
    /// </param>
    /// <param name="onErr">
    /// A function to execute if the result is in an "error" state.
    /// The function will be passed the error value.
    /// </param>
    /// <typeparam name="TOk">The type of the successful value contained in the result.</typeparam>
    /// <typeparam name="TErr">The type of the error value contained in the result.</typeparam>
    /// <typeparam name="TOut">
    /// The type of the return value after executing the
    /// functions.
    /// </typeparam>
    /// <returns>
    /// A task that represents the asynchronous operation, containing the
    /// value returned by the executed function.
    /// </returns>
    public static async Task<TOut> MatchAsync<TOk, TErr, TOut>(
        this Task<Result<TOk, TErr>> resultTask,
        Func<TOk, Task<TOut>> onOk,
        Func<TErr, Task<TOut>> onErr) where TOk : notnull where TErr : notnull
    {
        Result<TOk, TErr> result = await resultTask.ConfigureAwait(false);
        return await result.MatchAsync(onOk, onErr).ConfigureAwait(false);
    }

    /// <summary>
    /// Processes an asynchronous result by executing the appropriate function
    /// based on the result's state.
    /// </summary>
    /// <param name="resultTask">
    /// The task representing the asynchronous result to be
    /// processed.
    /// </param>
    /// <param name="onOk">
    /// A function to execute if the result is in an "ok" state. The
    /// function will be passed the successful value.
    /// </param>
    /// <param name="onErr">
    /// A function to execute if the result is in an "error" state.
    /// The function will be passed the error value.
    /// </param>
    /// <typeparam name="TOk">The type of the successful value contained in the result.</typeparam>
    /// <typeparam name="TErr">The type of the error value contained in the result.</typeparam>
    /// <typeparam name="TOut">
    /// The type of the return value after executing the
    /// functions.
    /// </typeparam>
    /// <returns>
    /// A task representing the asynchronous operation that returns the value
    /// produced by the executed function.
    /// </returns>
    public static async ValueTask<TOut> MatchAsync<TOk, TErr, TOut>(
        this ValueTask<Result<TOk, TErr>> resultTask,
        Func<TOk, ValueTask<TOut>> onOk,
        Func<TErr, ValueTask<TOut>> onErr)
        where TOk : notnull where TErr : notnull
    {
        Result<TOk, TErr> result = await resultTask.ConfigureAwait(false);
        return await result.MatchAsync(onOk, onErr).ConfigureAwait(false);
    }
}
