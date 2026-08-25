namespace Waystone.Monads.Results.Extensions;

using System;
using System.Threading.Tasks;
using Waystone.SourceGenerators;

/// <summary>
/// Asynchronous <c>IsOkAnd</c> extensions for <see cref="Result{TOk,TErr}" />.
/// </summary>
[GenerateAwaitedReceivers(typeof(Result<,>))]
[GenerateAwaitedMember(nameof(Result<,>.IsOkAnd))]
public static partial class IsOkAndExtensions
{
    extension<TOk, TErr>(Result<TOk, TErr> result)
        where TOk : notnull where TErr : notnull
    {
        /// <summary>
        /// Checks whether the result is an <see cref="Ok{TOk,TErr}" /> whose value
        /// satisfies an asynchronous <paramref name="predicate" />.
        /// </summary>
        /// <remarks>
        /// <paramref name="predicate" /> is not invoked for an
        /// <see cref="Err{TOk,TErr}" />, so any side effect it carries does not run
        /// in that case.
        /// </remarks>
        /// <param name="predicate">
        /// The asynchronous condition to evaluate against the contained ok value.
        /// </param>
        /// <returns>
        /// A <see cref="ValueTask{TResult}" /> completing with true if the result is
        /// an <see cref="Ok{TOk,TErr}" /> and <paramref name="predicate" /> returns
        /// true; false otherwise.
        /// </returns>
        public async ValueTask<bool> IsOkAndAsync(
            Func<TOk, Task<bool>> predicate)
        {
            if (result.IsErr) return false;

            TOk ok = result.Expect("Expected Ok but found Err.");

            return await predicate.Invoke(ok).ConfigureAwait(false);
        }
    }
}
