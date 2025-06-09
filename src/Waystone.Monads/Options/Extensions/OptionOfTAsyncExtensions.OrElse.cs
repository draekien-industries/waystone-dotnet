namespace Waystone.Monads.Options.Extensions;

using System;
using System.Threading.Tasks;

public static partial class OptionOfTAsyncExtensions
{
    /// <summary>
    /// Returns the current <see cref="Option{T}"/> instance if it is in the "Some" state;
    /// otherwise, evaluates the provided asynchronous function to produce an alternative <see cref="Option{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value contained in the option.</typeparam>
    /// <param name="option">The current option on which this method is called.</param>
    /// <param name="else">
    /// A function that asynchronously returns an alternative <see cref="Option{T}"/> if the current option is in the "None" state.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the original option
    /// if it is in the "Some" state, or the result of the <paramref name="else"/> function if the option is in the "None" state.
    /// </returns>
    public static async Task<Option<T>> OrElseAsync<T>(
        this Option<T> option,
        Func<Task<Option<T>>> @else) where T : notnull
    {
        if (option.IsSome) return option;
        return await @else().ConfigureAwait(false);
    }

    /// <summary>
    /// Returns the current <see cref="Option{T}"/> instance if it is in the "Some" state;
    /// otherwise, evaluates the provided asynchronous function to produce an alternative <see cref="Option{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value contained in the option.</typeparam>
    /// <param name="option">The current option on which this method is called.</param>
    /// <param name="else">
    /// A function that asynchronously returns an alternative <see cref="Option{T}"/> if the current option is in the "None" state.
    /// </param>
    /// <returns>
    /// A <see cref="ValueTask{TResult}"/> that represents the asynchronous operation.
    /// The result contains the original option if it is in the "Some" state, or the result of the <paramref name="else"/> function if the option is in the "None" state.
    /// </returns>
    public static async ValueTask<Option<T>> OrElseAsync<T>(
        this Option<T> option,
        Func<ValueTask<Option<T>>> @else) where T : notnull
    {
        if (option.IsSome) return option;
        return await @else().ConfigureAwait(false);
    }

    /// <summary>
    /// Returns the <see cref="Option{T}"/> contained in the completed task if it is in the "Some" state;
    /// otherwise, evaluates the provided asynchronous function to produce an alternative <see cref="Option{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value contained in the option.</typeparam>
    /// <param name="optionTask">A task that represents the current option being evaluated.</param>
    /// <param name="else">
    /// A function that asynchronously returns an alternative <see cref="Option{T}"/> if the completed option is in the "None" state.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the original option
    /// if it is in the "Some" state, or the result of the <paramref name="else"/> function if the option is in the "None" state.
    /// </returns>
    public static async Task<Option<T>> OrElseAsync<T>(
        this Task<Option<T>> optionTask,
        Func<Task<Option<T>>> @else) where T : notnull
    {
        Option<T> option = await optionTask.ConfigureAwait(false);
        return await option.OrElseAsync(@else).ConfigureAwait(false);
    }

    /// <summary>
    /// Returns the current <see cref="Option{T}"/> instance if it is in the "Some" state;
    /// otherwise, evaluates the provided asynchronous function to produce an alternative <see cref="Option{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value contained in the option.</typeparam>
    /// <param name="optionTask">An asynchronous task providing the current option to evaluate.</param>
    /// <param name="else">
    /// A function that asynchronously returns an alternative <see cref="Option{T}"/> if the current option is in the "None" state.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation. The result contains the original option
    /// if it is in the "Some" state, or the result of the <paramref name="else"/> function if the option is in the "None" state.
    /// </returns>
    public static async ValueTask<Option<T>> OrElseAsync<T>(
        this ValueTask<Option<T>> optionTask,
        Func<ValueTask<Option<T>>> @else) where T : notnull
    {
        Option<T> option = await optionTask.ConfigureAwait(false);
        return await option.OrElseAsync(@else).ConfigureAwait(false);
    }
}
