namespace Waystone.Monads.Results.Extensions;

using System;
using System.Threading.Tasks;

public static partial class ResultOfTOkTErrAsyncExtensions
{
    /// <summary>
    /// Invokes <paramref name="else" /> if the result is
    /// <see cref="Err{TOk,TErr}" />, otherwise returns the <see cref="Ok{TOk,TErr}" />
    /// value of this result instance.
    /// </summary>
    /// <param name="result">The result to be processed.</param>
    /// <param name="else">
    /// The function to be invoked if the result is
    /// <see cref="Err{TOk,TErr}" />.
    /// </param>
    /// <typeparam name="TOk">The type of the ok value contained in the result.</typeparam>
    /// <typeparam name="TErr">The type of the err value contained in the result.</typeparam>
    /// <typeparam name="TOut">
    /// The type of the return value after executing the
    /// function.
    /// </typeparam>
    /// <returns>
    /// The original result if it is <see cref="Ok{TOk,TErr}" />. Otherwise,
    /// the result of invoking <paramref name="else" />.
    /// </returns>
    public static Task<Result<TOk, TOut>> OrElseAsync<TOk, TErr, TOut>(
        this Result<TOk, TErr> result,
        Func<TErr, Task<Result<TOk, TOut>>> @else)
        where TOk : notnull where TErr : notnull where TOut : notnull =>
        result.MatchAsync(
            value => Task.FromResult(Result.Ok<TOk, TOut>(value)),
            @else);

    /// <summary>
    /// Invokes <paramref name="else" /> if the result is
    /// <see cref="Err{TOk,TErr}" />, otherwise returns the <see cref="Ok{TOk,TErr}" />
    /// value of this result instance.
    /// </summary>
    /// <param name="resultTask">The result to be processed.</param>
    /// <param name="else">
    /// The function to be invoked if the result is
    /// <see cref="Err{TOk,TErr}" />.
    /// </param>
    /// <typeparam name="TOk">The type of the ok value contained in the result.</typeparam>
    /// <typeparam name="TErr">The type of the err value contained in the result.</typeparam>
    /// <typeparam name="TOut">
    /// The type of the return value after executing the
    /// function.
    /// </typeparam>
    /// <returns>
    /// The original result if it is <see cref="Ok{TOk,TErr}" />. Otherwise,
    /// the result of invoking <paramref name="else" />.
    /// </returns>
    public static Task<Result<TOk, TOut>> OrElseAsync<TOk, TErr, TOut>(
        this Task<Result<TOk, TErr>> resultTask,
        Func<TErr, Task<Result<TOk, TOut>>> @else)
        where TOk : notnull where TErr : notnull where TOut : notnull =>
        resultTask.MatchAsync(
            value => Task.FromResult(Result.Ok<TOk, TOut>(value)),
            @else);
}
