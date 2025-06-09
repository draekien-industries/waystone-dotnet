namespace Waystone.Monads.Results.Extensions;

using System;
using System.Threading.Tasks;

public static partial class ResultOfTOkTErrAsyncExtensions
{
    /// <summary>
    /// Maps the value of a <see cref="Result{TOk, TErr}" /> to another type
    /// using the provided mapping functions. If the result is Ok, the
    /// <paramref name="map" /> function is invoked. If the result is Err, the
    /// <paramref name="else" /> function is invoked.
    /// </summary>
    /// <param name="result">The result instance to map.</param>
    /// <param name="else">
    /// A function to handle the Err case asynchronously and map to
    /// the desired output type.
    /// </param>
    /// <param name="map">
    /// A function to handle the Ok case asynchronously and map to
    /// the desired output type.
    /// </param>
    /// <typeparam name="TOk">The type of the Ok value in the result.</typeparam>
    /// <typeparam name="TErr">The type of the Err value in the result.</typeparam>
    /// <typeparam name="TOut">The type of the output value after mapping.</typeparam>
    /// <returns>An asynchronous operation that resolves to the mapped value.</returns>
    public static Task<TOut> MapOrElseAsync<TOk, TErr, TOut>(
        this Result<TOk, TErr> result,
        Func<TErr, Task<TOut>> @else,
        Func<TOk, Task<TOut>> map)
        where TOk : notnull where TErr : notnull where TOut : notnull =>
        result.Match(map, @else);

    /// <summary>
    /// Maps the value of a <see cref="Result{TOk, TErr}" /> to another type
    /// using the provided asynchronous mapping functions. If the result is Ok, the
    /// <paramref name="map" /> function is invoked. If the result is Err, the
    /// <paramref name="else" /> function is invoked.
    /// </summary>
    /// <param name="result">The result instance to map asynchronously.</param>
    /// <param name="else">
    /// A function to handle the Err case asynchronously and map to
    /// the desired output type.
    /// </param>
    /// <param name="map">
    /// A function to handle the Ok case asynchronously and map to
    /// the desired output type.
    /// </param>
    /// <typeparam name="TOk">The type of the Ok value in the result.</typeparam>
    /// <typeparam name="TErr">The type of the Err value in the result.</typeparam>
    /// <typeparam name="TOut">The type of the output value after mapping.</typeparam>
    /// <returns>An asynchronous operation that resolves to the mapped value.</returns>
    public static ValueTask<TOut> MapOrElseAsync<TOk, TErr, TOut>(
        this Result<TOk, TErr> result,
        Func<TErr, ValueTask<TOut>> @else,
        Func<TOk, ValueTask<TOut>> map)
        where TOk : notnull where TErr : notnull where TOut : notnull =>
        result.Match(map, @else);

    /// <summary>
    /// Maps the value of a <see cref="Result{TOk, TErr}" /> to another type
    /// using the provided asynchronous mapping functions. If the result is Ok, the
    /// <paramref name="map" /> function is invoked asynchronously. If the result is
    /// Err, the <paramref name="else" /> function is invoked asynchronously.
    /// </summary>
    /// <param name="resultTask">The result instance to map.</param>
    /// <param name="else">
    /// A function to handle the Err case asynchronously and map to
    /// the desired output type.
    /// </param>
    /// <param name="map">
    /// A function to handle the Ok case asynchronously and map to
    /// the desired output type.
    /// </param>
    /// <typeparam name="TOk">The type of the Ok value in the result.</typeparam>
    /// <typeparam name="TErr">The type of the Err value in the result.</typeparam>
    /// <typeparam name="TOut">The type of the output value after mapping.</typeparam>
    /// <returns>An asynchronous operation that resolves to the mapped value.</returns>
    public static Task<TOut> MapOrElseAsync<TOk, TErr, TOut>(
        this Task<Result<TOk, TErr>> resultTask,
        Func<TErr, Task<TOut>> @else,
        Func<TOk, Task<TOut>> map)
        where TOk : notnull where TErr : notnull where TOut : notnull =>
        resultTask.MatchAsync(map, @else);

    /// <summary>
    /// Maps the value of a <see cref="Result{TOk, TErr}" /> or asynchronous
    /// result task to another type using the provided mapping functions. If the result
    /// is Ok, the <paramref name="map" /> function is invoked. If the result is Err,
    /// the <paramref name="else" /> function is invoked.
    /// </summary>
    /// <param name="resultTask">The asynchronous result task to map.</param>
    /// <param name="else">
    /// A function to handle the Err case asynchronously and map to
    /// the desired output type.
    /// </param>
    /// <param name="map">
    /// A function to handle the Ok case asynchronously and map to
    /// the desired output type.
    /// </param>
    /// <typeparam name="TOk">The type of the Ok value in the result.</typeparam>
    /// <typeparam name="TErr">The type of the Err value in the result.</typeparam>
    /// <typeparam name="TOut">The type of the output value after mapping.</typeparam>
    /// <returns>An asynchronous operation that resolves to the mapped value.</returns>
    public static ValueTask<TOut> MapOrElseAsync<TOk, TErr, TOut>(
        this ValueTask<Result<TOk, TErr>> resultTask,
        Func<TErr, ValueTask<TOut>> @else,
        Func<TOk, ValueTask<TOut>> map)
        where TOk : notnull where TErr : notnull where TOut : notnull =>
        resultTask.MatchAsync(map, @else);
}
