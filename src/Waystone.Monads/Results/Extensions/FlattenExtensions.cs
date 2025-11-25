namespace Waystone.Monads.Results.Extensions;

using System.Threading.Tasks;

public static class FlattenExtensions
{
    extension<TOk, TErr>(Task<Result<Result<TOk, TErr>, TErr>> resultTask)
        where TOk : notnull where TErr : notnull
    {
        /// <summary>
        /// Flattens a nested <see cref="Result{TOk, TErr}" /> within another
        /// <see cref="Result{TOk, TErr}" /> into a single <see cref="Result{TOk, TErr}" />
        /// asynchronously.
        /// </summary>
        /// <returns>
        /// A <see cref="Task" /> representing the asynchronous operation. The task result
        /// contains a flattened <see cref="Result{TOk, TErr}" />.
        /// If the outer result is <c>Ok</c>, its inner value is returned. If the outer
        /// result is <c>Err</c>, its error value is propagated.
        /// </returns>
        public async Task<Result<TOk, TErr>> FlattenAsync()
        {
            Result<Result<TOk, TErr>, TErr>? result =
                await resultTask.ConfigureAwait(false);

            if (result.IsOk) return result.Expect("Expected Ok but found Err.");

            TErr err = result.ExpectErr("Expected Err but found Ok.");

            return Result.Err<TOk, TErr>(err);
        }
    }

    extension<TOk, TErr>(ValueTask<Result<Result<TOk, TErr>, TErr>> resultTask)
        where TOk : notnull where TErr : notnull
    {
        /// <summary>
        /// Flattens a nested <see cref="Result{TOk, TErr}" /> within a
        /// <see cref="ValueTask{TResult}" /> asynchronously into a single
        /// <see cref="Result{TOk, TErr}" />.
        /// </summary>
        /// <returns>
        /// A <see cref="Task" /> that represents the asynchronous operation. The task
        /// result
        /// contains a flattened <see cref="Result{TOk, TErr}" />.
        /// If the outer result is <c>Ok</c>, its inner value is returned.
        /// If the outer result is <c>Err</c>, its error value is propagated.
        /// </returns>
        public async Task<Result<TOk, TErr>> FlattenAsync()
        {
            Result<Result<TOk, TErr>, TErr>? result =
                await resultTask.ConfigureAwait(false);

            if (result.IsOk) return result.Expect("Expected Ok but found Err.");

            TErr err = result.ExpectErr("Expected Err but found Ok.");

            return Result.Err<TOk, TErr>(err);
        }
    }
}
