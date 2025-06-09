namespace Waystone.Monads.Results.Extensions;

using System;
using System.Threading.Tasks;

public static partial class ResultOfTOkTErrAsyncExtensions
{
    /// <summary>
    /// Determines asynchronously if the result is an error and satisfies the
    /// specified predicate.
    /// </summary>
    /// <param name="result">The result object to evaluate.</param>
    /// <param name="predicate">
    /// The asynchronous predicate function to evaluate against
    /// the error value.
    /// </param>
    /// <typeparam name="TOk">The type of the success value.</typeparam>
    /// <typeparam name="TErr">The type of the error value.</typeparam>
    /// <returns>
    /// A task representing the asynchronous operation. The task result is
    /// true if the result is an error and the predicate evaluates to true; otherwise,
    /// false.
    /// </returns>
    public static Task<bool> IsErrAndAsync<TOk, TErr>(
        this Result<TOk, TErr> result,
        Func<TErr, Task<bool>> predicate)
        where TOk : notnull where TErr : notnull =>
        result.Match(_ => Task.FromResult(false), predicate);

    /// <summary>
    /// Determines asynchronously if the result is an error and satisfies the
    /// specified predicate.
    /// </summary>
    /// <param name="result">The result object to evaluate.</param>
    /// <param name="predicate">
    /// The predicate function, as a task, to evaluate against
    /// the error value.
    /// </param>
    /// <typeparam name="TOk">The type of the success value.</typeparam>
    /// <typeparam name="TErr">The type of the error value.</typeparam>
    /// <returns>
    /// A task representing the asynchronous operation. The task result is
    /// true if the result is an error and the predicate evaluates to true; otherwise,
    /// false.
    /// </returns>
    public static ValueTask<bool> IsErrAndAsync<TOk, TErr>(
        this Result<TOk, TErr> result,
        Func<TErr, ValueTask<bool>> predicate)
        where TOk : notnull where TErr : notnull =>
        result.Match(_ => new ValueTask<bool>(false), predicate);

    /// <summary>
    /// Determines asynchronously if the result is an error and satisfies the
    /// specified predicate.
    /// </summary>
    /// <param name="resultTask">The result object to evaluate.</param>
    /// <param name="predicate">
    /// The asynchronous predicate function to evaluate against
    /// the error value.
    /// </param>
    /// <typeparam name="TOk">The type of the success value.</typeparam>
    /// <typeparam name="TErr">The type of the error value.</typeparam>
    /// <returns>
    /// A task representing the asynchronous operation. The task result is
    /// true if the result is an error and the predicate evaluates to true; otherwise,
    /// false.
    /// </returns>
    public static Task<bool> IsErrAndAsync<TOk, TErr>(
        this Task<Result<TOk, TErr>> resultTask,
        Func<TErr, Task<bool>> predicate)
        where TOk : notnull where TErr : notnull =>
        resultTask.MatchAsync(_ => Task.FromResult(false), predicate);

    /// <summary>
    /// Determines asynchronously if the result is an error and satisfies the
    /// specified predicate.
    /// </summary>
    /// <param name="resultTask">The result object to evaluate.</param>
    /// <param name="predicate">
    /// The asynchronous predicate function to evaluate against
    /// the error value.
    /// </param>
    /// <typeparam name="TOk">The type of the success value.</typeparam>
    /// <typeparam name="TErr">The type of the error value.</typeparam>
    /// <returns>
    /// A task representing the asynchronous operation. The task result is
    /// true if the result is an error and the predicate evaluates to true; otherwise,
    /// false.
    /// </returns>
    public static ValueTask<bool> IsErrAndAsync<TOk, TErr>(
        this ValueTask<Result<TOk, TErr>> resultTask,
        Func<TErr, ValueTask<bool>> predicate)
        where TOk : notnull where TErr : notnull =>
        resultTask.MatchAsync(_ => new ValueTask<bool>(false), predicate);
}
