namespace Waystone.Monads.Results.Extensions;

using System.Collections.Generic;
using System.Threading.Tasks;

public static class AsEnumerableExtensions
{
    extension<TOk, TErr>(Task<Result<TOk, TErr>> resultTask)
        where TOk : notnull where TErr : notnull
    {
        /// <summary>
        /// Awaits the <see cref="Task{TResult}" /> and returns a sequence over the
        /// possibly contained <see cref="Ok{TOk,TErr}" /> value.
        /// </summary>
        /// <returns>
        /// A <see cref="Task{TResult}" /> containing a sequence that yields the
        /// contained value once if the awaited result was an
        /// <see cref="Ok{TOk,TErr}" />, otherwise an empty sequence.
        /// </returns>
        public async Task<IEnumerable<TOk>> AsEnumerableAsync()
        {
            Result<TOk, TErr> result =
                await resultTask.ConfigureAwait(false);

            return result.AsEnumerable();
        }
    }

    extension<TOk, TErr>(ValueTask<Result<TOk, TErr>> resultTask)
        where TOk : notnull where TErr : notnull
    {
        /// <summary>
        /// Awaits the <see cref="ValueTask{TResult}" /> and returns a sequence over the
        /// possibly contained <see cref="Ok{TOk,TErr}" /> value.
        /// </summary>
        /// <returns>
        /// A <see cref="Task{TResult}" /> containing a sequence that yields the
        /// contained value once if the awaited result was an
        /// <see cref="Ok{TOk,TErr}" />, otherwise an empty sequence.
        /// </returns>
        public async Task<IEnumerable<TOk>> AsEnumerableAsync()
        {
            Result<TOk, TErr> result =
                await resultTask.ConfigureAwait(false);

            return result.AsEnumerable();
        }
    }
}
