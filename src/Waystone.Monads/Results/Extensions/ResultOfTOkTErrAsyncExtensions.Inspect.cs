namespace Waystone.Monads.Results.Extensions;

using System;
using System.Threading.Tasks;

public static partial class ResultOfTOkTErrAsyncExtensions
{
    /// <summary>
    /// Executes the provided asynchronous action if the result is successful
    /// and contains an "Ok" value.
    /// </summary>
    /// <param name="result">The result to inspect.</param>
    /// <param name="action">
    /// The asynchronous action to execute if the result is an
    /// "Ok" value.
    /// </param>
    /// <typeparam name="TOk">The type of the "Ok" value contained in the result.</typeparam>
    /// <typeparam name="TErr">The type of the "Err" value contained in the result.</typeparam>
    /// <returns>The original result.</returns>
    public static async Task<Result<TOk, TErr>> InspectAsync<TOk, TErr>(
        this Result<TOk, TErr> result,
        Func<TOk, Task> action)
        where TOk : notnull where TErr : notnull
    {
        if (result.IsOk)
        {
            await action(result.Unwrap()).ConfigureAwait(false);
        }

        return result;
    }

    /// <summary>
    /// Executes the provided asynchronous action if the result is successful
    /// and contains an "Ok" value.
    /// </summary>
    /// <param name="result">The result to inspect.</param>
    /// <param name="action">
    /// The asynchronous action to execute if the result is an
    /// "Ok" value.
    /// </param>
    /// <typeparam name="TOk">The type of the "Ok" value contained in the result.</typeparam>
    /// <typeparam name="TErr">The type of the "Err" value contained in the result.</typeparam>
    /// <returns>The original result.</returns>
    public static async ValueTask<Result<TOk, TErr>> InspectAsync<TOk, TErr>(
        this Result<TOk, TErr> result,
        Func<TOk, ValueTask> action)
        where TOk : notnull where TErr : notnull
    {
        if (result.IsOk)
        {
            await action(result.Unwrap()).ConfigureAwait(false);
        }

        return result;
    }

    /// <summary>
    /// Executes the provided asynchronous action on the "Ok" value of the
    /// result if it is successful, and returns the original result.
    /// </summary>
    /// <param name="resultTask">The asynchronous result to inspect.</param>
    /// <param name="action">
    /// The asynchronous action to execute if the result contains
    /// an "Ok" value.
    /// </param>
    /// <typeparam name="TOk">The type of the "Ok" value contained in the result.</typeparam>
    /// <typeparam name="TErr">The type of the "Err" value contained in the result.</typeparam>
    /// <returns>The original result.</returns>
    public static async Task<Result<TOk, TErr>> InspectAsync<TOk, TErr>(
        this Task<Result<TOk, TErr>> resultTask,
        Func<TOk, Task> action)
        where TOk : notnull where TErr : notnull
    {
        Result<TOk, TErr> result = await resultTask.ConfigureAwait(false);
        return await result.InspectAsync(action).ConfigureAwait(false);
    }

    /// <summary>
    /// Executes the provided asynchronous action if the result is successful
    /// and contains an "Ok" value.
    /// </summary>
    /// <param name="resultTask">The result to inspect.</param>
    /// <param name="action">
    /// The asynchronous action to execute if the result contains
    /// an "Ok" value.
    /// </param>
    /// <typeparam name="TOk">The type of the "Ok" value contained in the result.</typeparam>
    /// <typeparam name="TErr">The type of the "Err" value contained in the result.</typeparam>
    /// <returns>The original result.</returns>
    public static async ValueTask<Result<TOk, TErr>> InspectAsync<TOk, TErr>(
        this ValueTask<Result<TOk, TErr>> resultTask,
        Func<TOk, ValueTask> action)
        where TOk : notnull where TErr : notnull
    {
        Result<TOk, TErr> result = await resultTask.ConfigureAwait(false);
        return await result.InspectAsync(action).ConfigureAwait(false);
    }
}
