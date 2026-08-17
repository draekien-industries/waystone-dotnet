namespace Waystone.Monads.Results.Extensions;

using System.Threading.Tasks;
using Exceptions;

public static class UnwrapExtensions
{
    extension<TOk, TErr>(Task<Result<TOk, TErr>> resultTask)
        where TOk : notnull
        where TErr : notnull
    {
        /// <summary>
        /// Asynchronously awaits the <see cref="Result{TOk,TErr}" /> then returns
        /// the contained <see cref="Ok{TOk,TErr}" /> value.
        /// </summary>
        /// <exception cref="UnwrapException">
        /// Thrown when the awaited result is an
        /// <see cref="Err{TOk,TErr}" />
        /// </exception>
        public async Task<TOk> UnwrapAsync()
        {
            Result<TOk, TErr> result = await resultTask.ConfigureAwait(false);

            return result.Unwrap();
        }

        /// <summary>
        /// Asynchronously awaits the <see cref="Result{TOk,TErr}" /> then returns
        /// the contained <see cref="Err{TOk,TErr}" /> value.
        /// </summary>
        /// <exception cref="UnwrapException">
        /// Thrown when the awaited result is an
        /// <see cref="Ok{TOk,TErr}" />
        /// </exception>
        public async Task<TErr> UnwrapErrAsync()
        {
            Result<TOk, TErr> result = await resultTask.ConfigureAwait(false);

            return result.UnwrapErr();
        }

        /// <summary>
        /// Asynchronously awaits the <see cref="Result{TOk,TErr}" /> then returns
        /// the contained <see cref="Ok{TOk,TErr}" /> value, or the provided default
        /// when it is an <see cref="Err{TOk,TErr}" />.
        /// </summary>
        /// <param name="default">The value to return when the result is an error.</param>
        public async Task<TOk> UnwrapOrAsync(TOk @default)
        {
            Result<TOk, TErr> result = await resultTask.ConfigureAwait(false);

            return result.UnwrapOr(@default);
        }

        /// <summary>
        /// Asynchronously awaits the <see cref="Result{TOk,TErr}" /> then returns
        /// the contained <see cref="Ok{TOk,TErr}" /> value, or the default value of
        /// <typeparamref name="TOk" /> when it is an <see cref="Err{TOk,TErr}" />.
        /// </summary>
        public async Task<TOk?> UnwrapOrDefaultAsync()
        {
            Result<TOk, TErr> result = await resultTask.ConfigureAwait(false);

            return result.UnwrapOrDefault();
        }
    }

    extension<TOk, TErr>(ValueTask<Result<TOk, TErr>> resultTask)
        where TOk : notnull
        where TErr : notnull
    {
        /// <summary>
        /// Asynchronously awaits the <see cref="Result{TOk,TErr}" /> then returns
        /// the contained <see cref="Ok{TOk,TErr}" /> value.
        /// </summary>
        /// <exception cref="UnwrapException">
        /// Thrown when the awaited result is an
        /// <see cref="Err{TOk,TErr}" />
        /// </exception>
        public async Task<TOk> UnwrapAsync()
        {
            Result<TOk, TErr> result = await resultTask.ConfigureAwait(false);

            return result.Unwrap();
        }

        /// <summary>
        /// Asynchronously awaits the <see cref="Result{TOk,TErr}" /> then returns
        /// the contained <see cref="Err{TOk,TErr}" /> value.
        /// </summary>
        /// <exception cref="UnwrapException">
        /// Thrown when the awaited result is an
        /// <see cref="Ok{TOk,TErr}" />
        /// </exception>
        public async Task<TErr> UnwrapErrAsync()
        {
            Result<TOk, TErr> result = await resultTask.ConfigureAwait(false);

            return result.UnwrapErr();
        }

        /// <summary>
        /// Asynchronously awaits the <see cref="Result{TOk,TErr}" /> then returns
        /// the contained <see cref="Ok{TOk,TErr}" /> value, or the provided default
        /// when it is an <see cref="Err{TOk,TErr}" />.
        /// </summary>
        /// <param name="default">The value to return when the result is an error.</param>
        public async Task<TOk> UnwrapOrAsync(TOk @default)
        {
            Result<TOk, TErr> result = await resultTask.ConfigureAwait(false);

            return result.UnwrapOr(@default);
        }

        /// <summary>
        /// Asynchronously awaits the <see cref="Result{TOk,TErr}" /> then returns
        /// the contained <see cref="Ok{TOk,TErr}" /> value, or the default value of
        /// <typeparamref name="TOk" /> when it is an <see cref="Err{TOk,TErr}" />.
        /// </summary>
        public async Task<TOk?> UnwrapOrDefaultAsync()
        {
            Result<TOk, TErr> result = await resultTask.ConfigureAwait(false);

            return result.UnwrapOrDefault();
        }
    }
}
