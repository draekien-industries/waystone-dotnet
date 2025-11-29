namespace Waystone.Monads.Results.Extensions;

using Options;

/// <summary>Extension methods for <see cref="Result{TOk,TErr}" /></summary>
public static class TransposeExtensions
{
    extension<TOk, TErr>(Result<Option<TOk>, TErr> result)
        where TOk : notnull where TErr : notnull
    {
        /// <summary>
        /// Transposes a <c>result</c> of an <c>option</c> into an <c>option</c>
        /// of a <c>result</c>
        /// </summary>
        /// <list type="bullet">
        /// <item>
        /// <see cref="Ok{TOk,TErr}" /> of <see cref="None{T}" /> will be mapped to
        /// <see cref="None{T}" />.
        /// </item>
        /// <item>
        /// <see cref="Ok{TOk,TErr}" /> of <see cref="Some{T}" /> and
        /// <see cref="Err{TOk,TErr}" /> will be mapped to <see cref="Some{T}" /> of
        /// <see cref="Ok{TOk,TErr}" /> and <see cref="Some{T}" /> of
        /// <see cref="Err{TOk,TErr}" />
        /// </item>
        /// </list>
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
