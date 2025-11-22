namespace Waystone.Monads.Options.Extensions;

using System;
using System.Threading.Tasks;

/// <summary>
/// Provides a set of extension methods for filtering and processing asynchronous
/// operations.
/// </summary>
public static class AsyncFilterExtensions
{
    extension<T>(Option<T> option) where T : notnull
    {
        /// <summary>
        /// Filters the current <see cref="Option{T}" /> instance based on the
        /// provided asynchronous predicate.
        /// </summary>
        /// <param name="predicate">
        /// An asynchronous function that determines whether the
        /// value contained in the <see cref="Option{T}" /> satisfies the condition.
        /// </param>
        /// <returns>
        /// A <see cref="ValueTask{TResult}" /> containing an <see cref="Option{T}" /> of
        /// type <typeparamref name="T" /> that contains the initial value if it satisfies
        /// the predicate, or an empty <see cref="Option{T}" /> if it does not.
        /// </returns>
        public async ValueTask<Option<T>>
            Filter(Func<T, Task<bool>> predicate)
        {
            if (option.IsNone) return option;

            T some = option.Expect("Expected Some but found None.");

            return await predicate.Invoke(some).ConfigureAwait(false)
                ? option
                : Option.None<T>();
        }
    }

    extension<T>(Task<Option<T>> optionTask) where T : notnull
    {
        /// <summary>
        /// Filters the current asynchronous <see cref="Option{T}" /> instance based on
        /// the provided asynchronous predicate.
        /// </summary>
        /// <param name="predicate">
        /// An asynchronous function that determines whether the
        /// value contained in the <see cref="Option{T}" /> satisfies the condition.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}" /> containing an <see cref="Option{T}" /> of type
        /// <typeparamref name="T" /> that contains the initial value if it satisfies the
        /// predicate, or an empty <see cref="Option{T}" /> if it does not.
        /// </returns>
        public async Task<Option<T>>
            Filter(Func<T, Task<bool>> predicate)
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            if (option.IsNone) return option;

            T some = option.Expect("Expected Some but found None.");

            return await predicate.Invoke(some).ConfigureAwait(false)
                ? option
                : Option.None<T>();
        }
    }

    extension<T>(ValueTask<Option<T>> optionTask) where T : notnull
    {
        /// <summary>
        /// Filters the current <see cref="Option{T}" /> instance represented by
        /// the <see cref="ValueTask{TResult}" />, based on the provided asynchronous
        /// predicate.
        /// </summary>
        /// <param name="predicate">
        /// An asynchronous function that determines whether the value contained
        /// in the <see cref="Option{T}" /> satisfies the condition.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}" /> containing an <see cref="Option{T}" /> of type
        /// <typeparamref name="T" /> that contains the initial value if it satisfies the
        /// predicate,
        /// or an empty <see cref="Option{T}" /> if it does not.
        /// </returns>
        public async Task<Option<T>>
            Filter(Func<T, Task<bool>> predicate)
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            if (option.IsNone) return option;

            T some = option.Expect("Expected Some but found None.");

            return await predicate.Invoke(some).ConfigureAwait(false)
                ? option
                : Option.None<T>();
        }
    }
}
