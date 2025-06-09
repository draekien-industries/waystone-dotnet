namespace Waystone.Monads.Results.Extensions;

using System;
using System.Threading.Tasks;

public static partial class ResultOfTOkTErrAsyncExtensions
{
    /// <summary>
    /// Asynchronously transforms the error value of a result using the
    /// provided mapping function, returning a new result with the transformed error.
    /// </summary>
    /// <param name="result">
    /// The result object to process, containing either an ok
    /// value or an error value.
    /// </param>
    /// <param name="map">
    /// A function to asynchronously map the error value to a new
    /// type.
    /// </param>
    /// <typeparam name="TOk">The type of the ok value in the result.</typeparam>
    /// <typeparam name="TErr">The type of the error value in the result.</typeparam>
    /// <typeparam name="TOut">The type to which the error value will be transformed.</typeparam>
    /// <returns>
    /// A task that resolves to a new result with the same ok value, or the
    /// transformed error value.
    /// </returns>
    public static async Task<Result<TOk, TOut>> MapErrAsync<TOk, TErr, TOut>(
        this Result<TOk, TErr> result,
        Func<TErr, Task<TOut>> map)
        where TOk : notnull where TErr : notnull where TOut : notnull
    {
        if (result.IsOk) return Result.Ok<TOk, TOut>(result.Unwrap());
        TOut output = await map(result.UnwrapErr()).ConfigureAwait(false);
        return Result.Err<TOk, TOut>(output);
    }

    /// <summary>
    /// Asynchronously transforms the error value of a result using the
    /// provided mapping function, returning a new result with the transformed error.
    /// </summary>
    /// <param name="result">
    /// The result object to process, containing either an ok
    /// value or an error value.
    /// </param>
    /// <param name="map">
    /// A function to asynchronously map the error value to a new
    /// type.
    /// </param>
    /// <typeparam name="TOk">The type of the ok value in the result.</typeparam>
    /// <typeparam name="TErr">The type of the error value in the result.</typeparam>
    /// <typeparam name="TOut">The type to which the error value will be transformed.</typeparam>
    /// <returns>
    /// A ValueTask that resolves to a new result with the same ok value, or
    /// the transformed error value.
    /// </returns>
    public static async ValueTask<Result<TOk, TOut>> MapErrAsync<
        TOk, TErr, TOut>(
        this Result<TOk, TErr> result,
        Func<TErr, ValueTask<TOut>> map)
        where TOk : notnull where TErr : notnull where TOut : notnull
    {
        if (result.IsOk) return Result.Ok<TOk, TOut>(result.Unwrap());
        TOut output = await map(result.UnwrapErr()).ConfigureAwait(false);
        return Result.Err<TOk, TOut>(output);
    }

    /// <summary>
    /// Asynchronously transforms the error value of a result using the
    /// provided mapping function, returning a new result with the transformed error.
    /// </summary>
    /// <param name="resultTask">
    /// The result task to process, containing either an ok
    /// value or an error value.
    /// </param>
    /// <param name="map">
    /// A function to asynchronously map the error value to a new
    /// type.
    /// </param>
    /// <typeparam name="TOk">The type of the ok value in the result.</typeparam>
    /// <typeparam name="TErr">The type of the error value in the result.</typeparam>
    /// <typeparam name="TOut">The type to which the error value will be transformed.</typeparam>
    /// <returns>
    /// A task that resolves to a new result with the same ok value, or the
    /// transformed error value.
    /// </returns>
    public static async Task<Result<TOk, TOut>> MapErrAsync<TOk, TErr, TOut>(
        this Task<Result<TOk, TErr>> resultTask,
        Func<TErr, Task<TOut>> map)
        where TOk : notnull where TErr : notnull where TOut : notnull
    {
        Result<TOk, TErr> result = await resultTask.ConfigureAwait(false);
        return await result.MapErrAsync(map).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously transforms the error value of a result using the
    /// specified mapping function, returning a new result with the transformed error
    /// value.
    /// </summary>
    /// <param name="resultTask">
    /// The result object to process, containing either an ok
    /// value or an error value.
    /// </param>
    /// <param name="map">
    /// A function that asynchronously maps the error value to a new
    /// type.
    /// </param>
    /// <typeparam name="TOk">The type of the ok value in the result.</typeparam>
    /// <typeparam name="TErr">The type of the error value in the result.</typeparam>
    /// <typeparam name="TOut">The type to which the error value will be transformed.</typeparam>
    /// <returns>
    /// A task that resolves to a new result with the same ok value, or the
    /// transformed error value.
    /// </returns>
    public static async ValueTask<Result<TOk, TOut>> MapErrAsync<
        TOk, TErr, TOut>(
        this ValueTask<Result<TOk, TErr>> resultTask,
        Func<TErr, ValueTask<TOut>> map)
        where TOk : notnull where TErr : notnull where TOut : notnull
    {
        Result<TOk, TErr> result = await resultTask.ConfigureAwait(false);
        return await result.MapErrAsync(map).ConfigureAwait(false);
    }
}
