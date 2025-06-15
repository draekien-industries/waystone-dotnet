namespace Waystone.Monads.Options.Extensions;

using System;
using System.Threading.Tasks;

public static partial class OptionOfTAsyncExtensions
{
    /// <summary>
    /// Executes the provided asynchronous functions based on the state of the
    /// <see cref="Option{T}" />.
    /// </summary>
    /// <param name="option">
    /// The option whose state determines which function to
    /// execute.
    /// </param>
    /// <param name="onSome">
    /// The asynchronous function to invoke if the option is in
    /// the Some state.
    /// </param>
    /// <param name="onNone">
    /// The asynchronous function to invoke if the option is in
    /// the None state.
    /// </param>
    /// <typeparam name="TIn">The type of the value contained within the option.</typeparam>
    /// <returns>
    /// A task that represents the asynchronous operation and yields the
    /// result of the invoked function.
    /// </returns>
    public static Task MatchAsync<TIn>(
        this Option<TIn> option,
        Func<TIn, Task> onSome,
        Func<Task> onNone) where TIn : notnull => option.Match(onSome, onNone);

    /// <summary>
    /// Executes the provided asynchronous functions based on the state of the
    /// <see cref="Option{T}" /> retrieved from the task.
    /// </summary>
    /// <param name="optionTask">
    /// A task that resolves to an option whose state
    /// determines which function to execute.
    /// </param>
    /// <param name="onSome">
    /// The asynchronous function to invoke if the option is in
    /// the Some state.
    /// </param>
    /// <param name="onNone">
    /// The asynchronous function to invoke if the option is in
    /// the None state.
    /// </param>
    /// <typeparam name="TIn">The type of the value contained within the option.</typeparam>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static async Task MatchAsync<TIn>(
        this Task<Option<TIn>> optionTask,
        Func<TIn, Task> onSome,
        Func<Task> onNone) where TIn : notnull
    {
        Option<TIn> option = await optionTask.ConfigureAwait(false);
        await option.MatchAsync(onSome, onNone).ConfigureAwait(false);
    }

    /// <summary>
    /// Executes one of the provided functions based on the state of the
    /// <see cref="Option{T}" />.
    /// </summary>
    /// <param name="option">The option to evaluate.</param>
    /// <param name="onSome">The function to invoke if the option is in the Some state.</param>
    /// <param name="onNone">The function to invoke if the option is in the None state.</param>
    /// <typeparam name="TIn">The type of the value contained in the option.</typeparam>
    /// <typeparam name="TOut">The return type of the provided functions.</typeparam>
    /// <returns>
    /// A task that represents the asynchronous operation and returns the
    /// result of the invoked function.
    /// </returns>
    public static Task<TOut> MatchAsync<TIn, TOut>(
        this Option<TIn> option,
        Func<TIn, Task<TOut>> onSome,
        Func<Task<TOut>> onNone) where TIn : notnull =>
        option.Match(onSome, onNone);

    /// <summary>
    /// Executes one of the provided asynchronous functions based on the state
    /// of the <see cref="Option{T}" />.
    /// </summary>
    /// <param name="option">The option to evaluate.</param>
    /// <param name="onSome">The function to invoke if the option is in the Some state.</param>
    /// <param name="onNone">The function to invoke if the option is in the None state.</param>
    /// <typeparam name="TIn">The type of the value contained in the option.</typeparam>
    /// <typeparam name="TOut">The return type of the provided functions.</typeparam>
    /// <returns>
    /// A task that represents the asynchronous operation and returns the
    /// result of the invoked function.
    /// </returns>
    public static ValueTask<TOut> MatchAsync<TIn, TOut>(
        this Option<TIn> option,
        Func<TIn, ValueTask<TOut>> onSome,
        Func<ValueTask<TOut>> onNone) where TIn : notnull =>
        option.Match(onSome, onNone);

    /// <summary>
    /// Matches on the asynchronous <see cref="Option{T}" /> task and executes
    /// one of the provided functions depending on the option's state.
    /// </summary>
    /// <param name="optionTask">The task representing the option to evaluate.</param>
    /// <param name="onSome">The function to invoke if the option is in the Some state.</param>
    /// <param name="onNone">The function to invoke if the option is in the None state.</param>
    /// <typeparam name="TIn">The type of the value contained in the option.</typeparam>
    /// <typeparam name="TOut">The return type of the provided functions.</typeparam>
    /// <returns>
    /// A task that represents the asynchronous operation and returns the
    /// result of the invoked function.
    /// </returns>
    public static async Task<TOut> MatchAsync<TIn, TOut>(
        this Task<Option<TIn>> optionTask,
        Func<TIn, Task<TOut>> onSome,
        Func<Task<TOut>> onNone) where TIn : notnull
    {
        Option<TIn> option = await optionTask.ConfigureAwait(false);
        return await option.Match(onSome, onNone).ConfigureAwait(false);
    }

    /// <summary>
    /// Evaluates the <see cref="Option{T}" /> asynchronously and returns a
    /// result based on the provided functions for the Some and None states.
    /// </summary>
    /// <param name="optionTask">A value task representing the option to evaluate.</param>
    /// <param name="onSome">
    /// The asynchronous function to invoke if the option is in
    /// the Some state.
    /// </param>
    /// <param name="onNone">
    /// The asynchronous function to invoke if the option is in
    /// the None state.
    /// </param>
    /// <typeparam name="TIn">The type of the value contained in the option.</typeparam>
    /// <typeparam name="TOut">The return type of the provided functions.</typeparam>
    /// <returns>
    /// A value task that represents the asynchronous operation and returns
    /// the result of the invoked function.
    /// </returns>
    public static async ValueTask<TOut> MatchAsync<TIn, TOut>(
        this ValueTask<Option<TIn>> optionTask,
        Func<TIn, ValueTask<TOut>> onSome,
        Func<ValueTask<TOut>> onNone) where TIn : notnull
    {
        Option<TIn> option = await optionTask.ConfigureAwait(false);
        return await option.Match(onSome, onNone).ConfigureAwait(false);
    }
}
