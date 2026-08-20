namespace Waystone.Monads.Results.Extensions;

using System.Threading.Tasks;

public static class UnwrapOrNullExtensions
{
    extension<TOk, TErr>(Result<TOk, TErr> result)
        where TOk : struct where TErr : notnull
    {
        /// <summary>
        /// Returns the contained value if the result is an
        /// <see cref="Ok{TOk,TErr}" />, otherwise <see langword="null" />.
        /// </summary>
        /// <remarks>
        /// Prefer this to <see cref="Result{TOk,TErr}.UnwrapOrDefault" /> when
        /// <typeparamref name="TOk" /> is a value type. <c>UnwrapOrDefault</c> returns
        /// the default of <typeparamref name="TOk" /> for an
        /// <see cref="Err{TOk,TErr}" />, which is indistinguishable from a legitimate
        /// zero.
        /// </remarks>
        /// <returns>
        /// The contained value if the result was an <see cref="Ok{TOk,TErr}" />,
        /// otherwise <see langword="null" />.
        /// </returns>
        public TOk? UnwrapOrNull() =>
            result.Match<TOk?>(value => value, _ => null);
    }

    extension<TOk, TErr>(Task<Result<TOk, TErr>> resultTask)
        where TOk : struct where TErr : notnull
    {
        /// <summary>
        /// Awaits the <see cref="Task{TResult}" /> and returns the contained value if
        /// the result is an <see cref="Ok{TOk,TErr}" />, otherwise
        /// <see langword="null" />.
        /// </summary>
        /// <returns>
        /// A <see cref="ValueTask{TResult}" /> containing the contained value if the awaited
        /// result was an <see cref="Ok{TOk,TErr}" />, otherwise
        /// <see langword="null" />.
        /// </returns>
        public async ValueTask<TOk?> UnwrapOrNullAsync()
        {
            Result<TOk, TErr> result = await resultTask.ConfigureAwait(false);

            return result.UnwrapOrNull();
        }
    }

    extension<TOk, TErr>(ValueTask<Result<TOk, TErr>> resultTask)
        where TOk : struct where TErr : notnull
    {
        /// <summary>
        /// Awaits the <see cref="ValueTask{TResult}" /> and returns the contained value
        /// if the result is an <see cref="Ok{TOk,TErr}" />, otherwise
        /// <see langword="null" />.
        /// </summary>
        /// <returns>
        /// A <see cref="ValueTask{TResult}" /> containing the contained value if the awaited
        /// result was an <see cref="Ok{TOk,TErr}" />, otherwise
        /// <see langword="null" />.
        /// </returns>
        public async ValueTask<TOk?> UnwrapOrNullAsync()
        {
            Result<TOk, TErr> result = await resultTask.ConfigureAwait(false);

            return result.UnwrapOrNull();
        }
    }
}
