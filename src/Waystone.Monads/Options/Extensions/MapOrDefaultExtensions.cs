namespace Waystone.Monads.Options.Extensions;

using System;
using System.Threading.Tasks;
using Waystone.SourceGenerators;

/// <summary>
/// Asynchronous <c>MapOrDefault</c> extensions for <see cref="Option{T}" />.
/// </summary>
[GenerateAwaitedReceivers(typeof(Option<>))]
[GenerateAwaitedMember(nameof(Option<>.MapOrDefault))]
public static partial class MapOrDefaultExtensions
{
    extension<T>(Option<T> option) where T : notnull
    {
        /// <summary>
        /// Awaits <paramref name="map" /> against the contained value if the option is
        /// a <see cref="Some{T}" />, otherwise returns the <see langword="default" /> of
        /// <typeparamref name="TOut" />.
        /// </summary>
        /// <remarks>
        /// <paramref name="map" /> is not invoked when the option is a
        /// <see cref="None{T}" />. When <typeparamref name="TOut" /> is a value type
        /// the returned default is indistinguishable from a mapped zero; use
        /// <c>MapOrNullAsync</c> if the caller has to tell the two apart.
        /// </remarks>
        /// <typeparam name="TOut">The type of the output value.</typeparam>
        /// <param name="map">
        /// Asynchronously transforms the contained value.
        /// </param>
        /// <returns>
        /// A <see cref="ValueTask{TResult}" /> containing the transformed value if the
        /// option was a <see cref="Some{T}" />, or the <see langword="default" /> of
        /// <typeparamref name="TOut" /> otherwise.
        /// </returns>
        public async ValueTask<TOut?> MapOrDefaultAsync<TOut>(
            Func<T, Task<TOut>> map)
            where TOut : notnull
        {
            if (option.IsNone) return default;

            T some = option.Expect("Expected Some but found None.");

            return await map.Invoke(some).ConfigureAwait(false);
        }
    }
}
