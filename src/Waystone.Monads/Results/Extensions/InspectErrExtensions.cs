namespace Waystone.Monads.Results.Extensions;

using System;
using System.Threading.Tasks;
using Waystone.SourceGenerators;

[GenerateAwaitedReceivers(typeof(Result<,>))]
[GenerateAwaitedMember(nameof(Result<,>.InspectErr))]
public static partial class InspectErrExtensions
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
}
