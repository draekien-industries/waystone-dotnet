namespace Waystone.Monads.Results.Extensions;

using System;
using System.Threading.Tasks;
using Waystone.SourceGenerators;

/// <summary>
/// Asynchronous <c>MapOrDefault</c> extensions for
/// <see cref="Result{TOk,TErr}" />.
/// </summary>
[GenerateAwaitedReceivers(typeof(Result<,>))]
[GenerateAwaitedMember(nameof(Result<,>.MapOrDefault))]
public static partial class MapOrDefaultExtensions
{
    extension<TOk, TErr>(Result<TOk, TErr> result)
        where TOk : notnull where TErr : notnull
    {
        /// <summary>
        /// Awaits <paramref name="map" /> against the contained value if the result is
        /// an <see cref="Ok{TOk,TErr}" />, otherwise returns the
        /// <see langword="default" /> of <typeparamref name="TOut" />.
        /// </summary>
        /// <remarks>
        /// <paramref name="map" /> is not invoked for an
        /// <see cref="Err{TOk,TErr}" />. When <typeparamref name="TOut" /> is a value
        /// type the returned default is indistinguishable from a mapped zero; use
        /// <c>MapOrNullAsync</c> if the caller must tell the two apart.
        /// </remarks>
        /// <typeparam name="TOut">The mapped result value type</typeparam>
        /// <param name="map">
        /// Asynchronously produces the mapped value from the contained ok value.
        /// </param>
        /// <returns>
        /// A <see cref="ValueTask{TResult}" /> completing with the mapped value, or
        /// with the <see langword="default" /> of <typeparamref name="TOut" /> if the
        /// result was an <see cref="Err{TOk,TErr}" />.
        /// </returns>
        public async ValueTask<TOut?> MapOrDefaultAsync<TOut>(
            Func<TOk, Task<TOut>> map)
            where TOut : notnull
        {
            if (result.IsErr) return default;

            TOk ok = result.Expect("Expected Ok but found Err.");

            return await map.Invoke(ok).ConfigureAwait(false);
        }
    }
}
