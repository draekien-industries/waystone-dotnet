namespace Waystone.Monads.Results.Extensions;

using System;
using System.Threading.Tasks;
using Waystone.SourceGenerators;

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
        /// <typeparam name="TOut">The mapped result value type</typeparam>
        /// <param name="map">
        /// A function to asynchronously transform the
        /// <see cref="Ok{TOk,TErr}" /> value.
        /// </param>
        /// <returns>
        /// A <see cref="ValueTask{TResult}" /> containing the transformed value if the
        /// result was an <see cref="Ok{TOk,TErr}" />, or the <see langword="default" /> of
        /// <typeparamref name="TOut" /> otherwise.
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
