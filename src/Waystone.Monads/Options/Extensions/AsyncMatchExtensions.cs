namespace Waystone.Monads.Options.Extensions;

using System;
using System.Threading.Tasks;

/// <summary>
/// Provides extension methods for asynchronous pattern matching on monadic types.
/// </summary>
public static class AsyncMatchExtensions
{
    /// <summary>
    /// Provides extension methods for asynchronous pattern matching on instances of
    /// <see cref="Option{T}" />.
    /// </summary>
    extension<T>(Option<T> option) where T : notnull
    {
        /// <summary>
        /// Asynchronously performs a pattern match on the given <see cref="Option{T}" />
        /// instance and produces a result
        /// based on whether the option contains a value or is empty.
        /// </summary>
        /// <typeparam name="TOut">
        /// The type of the result produced by the match
        /// expressions.
        /// </typeparam>
        /// <param name="onSome">
        /// A function to execute if the <see cref="Option{T}" /> instance contains a
        /// value.
        /// The function receives the contained value as a parameter.
        /// </param>
        /// <param name="onNone">
        /// A function to execute if the <see cref="Option{T}" /> instance is empty.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation. When completed, the task
        /// contains the result of
        /// either the <paramref name="onSome" /> or <paramref name="onNone" /> function.
        /// </returns>
        public async Task<TOut> Match<TOut>(
            Func<T, Task<TOut>> onSome,
            Func<Task<TOut>> onNone) =>
            option.IsSome
                ? await onSome
                   .Invoke(option.Expect("Expected Some but found None."))
                   .ConfigureAwait(false)
                : await onNone.Invoke().ConfigureAwait(false);

        /// <summary>
        /// Asynchronously performs a pattern match on the given <see cref="Option{T}" />
        /// instance and executes the appropriate function based on whether the option
        /// contains
        /// a value or is empty.
        /// </summary>
        /// <param name="onSome">
        /// A function to execute if the <see cref="Option{T}" /> instance contains a
        /// value.
        /// The function receives the contained value as a parameter and returns a task.
        /// </param>
        /// <param name="onNone">
        /// A function to execute if the <see cref="Option{T}" /> instance is empty.
        /// The function returns a task.
        /// </param>
        public async Task Match(
            Func<T, Task> onSome,
            Func<Task> onNone)
        {
            if (option.IsSome)
            {
                await onSome
                   .Invoke(option.Expect("Expected Some but found None."))
                   .ConfigureAwait(false);
            }
            else
            {
                await onNone.Invoke().ConfigureAwait(false);
            }
        }
    }
}
