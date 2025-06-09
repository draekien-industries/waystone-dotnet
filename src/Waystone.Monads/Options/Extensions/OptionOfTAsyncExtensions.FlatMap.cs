namespace Waystone.Monads.Options.Extensions;

using System;
using System.Threading.Tasks;

public static partial class OptionOfTAsyncExtensions
{
    /// <summary>
    /// Transforms the current option asynchronously using the provided
    /// mapping function and flattens the result.
    /// </summary>
    /// <param name="option">The option to transform.</param>
    /// <param name="map">
    /// A function that takes the value inside the option and returns
    /// a task containing a nested option.
    /// </param>
    /// <typeparam name="T1">The type of the value in the current option.</typeparam>
    /// <typeparam name="T2">The type of the value in the resulting option.</typeparam>
    /// <returns>A task containing the transformed and flattened option.</returns>
    public static Task<Option<T2>> FlatMapAsync<T1, T2>(
        this Option<T1> option,
        Func<T1, Task<Option<T2>>> map) where T1 : notnull where T2 : notnull =>
        option.MapAsync(map).FlattenAsync();

    /// <summary>
    /// Transforms the current option asynchronously using the provided
    /// mapping function and flattens the result.
    /// </summary>
    /// <param name="option">The option to transform.</param>
    /// <param name="map">
    /// A function that takes the value inside the option and returns
    /// a task containing a nested option.
    /// </param>
    /// <typeparam name="T1">The type of the value in the current option.</typeparam>
    /// <typeparam name="T2">The type of the value in the resulting option.</typeparam>
    /// <returns>A task containing the transformed and flattened option.</returns>
    public static ValueTask<Option<T2>> FlatMapAsync<T1, T2>(
        this Option<T1> option,
        Func<T1, ValueTask<Option<T2>>> map)
        where T1 : notnull where T2 : notnull =>
        option.MapAsync(map).FlattenAsync();

    /// <summary>
    /// Transforms the current task containing an option asynchronously using
    /// the provided mapping function and flattens the result.
    /// </summary>
    /// <param name="optionTask">The task containing the option to transform.</param>
    /// <param name="map">
    /// A function that takes the value inside the option and returns
    /// a task containing a nested option.
    /// </param>
    /// <typeparam name="T1">The type of the value in the initial option.</typeparam>
    /// <typeparam name="T2">The type of the value in the resulting option.</typeparam>
    /// <returns>A task containing the transformed and flattened option.</returns>
    public static async Task<Option<T2>> FlatMapAsync<T1, T2>(
        this Task<Option<T1>> optionTask,
        Func<T1, Task<Option<T2>>> map) where T1 : notnull where T2 : notnull
    {
        Option<T1> option = await optionTask.ConfigureAwait(false);
        return await option.FlatMapAsync(map).ConfigureAwait(false);
    }

    /// <summary>
    /// Transforms the current option asynchronously using the provided
    /// mapping function and flattens the result.
    /// </summary>
    /// <param name="optionTask">The option to transform.</param>
    /// <param name="map">
    /// A function that takes the value inside the option and returns
    /// a task containing a nested option.
    /// </param>
    /// <typeparam name="T1">The type of the value in the current option.</typeparam>
    /// <typeparam name="T2">The type of the value in the resulting option.</typeparam>
    /// <returns>A task containing the transformed and flattened option.</returns>
    public static async ValueTask<Option<T2>> FlatMapAsync<T1, T2>(
        this ValueTask<Option<T1>> optionTask,
        Func<T1, ValueTask<Option<T2>>> map)
        where T1 : notnull where T2 : notnull
    {
        Option<T1> option = await optionTask.ConfigureAwait(false);
        return await option.FlatMapAsync(map).ConfigureAwait(false);
    }
}
