namespace Waystone.Monads.Results.Extensions;

using System;
using System.Threading.Tasks;
using Waystone.SourceGenerators;

/// <summary>
/// Maps a <see cref="Result{TOk,TErr}" /> to a nullable value type, using
/// <see langword="null" /> rather than <see langword="default" /> for the error
/// case.
/// </summary>
[GenerateAwaitedReceivers(typeof(Result<,>))]
public static partial class MapOrNullExtensions
{
    extension<TOk, TErr>(Result<TOk, TErr> result)
        where TOk : notnull where TErr : notnull
    {
        /// <summary>
        /// Applies <paramref name="map" /> to the contained value if the result is an
        /// <see cref="Ok{TOk,TErr}" />, otherwise returns <see langword="null" />.
        /// </summary>
        /// <remarks>
        /// Prefer this to <see cref="Result{TOk,TErr}.MapOrDefault{TOut}" /> when
        /// <typeparamref name="TOut" /> is a value type. <c>MapOrDefault</c> returns the
        /// default of <typeparamref name="TOut" /> for an <see cref="Err{TOk,TErr}" />,
        /// which is indistinguishable from a legitimate zero.
        /// </remarks>
        /// <typeparam name="TOut">The mapped result value type</typeparam>
        /// <param name="map">
        /// Produces the mapped value from the contained ok value. Not invoked for an
        /// <see cref="Err{TOk,TErr}" />.
        /// </param>
        /// <returns>
        /// The transformed value if the result was an <see cref="Ok{TOk,TErr}" />,
        /// otherwise <see langword="null" />.
        /// </returns>
        public TOut? MapOrNull<TOut>(Func<TOk, TOut> map) where TOut : struct =>
            result.Match<TOut?>(value => map.Invoke(value), _ => null);

        /// <summary>
        /// Awaits <paramref name="map" /> against the contained value if the result
        /// is an <see cref="Ok{TOk,TErr}" />, otherwise returns
        /// <see langword="null" />.
        /// </summary>
        /// <remarks>
        /// Prefer this to <c>MapOrDefaultAsync</c> when
        /// <typeparamref name="TOut" /> is a value type, for the same reason as the
        /// synchronous pair: a returned zero would be indistinguishable from a
        /// mapped one.
        /// </remarks>
        /// <typeparam name="TOut">The mapped result value type</typeparam>
        /// <param name="map">
        /// Asynchronously produces the mapped value from the contained ok value. Not
        /// invoked for an <see cref="Err{TOk,TErr}" />.
        /// </param>
        /// <returns>
        /// A <see cref="ValueTask{TResult}" /> completing with the mapped value, or
        /// with <see langword="null" /> if the result was an
        /// <see cref="Err{TOk,TErr}" />.
        /// </returns>
        public async ValueTask<TOut?> MapOrNullAsync<TOut>(
            Func<TOk, Task<TOut>> map)
            where TOut : struct
        {
            if (result.IsErr) return null;

            TOk ok = result.Expect("Expected Ok but found Err.");

            return await map.Invoke(ok).ConfigureAwait(false);
        }
    }
}
