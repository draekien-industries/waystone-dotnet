namespace Waystone.Monads.Options.Extensions;

using System;
using System.Threading.Tasks;
using Waystone.SourceGenerators;

/// <summary>
/// Applies an asynchronous map to an <see cref="Option{T}" />, and applies
/// <c>MapAsync</c> to an <see cref="Option{T}" /> that is still inside a
/// <see cref="Task{TResult}" /> or <see cref="ValueTask{TResult}" />.
/// </summary>
/// <remarks>
/// Only the overload below is hand-written. The awaited-receiver generator
/// lifts it, and every overload of <c>Option&lt;T&gt;.Map</c>, onto a
/// <see cref="Task{TResult}" /> and a <see cref="ValueTask{TResult}" />
/// receiver.
/// </remarks>
[GenerateAwaitedReceivers(typeof(Option<>))]
[GenerateAwaitedMember(nameof(Option<>.Map))]
public static partial class MapExtensions
{
    extension<T>(Option<T> option) where T : notnull
    {
        /// <summary>
        /// Awaits <paramref name="map" /> against the contained value if the option
        /// is a <see cref="Some{T}" />.
        /// </summary>
        /// <typeparam name="TOut">The type of the mapped value.</typeparam>
        /// <param name="map">
        /// A function that asynchronously transforms the contained value. It is not
        /// invoked when the option is a <see cref="None{T}" />.
        /// </param>
        /// <returns>
        /// A <see cref="ValueTask{TResult}" /> containing <see cref="Some{T}" /> of
        /// the mapped value if the option was a <see cref="Some{T}" />, otherwise
        /// <see cref="None{T}" /> of <typeparamref name="TOut" />.
        /// </returns>
        public async ValueTask<Option<TOut>> MapAsync<TOut>(
            Func<T, Task<TOut>> map)
            where TOut : notnull
        {
            if (option.IsNone) return Option.None<TOut>();

            T some = option.Expect("Expected Some but found None.");

            TOut mapped = await map.Invoke(some).ConfigureAwait(false);

            return Option.Some(mapped);
        }
    }
}
