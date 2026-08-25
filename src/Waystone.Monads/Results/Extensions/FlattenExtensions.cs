namespace Waystone.Monads.Results.Extensions;

using Waystone.SourceGenerators;

/// <summary>
/// Collapses a <see cref="Result{TOk,TErr}" /> nested inside another
/// <see cref="Result{TOk,TErr}" />.
/// </summary>
[GenerateAwaitedReceivers(typeof(Result<,>))]
public static partial class FlattenExtensions
{
    extension<TOk, TErr>(Result<Result<TOk, TErr>, TErr> result)
        where TOk : notnull where TErr : notnull
    {
        /// <summary>
        /// Collapses one level of nesting, so a result of a result becomes a plain
        /// result.
        /// </summary>
        /// <returns>
        /// The inner result unchanged if the outer result is an
        /// <see cref="Ok{TOk,TErr}" /> — including when that inner result is itself
        /// an <see cref="Err{TOk,TErr}" /> — otherwise the outer error re-wrapped as
        /// an <see cref="Err{TOk,TErr}" /> of <typeparamref name="TOk" />.
        /// </returns>
        public Result<TOk, TErr> Flatten()
        {
            if (result.IsOk) return result.Expect("Expected Ok but found Err.");

            TErr err = result.ExpectErr("Expected Err but found Ok.");

            return Result.Err<TOk, TErr>(err);
        }
    }
}
