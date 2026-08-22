namespace Waystone.Monads.Results.Extensions;

using System;
using System.Threading.Tasks;
using Waystone.SourceGenerators;

[GenerateAwaitedReceivers(typeof(Result<,>))]
[GenerateAwaitedMember(nameof(Result<,>.IsErrAnd))]
public static partial class IsErrAndExtensions
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
}
