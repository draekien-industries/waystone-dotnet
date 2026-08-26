namespace Waystone.Monads.Results.Extensions;

using System;
using System.Threading.Tasks;
using Waystone.SourceGenerators;

/// <summary>
/// Asynchronous <c>AndThen</c> extensions for <see cref="Result{TOk,TErr}" />
/// and for tasks producing one.
/// </summary>
[GenerateAwaitedReceivers(typeof(Result<,>))]
[GenerateAwaitedMember(nameof(Result<,>.AndThen))]
public static partial class AndThenExtensions
{
    extension<TOk, TErr>(Result<TOk, TErr> result)
        where TOk : notnull where TErr : notnull
    {
        /// <summary>
        /// Calls <paramref name="resultFactory" /> with the contained value if the result
        /// is an <see cref="Ok{TOk,TErr}" /> and awaits it, otherwise propagates the
        /// error.
        /// </summary>
        /// <remarks>
        /// <paramref name="resultFactory" /> is not invoked for an
        /// <see cref="Err{TOk,TErr}" />. Any exception the returned task faults with
        /// surfaces to the caller unchanged.
        /// </remarks>
        /// <typeparam name="TOut">
        /// The ok value type of the result produced by <paramref name="resultFactory" />.
        /// </typeparam>
        /// <param name="resultFactory">
        /// Produces the next result from the contained ok value.
        /// </param>
        /// <returns>
        /// The result <paramref name="resultFactory" /> produced, or the original error
        /// re-wrapped as an <see cref="Err{TOk,TErr}" /> of
        /// <typeparamref name="TOut" />.
        /// </returns>
        public async ValueTask<Result<TOut, TErr>>
            AndThenAsync<TOut>(Func<TOk, ValueTask<Result<TOut, TErr>>> resultFactory)
            where TOut : notnull
        {
            if (result.IsErr)
            {
                TErr err = result.ExpectErr("Expected Err but found Ok.");

                return Result.Err<TOut, TErr>(err);
            }

            TOk ok = result.Expect("Expected Ok but found Err.");

            return await resultFactory.Invoke(ok).ConfigureAwait(false);
        }
    }
}
