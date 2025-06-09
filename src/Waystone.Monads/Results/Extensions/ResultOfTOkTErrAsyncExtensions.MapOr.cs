namespace Waystone.Monads.Results.Extensions;

using System;
using System.Threading.Tasks;

public static partial class ResultOfTOkTErrAsyncExtensions
{
    /// <summary>
    /// Transforms the <see cref="Result{TOk, TErr}" /> into a new value using
    /// the provided asynchronous mapping function if it is an Ok value. Returns a
    /// default value if it is an Err value.
    /// </summary>
    /// <param name="result">
    /// The <see cref="Result{TOk, TErr}" /> instance to
    /// transform.
    /// </param>
    /// <param name="default">
    /// The default value that will be returned if the
    /// <see cref="Result{TOk, TErr}" /> is an Err value.
    /// </param>
    /// <param name="map">
    /// An asynchronous function to apply to the Ok value if the
    /// <see cref="Result{TOk, TErr}" /> is Ok.
    /// </param>
    /// <typeparam name="TOk">The type of the Ok value.</typeparam>
    /// <typeparam name="TErr">The type of the Err value.</typeparam>
    /// <typeparam name="TOut">The type of the resulting output value.</typeparam>
    /// <returns>
    /// A <see cref="Task{TResult}" /> or <see cref="ValueTask{TResult}" />
    /// containing the transformed value if the <see cref="Result{TOk, TErr}" /> is Ok,
    /// or the default value if it is Err.
    /// </returns>
    public static Task<TOut> MapOrAsync<TOk, TErr, TOut>(
        this Result<TOk, TErr> result,
        TOut @default,
        Func<TOk, Task<TOut>> map)
        where TOk : notnull where TErr : notnull where TOut : notnull =>
        result.MatchAsync(map, _ => Task.FromResult(@default));

    /// <summary>
    /// Transforms the <see cref="Result{TOk, TErr}" /> into a new value using
    /// the provided asynchronous mapping function if it is an Ok value. Returns a
    /// default value if it is an Err value.
    /// </summary>
    /// <param name="result">
    /// The <see cref="Result{TOk, TErr}" /> instance to
    /// transform.
    /// </param>
    /// <param name="default">
    /// The default value that will be returned if the
    /// <see cref="Result{TOk, TErr}" /> is an Err value.
    /// </param>
    /// <param name="map">
    /// An asynchronous function to apply to the Ok value if the
    /// <see cref="Result{TOk, TErr}" /> is Ok.
    /// </param>
    /// <typeparam name="TOk">The type of the Ok value.</typeparam>
    /// <typeparam name="TErr">The type of the Err value.</typeparam>
    /// <typeparam name="TOut">The type of the resulting output value.</typeparam>
    /// <returns>
    /// A <see cref="Task{TResult}" /> containing the transformed value if the
    /// <see cref="Result{TOk, TErr}" /> is Ok, or the default value if it is Err.
    /// </returns>
    public static ValueTask<TOut> MapOrAsync<TOk, TErr, TOut>(
        this Result<TOk, TErr> result,
        TOut @default,
        Func<TOk, ValueTask<TOut>> map)
        where TOk : notnull where TErr : notnull where TOut : notnull =>
        result.MatchAsync(map, _ => new ValueTask<TOut>(@default));

    /// <summary>
    /// Transforms the asynchronous <see cref="Result{TOk, TErr}" /> into a
    /// new value using the provided asynchronous mapping function if it is an Ok
    /// value. Returns a default value if it is an Err value.
    /// </summary>
    /// <param name="resultTask">
    /// The asynchronous <see cref="Result{TOk, TErr}" />
    /// instance to transform.
    /// </param>
    /// <param name="default">
    /// The default value that will be returned if the
    /// <see cref="Result{TOk, TErr}" /> is an Err value.
    /// </param>
    /// <param name="map">
    /// An asynchronous function to apply to the Ok value if the
    /// <see cref="Result{TOk, TErr}" /> is Ok.
    /// </param>
    /// <typeparam name="TOk">The type of the Ok value.</typeparam>
    /// <typeparam name="TErr">The type of the Err value.</typeparam>
    /// <typeparam name="TOut">The type of the resulting output value.</typeparam>
    /// <returns>
    /// A task containing the transformed value if the
    /// <see cref="Result{TOk, TErr}" /> is Ok, or the default value if it is Err.
    /// </returns>
    public static Task<TOut> MapOrAsync<TOk, TErr, TOut>(
        this Task<Result<TOk, TErr>> resultTask,
        TOut @default,
        Func<TOk, Task<TOut>> map)
        where TOk : notnull where TErr : notnull where TOut : notnull =>
        resultTask.MatchAsync(map, _ => Task.FromResult(@default));

    /// <summary>
    /// Transforms the <see cref="Result{TOk, TErr}" /> into a new value using
    /// the provided asynchronous mapping function if it is an Ok value. Returns a
    /// default value if it is an Err value.
    /// </summary>
    /// <param name="resultTask">
    /// The <see cref="Result{TOk, TErr}" /> instance to
    /// transform.
    /// </param>
    /// <param name="default">
    /// The default value that will be returned if the
    /// <see cref="Result{TOk, TErr}" /> is an Err value.
    /// </param>
    /// <param name="map">
    /// An asynchronous function to apply to the Ok value if the
    /// <see cref="Result{TOk, TErr}" /> is Ok.
    /// </param>
    /// <typeparam name="TOk">The type of the Ok value.</typeparam>
    /// <typeparam name="TErr">The type of the Err value.</typeparam>
    /// <typeparam name="TOut">The type of the resulting output value.</typeparam>
    /// <returns>
    /// A <see cref="Task{TResult}" /> or <see cref="ValueTask{TResult}" />
    /// containing the transformed value if the <see cref="Result{TOk, TErr}" /> is Ok,
    /// or the default value if it is Err.
    /// </returns>
    public static ValueTask<TOut> MapOrAsync<TOk, TErr, TOut>(
        this ValueTask<Result<TOk, TErr>> resultTask,
        TOut @default,
        Func<TOk, ValueTask<TOut>> map)
        where TOk : notnull where TErr : notnull where TOut : notnull =>
        resultTask.MatchAsync(map, _ => new ValueTask<TOut>(@default));
}
