namespace Waystone.Monads.Options.Extensions;

using System;
using System.Threading.Tasks;
using Waystone.SourceGenerators;

/// <summary>
/// Provides <c>MapOrAsync</c> overloads for an <see cref="Option{T}" />, for the
/// cases the synchronous <see cref="Option{T}.MapOr{TOut}" /> cannot cover: an
/// asynchronous map function, a receiver still inside a task, or both.
/// </summary>
/// <remarks>
/// Only the asynchronous-map overload below is hand-written. The awaited-receiver
/// generator lifts it, and every overload of
/// <see cref="Option{T}.MapOr{TOut}" />, onto a <see cref="Task{TResult}" /> and
/// a <see cref="ValueTask{TResult}" /> receiver.
/// </remarks>
[GenerateAwaitedReceivers(typeof(Option<>))]
[GenerateAwaitedMember(nameof(Option<>.MapOr))]
public static partial class MapOrExtensions
{
    extension<T>(Option<T> option) where T : notnull
    {
        /// <summary>
        /// Applies an asynchronous map function to the contained
        /// <see cref="Some{T}" /> value, or returns a default without awaiting
        /// anything when the option is a <see cref="None{T}" />.
        /// </summary>
        /// <remarks>
        /// The map function is not invoked on a <see cref="None{T}" />. This is the
        /// overload to pick over <see cref="Option{T}.MapOr{TOut}" /> when the map
        /// itself is asynchronous; the receiver is already available, so nothing is
        /// awaited on the <see cref="None{T}" /> path.
        /// </remarks>
        /// <typeparam name="TOut">The type of the output value.</typeparam>
        /// <param name="defaultValue">
        /// The value to return when the option is a <see cref="None{T}" />.
        /// </param>
        /// <param name="map">
        /// The asynchronous transform applied to the <see cref="Some{T}" /> value.
        /// </param>
        /// <returns>
        /// The awaited result of <paramref name="map" /> on a
        /// <see cref="Some{T}" />, or <paramref name="defaultValue" /> on a
        /// <see cref="None{T}" />.
        /// </returns>
        public async ValueTask<TOut> MapOrAsync<TOut>(
            TOut defaultValue,
            Func<T, Task<TOut>> map)
        {
            if (option.IsNone) return defaultValue;

            T some = option.Expect("Expected Some but found None.");

            return await map.Invoke(some).ConfigureAwait(false);
        }
    }
}
