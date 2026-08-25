namespace Waystone.Monads.Options.Extensions;

using System;
using System.Threading.Tasks;

/// <summary>
/// Provides <c>IsSomeAndAsync</c> overloads for testing an
/// <see cref="Option{T}" /> with an asynchronous predicate, a receiver still
/// inside a task, or both.
/// </summary>
public static class IsSomeAndExtensions
{
    extension<T>(Option<T> option) where T : notnull
    {
        /// <summary>
        /// Checks whether an <see cref="Option{T}" /> is a <see cref="Some{T}" />
        /// whose value satisfies an asynchronous predicate.
        /// </summary>
        /// <remarks>
        /// The predicate is not invoked when the option is a
        /// <see cref="None{T}" />.
        /// </remarks>
        /// <param name="predicate">
        /// The asynchronous condition to evaluate against the contained value.
        /// </param>
        /// <returns>
        /// True if the option is a <see cref="Some{T}" /> and the predicate returns
        /// true; false otherwise.
        /// </returns>
        public async ValueTask<bool> IsSomeAndAsync(
            Func<T, Task<bool>> predicate)
        {
            if (option.IsNone) return false;

            T some = option.Expect("Expected Some but found None.");

            return await predicate.Invoke(some).ConfigureAwait(false);
        }
    }

    extension<T>(Task<Option<T>> optionTask) where T : notnull
    {
        /// <summary>
        /// Awaits a task of <see cref="Option{T}" /> and checks whether it is a
        /// <see cref="Some{T}" /> whose value satisfies an asynchronous predicate.
        /// </summary>
        /// <remarks>
        /// The predicate is not invoked when the option is a
        /// <see cref="None{T}" />.
        /// </remarks>
        /// <param name="predicate">
        /// The asynchronous condition to evaluate against the contained value.
        /// </param>
        /// <returns>
        /// True if the option is a <see cref="Some{T}" /> and the predicate returns
        /// true; false otherwise.
        /// </returns>
        public async ValueTask<bool> IsSomeAndAsync(Func<T, Task<bool>> predicate)
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            return await option.IsSomeAndAsync(predicate).ConfigureAwait(false);
        }

        /// <summary>
        /// Awaits a task of <see cref="Option{T}" /> and checks whether it is a
        /// <see cref="Some{T}" /> whose value satisfies a synchronous predicate.
        /// </summary>
        /// <remarks>
        /// The predicate is not invoked when the option is a
        /// <see cref="None{T}" />. Only the receiver is awaited, so pick this
        /// overload when the test itself does no asynchronous work.
        /// </remarks>
        /// <param name="predicate">
        /// The condition to evaluate against the contained value.
        /// </param>
        /// <returns>
        /// True if the option is a <see cref="Some{T}" /> and the predicate returns
        /// true; false otherwise.
        /// </returns>
        public async ValueTask<bool> IsSomeAndAsync(Func<T, bool> predicate)
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            return option.IsSomeAnd(predicate);
        }
    }

    extension<T>(ValueTask<Option<T>> optionValueTask) where T : notnull
    {
        /// <summary>
        /// Awaits a value task of <see cref="Option{T}" /> and checks whether it is
        /// a <see cref="Some{T}" /> whose value satisfies an asynchronous predicate.
        /// </summary>
        /// <remarks>
        /// The predicate is not invoked when the option is a
        /// <see cref="None{T}" />.
        /// </remarks>
        /// <param name="predicate">
        /// The asynchronous condition to evaluate against the contained value.
        /// </param>
        /// <returns>
        /// True if the option is a <see cref="Some{T}" /> and the predicate returns
        /// true; false otherwise.
        /// </returns>
        public async ValueTask<bool> IsSomeAndAsync(Func<T, Task<bool>> predicate)
        {
            Option<T> option = await optionValueTask.ConfigureAwait(false);

            return await option.IsSomeAndAsync(predicate).ConfigureAwait(false);
        }

        /// <summary>
        /// Awaits a value task of <see cref="Option{T}" /> and checks whether it is
        /// a <see cref="Some{T}" /> whose value satisfies a synchronous predicate.
        /// </summary>
        /// <remarks>
        /// The predicate is not invoked when the option is a
        /// <see cref="None{T}" />. Only the receiver is awaited, so pick this
        /// overload when the test itself does no asynchronous work.
        /// </remarks>
        /// <param name="predicate">
        /// The condition to evaluate against the contained value.
        /// </param>
        /// <returns>
        /// True if the option is a <see cref="Some{T}" /> and the predicate returns
        /// true; false otherwise.
        /// </returns>
        public async ValueTask<bool> IsSomeAndAsync(Func<T, bool> predicate)
        {
            Option<T> option = await optionValueTask.ConfigureAwait(false);

            return option.IsSomeAnd(predicate);
        }
    }
}
