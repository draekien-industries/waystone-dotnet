namespace Waystone.Monads.Options.Extensions;

using System;
using System.Threading.Tasks;

public static partial class OptionOfTAsyncExtensions
{
    /// <summary>
    /// Determines whether the option is None or satisfies the given predicate
    /// asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of the value contained in the option.</typeparam>
    /// <param name="option">The option to evaluate.</param>
    /// <param name="predicate">
    /// An asynchronous predicate function to evaluate if the
    /// option is Some.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation that returns true if
    /// the option is None or the predicate returns true; otherwise, false.
    /// </returns>
    public static Task<bool> IsNoneOr<T>(
        this Option<T> option,
        Func<T, Task<bool>> predicate) where T : notnull =>
        option.Match(predicate, () => Task.FromResult(true));

    /// <summary>
    /// Determines whether the option is None or satisfies the provided
    /// asynchronous predicate.
    /// </summary>
    /// <typeparam name="T">The type of the value contained in the option.</typeparam>
    /// <param name="option">The option to evaluate.</param>
    /// <param name="predicate">
    /// An asynchronous predicate function to evaluate if the
    /// option is Some.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation that returns true if
    /// the option is None or the predicate evaluates to true; otherwise, false.
    /// </returns>
    public static ValueTask<bool> IsNoneOr<T>(
        this Option<T> option,
        Func<T, ValueTask<bool>> predicate) where T : notnull =>
        option.Match(predicate, () => new ValueTask<bool>(true));

    /// <summary>
    /// Determines whether the asynchronous option is None or satisfies the
    /// specified asynchronous predicate.
    /// </summary>
    /// <typeparam name="T">The type of the value contained in the option.</typeparam>
    /// <param name="optionTask">A task representing the option to evaluate.</param>
    /// <param name="predicate">
    /// An asynchronous predicate function to evaluate if the
    /// option is Some.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation that returns true if
    /// the option is None or the predicate returns true; otherwise, false.
    /// </returns>
    public static Task<bool> IsNoneOr<T>(
        this Task<Option<T>> optionTask,
        Func<T, Task<bool>> predicate) where T : notnull =>
        optionTask.Match(predicate, () => Task.FromResult(true));

    /// <summary>
    /// Determines whether the asynchronous option is None or satisfies the
    /// given predicate asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of the value contained in the option.</typeparam>
    /// <param name="optionTask">The asynchronous option to evaluate.</param>
    /// <param name="predicate">
    /// An asynchronous predicate function to evaluate if the
    /// option is Some.
    /// </param>
    /// <returns>
    /// A value task representing the asynchronous operation that returns true
    /// if the option is None or the predicate returns true; otherwise, false.
    /// </returns>
    public static ValueTask<bool> IsNoneOr<T>(
        this ValueTask<Option<T>> optionTask,
        Func<T, ValueTask<bool>> predicate) where T : notnull =>
        optionTask.Match(predicate, () => new ValueTask<bool>(true));
}
