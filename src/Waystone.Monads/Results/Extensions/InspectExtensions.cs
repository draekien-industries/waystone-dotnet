namespace Waystone.Monads.Results.Extensions;

using System;
using System.Threading.Tasks;
using Waystone.SourceGenerators;

/// <summary>
/// Asynchronous <c>Inspect</c> extensions for <see cref="Result{TOk,TErr}" />.
/// </summary>
[GenerateAwaitedReceivers(typeof(Result<,>))]
[GenerateAwaitedMember(nameof(Result<,>.Inspect))]
public static partial class InspectExtensions
{
    extension<TOk, TErr>(Result<TOk, TErr> result)
        where TOk : notnull where TErr : notnull
    {
        /// <summary>
        /// Awaits <paramref name="action" /> against the contained value if the
        /// result is an <see cref="Ok{TOk,TErr}" />, then returns the result
        /// unchanged.
        /// </summary>
        /// <remarks>
        /// <paramref name="action" /> is not invoked for an
        /// <see cref="Err{TOk,TErr}" />. Use this to observe an ok value — logging or
        /// metrics — without altering the pipeline; any exception the action faults
        /// with surfaces to the caller.
        /// </remarks>
        /// <param name="action">
        /// The asynchronous side effect to run against the contained ok value.
        /// </param>
        /// <returns>
        /// A <see cref="ValueTask{TResult}" /> completing with the receiver, never a
        /// new instance.
        /// </returns>
        public async ValueTask<Result<TOk, TErr>>
            InspectAsync(Func<TOk, Task> action)
        {
            if (result.IsErr) return result;

            TOk ok = result.Expect("Expected Ok but found Err.");

            await action.Invoke(ok).ConfigureAwait(false);

            return result;
        }
    }
}
