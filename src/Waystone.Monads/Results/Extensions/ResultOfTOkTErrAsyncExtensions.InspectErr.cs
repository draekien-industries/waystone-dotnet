namespace Waystone.Monads.Results.Extensions;

using System;
using System.Threading.Tasks;

public static partial class ResultOfTOkTErrAsyncExtensions
{
    /// <summary>
    /// Asynchronously executes a specified action on the error value of the
    /// result, if the result represents an error.
    /// </summary>
    /// <param name="result">The result containing either a value or an error.</param>
    /// <param name="action">
    /// The asynchronous action to execute if the result is an
    /// error.
    /// </param>
    /// <typeparam name="TOk">The type of the success value.</typeparam>
    /// <typeparam name="TErr">The type of the error value.</typeparam>
    /// <returns>The original result.</returns>
    public static async Task<Result<TOk, TErr>> InspectErrAsync<TOk, TErr>(
        this Result<TOk, TErr> result,
        Func<TErr, Task> action)
        where TOk : notnull where TErr : notnull
    {
        if (result.IsErr)
        {
            await action(result.UnwrapErr()).ConfigureAwait(false);
        }

        return result;
    }

    /// <summary>
    /// Asynchronously executes the specified asynchronous action on the error
    /// value of the result, if the result represents an error.
    /// </summary>
    /// <param name="result">The result containing either a success or an error value.</param>
    /// <param name="action">
    /// The asynchronous action to execute if the result contains
    /// an error value.
    /// </param>
    /// <typeparam name="TOk">The type of the success value.</typeparam>
    /// <typeparam name="TErr">The type of the error value.</typeparam>
    /// <returns>The original result.</returns>
    public static async ValueTask<Result<TOk, TErr>> InspectErrAsync<TOk, TErr>(
        this Result<TOk, TErr> result,
        Func<TErr, ValueTask> action)
        where TOk : notnull where TErr : notnull
    {
        if (result.IsErr)
        {
            await action(result.UnwrapErr()).ConfigureAwait(false);
        }

        return result;
    }

    /// <summary>
    /// Asynchronously executes a specified action on the error value of the
    /// result after the result is resolved, if the result represents an error.
    /// </summary>
    /// <param name="resultTask">
    /// The task resulting in a result containing either a
    /// value or an error.
    /// </param>
    /// <param name="action">
    /// The asynchronous action to execute on the error value if
    /// the result represents an error.
    /// </param>
    /// <typeparam name="TOk">The type of the success value.</typeparam>
    /// <typeparam name="TErr">The type of the error value.</typeparam>
    /// <returns>The original result.</returns>
    public static async Task<Result<TOk, TErr>> InspectErrAsync<TOk, TErr>(
        this Task<Result<TOk, TErr>> resultTask,
        Func<TErr, Task> action)
        where TOk : notnull where TErr : notnull
    {
        Result<TOk, TErr> result = await resultTask.ConfigureAwait(false);
        return await result.InspectErrAsync(action).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously executes a specified action on the error value of the
    /// result, if the result represents an error.
    /// </summary>
    /// <param name="resultTask">The result containing either a value or an error.</param>
    /// <param name="action">
    /// The asynchronous action to execute if the result is an
    /// error.
    /// </param>
    /// <typeparam name="TOk">The type of the success value.</typeparam>
    /// <typeparam name="TErr">The type of the error value.</typeparam>
    /// <returns>The original result.</returns>
    public static async ValueTask<Result<TOk, TErr>> InspectErrAsync<TOk, TErr>(
        this ValueTask<Result<TOk, TErr>> resultTask,
        Func<TErr, ValueTask> action)
        where TOk : notnull where TErr : notnull
    {
        Result<TOk, TErr> result = await resultTask.ConfigureAwait(false);
        return await result.InspectErrAsync(action).ConfigureAwait(false);
    }
}
