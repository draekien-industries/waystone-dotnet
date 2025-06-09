namespace Waystone.Monads.Options.Extensions;

using System;
using System.Threading.Tasks;

public static partial class OptionOfTAsyncExtensions
{
    /// <summary>
    /// Asynchronously evaluates the <see cref="Option{T}" /> and maps its
    /// value using the provided functions.
    /// </summary>
    /// <typeparam name="T1">The type of the value encapsulated in the Option.</typeparam>
    /// <typeparam name="T2">The type of the result.</typeparam>
    /// <param name="option">The option to be evaluated.</param>
    /// <param name="else">
    /// A function that asynchronously provides a default value in
    /// case the option is none.
    /// </param>
    /// <param name="map">
    /// A function that asynchronously maps the value of the option
    /// if it is some.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation, containing the mapped
    /// value if the Option is some, or the default value if the Option is none.
    /// </returns>
    public static Task<T2> MapOrElseAsync<T1, T2>(
        this Option<T1> option,
        Func<Task<T2>> @else,
        Func<T1, Task<T2>> map) where T1 : notnull =>
        option.MatchAsync(map, @else);

    /// <summary>
    /// Asynchronously evaluates the <see cref="Option{T}" /> and maps its
    /// value using the provided functions, returning a default value or a mapped value
    /// based on the state of the option.
    /// </summary>
    /// <typeparam name="T1">The type of the value encapsulated in the Option.</typeparam>
    /// <typeparam name="T2">The type of the result.</typeparam>
    /// <param name="option">The option to be evaluated.</param>
    /// <param name="else">
    /// A function that asynchronously provides a default value in
    /// case the option is none.
    /// </param>
    /// <param name="map">
    /// A function that asynchronously maps the value of the option
    /// if it is some.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation, containing either the
    /// mapped value from <paramref name="map" /> if the Option is some, or the default
    /// value from <paramref name="else" /> if the Option is none.
    /// </returns>
    public static ValueTask<T2> MapOrElseAsync<T1, T2>(
        this Option<T1> option,
        Func<ValueTask<T2>> @else,
        Func<T1, ValueTask<T2>> map) where T1 : notnull =>
        option.MatchAsync(map, @else);

    /// <summary>
    /// Asynchronously evaluates a task representing an
    /// <see cref="Option{T}" /> and maps its value using the provided functions.
    /// </summary>
    /// <typeparam name="T1">The type of the value encapsulated in the Option.</typeparam>
    /// <typeparam name="T2">The type of the result.</typeparam>
    /// <param name="optionTask">A task representing the option to be evaluated.</param>
    /// <param name="else">
    /// A function that asynchronously provides a default value in
    /// case the option is none.
    /// </param>
    /// <param name="map">
    /// A function that asynchronously maps the value of the option
    /// if it is some.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation, containing the mapped
    /// value if the Option is some, or the default value if the Option is none.
    /// </returns>
    public static Task<T2> MapOrElseAsync<T1, T2>(
        this Task<Option<T1>> optionTask,
        Func<Task<T2>> @else,
        Func<T1, Task<T2>> map) where T1 : notnull =>
        optionTask.MatchAsync(map, @else);

    /// <summary>
    /// Asynchronously evaluates the <see cref="Option{T}" /> and maps its
    /// value using the provided functions.
    /// </summary>
    /// <typeparam name="T1">The type of the value encapsulated in the Option.</typeparam>
    /// <typeparam name="T2">The type of the result.</typeparam>
    /// <param name="optionTask">The option to be evaluated.</param>
    /// <param name="else">
    /// A function that asynchronously provides a default value in
    /// case the Option is none.
    /// </param>
    /// <param name="map">
    /// A function that asynchronously maps the value of the Option
    /// if it is some.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation, containing the mapped
    /// value if the Option is some, or the default value if the Option is none.
    /// </returns>
    public static ValueTask<T2> MapOrElseAsync<T1, T2>(
        this ValueTask<Option<T1>> optionTask,
        Func<ValueTask<T2>> @else,
        Func<T1, ValueTask<T2>> map) where T1 : notnull =>
        optionTask.MatchAsync(map, @else);
}
