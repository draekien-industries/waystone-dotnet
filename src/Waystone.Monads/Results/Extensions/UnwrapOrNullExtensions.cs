namespace Waystone.Monads.Results.Extensions;

using Waystone.SourceGenerators;

/// <summary>
/// Unwraps a <see cref="Result{TOk,TErr}" /> with a value-type ok value to a
/// nullable, using <see langword="null" /> rather than
/// <see langword="default" /> for the error case.
/// </summary>
[GenerateAwaitedReceivers(typeof(Result<,>))]
public static partial class UnwrapOrNullExtensions
{
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
