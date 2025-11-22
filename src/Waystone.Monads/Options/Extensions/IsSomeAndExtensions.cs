namespace Waystone.Monads.Options.Extensions;

using System;
using System.Threading.Tasks;

/// <summary>
/// Provides extension methods for handling asynchronous control flow with
/// instances of <see cref="Option{T}" />.
/// </summary>
public static class IsSomeAndExtensions
{
    extension<T>(Option<T> option) where T : notnull
    {
        /// <summary>
        /// Evaluates whether the current <see cref="Option{T}" /> instance is in a "Some"
        /// state and satisfies the provided asynchronous predicate.
        /// </summary>
        /// <param name="predicate">
        /// An asynchronous function to test the value contained in the
        /// <see cref="Option{T}" /> if it is in a "Some" state.
        /// </param>
        /// <returns>
        /// A <see cref="ValueTask{TResult}" /> containing <see langword="true" /> if the
        /// <see cref="Option{T}" /> is in a "Some" state
        /// and the predicate evaluates to <see langword="true" /> for the contained value;
        /// otherwise, <see langword="false" />.
        /// </returns>
        public async ValueTask<bool> IsSomeAnd(Func<T, Task<bool>> predicate)
        {
            if (option.IsNone) return false;

            T some = option.Expect("Expected Some but found None.");

            return await predicate.Invoke(some).ConfigureAwait(false);
        }
    }

    extension<T>(Task<Option<T>> optionTask) where T : notnull
    {
        /// <summary>
        /// Asynchronously evaluates whether the <see cref="Option{T}" /> produced by the
        /// task is in a "Some" state and satisfies the provided asynchronous predicate.
        /// </summary>
        /// <param name="predicate">
        /// An asynchronous function to test the value contained in the
        /// <see cref="Option{T}" /> if it is in a "Some" state.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}" /> containing <see langword="true" /> if the
        /// <see cref="Option{T}" /> is in a "Some" state
        /// and the predicate evaluates to <see langword="true" /> for the contained value;
        /// otherwise, <see langword="false" />.
        /// </returns>
        public async Task<bool> IsSomeAnd(Func<T, Task<bool>> predicate)
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            return await option.IsSomeAnd(predicate).ConfigureAwait(false);
        }

        /// <summary>
        /// Evaluates whether the current <see cref="Option{T}" /> instance, awaited from
        /// the <see cref="Task{TResult}" />, is in a "Some" state and satisfies the
        /// provided predicate.
        /// </summary>
        /// <param name="predicate">
        /// A function to test the value contained in the <see cref="Option{T}" /> if it is
        /// in a "Some" state.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}" /> containing <see langword="true" /> if the
        /// <see cref="Option{T}" /> is in a "Some" state and the predicate evaluates to
        /// <see langword="true" /> for the contained value; otherwise,
        /// <see langword="false" />.
        /// </returns>
        public async Task<bool> IsSomeAnd(Func<T, bool> predicate)
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            return option.IsSomeAnd(predicate);
        }
    }

    extension<T>(ValueTask<Option<T>> optionValueTask) where T : notnull
    {
        /// <summary>
        /// Asynchronously evaluates whether the <see cref="Option{T}" /> produced by the
        /// ValueTask is in a "Some" state and satisfies the provided asynchronous
        /// predicate.
        /// </summary>
        /// <param name="predicate">
        /// An asynchronous function to test the value contained in the
        /// <see cref="Option{T}" /> if it is in a "Some" state.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}" /> containing <see langword="true" /> if the
        /// <see cref="Option{T}" /> is in a "Some" state
        /// and the predicate evaluates to <see langword="true" /> for the contained value;
        /// otherwise, <see langword="false" />.
        /// </returns>
        public async Task<bool> IsSomeAnd(Func<T, Task<bool>> predicate)
        {
            Option<T> option = await optionValueTask.ConfigureAwait(false);

            return await option.IsSomeAnd(predicate).ConfigureAwait(false);
        }

        /// <summary>
        /// Evaluates whether the current <see cref="Option{T}" /> instance, obtained
        /// from the asynchronous <see cref="ValueTask{T}" />, is in a "Some" state
        /// and satisfies the provided predicate.
        /// </summary>
        /// <param name="predicate">
        /// A function to test the value contained in the <see cref="Option{T}" />
        /// if it is in a "Some" state.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}" /> containing <see langword="true" /> if the
        /// <see cref="Option{T}" /> is in a "Some" state and the predicate evaluates
        /// to <see langword="true" /> for the contained value; otherwise,
        /// <see langword="false" />.
        /// </returns>
        public async Task<bool> IsSomeAnd(Func<T, bool> predicate)
        {
            Option<T> option = await optionValueTask.ConfigureAwait(false);

            return option.IsSomeAnd(predicate);
        }
    }
}
