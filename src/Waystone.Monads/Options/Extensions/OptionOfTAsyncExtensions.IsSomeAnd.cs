namespace Waystone.Monads.Options.Extensions;

using System;
using System.Threading.Tasks;

/// <summary>Async extensions for <see cref="Option{T}" /></summary>
public static partial class OptionOfTAsyncExtensions
{
    /// <summary>
    /// Determines whether the option contains a value that satisfies the
    /// provided predicate asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of the value contained in the option.</typeparam>
    /// <param name="option">The option to evaluate.</param>
    /// <param name="predicate">
    /// An asynchronous predicate to apply to the value
    /// contained in the option, if any.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result
    /// contains a boolean: - true if the option contains a value and the value
    /// satisfies the predicate. - false otherwise.
    /// </returns>
    public static Task<bool> IsSomeAnd<T>(
        this Option<T> option,
        Func<T, Task<bool>> predicate) where T : notnull =>
        option.Match(predicate, () => Task.FromResult(false));

    /// <summary>
    /// Asynchronously evaluates whether the option contains a value (is Some)
    /// and satisfies a given predicate.
    /// </summary>
    /// <typeparam name="T">The type of the value encapsulated in the option.</typeparam>
    /// <param name="optionTask">
    /// A task representing the asynchronous computation of
    /// the option.
    /// </param>
    /// <param name="predicate">
    /// An asynchronous function to evaluate on the value
    /// contained in the option if it is Some.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result is
    /// <c>true</c> if the option contains a value and the value satisfies the
    /// predicate; otherwise, <c>false</c>.
    /// </returns>
    public static Task<bool> IsSomeAnd<T>(
        this Task<Option<T>> optionTask,
        Func<T, Task<bool>> predicate) where T : notnull =>
        optionTask.Match(predicate, () => Task.FromResult(false));

    /// <summary>
    /// Determines whether the option contains a value that satisfies the
    /// provided predicate asynchronously, using a ValueTask.
    /// </summary>
    /// <typeparam name="T">The type of the value contained in the option.</typeparam>
    /// <param name="option">The option to evaluate.</param>
    /// <param name="predicate">
    /// An asynchronous predicate to apply to the value
    /// contained in the option, if any.
    /// </param>
    /// <returns>
    /// A ValueTask that represents the asynchronous operation. The task
    /// result contains a boolean: - true if the option contains a value and the value
    /// satisfies the predicate. - false otherwise.
    /// </returns>
    public static ValueTask<bool> IsSomeAnd<T>(
        this Option<T> option,
        Func<T, ValueTask<bool>> predicate) where T : notnull =>
        option.Match(predicate, () => new ValueTask<bool>(false));

    /// <summary>
    /// Determines asynchronously whether the option task contains a value
    /// that satisfies the provided asynchronous predicate.
    /// </summary>
    /// <typeparam name="T">The type of the value contained in the option.</typeparam>
    /// <param name="optionTask">The task representing the Option to evaluate.</param>
    /// <param name="predicate">
    /// An asynchronous predicate to apply to the value
    /// contained in the option, if any.
    /// </param>
    /// <returns>
    /// A value task that represents the asynchronous operation. The value
    /// task result contains a boolean: - true if the option contains a value and the
    /// value satisfies the predicate, or - false otherwise.
    /// </returns>
    public static ValueTask<bool> IsSomeAnd<T>(
        this ValueTask<Option<T>> optionTask,
        Func<T, ValueTask<bool>> predicate) where T : notnull
        => optionTask.Match(predicate, () => new ValueTask<bool>(false));
}
