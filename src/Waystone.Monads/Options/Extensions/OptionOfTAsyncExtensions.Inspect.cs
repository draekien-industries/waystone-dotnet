namespace Waystone.Monads.Options.Extensions;

using System;
using System.Threading.Tasks;

public static partial class OptionOfTAsyncExtensions
{
    /// <summary>
    /// Executes a specified asynchronous action if the option contains a
    /// value.
    /// </summary>
    /// <typeparam name="T">The type of the value contained in the option.</typeparam>
    /// <param name="option">The option to inspect.</param>
    /// <param name="action">
    /// The asynchronous action to execute if the option contains
    /// a value.
    /// </param>
    /// <returns>The original option after executing the action (if applicable).</returns>
    public static async Task<Option<T>> InspectAsync<T>(
        this Option<T> option,
        Func<T, Task> action) where T : notnull
    {
        if (option.IsSome)
        {
            await action(option.Unwrap()).ConfigureAwait(false);
        }

        return option;
    }

    /// <summary>
    /// Executes the specified asynchronous action if the option contains a
    /// value.
    /// </summary>
    /// <typeparam name="T">The type of the value contained in the option.</typeparam>
    /// <param name="option">The option to inspect.</param>
    /// <param name="action">
    /// The asynchronous action to execute if the option contains
    /// a value.
    /// </param>
    /// <returns>
    /// A task containing the original option after executing the action (if
    /// applicable).
    /// </returns>
    public static async ValueTask<Option<T>> InspectAsync<T>(
        this Option<T> option,
        Func<T, ValueTask> action) where T : notnull
    {
        if (option.IsSome)
        {
            await action(option.Unwrap()).ConfigureAwait(false);
        }

        return option;
    }

    /// <summary>
    /// Executes a specified asynchronous action if the option contains a
    /// value.
    /// </summary>
    /// <typeparam name="T">The type of the value contained in the option.</typeparam>
    /// <param name="optionTask">The task that represents the option to inspect.</param>
    /// <param name="action">
    /// The asynchronous action to execute if the option contains
    /// a value.
    /// </param>
    /// <returns>
    /// A task representing the original option after executing the action (if
    /// applicable).
    /// </returns>
    public static async Task<Option<T>> InspectAsync<T>(
        this Task<Option<T>> optionTask,
        Func<T, Task> action) where T : notnull
    {
        Option<T> option = await optionTask.ConfigureAwait(false);
        return await option.InspectAsync(action).ConfigureAwait(false);
    }

    /// <summary>
    /// Executes a specified asynchronous action if the option contains a
    /// value.
    /// </summary>
    /// <typeparam name="T">The type of the value contained in the option.</typeparam>
    /// <param name="optionTask">The option to inspect.</param>
    /// <param name="action">
    /// The asynchronous action to execute if the option contains
    /// a value.
    /// </param>
    /// <returns>The original option after executing the action (if applicable).</returns>
    public static async ValueTask<Option<T>> InspectAsync<T>(
        this ValueTask<Option<T>> optionTask,
        Func<T, ValueTask> action) where T : notnull
    {
        Option<T> option = await optionTask.ConfigureAwait(false);
        return await option.InspectAsync(action).ConfigureAwait(false);
    }
}
