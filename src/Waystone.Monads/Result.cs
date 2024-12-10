namespace Waystone.Monads;

/// <summary>Static methods for <see cref="IResult{TOk,TErr}" /></summary>
public static class Result
{
    /// <summary>
    /// Creates an <see cref="Ok{TOk,TErr}" /> result containing the provided
    /// value.
    /// </summary>
    /// <param name="value">The value of the result type.</param>
    public static IResult<TOk, TErr> Ok<TOk, TErr>(TOk value)
        where TOk : notnull
        where TErr : notnull =>
        new Ok<TOk, TErr>(value);

    /// <summary>
    /// Creates an <see cref="Err{TOk,TErr}" /> result containing the provided
    /// value.
    /// </summary>
    /// <param name="value">The value of the result type.</param>
    public static IResult<TOk, TErr> Err<TOk, TErr>(TErr value)
        where TOk : notnull
        where TErr : notnull =>
        new Err<TOk, TErr>(value);

    /// <summary>
    /// Converts from <c>IResult&lt;IResult&lt;TOk, TErr&gt;, TErr&gt;</c> to
    /// <c>IResult&lt;TOk, TErr&gt;</c>
    /// </summary>
    /// <remarks>Flattening only removes one level of nesting at a time.</remarks>
    /// <param name="result">The result to flatten.</param>
    /// <typeparam name="TOk">The <see cref="Ok{TOk,TErr}" /> value type</typeparam>
    /// <typeparam name="TErr">The <see cref="Err{TOk,TErr}" /> value type</typeparam>
    public static IResult<TOk, TErr> Flatten<TOk, TErr>(
        this IResult<IResult<TOk, TErr>, TErr> result)
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
    public static IOption<IResult<TOk, TErr>> Transpose<TOk, TErr>(
        this IResult<IOption<TOk>, TErr> result)
        where TOk : notnull where TErr : notnull =>
        result.Match(
            option => option.Match(
                value => Option.Some(Ok<TOk, TErr>(value)),
                Option.None<IResult<TOk, TErr>>),
            err => Option.Some(Err<TOk, TErr>(err)));
}
