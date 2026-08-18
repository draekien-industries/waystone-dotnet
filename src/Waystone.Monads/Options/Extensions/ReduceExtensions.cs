namespace Waystone.Monads.Options.Extensions;

using System;
using System.Threading.Tasks;

public static class ReduceExtensions
{
    extension<T>(Option<T> option) where T : notnull
    {
        /// <summary>
        /// Merges the option with another option, awaiting
        /// <paramref name="reduce" /> when both are a <see cref="Some{T}" />.
        /// </summary>
        /// <param name="other">The option to merge with.</param>
        /// <param name="reduce">
        /// A function that asynchronously combines two present
        /// values.
        /// </param>
        /// <returns>
        /// A <see cref="ValueTask{TResult}" /> containing the combined option when both
        /// are a <see cref="Some{T}" />, whichever option is a <see cref="Some{T}" /> when
        /// only one of them is, and <see cref="None{T}" /> when neither is.
        /// </returns>
        public async ValueTask<Option<T>> ReduceAsync(
            Option<T> other,
            Func<T, T, Task<T>> reduce)
        {
            if (option.IsNone) return other;

            if (other.IsNone) return option;

            T some = option.Expect("Expected Some but found None.");
            T otherSome = other.Expect("Expected Some but found None.");

            return await reduce.Invoke(some, otherSome).ConfigureAwait(false);
        }
    }

    extension<T>(Task<Option<T>> optionTask) where T : notnull
    {
        /// <summary>
        /// Awaits the <see cref="Task{TResult}" /> and merges the option with another
        /// option.
        /// </summary>
        /// <param name="other">The option to merge with.</param>
        /// <param name="reduce">The function that combines two present values.</param>
        /// <returns>
        /// A <see cref="Task{TResult}" /> containing the combined option when both are a
        /// <see cref="Some{T}" />, whichever option is a <see cref="Some{T}" /> when only
        /// one of them is, and <see cref="None{T}" /> when neither is.
        /// </returns>
        public async Task<Option<T>> ReduceAsync(
            Option<T> other,
            Func<T, T, T> reduce)
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            return option.Reduce(other, reduce);
        }

        /// <summary>
        /// Awaits the <see cref="Task{TResult}" /> and merges the option with another
        /// option, awaiting <paramref name="reduce" /> when both are a
        /// <see cref="Some{T}" />.
        /// </summary>
        /// <param name="other">The option to merge with.</param>
        /// <param name="reduce">
        /// A function that asynchronously combines two present
        /// values.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}" /> containing the combined option when both are a
        /// <see cref="Some{T}" />, whichever option is a <see cref="Some{T}" /> when only
        /// one of them is, and <see cref="None{T}" /> when neither is.
        /// </returns>
        public async Task<Option<T>> ReduceAsync(
            Option<T> other,
            Func<T, T, Task<T>> reduce)
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            return await option.ReduceAsync(other, reduce)
               .ConfigureAwait(false);
        }
    }

    extension<T>(ValueTask<Option<T>> optionTask) where T : notnull
    {
        /// <summary>
        /// Awaits the <see cref="ValueTask{TResult}" /> and merges the option with
        /// another option.
        /// </summary>
        /// <param name="other">The option to merge with.</param>
        /// <param name="reduce">The function that combines two present values.</param>
        /// <returns>
        /// A <see cref="Task{TResult}" /> containing the combined option when both are a
        /// <see cref="Some{T}" />, whichever option is a <see cref="Some{T}" /> when only
        /// one of them is, and <see cref="None{T}" /> when neither is.
        /// </returns>
        public async Task<Option<T>> ReduceAsync(
            Option<T> other,
            Func<T, T, T> reduce)
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            return option.Reduce(other, reduce);
        }

        /// <summary>
        /// Awaits the <see cref="ValueTask{TResult}" /> and merges the option with
        /// another option, awaiting <paramref name="reduce" /> when both are a
        /// <see cref="Some{T}" />.
        /// </summary>
        /// <param name="other">The option to merge with.</param>
        /// <param name="reduce">
        /// A function that asynchronously combines two present
        /// values.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}" /> containing the combined option when both are a
        /// <see cref="Some{T}" />, whichever option is a <see cref="Some{T}" /> when only
        /// one of them is, and <see cref="None{T}" /> when neither is.
        /// </returns>
        public async Task<Option<T>> ReduceAsync(
            Option<T> other,
            Func<T, T, Task<T>> reduce)
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            return await option.ReduceAsync(other, reduce)
               .ConfigureAwait(false);
        }
    }
}
