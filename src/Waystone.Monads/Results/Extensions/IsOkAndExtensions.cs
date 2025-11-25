namespace Waystone.Monads.Results.Extensions;

using System;
using System.Threading.Tasks;

public static class IsOkAndExtensions
{
    extension<TOk, TErr>(Result<TOk, TErr> result)
        where TOk : notnull where TErr : notnull
    {
        /// <summary>
        /// Determines whether the current <see cref="Result{TOk, TErr}" /> is an <c>Ok</c>
        /// value
        /// and satisfies the given asynchronous <paramref name="predicate" />.
        /// </summary>
        /// <param name="predicate">
        /// A <see cref="Func{T, TResult}" /> that represents the asynchronous predicate
        /// to evaluate the <c>Ok</c> value against.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}" /> containing <c>true</c> if the current result is
        /// an
        /// <c>Ok</c> value and the <paramref name="predicate" /> returns <c>true</c>;
        /// otherwise, <c>false</c>.
        /// </returns>
        public async ValueTask<bool> IsOkAndAsync(
            Func<TOk, Task<bool>> predicate)
        {
            if (result.IsErr) return false;

            TOk ok = result.Expect("Expected Ok but found Err.");

            return await predicate.Invoke(ok).ConfigureAwait(false);
        }
    }

    extension<TOk, TErr>(Task<Result<TOk, TErr>> resultTask)
        where TOk : notnull where TErr : notnull
    {
        /// <summary>
        /// Determines whether the resolved <see cref="Result{TOk, TErr}" /> is an
        /// <c>Ok</c> value
        /// and satisfies the given asynchronous <paramref name="predicate" />.
        /// </summary>
        /// <param name="predicate">
        /// A <see cref="Func{T, TResult}" /> that represents the asynchronous predicate
        /// to evaluate the <c>Ok</c> value against.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}" /> containing <c>true</c> if the resolved result is
        /// an
        /// <c>Ok</c> value and the <paramref name="predicate" /> returns <c>true</c>;
        /// otherwise, <c>false</c>.
        /// </returns>
        public async Task<bool> IsOkAndAsync(Func<TOk, Task<bool>> predicate)
        {
            Result<TOk, TErr> result = await resultTask.ConfigureAwait(false);

            if (result.IsErr) return false;

            TOk ok = result.Expect("Expected Ok but found Err.");

            return await predicate.Invoke(ok).ConfigureAwait(false);
        }

        /// <summary>
        /// Determines whether the current <see cref="Result{TOk, TErr}" /> is an <c>Ok</c>
        /// value
        /// and satisfies the specified synchronous <paramref name="predicate" />.
        /// </summary>
        /// <param name="predicate">
        /// A <see cref="Func{T, TResult}" /> that represents the synchronous predicate
        /// to evaluate the <c>Ok</c> value against.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}" /> containing <c>true</c> if the current result is
        /// an
        /// <c>Ok</c> value and the <paramref name="predicate" /> returns <c>true</c>;
        /// otherwise, <c>false</c>.
        /// </returns>
        public async Task<bool> IsOkAndAsync(Func<TOk, bool> predicate)
        {
            Result<TOk, TErr>? result = await resultTask.ConfigureAwait(false);

            if (result.IsErr) return false;

            TOk ok = result.Expect("Expected Ok but found Err.");

            return predicate.Invoke(ok);
        }
    }

    extension<TOk, TErr>(ValueTask<Result<TOk, TErr>> resultTask)
        where TOk : notnull where TErr : notnull
    {
        /// <summary>
        /// Determines whether the resolved <see cref="Result{TOk, TErr}" /> is an
        /// <c>Ok</c> value
        /// and satisfies the given asynchronous <paramref name="predicate" />.
        /// </summary>
        /// <param name="predicate">
        /// A <see cref="Func{T, TResult}" /> that represents the asynchronous predicate
        /// to evaluate the <c>Ok</c> value against.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}" /> containing <c>true</c> if the resolved result is
        /// an
        /// <c>Ok</c> value and the <paramref name="predicate" /> returns <c>true</c>;
        /// otherwise, <c>false</c>.
        /// </returns>
        public async Task<bool> IsOkAndAsync(Func<TOk, Task<bool>> predicate)
        {
            Result<TOk, TErr> result = await resultTask.ConfigureAwait(false);

            if (result.IsErr) return false;

            TOk ok = result.Expect("Expected Ok but found Err.");

            return await predicate.Invoke(ok).ConfigureAwait(false);
        }

        /// <summary>
        /// Determines whether the current <see cref="Result{TOk, TErr}" /> is an <c>Ok</c>
        /// value and satisfies the given asynchronous <paramref name="predicate" />.
        /// </summary>
        /// <param name="predicate">
        /// A <see cref="Func{T, TResult}" /> that represents the asynchronous predicate
        /// to evaluate the <c>Ok</c> value against.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}" /> containing <see langword="true" /> if the
        /// current result
        /// is an <c>Ok</c> value and the <paramref name="predicate" /> returns
        /// <see langword="true" />;
        /// otherwise, <see langword="false" />.
        /// </returns>
        public async Task<bool> IsOkAndAsync(Func<TOk, bool> predicate)
        {
            Result<TOk, TErr>? result = await resultTask.ConfigureAwait(false);

            if (result.IsErr) return false;

            TOk ok = result.Expect("Expected Ok but found Err.");

            return predicate.Invoke(ok);
        }
    }
}
