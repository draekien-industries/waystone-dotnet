namespace Waystone.Monads.Options.Extensions;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
#if !DEBUG
using System.Diagnostics;
#endif

[ExcludeFromCodeCoverage]
#if !DEBUG
[DebuggerStepThrough]
#endif
public static partial class OptionOfTAsyncExtensions
{
    /// <summary>Filters an asynchronous Option based on a predicate.</summary>
    /// <typeparam name="T">The type of the value contained in the Option.</typeparam>
    /// <param name="option">The Option to filter.</param>
    /// <param name="predicate">
    /// An asynchronous function that determines whether the
    /// value within the Option satisfies the condition.
    /// </param>
    /// <returns>
    /// An Option of type <typeparamref name="T" /> that contains the initial
    /// value if it satisfies the predicate, or an empty Option if it does not.
    /// </returns>
    public static async Task<Option<T>> FilterAsync<T>(
        this Option<T> option,
        Func<T, Task<bool>> predicate)
        where T : notnull =>
        await option.IsSomeAndAsync(predicate).ConfigureAwait(false)
            ? option
            : Option.None<T>();

    /// <summary>Filters an asynchronous Option based on a predicate.</summary>
    /// <typeparam name="T">The type of the value contained in the Option.</typeparam>
    /// <param name="option">The Option to filter.</param>
    /// <param name="predicate">
    /// An asynchronous function that determines whether the
    /// value within the Option satisfies the condition.
    /// </param>
    /// <returns>
    /// An Option of type <typeparamref name="T" /> that contains the initial
    /// value if it satisfies the predicate, or an empty Option if it does not.
    /// </returns>
    public static async ValueTask<Option<T>> FilterAsync<T>(
        this Option<T> option,
        Func<T, ValueTask<bool>> predicate)
        where T : notnull =>
        await option.IsSomeAndAsync(predicate).ConfigureAwait(false)
            ? option
            : Option.None<T>();

    /// <summary>
    /// Filters an asynchronous Option based on a given predicate that returns
    /// a Task.
    /// </summary>
    /// <typeparam name="T">The type of the value contained in the Option.</typeparam>
    /// <param name="optionTask">
    /// A task representing an Option of type
    /// <typeparamref name="T" /> to filter.
    /// </param>
    /// <param name="predicate">
    /// An asynchronous function that determines whether the
    /// value contained in the Option satisfies the specified condition.
    /// </param>
    /// <returns>
    /// A Task representing an Option of type <typeparamref name="T" /> that
    /// contains the initial value if it satisfies the predicate, or an empty Option if
    /// it does not.
    /// </returns>
    public static async Task<Option<T>> FilterAsync<T>(
        this Task<Option<T>> optionTask,
        Func<T, Task<bool>> predicate)
        where T : notnull
    {
        Option<T> option = await optionTask.ConfigureAwait(false);

        return await option.FilterAsync(predicate).ConfigureAwait(false);
    }

    /// <summary>Filters an asynchronous Option based on a predicate.</summary>
    /// <typeparam name="T">The type of the value contained in the Option.</typeparam>
    /// <param name="optionTask">The Option to filter.</param>
    /// <param name="predicate">
    /// A function that evaluates asynchronously whether the
    /// value within the Option satisfies the condition.
    /// </param>
    /// <returns>
    /// An Option of type <typeparamref name="T" /> that contains the initial
    /// value if it satisfies the predicate, or an empty Option if it does not.
    /// </returns>
    public static async ValueTask<Option<T>> FilterAsync<T>(
        this ValueTask<Option<T>> optionTask,
        Func<T, ValueTask<bool>> predicate)
        where T : notnull
    {
        Option<T> option = await optionTask.ConfigureAwait(false);

        return await option.FilterAsync(predicate).ConfigureAwait(false);
    }
}
