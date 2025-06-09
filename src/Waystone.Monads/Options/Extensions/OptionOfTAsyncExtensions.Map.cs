namespace Waystone.Monads.Options.Extensions;

using System;
using System.Threading.Tasks;

public static partial class OptionOfTAsyncExtensions
{
    /// <summary>
    /// Maps the value of the option asynchronously using the provided mapping
    /// function. If the option is None, returns a None-typed option of the target
    /// type.
    /// </summary>
    /// <typeparam name="T1">The type of the initial option's value.</typeparam>
    /// <typeparam name="T2">The type of the resulting option's value.</typeparam>
    /// <param name="option">The option to be mapped.</param>
    /// <param name="map">
    /// The asynchronous mapping function to apply to the value of
    /// the option.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains an
    /// Option of type T2:
    /// <list type="bullet">
    /// <item>
    /// Some(T2) if the input option is Some(T1) and the mapping operation is
    /// successful.
    /// </item>
    /// <item>None if the input option is None.</item>
    /// </list>
    /// </returns>
    public static async Task<Option<T2>> MapAsync<T1, T2>(
        this Option<T1> option,
        Func<T1, Task<T2>> map) where T1 : notnull where T2 : notnull
    {
        if (option.IsNone) return Option.None<T2>();

        T2 result = await map(option.Unwrap()).ConfigureAwait(false);
        return Option.Some(result);
    }

    /// <summary>
    /// Maps the value of the option asynchronously using the provided mapping
    /// function that returns a ValueTask. If the option is None, returns a None-typed
    /// option of the target type.
    /// </summary>
    /// <typeparam name="T1">The type of the initial option's value.</typeparam>
    /// <typeparam name="T2">The type of the resulting option's value.</typeparam>
    /// <param name="option">The option to be mapped.</param>
    /// <param name="map">
    /// The asynchronous mapping function to apply to the value of
    /// the option, which returns a ValueTask.
    /// </param>
    /// <returns>
    /// A ValueTask that represents the asynchronous operation. The result contains an
    /// Option of type T2:
    /// <list type="bullet">
    /// <item>
    /// Some(T2) if the input option is Some(T1) and the mapping operation is
    /// successful.
    /// </item>
    /// <item>None if the input option is None.</item>
    /// </list>
    /// </returns>
    public static async ValueTask<Option<T2>> MapAsync<T1, T2>(
        this Option<T1> option,
        Func<T1, ValueTask<T2>> map) where T1 : notnull where T2 : notnull
    {
        if (option.IsNone) return Option.None<T2>();

        T2 result = await map(option.Unwrap()).ConfigureAwait(false);
        return Option.Some(result);
    }

    /// <summary>
    /// Maps the value of an asynchronously resolved option using the provided
    /// asynchronous mapping function. If the resolved option is None, returns a
    /// None-typed option of the target type.
    /// </summary>
    /// <typeparam name="T1">The type of the initial option's value.</typeparam>
    /// <typeparam name="T2">The type of the resulting option's value.</typeparam>
    /// <param name="optionTask">A task that resolves to the option to be mapped.</param>
    /// <param name="map">
    /// The asynchronous mapping function to apply to the value of
    /// the option.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains an
    /// Option of type T2:
    /// <list type="bullet">
    /// <item>
    /// Some(T2) if the input option is Some(T1) and the mapping operation is
    /// successful.
    /// </item>
    /// <item>None if the input option is None.</item>
    /// </list>
    /// </returns>
    public static async Task<Option<T2>> MapAsync<T1, T2>(
        this Task<Option<T1>> optionTask,
        Func<T1, Task<T2>> map) where T1 : notnull where T2 : notnull
    {
        Option<T1> option = await optionTask.ConfigureAwait(false);

        if (option.IsNone) return Option.None<T2>();

        T2 result = await map(option.Unwrap()).ConfigureAwait(false);
        return Option.Some(result);
    }

    /// <summary>
    /// Maps the value of the option asynchronously using the provided mapping
    /// function. If the option is None, returns a None-typed option of the target
    /// type.
    /// </summary>
    /// <typeparam name="T1">The type of the initial option's value.</typeparam>
    /// <typeparam name="T2">The type of the resulting option's value.</typeparam>
    /// <param name="optionTask">
    /// The asynchronous task that evaluates to an option of
    /// type T1.
    /// </param>
    /// <param name="map">
    /// The asynchronous mapping function to apply to the value of
    /// the option.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains an
    /// Option of type T2:
    /// <list type="bullet">
    /// <item>
    /// Some(T2) if the input option is Some(T1) and the mapping operation is
    /// successful.
    /// </item>
    /// <item>None if the input option is None.</item>
    /// </list>
    /// </returns>
    public static async ValueTask<Option<T2>> MapAsync<T1, T2>(
        this ValueTask<Option<T1>> optionTask,
        Func<T1, ValueTask<T2>> map) where T1 : notnull where T2 : notnull
    {
        Option<T1> option = await optionTask.ConfigureAwait(false);
        if (option.IsNone) return Option.None<T2>();

        T2 result = await map(option.Unwrap()).ConfigureAwait(false);
        return Option.Some(result);
    }
}
