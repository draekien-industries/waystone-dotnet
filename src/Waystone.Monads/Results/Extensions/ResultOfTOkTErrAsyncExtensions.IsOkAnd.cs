namespace Waystone.Monads.Results.Extensions;

using System;
using System.Threading.Tasks;

public static partial class ResultOfTOkTErrAsyncExtensions
{
    /// <summary>
    /// Evaluates whether the result is "Ok" and satisfies the provided
    /// asynchronous predicate.
    /// </summary>
    /// <param name="result">The result to evaluate.</param>
    /// <param name="predicate">
    /// An asynchronous predicate to evaluate on the "Ok"
    /// value.
    /// </param>
    /// <typeparam name="TOk">The type of the "Ok" value.</typeparam>
    /// <typeparam name="TErr">The type of the "Err" value.</typeparam>
    /// <returns>
    /// A task representing the asynchronous operation, with a result of
    /// <c>true</c> if the result is "Ok" and the predicate returns <c>true</c>, or
    /// <c>false</c> otherwise.
    /// </returns>
    public static Task<bool> IsOkAndAsync<TOk, TErr>(
        this Result<TOk, TErr> result,
        Func<TOk, Task<bool>> predicate)
        where TOk : notnull where TErr : notnull =>
        result.Match(predicate, _ => Task.FromResult(false));

    /// <summary>
    /// Evaluates whether the result is "Ok" and satisfies the provided
    /// asynchronous predicate.
    /// </summary>
    /// <param name="result">The result to evaluate.</param>
    /// <param name="predicate">
    /// An asynchronous predicate to evaluate on the "Ok"
    /// value.
    /// </param>
    /// <typeparam name="TOk">The type of the "Ok" value.</typeparam>
    /// <typeparam name="TErr">The type of the "Err" value.</typeparam>
    /// <returns>
    /// A task representing the asynchronous operation, with a result of
    /// <c>true</c> if the result is "Ok" and the predicate returns <c>true</c>, or
    /// <c>false</c> otherwise.
    /// </returns>
    public static ValueTask<bool> IsOkAndAsync<TOk, TErr>(
        this Result<TOk, TErr> result,
        Func<TOk, ValueTask<bool>> predicate)
        where TOk : notnull where TErr : notnull =>
        result.Match(predicate, _ => new ValueTask<bool>(false));

    /// <summary>
    /// Evaluates whether the resolved result is "Ok" and satisfies the
    /// provided asynchronous predicate.
    /// </summary>
    /// <param name="resultTask">The task representing the result to evaluate.</param>
    /// <param name="predicate">
    /// An asynchronous predicate to evaluate on the "Ok"
    /// value.
    /// </param>
    /// <typeparam name="TOk">The type of the "Ok" value.</typeparam>
    /// <typeparam name="TErr">The type of the "Err" value.</typeparam>
    /// <returns>
    /// A task representing the asynchronous operation, with a result of
    /// <c>true</c> if the result is "Ok" and the predicate returns <c>true</c>, or
    /// <c>false</c> otherwise.
    /// </returns>
    public static Task<bool> IsOkAndAsync<TOk, TErr>(
        this Task<Result<TOk, TErr>> resultTask,
        Func<TOk, Task<bool>> predicate)
        where TOk : notnull where TErr : notnull => resultTask.MatchAsync(
        predicate,
        _ => Task.FromResult(false));

    /// <summary>
    /// Evaluates whether the result is "Ok" and satisfies the provided
    /// asynchronous predicate.
    /// </summary>
    /// <param name="resultTask">
    /// A value task representing the asynchronous result to
    /// evaluate.
    /// </param>
    /// <param name="predicate">
    /// An asynchronous predicate to evaluate on the "Ok"
    /// value.
    /// </param>
    /// <typeparam name="TOk">The type of the "Ok" value.</typeparam>
    /// <typeparam name="TErr">The type of the "Err" value.</typeparam>
    /// <returns>
    /// A value task representing the asynchronous operation, with a result of
    /// <c>true</c> if the result is "Ok" and the predicate returns <c>true</c>, or
    /// <c>false</c> otherwise.
    /// </returns>
    public static ValueTask<bool> IsOkAndAsync<TOk, TErr>(
        this ValueTask<Result<TOk, TErr>> resultTask,
        Func<TOk, ValueTask<bool>> predicate)
        where TOk : notnull where TErr : notnull => resultTask.MatchAsync(
        predicate,
        _ => new ValueTask<bool>(false));
}
