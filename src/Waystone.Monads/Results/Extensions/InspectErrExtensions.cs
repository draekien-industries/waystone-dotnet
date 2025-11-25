namespace Waystone.Monads.Results.Extensions;

using System;
using System.Threading.Tasks;

public static class InspectErrExtensions
{
    extension<TOk, TErr>(Result<TOk, TErr> result)
        where TOk : notnull
        where TErr : notnull
    {
        /// <summary>
        /// Asynchronously invokes the specified action on the
        /// <see cref="Err{TOk, TErr}" /> value,
        /// if the <see cref="Result{TOk, TErr}" /> represents an error.
        /// Does nothing if the result is <see cref="Ok{TOk, TErr}" />.
        /// </summary>
        /// <param name="action">
        /// The asynchronous action to invoke with the
        /// <see cref="Err{TOk, TErr}" /> value.
        /// </param>
        /// <returns>The original <see cref="Result{TOk, TErr}" /> instance, unmodified.</returns>
        public async ValueTask<Result<TOk, TErr>> InspectErrAsync(
            Func<TErr, Task> action)
        {
            if (result.IsOk) return result;

            TErr err = result.ExpectErr("Expected Err but found Ok.");

            await action.Invoke(err).ConfigureAwait(false);

            return result;
        }
    }

    extension<TOk, TErr>(Task<Result<TOk, TErr>> resultTask)
        where TOk : notnull
        where TErr : notnull
    {
        /// <summary>
        /// Asynchronously invokes the specified action on the
        /// <see cref="Err{TOk, TErr}" /> value, if the result represents an error.
        /// Does nothing if the result is <see cref="Ok{TOk, TErr}" />.
        /// </summary>
        /// <param name="action">
        /// The asynchronous action to invoke with the
        /// <see cref="Err{TOk, TErr}" /> value.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}" /> that represents the asynchronous operation.
        /// The result of the task is the original <see cref="Result{TOk, TErr}" />
        /// instance, unmodified.
        /// </returns>
        public async Task<Result<TOk, TErr>> InspectErrAsync(
            Func<TErr, Task> action)
        {
            Result<TOk, TErr>? result = await resultTask.ConfigureAwait(false);

            if (result.IsOk) return result;

            TErr err = result.ExpectErr("Expected Err but found Ok.");

            await action.Invoke(err).ConfigureAwait(false);

            return result;
        }

        /// <summary>
        /// Asynchronously invokes the specified action on the
        /// <see cref="Err{TOk, TErr}" /> value,
        /// if the <see cref="Result{TOk, TErr}" /> represents an error.
        /// Does nothing if the result is <see cref="Ok{TOk, TErr}" />.
        /// </summary>
        /// <param name="action">
        /// The action to invoke with the
        /// <see cref="Err{TOk, TErr}" /> value.
        /// </param>
        /// <returns>The original <see cref="Result{TOk, TErr}" /> instance, unmodified.</returns>
        public async Task<Result<TOk, TErr>> InspectErrAsync(
            Action<TErr> action)
        {
            Result<TOk, TErr>? result = await resultTask.ConfigureAwait(false);

            if (result.IsOk) return result;

            TErr err = result.ExpectErr("Expected Err but found Ok.");

            action.Invoke(err);

            return result;
        }
    }

    extension<TOk, TErr>(ValueTask<Result<TOk, TErr>> resultTask)
        where TOk : notnull
        where TErr : notnull
    {
        /// <summary>
        /// Asynchronously invokes the specified action on the
        /// <see cref="Err{TOk, TErr}" /> value, if the
        /// <see cref="Result{TOk, TErr}" /> represents an error.
        /// Does nothing if the result is <see cref="Ok{TOk, TErr}" />.
        /// </summary>
        /// <param name="action">
        /// The asynchronous action to invoke with the
        /// <see cref="Err{TOk, TErr}" /> value.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation, returning the
        /// original <see cref="Result{TOk, TErr}" /> instance, unmodified.
        /// </returns>
        public async Task<Result<TOk, TErr>> InspectErrAsync(
            Func<TErr, Task> action)
        {
            Result<TOk, TErr>? result = await resultTask.ConfigureAwait(false);

            if (result.IsOk) return result;

            TErr err = result.ExpectErr("Expected Err but found Ok.");

            await action.Invoke(err).ConfigureAwait(false);

            return result;
        }

        /// <summary>
        /// Asynchronously invokes the specified action on the
        /// <see cref="Err{TOk, TErr}" /> value of the <see cref="Result{TOk, TErr}" />
        /// if the result represents an error. Does nothing if the result is
        /// <see cref="Ok{TOk, TErr}" />.
        /// </summary>
        /// <param name="action">
        /// The asynchronous action to invoke with the <see cref="Err{TOk, TErr}" /> value.
        /// </param>
        /// <returns>
        /// The original <see cref="Result{TOk, TErr}" /> instance, unmodified.
        /// </returns>
        public async Task<Result<TOk, TErr>> InspectErrAsync(
            Action<TErr> action)
        {
            Result<TOk, TErr>? result = await resultTask.ConfigureAwait(false);

            if (result.IsOk) return result;

            TErr err = result.ExpectErr("Expected Err but found Ok.");

            action.Invoke(err);

            return result;
        }
    }
}
