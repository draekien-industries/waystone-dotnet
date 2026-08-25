namespace Waystone.Monads.Results.Extensions;

using Options;

/// <summary>
/// Turns a <see cref="Result{TOk,TErr}" /> of an <see cref="Option{T}" /> inside
/// out.
/// </summary>
public static class TransposeExtensions
{
    extension<TOk, TErr>(Result<Option<TOk>, TErr> result)
        where TOk : notnull where TErr : notnull
    {
        /// <summary>
        /// Turns a result of an option into an option of a result.
        /// </summary>
        /// <returns>
        /// <see cref="None{T}" /> if the result is an <see cref="Ok{TOk,TErr}" /> of
        /// <see cref="None{T}" />; <see cref="Some{T}" /> of an
        /// <see cref="Ok{TOk,TErr}" /> if it is an <see cref="Ok{TOk,TErr}" /> of
        /// <see cref="Some{T}" />; and <see cref="Some{T}" /> of an
        /// <see cref="Err{TOk,TErr}" /> if it is an <see cref="Err{TOk,TErr}" />. An
        /// error is never discarded.
        /// </returns>
        public Option<Result<TOk, TErr>> Transpose()
        {
            if (result.IsErr)
            {
                TErr err = result.ExpectErr("Expected Err but found Ok.");
                Result<TOk, TErr> errResult = Result.Err<TOk, TErr>(err);

                return Option.Some(errResult);
            }

            Option<TOk> option = result.Expect("Expected Ok but found Err.");

            if (option.IsNone)
            {
                return Option.None<Result<TOk, TErr>>();
            }

            TOk value = option.Expect("Expected Some but found None.");

            return Option.Some(Result.Ok<TOk, TErr>(value));
        }
    }
}
