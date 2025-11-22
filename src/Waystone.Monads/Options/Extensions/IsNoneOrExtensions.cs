namespace Waystone.Monads.Options.Extensions;

using System;
using System.Threading.Tasks;

public static class IsNoneOrExtensions
{
    extension<T>(Option<T> option) where T : notnull
    {
        /// <summary>
        /// Evaluates whether the current <see cref="Option{T}" /> instance is in a "None"
        /// state or
        /// satisfies the provided asynchronous predicate if it is in a "Some" state.
        /// </summary>
        /// <param name="predicate">
        /// An asynchronous function to test the value contained in the
        /// <see cref="Option{T}" /> if it is in a "Some" state.
        /// </param>
        /// <returns>
        /// A <see cref="ValueTask{TResult}" /> containing <see langword="true" /> if the
        /// <see cref="Option{T}" /> is in a "None" state,
        /// or the predicate evaluates to <see langword="true" /> for the contained value
        /// if it is in a "Some" state; otherwise, <see langword="false" />.
        /// </returns>
        public async ValueTask<bool> IsNoneOr(Func<T, Task<bool>> predicate)
        {
            if (option.IsNone) return true;

            T some = option.Expect("Expected Some but found None.");

            return await predicate.Invoke(some).ConfigureAwait(false);
        }
    }

    extension<T>(Task<Option<T>> optionTask) where T : notnull
    {
        /// <summary>
        /// Evaluates whether the current <see cref="Option{T}" /> instance is in a "None"
        /// state or does not satisfy the provided predicate when in a "Some" state.
        /// </summary>
        /// <param name="predicate">
        /// A function to test the value contained in the <see cref="Option{T}" />
        /// if it is in a "Some" state.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}" /> containing <see langword="true" /> if the
        /// <see cref="Option{T}" /> is in a "None" state or if in a "Some" state
        /// and the predicate evaluates to <see langword="false" /> for the contained
        /// value;
        /// otherwise, <see langword="false" />.
        /// </returns>
        public async Task<bool> IsNoneOr(Func<T, bool> predicate)
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            return option.IsNoneOr(predicate);
        }

        /// <summary>
        /// Evaluates whether the current <see cref="Option{T}" /> instance is in a "None"
        /// state or satisfies the provided asynchronous predicate.
        /// </summary>
        /// <param name="predicate">
        /// An asynchronous function to test the value contained in the
        /// <see cref="Option{T}" /> if it is in a "Some" state.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}" /> containing <see langword="true" /> if the
        /// <see cref="Option{T}" /> is in a "None" state or the predicate evaluates to
        /// <see langword="true" /> for the contained value; otherwise,
        /// <see langword="false" />.
        /// </returns>
        public async Task<bool> IsNoneOr(Func<T, Task<bool>> predicate)
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            return await option.IsNoneOr(predicate).ConfigureAwait(false);
        }
    }

    extension<T>(ValueTask<Option<T>> optionTask) where T : notnull
    {
        /// <summary>
        /// Evaluates whether the current <see cref="Option{T}" /> instance is in a "None"
        /// state or satisfies the provided asynchronous predicate when in a "Some" state.
        /// </summary>
        /// <param name="predicate">
        /// An asynchronous function to evaluate the value contained in the
        /// <see cref="Option{T}" /> if it is in a "Some" state.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}" /> containing <see langword="true" /> if the
        /// <see cref="Option{T}" /> is in a "None" state, or if it is in a "Some" state
        /// and the predicate evaluates to <see langword="true" /> for the contained value;
        /// otherwise, <see langword="false" />.
        /// </returns>
        public async Task<bool> IsNoneOr(Func<T, Task<bool>> predicate)
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            return await option.IsNoneOr(predicate).ConfigureAwait(false);
        }

        /// <summary>
        /// Evaluates whether the current <see cref="Option{T}" /> instance is in a "None"
        /// state or satisfies the provided predicate.
        /// </summary>
        /// <param name="predicate">
        /// A function to test the value contained in the <see cref="Option{T}" />
        /// if it is in a "Some" state.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}" /> containing <see langword="true" /> if the
        /// <see cref="Option{T}" /> is in a "None" state or the predicate evaluates to
        /// <see langword="true" /> for the contained value; otherwise,
        /// <see langword="false" />.
        /// </returns>
        public async Task<bool> IsNoneOr(Func<T, bool> predicate)
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            return option.IsNoneOr(predicate);
        }
    }
}
