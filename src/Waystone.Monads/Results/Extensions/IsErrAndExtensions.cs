namespace Waystone.Monads.Results.Extensions;

using System;
using System.Threading.Tasks;
using Waystone.SourceGenerators;

/// <summary>
/// Asynchronous <c>IsErrAnd</c> extensions for <see cref="Result{TOk,TErr}" />.
/// </summary>
[GenerateAwaitedReceivers(typeof(Result<,>))]
[GenerateAwaitedMember(nameof(Result<,>.IsErrAnd))]
public static partial class IsErrAndExtensions
{
    extension<TOk, TErr>(Result<TOk, TErr> result)
        where TOk : notnull where TErr : notnull
    {
        /// <summary>
        /// Checks whether the result is an <see cref="Err{TOk,TErr}" /> whose error
        /// satisfies an asynchronous <paramref name="predicate" />.
        /// </summary>
        /// <remarks>
        /// <paramref name="predicate" /> is not invoked for an
        /// <see cref="Ok{TOk,TErr}" />, so any side effect it carries does not run in
        /// that case.
        /// </remarks>
        /// <param name="predicate">
        /// The asynchronous condition to evaluate against the contained error.
        /// </param>
        /// <returns>
        /// A <see cref="ValueTask{TResult}" /> completing with true if the result is
        /// an <see cref="Err{TOk,TErr}" /> and <paramref name="predicate" /> returns
        /// true; false otherwise.
        /// </returns>
        public async ValueTask<bool> IsErrAndAsync(
            Func<TErr, Task<bool>> predicate)
        {
            if (result.IsOk) return false;

            TErr err = result.ExpectErr("Expected Err but found Ok.");

            return await predicate.Invoke(err).ConfigureAwait(false);
        }
    }
}
