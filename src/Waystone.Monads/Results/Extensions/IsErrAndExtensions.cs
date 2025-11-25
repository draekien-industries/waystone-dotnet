namespace Waystone.Monads.Results.Extensions;

using System;
using System.Threading.Tasks;

public static class IsErrAndExtensions
{
    extension<TOk, TErr>(Result<TOk, TErr> result)
        where TOk : notnull where TErr : notnull
    {
        /// <summary>
        /// Determines whether the result is an Err value and satisfies the specified
        /// predicate.
        /// </summary>
        /// <param name="predicate">
        /// A function that defines the condition to check against the Err value.
        /// </param>
        /// <returns>
        /// A <see cref="ValueTask{TResult}" /> that represents the asynchronous operation,
        /// containing
        /// <see langword="true" /> if the result is an Err value and satisfies the
        /// specified predicate;
        /// otherwise, <see langword="false" />.
        /// </returns>
        public async ValueTask<bool> IsErrAndAsync(
            Func<TErr, Task<bool>> predicate)
        {
            if (result.IsOk) return false;

            TErr err = result.ExpectErr("Expected Err but found Ok.");

            return await predicate.Invoke(err).ConfigureAwait(false);
        }
    }

    extension<TOk, TErr>(Task<Result<TOk, TErr>> resultTask)
        where TOk : notnull where TErr : notnull
    {
        /// <summary>
        /// Determines whether the asynchronous result is an Err value and satisfies the
        /// specified
        /// asynchronous predicate.
        /// </summary>
        /// <param name="predicate">
        /// A function that defines an asynchronous condition to check against the Err
        /// value.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}" /> representing the asynchronous operation,
        /// containing
        /// <see langword="true" /> if the result is an Err value and satisfies the
        /// specified predicate;
        /// otherwise, <see langword="false" />.
        /// </returns>
        public async Task<bool> IsErrAndAsync(Func<TErr, Task<bool>> predicate)
        {
            Result<TOk, TErr> result = await resultTask.ConfigureAwait(false);

            if (result.IsOk) return false;

            TErr err = result.ExpectErr("Expected Err but found Ok.");

            return await predicate.Invoke(err).ConfigureAwait(false);
        }

        /// <summary>
        /// Determines whether the result is an Err value and satisfies the specified
        /// predicate asynchronously.
        /// </summary>
        /// <param name="predicate">
        /// A function that defines the asynchronous condition to check against the Err
        /// value.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}" /> representing the asynchronous operation,
        /// containing <see langword="true" /> if the result is an Err value and satisfies
        /// the
        /// specified predicate; otherwise, <see langword="false" />.
        /// </returns>
        public async Task<bool> IsErrAndAsync(Func<TErr, bool> predicate)
        {
            Result<TOk, TErr> result = await resultTask.ConfigureAwait(false);

            if (result.IsOk) return false;

            TErr err = result.ExpectErr("Expected Err but found Ok.");

            return predicate.Invoke(err);
        }
    }

    extension<TOk, TErr>(ValueTask<Result<TOk, TErr>> resultTask)
        where TOk : notnull where TErr : notnull
    {
        /// <summary>
        /// Determines whether the result is an Err value and satisfies the specified
        /// asynchronous predicate.
        /// </summary>
        /// <param name="predicate">
        /// A function that defines the condition to check against the Err value
        /// asynchronously.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}" /> that represents the asynchronous operation,
        /// containing
        /// <see langword="true" /> if the result is an Err value and satisfies the
        /// specified predicate;
        /// otherwise, <see langword="false" />.
        /// </returns>
        public async Task<bool> IsErrAndAsync(Func<TErr, Task<bool>> predicate)
        {
            Result<TOk, TErr> result = await resultTask.ConfigureAwait(false);

            if (result.IsOk) return false;

            TErr err = result.ExpectErr("Expected Err but found Ok.");

            return await predicate.Invoke(err).ConfigureAwait(false);
        }

        /// <summary>
        /// Determines whether the result is an Err value and satisfies the specified
        /// predicate asynchronously.
        /// </summary>
        /// <param name="predicate">
        /// A function that defines the condition to check against the Err value
        /// asynchronously.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}" /> that represents the asynchronous operation,
        /// containing <see langword="true" /> if the result is an Err value and satisfies
        /// the
        /// specified predicate; otherwise, <see langword="false" />.
        /// </returns>
        public async Task<bool> IsErrAndAsync(Func<TErr, bool> predicate)
        {
            Result<TOk, TErr> result = await resultTask.ConfigureAwait(false);

            if (result.IsOk) return false;

            TErr err = result.ExpectErr("Expected Err but found Ok.");

            return predicate.Invoke(err);
        }
    }
}
