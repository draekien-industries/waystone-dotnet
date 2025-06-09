namespace Waystone.Monads.Options.Extensions;

using System;
using System.Threading.Tasks;

public static partial class OptionOfTAsyncExtensions
{
    /// <summary>
    /// Unwraps the value of the option if it is "Some", otherwise executes
    /// the provided fallback function to obtain a default value of the same type.
    /// </summary>
    /// <param name="option">
    /// The option instance to unwrap or provide a fallback value
    /// for.
    /// </param>
    /// <param name="else">
    /// A delegate that produces a fallback value asynchronously
    /// when the option is "None".
    /// </param>
    /// <typeparam name="T">The type of the value contained within the option.</typeparam>
    /// <returns>
    /// A task representing the asynchronous operation, returning the
    /// unwrapped value if the option is "Some", or a fallback value obtained from the
    /// provided delegate if the option is "None".
    /// </returns>
    public static Task<T> UnwrapOrElseAsync<T>(
        this Option<T> option,
        Func<Task<T>> @else) where T : notnull =>
        option.Match(Task.FromResult, @else);

    /// <summary>
    /// Unwraps the value of the option if it is "Some", otherwise executes
    /// the provided fallback function asynchronously to obtain a default value of the
    /// same type.
    /// </summary>
    /// <param name="option">
    /// The option instance to unwrap or provide a fallback value
    /// for.
    /// </param>
    /// <param name="else">
    /// A delegate that produces a fallback value when the option is
    /// "None".
    /// </param>
    /// <typeparam name="T">The type of the value contained within the option.</typeparam>
    /// <returns>
    /// A <see cref="ValueTask{TResult}" /> representing the asynchronous
    /// operation, returning the unwrapped value if the option is "Some", or a fallback
    /// value obtained from the provided delegate if the option is "None".
    /// </returns>
    public static ValueTask<T> UnwrapOrElseAsync<T>(
        this Option<T> option,
        Func<ValueTask<T>> @else) where T : notnull =>
        option.Match(value => new ValueTask<T>(value), @else);

    /// <summary>
    /// Asynchronously unwraps the value of an <see cref="Option{T}" /> if it
    /// is "Some", or executes the provided fallback function to produce a default
    /// value when the option is "None".
    /// </summary>
    /// <param name="optionTask">
    /// A task representing the asynchronous computation of an
    /// <see cref="Option{T}" />.
    /// </param>
    /// <param name="else">
    /// A delegate that asynchronously produces a default value when
    /// the option is "None".
    /// </param>
    /// <typeparam name="T">The type of the value contained within the option.</typeparam>
    /// <returns>
    /// A task representing the asynchronous operation, returning the
    /// unwrapped value if the option is "Some", or a default value produced by the
    /// provided delegate if the option is "None".
    /// </returns>
    public static async Task<T> UnwrapOrElseAsync<T>(
        this Task<Option<T>> optionTask,
        Func<Task<T>> @else) where T : notnull
    {
        Option<T> option = await optionTask.ConfigureAwait(false);
        return await option.UnwrapOrElseAsync(@else).ConfigureAwait(false);
    }

    /// <summary>
    /// Unwraps the value of the option if it is "Some", otherwise executes
    /// the provided fallback function asynchronously to obtain a default value of the
    /// same type.
    /// </summary>
    /// <param name="optionTask">
    /// The option instance to unwrap or provide a fallback
    /// value for.
    /// </param>
    /// <param name="else">
    /// A delegate that produces a fallback value asynchronously
    /// when the option is "None".
    /// </param>
    /// <typeparam name="T">The type of the value contained within the option.</typeparam>
    /// <returns>
    /// A task representing the asynchronous operation, returning the
    /// unwrapped value if the option is "Some", or a fallback value obtained from the
    /// provided delegate if the option is "None".
    /// </returns>
    public static async ValueTask<T> UnwrapOrElseAsync<T>(
        this ValueTask<Option<T>> optionTask,
        Func<ValueTask<T>> @else) where T : notnull
    {
        Option<T> option = await optionTask.ConfigureAwait(false);
        return await option.UnwrapOrElseAsync(@else).ConfigureAwait(false);
    }
}
