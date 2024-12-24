namespace Waystone.Monads.Async;

using System;
using System.Threading.Tasks;

/// <summary>Extension methods for working with asynchronous options</summary>
public static class AsyncOptionExtensions
{
    /// <summary>
    /// Performs a <see langword="switch" /> on the async option, invoking the
    /// <paramref name="onSomeAsync" /> callback when it is a <see cref="Some{T}" />
    /// and the <paramref name="onNoneAsync" /> callback when it is a
    /// <see cref="None{T}" />
    /// </summary>
    /// <param name="optionTask">The async option</param>
    /// <param name="onSomeAsync">
    /// A callback for handling the <see cref="Some{T}" />
    /// case
    /// </param>
    /// <param name="onNoneAsync">
    /// A callback for handling the <see cref="None{T}" />
    /// case
    /// </param>
    /// <typeparam name="TIn">The async option value's type</typeparam>
    /// <typeparam name="TOut">The output type</typeparam>
    /// <returns>
    /// The output of either the <paramref name="onSomeAsync" /> or
    /// <paramref name="onNoneAsync" />
    /// </returns>
    public static async Task<TOut> MatchAsync<TIn, TOut>(
        this Task<Option<TIn>> optionTask,
        Func<TIn, Task<TOut>> onSomeAsync,
        Func<Task<TOut>> onNoneAsync) where TIn : notnull
    {
        Option<TIn> option = await optionTask.ConfigureAwait(false);
        return await option.Match(onSomeAsync, onNoneAsync);
    }

    /// <summary>
    /// Performs a <see langword="switch" /> on the async option, invoking the
    /// <paramref name="onSomeAsync" /> callback when it is a <see cref="Some{T}" />
    /// and the <paramref name="onNoneAsync" /> callback when it is a
    /// <see cref="None{T}" />
    /// </summary>
    /// <param name="optionTask">The async option</param>
    /// <param name="onSomeAsync">
    /// A callback for handling the <see cref="Some{T}" />
    /// case
    /// </param>
    /// <param name="onNoneAsync">
    /// A callback for handling the <see cref="None{T}" />
    /// case
    /// </param>
    /// <typeparam name="TIn">The async option value's type</typeparam>
    public static async Task MatchAsync<TIn>(
        this Task<Option<TIn>> optionTask,
        Func<TIn, Task> onSomeAsync,
        Func<Task> onNoneAsync) where TIn : notnull
    {
        Option<TIn> option = await optionTask.ConfigureAwait(false);
        await option.Match(onSomeAsync, onNoneAsync);
    }

    /// <summary>Unzips an option containing a tuple value into two options</summary>
    /// <param name="optionTask">The async option to be unzipped</param>
    /// <typeparam name="T1">The first option value's type</typeparam>
    /// <typeparam name="T2">The second option value's type</typeparam>
    /// <returns>
    /// If <paramref name="optionTask" /> is <c>Some&lt;(T1, T2)&gt;</c> this
    /// method returns <c>(Some&lt;T1&gt;, Some&lt;T2&gt;)</c>. Otherwise
    /// <c>(None&lt;T1&gt;, None&lt;T2&gt;)</c> is returned.
    /// </returns>
    public static Task<(Option<T1>, Option<T2>)> UnzipAsync<T1, T2>(
        this Task<Option<(T1, T2)>> optionTask)
        where T1 : notnull where T2 : notnull =>
        optionTask.MatchAsync(
            tuple => Task.FromResult(
                (Option.Some(tuple.Item1), Option.Some(tuple.Item2))),
            () => Task.FromResult((Option.None<T1>(), Option.None<T2>())));
}
