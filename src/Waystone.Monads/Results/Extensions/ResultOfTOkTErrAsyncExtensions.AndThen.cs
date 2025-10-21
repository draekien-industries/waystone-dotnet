namespace Waystone.Monads.Results.Extensions;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

[ExcludeFromCodeCoverage]
#if !DEBUG
    [DebuggerStepThrough]
#endif
public static partial class ResultOfTOkTErrAsyncExtensions
{
    /// <summary>
    /// Invokes the <paramref name="factory" /> if the result is
    /// <see cref="Ok{TOk,TErr}" />, otherwise returns the <see cref="Err{TOk,TErr}" />
    /// value of this result instance.
    /// </summary>
    /// <param name="result">The result instance to be processed.</param>
    /// <param name="factory">A function that creates the other result.</param>
    /// <typeparam name="TOk">The type of the ok value contained in the result.</typeparam>
    /// <typeparam name="TErr">The type of the err value contained in the result.</typeparam>
    /// <typeparam name="TOut">
    /// The type of the return value after executing the
    /// factory.
    /// </typeparam>
    /// <returns>
    /// The result of invoking the factory if the result is an
    /// <see cref="Ok{TOk,TErr}" />, otherwise the original result.
    /// </returns>
    public static Task<Result<TOut, TErr>> AndThenAsync<TOk, TErr, TOut>(
        this Result<TOk, TErr> result,
        Func<TOk, Task<Result<TOut, TErr>>> factory)
        where TOk : notnull where TErr : notnull where TOut : notnull =>
        result.MatchAsync(
            factory,
            err => Task.FromResult(Result.Err<TOut, TErr>(err)));

    /// <summary>
    /// Invokes the <paramref name="factory" /> if the result is
    /// <see cref="Ok{TOk,TErr}" />, otherwise returns the <see cref="Err{TOk,TErr}" />
    /// value of this result instance.
    /// </summary>
    /// <param name="result">The result instance to be processed.</param>
    /// <param name="factory">A function that creates the other result.</param>
    /// <typeparam name="TOk">The type of the ok value contained in the result.</typeparam>
    /// <typeparam name="TErr">The type of the err value contained in the result.</typeparam>
    /// <typeparam name="TOut">
    /// The type of the return value after executing the
    /// factory.
    /// </typeparam>
    /// <returns>
    /// The result of invoking the factory if the result is an
    /// <see cref="Ok{TOk,TErr}" />, otherwise the original result.
    /// </returns>
    public static ValueTask<Result<TOut, TErr>> AndThenAsync<TOk, TErr, TOut>(
        this Result<TOk, TErr> result,
        Func<TOk, ValueTask<Result<TOut, TErr>>> factory)
        where TOk : notnull where TErr : notnull where TOut : notnull =>
        result.MatchAsync(
            factory,
            err => new ValueTask<Result<TOut, TErr>>(
                Result.Err<TOut, TErr>(err)));

    /// <summary>
    /// Invokes the <paramref name="factory" /> if the result is
    /// <see cref="Ok{TOk,TErr}" />, otherwise returns the <see cref="Err{TOk,TErr}" />
    /// value of this result instance.
    /// </summary>
    /// <param name="resultTask">The result instance to be processed.</param>
    /// <param name="factory">A function that creates the other result.</param>
    /// <typeparam name="TOk">The type of the ok value contained in the result.</typeparam>
    /// <typeparam name="TErr">The type of the err value contained in the result.</typeparam>
    /// <typeparam name="TOut">
    /// The type of the return value after executing the
    /// factory.
    /// </typeparam>
    /// <returns>
    /// The result of invoking the factory if the result is an
    /// <see cref="Ok{TOk,TErr}" />, otherwise the original result.
    /// </returns>
    public static Task<Result<TOut, TErr>> AndThenAsync<TOk, TErr, TOut>(
        this Task<Result<TOk, TErr>> resultTask,
        Func<TOk, Task<Result<TOut, TErr>>> factory)
        where TOk : notnull where TErr : notnull where TOut : notnull =>
        resultTask.MatchAsync(
            factory,
            err => Task.FromResult(Result.Err<TOut, TErr>(err)));

    /// <summary>
    /// Invokes the <paramref name="factory" /> if the result is
    /// <see cref="Ok{TOk,TErr}" />, otherwise returns the <see cref="Err{TOk,TErr}" />
    /// value of this result instance.
    /// </summary>
    /// <param name="resultTask">The result instance to be processed.</param>
    /// <param name="factory">A function that creates the other result.</param>
    /// <typeparam name="TOk">The type of the ok value contained in the result.</typeparam>
    /// <typeparam name="TErr">The type of the err value contained in the result.</typeparam>
    /// <typeparam name="TOut">
    /// The type of the return value after executing the
    /// factory.
    /// </typeparam>
    /// <returns>
    /// The result of invoking the factory if the result is an
    /// <see cref="Ok{TOk,TErr}" />, otherwise the original result.
    /// </returns>
    public static ValueTask<Result<TOut, TErr>> AndThenAsync<TOk, TErr, TOut>(
        this ValueTask<Result<TOk, TErr>> resultTask,
        Func<TOk, ValueTask<Result<TOut, TErr>>> factory)
        where TOk : notnull where TErr : notnull where TOut : notnull =>
        resultTask.MatchAsync(
            factory,
            err => new ValueTask<Result<TOut, TErr>>(
                Result.Err<TOut, TErr>(err)));
}
