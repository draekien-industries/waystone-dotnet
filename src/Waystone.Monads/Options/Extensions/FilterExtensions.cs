namespace Waystone.Monads.Options.Extensions;

using System;
using System.Threading.Tasks;

/// <summary>
/// Filters an <see cref="Option{T}" /> with an asynchronous predicate, and filters
/// an <see cref="Option{T}" /> that is still inside a <see cref="Task{TResult}" />
/// or <see cref="ValueTask{TResult}" />.
/// </summary>
public static class FilterExtensions
{
    extension<T>(Option<T> option) where T : notnull
    {
        /// <summary>
        /// Awaits <paramref name="predicate" /> against the contained value if the
        /// option is a <see cref="Some{T}" />, keeping the option when it passes.
        /// </summary>
        /// <param name="predicate">
        /// The asynchronous condition the contained value must satisfy.
        /// </param>
        /// <returns>
        /// A <see cref="ValueTask{TResult}" /> containing the original option if it
        /// was a <see cref="Some{T}" /> whose value satisfies
        /// <paramref name="predicate" />, otherwise <see cref="None{T}" />. A
        /// <see cref="None{T}" /> passes through unchanged and the predicate is not
        /// invoked.
        /// </returns>
        public async ValueTask<Option<T>>
            FilterAsync(Func<T, Task<bool>> predicate)
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
        /// Awaits the <see cref="Task{TResult}" />, then awaits
        /// <paramref name="predicate" /> against the contained value if the option is
        /// a <see cref="Some{T}" />, keeping the option when it passes.
        /// </summary>
        /// <param name="predicate">
        /// The asynchronous condition the contained value must satisfy.
        /// </param>
        /// <returns>
        /// A <see cref="ValueTask{TResult}" /> containing the original option if it
        /// was a <see cref="Some{T}" /> whose value satisfies
        /// <paramref name="predicate" />, otherwise <see cref="None{T}" />. A
        /// <see cref="None{T}" /> passes through unchanged and the predicate is not
        /// invoked.
        /// </returns>
        public async ValueTask<Option<T>>
            FilterAsync(Func<T, Task<bool>> predicate)
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            if (option.IsNone) return option;

            T some = option.Expect("Expected Some but found None.");

            return await predicate.Invoke(some).ConfigureAwait(false)
                ? option
                : Option.None<T>();
        }

        /// <summary>
        /// Awaits the <see cref="Task{TResult}" />, then tests the contained value
        /// with a synchronous <paramref name="predicate" /> if the option is a
        /// <see cref="Some{T}" />, keeping the option when it passes.
        /// </summary>
        /// <param name="predicate">
        /// The condition the contained value must satisfy.
        /// </param>
        /// <returns>
        /// A <see cref="ValueTask{TResult}" /> containing the original option if it
        /// was a <see cref="Some{T}" /> whose value satisfies
        /// <paramref name="predicate" />, otherwise <see cref="None{T}" />. A
        /// <see cref="None{T}" /> passes through unchanged and the predicate is not
        /// invoked.
        /// </returns>
        public async ValueTask<Option<T>>
            FilterAsync(Func<T, bool> predicate)
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            if (option.IsNone) return option;

            T some = option.Expect("Expected Some but found None.");

            return predicate.Invoke(some)
                ? option
                : Option.None<T>();
        }
    }

    extension<T>(ValueTask<Option<T>> optionTask) where T : notnull
    {
        /// <summary>
        /// Awaits the <see cref="ValueTask{TResult}" />, then awaits
        /// <paramref name="predicate" /> against the contained value if the option is
        /// a <see cref="Some{T}" />, keeping the option when it passes.
        /// </summary>
        /// <param name="predicate">
        /// The asynchronous condition the contained value must satisfy.
        /// </param>
        /// <returns>
        /// A <see cref="ValueTask{TResult}" /> containing the original option if it
        /// was a <see cref="Some{T}" /> whose value satisfies
        /// <paramref name="predicate" />, otherwise <see cref="None{T}" />. A
        /// <see cref="None{T}" /> passes through unchanged and the predicate is not
        /// invoked.
        /// </returns>
        public async ValueTask<Option<T>>
            FilterAsync(Func<T, Task<bool>> predicate)
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            if (option.IsNone) return option;

            T some = option.Expect("Expected Some but found None.");

            return await predicate.Invoke(some).ConfigureAwait(false)
                ? option
                : Option.None<T>();
        }

        /// <summary>
        /// Awaits the <see cref="ValueTask{TResult}" />, then tests the contained
        /// value with a synchronous <paramref name="predicate" /> if the option is a
        /// <see cref="Some{T}" />, keeping the option when it passes.
        /// </summary>
        /// <param name="predicate">
        /// The condition the contained value must satisfy.
        /// </param>
        /// <returns>
        /// A <see cref="ValueTask{TResult}" /> containing the original option if it
        /// was a <see cref="Some{T}" /> whose value satisfies
        /// <paramref name="predicate" />, otherwise <see cref="None{T}" />. A
        /// <see cref="None{T}" /> passes through unchanged and the predicate is not
        /// invoked.
        /// </returns>
        public async ValueTask<Option<T>>
            FilterAsync(Func<T, bool> predicate)
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            if (option.IsNone) return option;

            T some = option.Expect("Expected Some but found None.");

            return predicate.Invoke(some)
                ? option
                : Option.None<T>();
        }
    }
}
