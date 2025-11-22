namespace Waystone.Monads.Options.Extensions;

using System;
using System.Diagnostics;
using System.Threading.Tasks;

/// <summary>
/// Provides extension methods for handling asynchronous control flow with
/// instances of <see cref="Option{T}" />.
/// </summary>
public static class AsyncControlFlowExtensions
{
    /// <summary>
    /// Provides extension methods for handling asynchronous control flow with
    /// instances of <see cref="Option{T}" />.
    /// </summary>
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
        [DebuggerStepThrough]
        public async ValueTask<bool> IsSomeAnd(Func<T, Task<bool>> predicate)
        {
            if (option.IsNone) return false;

            T some = option.Expect("Expected Some but found None.");

            return await predicate.Invoke(some).ConfigureAwait(false);
        }

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
        [DebuggerStepThrough]
        public async ValueTask<bool> IsNoneOr(Func<T, Task<bool>> predicate)
        {
            if (option.IsNone) return true;

            T some = option.Expect("Expected Some but found None.");

            return await predicate.Invoke(some).ConfigureAwait(false);
        }
    }
}
