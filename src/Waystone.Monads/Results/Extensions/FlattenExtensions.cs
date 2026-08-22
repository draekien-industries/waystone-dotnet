namespace Waystone.Monads.Results.Extensions;

using Waystone.SourceGenerators;

[GenerateAwaitedReceivers(typeof(Result<,>))]
public static partial class FlattenExtensions
{
    extension<TOk, TErr>(Result<Result<TOk, TErr>, TErr> result)
        where TOk : notnull where TErr : notnull
    {
        /// <summary>
        /// Flattens a nested <see cref="Result{TOk, TErr}" /> within another
        /// <see cref="Result{TOk, TErr}" /> into a single <see cref="Result{TOk, TErr}" />
        /// .
        /// </summary>
        /// <returns>
        /// A flattened <see cref="Result{TOk, TErr}" />.
        /// If the outer result is <see langword="Ok" />, its inner value is returned.
        /// If the outer result is <see langword="Err" />, its error value is propagated.
        /// </returns>
        public Result<TOk, TErr> Flatten()
        {
            if (result.IsOk) return result.Expect("Expected Ok but found Err.");

            TErr err = result.ExpectErr("Expected Err but found Ok.");

            return Result.Err<TOk, TErr>(err);
        }
    }
}
