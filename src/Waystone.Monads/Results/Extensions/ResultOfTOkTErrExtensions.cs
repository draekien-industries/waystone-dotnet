namespace Waystone.Monads.Results.Extensions;

using Options;
using static Result;

/// <summary>Extension methods for <see cref="Result{TOk,TErr}" /></summary>
public static class ResultOfTOkTErrExtensions
{
    /// <summary>
    /// Converts from <c>Result&lt;Result&lt;TOk, TErr&gt;, TErr&gt;</c> to
    /// <c>Result&lt;TOk, TErr&gt;</c>
    /// </summary>
    /// <remarks>Flattening only removes one level of nesting at a time.</remarks>
    /// <param name="result">The result to flatten.</param>
    /// <typeparam name="TOk">The <see cref="Ok{TOk,TErr}" /> value type</typeparam>
    /// <typeparam name="TErr">The <see cref="Err{TOk,TErr}" /> value type</typeparam>
    public static Result<TOk, TErr> Flatten<TOk, TErr>(
        this Result<Result<TOk, TErr>, TErr> result)
        where TOk : notnull where TErr : notnull =>
        result.Match(
            inner => inner,
            Err<TOk, TErr>);

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
    public static Option<Result<TOk, TErr>> Transpose<TOk, TErr>(
        this Result<Option<TOk>, TErr> result)
        where TOk : notnull where TErr : notnull =>
        result.Match(
            option => option.Match(
                value => Option.Some(Ok<TOk, TErr>(value)),
                Option.None<Result<TOk, TErr>>),
            err => Option.Some(Err<TOk, TErr>(err)));
}
