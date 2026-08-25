namespace Waystone.Monads.Results.Extensions;

using System;
using System.Threading.Tasks;
using Waystone.SourceGenerators;

/// <summary>
/// Asynchronous <c>InspectErr</c> extensions for
/// <see cref="Result{TOk,TErr}" />.
/// </summary>
[GenerateAwaitedReceivers(typeof(Result<,>))]
[GenerateAwaitedMember(nameof(Result<,>.InspectErr))]
public static partial class InspectErrExtensions
{
    extension<TOk, TErr>(Result<TOk, TErr> result)
        where TOk : notnull
        where TErr : notnull
    {
        /// <summary>
        /// Awaits <paramref name="action" /> against the contained error if the
        /// result is an <see cref="Err{TOk,TErr}" />, then returns the result
        /// unchanged.
        /// </summary>
        /// <remarks>
        /// <paramref name="action" /> is not invoked for an
        /// <see cref="Ok{TOk,TErr}" />. Use this to observe a failure — logging or
        /// metrics — without handling it; the error is still carried forward.
        /// </remarks>
        /// <param name="action">
        /// The asynchronous side effect to run against the contained error.
        /// </param>
        /// <returns>
        /// A <see cref="ValueTask{TResult}" /> completing with the receiver, never a
        /// new instance.
        /// </returns>
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
