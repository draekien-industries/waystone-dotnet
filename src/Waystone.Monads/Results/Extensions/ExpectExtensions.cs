namespace Waystone.Monads.Results.Extensions;

using System.Threading.Tasks;
using Exceptions;

public static class ExpectExtensions
{
    extension<TOk, TErr>(Task<Result<TOk, TErr>> resultTask)
        where TOk : notnull
        where TErr : notnull
    {
        /// <summary>
        /// Asynchronously awaits the <see cref="Result{TOk,TErr}" /> then returns
        /// the contained <see cref="Ok{TOk,TErr}" /> value.
        /// </summary>
        /// <param name="message">
        /// The message that will be included in the thrown
        /// exception when the result is an error.
        /// </param>
        /// <exception cref="UnmetExpectationException">
        /// Thrown when the awaited
        /// result is an <see cref="Err{TOk,TErr}" />
        /// </exception>
        public async ValueTask<TOk> ExpectAsync(string message)
        {
            Result<TOk, TErr> result = await resultTask.ConfigureAwait(false);

            return result.Expect(message);
        }

        /// <summary>
        /// Asynchronously awaits the <see cref="Result{TOk,TErr}" /> then returns
        /// the contained <see cref="Err{TOk,TErr}" /> value.
        /// </summary>
        /// <param name="message">
        /// The message that will be included in the thrown
        /// exception when the result is ok.
        /// </param>
        /// <exception cref="UnmetExpectationException">
        /// Thrown when the awaited
        /// result is an <see cref="Ok{TOk,TErr}" />
        /// </exception>
        public async ValueTask<TErr> ExpectErrAsync(string message)
        {
            Result<TOk, TErr> result = await resultTask.ConfigureAwait(false);

            return result.ExpectErr(message);
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
        /// <param name="message">
        /// The message that will be included in the thrown
        /// exception when the result is an error.
        /// </param>
        /// <exception cref="UnmetExpectationException">
        /// Thrown when the awaited
        /// result is an <see cref="Err{TOk,TErr}" />
        /// </exception>
        public async ValueTask<TOk> ExpectAsync(string message)
        {
            Result<TOk, TErr> result = await resultTask.ConfigureAwait(false);

            return result.Expect(message);
        }

        /// <summary>
        /// Asynchronously awaits the <see cref="Result{TOk,TErr}" /> then returns
        /// the contained <see cref="Err{TOk,TErr}" /> value.
        /// </summary>
        /// <param name="message">
        /// The message that will be included in the thrown
        /// exception when the result is ok.
        /// </param>
        /// <exception cref="UnmetExpectationException">
        /// Thrown when the awaited
        /// result is an <see cref="Ok{TOk,TErr}" />
        /// </exception>
        public async ValueTask<TErr> ExpectErrAsync(string message)
        {
            Result<TOk, TErr> result = await resultTask.ConfigureAwait(false);

            return result.ExpectErr(message);
        }
    }
}
