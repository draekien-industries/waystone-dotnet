namespace Waystone.Monads.Options.Extensions;

using System;
using System.Threading.Tasks;

public static partial class OptionOfTAsyncExtensions
{
    /// <summary>
    /// Transforms the value contained in an <see cref="Option{T}" />
    /// asynchronously if it is present, or returns a specified default value if it is
    /// not present.
    /// </summary>
    /// <param name="option">The option to transform.</param>
    /// <param name="default">The default value to return if the option is empty.</param>
    /// <param name="map">An asynchronous function to apply to the contained value.</param>
    /// <typeparam name="T1">The type of the value contained in the option.</typeparam>
    /// <typeparam name="T2">The type of the result after transformation.</typeparam>
    /// <returns>
    /// A task representing the asynchronous operation, which produces either
    /// the result of the transformation or the default value.
    /// </returns>
    public static Task<T2> MapOrAsync<T1, T2>(
        this Option<T1> option,
        T2 @default,
        Func<T1, Task<T2>> map) where T1 : notnull =>
        option.MatchAsync(map, () => Task.FromResult(@default));

    /// <summary>
    /// Transforms the value contained in an <see cref="Option{T}" />
    /// asynchronously if it is present, or returns a specified default value if it is
    /// not present.
    /// </summary>
    /// <param name="option">The option to transform.</param>
    /// <param name="default">The default value to return if the option is empty.</param>
    /// <param name="map">An asynchronous function to apply to the contained value.</param>
    /// <typeparam name="T1">The type of the value contained in the option.</typeparam>
    /// <typeparam name="T2">The type of the result after transformation.</typeparam>
    /// <returns>
    /// A task representing the asynchronous operation, which produces either
    /// the result of the transformation or the default value.
    /// </returns>
    public static ValueTask<T2> MapOrAsync<T1, T2>(
        this Option<T1> option,
        T2 @default,
        Func<T1, ValueTask<T2>> map) where T1 : notnull =>
        option.MatchAsync(map, () => new ValueTask<T2>(@default));

    /// <summary>
    /// Transforms the value contained in a <see cref="Option{T}" />
    /// asynchronously if it is present, or returns a specified default value if it is
    /// not present.
    /// </summary>
    /// <param name="optionTask">A task producing the option to transform.</param>
    /// <param name="default">The default value to return if the option is empty.</param>
    /// <param name="map">An asynchronous function to apply to the contained value.</param>
    /// <typeparam name="T1">The type of the value contained in the option.</typeparam>
    /// <typeparam name="T2">The type of the result after transformation.</typeparam>
    /// <returns>
    /// A task representing the asynchronous operation, which produces either
    /// the result of the transformation or the default value.
    /// </returns>
    public static Task<T2> MapOrAsync<T1, T2>(
        this Task<Option<T1>> optionTask,
        T2 @default,
        Func<T1, Task<T2>> map) where T1 : notnull =>
        optionTask.MatchAsync(map, () => Task.FromResult(@default));

    /// <summary>
    /// Transforms the value contained in a <see cref="Option{T}" />
    /// asynchronously if it is present, or returns a specified default value if it is
    /// not present. Operates on a <see cref="ValueTask{T}" /> of
    /// <see cref="Option{T}" />.
    /// </summary>
    /// <param name="optionTask">
    /// The asynchronous operation representing the option to
    /// transform.
    /// </param>
    /// <param name="default">The default value to return if the option is empty.</param>
    /// <param name="map">An asynchronous function to apply to the contained value.</param>
    /// <typeparam name="T1">The type of the value contained in the option.</typeparam>
    /// <typeparam name="T2">The type of the result after transformation.</typeparam>
    /// <returns>
    /// A <see cref="ValueTask{T}" /> representing the asynchronous operation,
    /// which produces either the result of the transformation or the default value.
    /// </returns>
    public static ValueTask<T2> MapOrAsync<T1, T2>(
        this ValueTask<Option<T1>> optionTask,
        T2 @default,
        Func<T1, ValueTask<T2>> map) where T1 : notnull =>
        optionTask.MatchAsync(map, () => new ValueTask<T2>(@default));
}
