namespace Waystone.Monads.Results.Extensions;

using Options;
using Waystone.SourceGenerators;
#if !DEBUG
using System.Diagnostics;
#endif

/// <summary>
/// Everything callable on a <see cref="Result{TOk,TErr}" /> that is not
/// declared on the type itself.
/// </summary>
/// <remarks>
/// Two kinds of member live here. Most are generated: the awaited-receiver
/// overloads that let a call chain stay in one expression when the result is
/// still inside a <see cref="System.Threading.Tasks.Task{TResult}" /> or a
/// <see cref="System.Threading.Tasks.ValueTask{TResult}" />, listed in the
/// attributes below and forwarding into the member of the same name on
/// <see cref="Result{TOk,TErr}" />. The rest are hand-written, and each is here
/// because its receiver is a *particular* result rather than any result:
/// <c>Flatten</c> reads a nested result, <c>Transpose</c> a result of an
/// <see cref="Option{T}" />, and <c>UnwrapOrNull</c> one whose ok value is a
/// value type. All three are lifted onto the two awaited receivers as well, so
/// none needs an attribute.
/// <para>
/// Operations over a *sequence* of results are the one thing that is not here.
/// They take an <see cref="System.Collections.Generic.IEnumerable{T}" />
/// receiver rather than a <see cref="Result{TOk,TErr}" />, so they share
/// nothing with the members below and live in
/// <see cref="ResultsCollectionExtensions" />.
/// </para>
/// </remarks>
#if !DEBUG
[DebuggerStepThrough]
#endif
[GenerateAwaitedReceivers(typeof(Result<,>))]
[GenerateAwaitedMember(nameof(Result<,>.And))]
[GenerateAwaitedMember(nameof(Result<,>.AndThen))]
[GenerateAwaitedMember(nameof(Result<,>.AndThenAsync))]
[GenerateAwaitedMember(nameof(Result<,>.AsEnumerable))]
[GenerateAwaitedMember(nameof(Result<,>.Expect))]
[GenerateAwaitedMember(nameof(Result<,>.ExpectErr))]
[GenerateAwaitedMember(
    nameof(Result<,>.GetErr),
    Summary = "Awaits the result and returns an option holding its error.")]
[GenerateAwaitedMember(
    nameof(Result<,>.GetOk),
    Summary = "Awaits the result and returns an option holding its success value.")]
[GenerateAwaitedMember(nameof(Result<,>.Inspect))]
[GenerateAwaitedMember(nameof(Result<,>.InspectAsync))]
[GenerateAwaitedMember(nameof(Result<,>.InspectErr))]
[GenerateAwaitedMember(nameof(Result<,>.InspectErrAsync))]
[GenerateAwaitedMember(nameof(Result<,>.IsErrAnd))]
[GenerateAwaitedMember(nameof(Result<,>.IsErrAndAsync))]
[GenerateAwaitedMember(nameof(Result<,>.IsOkAnd))]
[GenerateAwaitedMember(nameof(Result<,>.IsOkAndAsync))]
[GenerateAwaitedMember(nameof(Result<,>.Map))]
[GenerateAwaitedMember(nameof(Result<,>.MapAsync))]
[GenerateAwaitedMember(nameof(Result<,>.MapErr))]
[GenerateAwaitedMember(nameof(Result<,>.MapErrAsync))]
[GenerateAwaitedMember(nameof(Result<,>.MapOr))]
[GenerateAwaitedMember(nameof(Result<,>.MapOrAsync))]
[GenerateAwaitedMember(nameof(Result<,>.MapOrDefault))]
[GenerateAwaitedMember(nameof(Result<,>.MapOrDefaultAsync))]
[GenerateAwaitedMember(nameof(Result<,>.MapOrElse))]
[GenerateAwaitedMember(nameof(Result<,>.MapOrElseAsync))]
[GenerateAwaitedMember(nameof(Result<,>.MapOrNull))]
[GenerateAwaitedMember(nameof(Result<,>.MapOrNullAsync))]
[GenerateAwaitedMember(nameof(Result<,>.Match))]
[GenerateAwaitedMember(nameof(Result<,>.MatchAsync))]
[GenerateAwaitedMember(nameof(Result<,>.Or))]
[GenerateAwaitedMember(nameof(Result<,>.OrElse))]
[GenerateAwaitedMember(nameof(Result<,>.OrElseAsync))]
[GenerateAwaitedMember(nameof(Result<,>.Unwrap))]
[GenerateAwaitedMember(nameof(Result<,>.UnwrapErr))]
[GenerateAwaitedMember(nameof(Result<,>.UnwrapOr))]
[GenerateAwaitedMember(nameof(Result<,>.UnwrapOrDefault))]
[GenerateAwaitedMember(nameof(Result<,>.UnwrapOrElse))]
[GenerateAwaitedMember(nameof(Result<,>.UnwrapOrElseAsync))]
public static partial class ResultExtensions
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

    extension<TOk, TErr>(Result<TOk, TErr> result)
        where TOk : struct where TErr : notnull
    {
        /// <summary>
        /// Returns the contained value if the result is an
        /// <see cref="Ok{TOk,TErr}" />, otherwise <see langword="null" />.
        /// </summary>
        /// <remarks>
        /// Prefer this to <see cref="Result{TOk,TErr}.UnwrapOrDefault" /> when
        /// <typeparamref name="TOk" /> is a value type. <c>UnwrapOrDefault</c> returns
        /// the default of <typeparamref name="TOk" /> for an
        /// <see cref="Err{TOk,TErr}" />, which is indistinguishable from a legitimate
        /// zero.
        /// </remarks>
        /// <returns>
        /// The contained value if the result was an <see cref="Ok{TOk,TErr}" />,
        /// otherwise <see langword="null" />.
        /// </returns>
        public TOk? UnwrapOrNull() =>
            result.Match<TOk?>(value => value, _ => null);
    }
}
