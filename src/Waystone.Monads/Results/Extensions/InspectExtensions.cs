namespace Waystone.Monads.Results.Extensions;

using System;
using System.Threading.Tasks;
using Waystone.SourceGenerators;

[GenerateAwaitedReceivers(typeof(Result<,>))]
[GenerateAwaitedMember(nameof(Result<,>.Inspect))]
public static partial class InspectExtensions
{
    extension<TOk, TErr>(Result<TOk, TErr> result)
        where TOk : notnull where TErr : notnull
    {
        /// <summary>
        /// Asynchronously inspects the <see cref="Result{TOk, TErr}" /> if it is
        /// <see cref="Result{TOk, TErr}.IsOk" />
        /// by executing the specified action on the <typeparamref name="TOk" /> value.
        /// </summary>
        /// <param name="action">
        /// A function that represents the asynchronous operation to be performed
        /// on the <typeparamref name="TOk" /> value if the result is
        /// <see cref="Result{TOk, TErr}.IsOk" />.
        /// </param>
        /// <returns>
        /// The original <see cref="Result{TOk, TErr}" /> after executing the specified
        /// <paramref name="action" />,
        /// regardless of its state.
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
