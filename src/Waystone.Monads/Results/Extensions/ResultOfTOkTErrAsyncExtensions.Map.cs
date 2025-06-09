namespace Waystone.Monads.Results.Extensions;

using System;
using System.Threading.Tasks;

public static partial class ResultOfTOkTErrAsyncExtensions
{
    /// <summary>
    /// Maps the result's success value using the specified asynchronous
    /// mapping function.
    /// </summary>
    /// <param name="result">The result instance to map.</param>
    /// <param name="map">
    /// An asynchronous function to map the success value to a
    /// different type.
    /// </param>
    /// <typeparam name="TOk">The type of the success value in the initial result.</typeparam>
    /// <typeparam name="TErr">The type of the error value in the result.</typeparam>
    /// <typeparam name="TOut">The type of the success value after mapping.</typeparam>
    /// <returns>
    /// A new result instance containing the mapped success value or the
    /// original error.
    /// </returns>
    public static async Task<Result<TOut, TErr>> MapAsync<TOk, TErr, TOut>(
        this Result<TOk, TErr> result,
        Func<TOk, Task<TOut>> map)
        where TOk : notnull where TErr : notnull where TOut : notnull
    {
        if (result.IsErr) return Result.Err<TOut, TErr>(result.UnwrapErr());

        TOut output = await map(result.Unwrap()).ConfigureAwait(false);
        return Result.Ok<TOut, TErr>(output);
    }

    /// <summary>
    /// Maps the success value of the result using a specified asynchronous
    /// mapping function.
    /// </summary>
    /// <param name="result">The result instance to map.</param>
    /// <param name="map">
    /// An asynchronous function to transform the success value to a
    /// different type.
    /// </param>
    /// <typeparam name="TOk">The type of the success value in the initial result.</typeparam>
    /// <typeparam name="TErr">The type of the error value in the result.</typeparam>
    /// <typeparam name="TOut">The type of the success value after mapping.</typeparam>
    /// <returns>
    /// A new result containing the mapped success value or the original
    /// error.
    /// </returns>
    public static async ValueTask<Result<TOut, TErr>> MapAsync<TOk, TErr, TOut>(
        this Result<TOk, TErr> result,
        Func<TOk, ValueTask<TOut>> map)
        where TOk : notnull where TErr : notnull where TOut : notnull
    {
        if (result.IsErr) return Result.Err<TOut, TErr>(result.UnwrapErr());

        TOut output = await map(result.Unwrap()).ConfigureAwait(false);
        return Result.Ok<TOut, TErr>(output);
    }

    /// <summary>
    /// Asynchronously maps the success value of a result using the specified
    /// asynchronous mapping function.
    /// </summary>
    /// <param name="resultTask">A task that represents the result instance to map.</param>
    /// <param name="map">
    /// An asynchronous function to map the success value to a
    /// different type.
    /// </param>
    /// <typeparam name="TOk">The type of the success value in the initial result.</typeparam>
    /// <typeparam name="TErr">The type of the error value in the result.</typeparam>
    /// <typeparam name="TOut">The type of the success value after mapping.</typeparam>
    /// <returns>
    /// A task representing a new result instance containing the mapped
    /// success value or the original error.
    /// </returns>
    public static async Task<Result<TOut, TErr>> MapAsync<TOk, TErr, TOut>(
        this Task<Result<TOk, TErr>> resultTask,
        Func<TOk, Task<TOut>> map)
        where TOk : notnull where TErr : notnull where TOut : notnull
    {
        Result<TOk, TErr> result = await resultTask.ConfigureAwait(false);
        return await result.MapAsync(map).ConfigureAwait(false);
    }

    /// <summary>
    /// Maps the success value of a result asynchronously using the provided
    /// mapping function.
    /// </summary>
    /// <param name="resultTask">A ValueTask representing the result to map.</param>
    /// <param name="map">
    /// An asynchronous function to map the success value to a
    /// different type.
    /// </param>
    /// <typeparam name="TOk">The type of the success value in the initial result.</typeparam>
    /// <typeparam name="TErr">The type of the error value in the result.</typeparam>
    /// <typeparam name="TOut">The type of the success value after mapping.</typeparam>
    /// <returns>
    /// A ValueTask of a result containing the mapped success value or the
    /// original error.
    /// </returns>
    public static async ValueTask<Result<TOut, TErr>> MapAsync<TOk, TErr, TOut>(
        this ValueTask<Result<TOk, TErr>> resultTask,
        Func<TOk, ValueTask<TOut>> map)
        where TOk : notnull where TErr : notnull where TOut : notnull
    {
        Result<TOk, TErr> result = await resultTask.ConfigureAwait(false);
        return await result.MapAsync(map).ConfigureAwait(false);
    }
}
