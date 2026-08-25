namespace Waystone.Monads.Options.Extensions;

using System;
using System.Threading.Tasks;

/// <summary>
/// Provides <c>MapOrAsync</c> overloads for an <see cref="Option{T}" />, for the
/// cases the synchronous <see cref="Option{T}.MapOr{TOut}" /> cannot cover: an
/// asynchronous map function, a receiver still inside a task, or both.
/// </summary>
public static class MapOrExtensions
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

    extension<T>(Task<Option<T>> optionTask) where T : notnull
    {
        /// <summary>
        /// Awaits a task of <see cref="Option{T}" />, then applies a synchronous
        /// map function to a <see cref="Some{T}" /> value or returns a default for
        /// a <see cref="None{T}" />.
        /// </summary>
        /// <remarks>
        /// The map function is not invoked on a <see cref="None{T}" />. Only the
        /// receiver is awaited.
        /// </remarks>
        /// <typeparam name="TOut">The type of the output value.</typeparam>
        /// <param name="defaultValue">
        /// The value to return when the option is a <see cref="None{T}" />.
        /// </param>
        /// <param name="map">
        /// The transform applied to the <see cref="Some{T}" /> value.
        /// </param>
        /// <returns>
        /// The result of <paramref name="map" /> on a <see cref="Some{T}" />, or
        /// <paramref name="defaultValue" /> on a <see cref="None{T}" />.
        /// </returns>
        public async ValueTask<TOut> MapOrAsync<TOut>(
            TOut defaultValue,
            Func<T, TOut> map)
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            if (option.IsNone) return defaultValue;

            T some = option.Expect("Expected Some but found None.");

            return map.Invoke(some);
        }

        /// <summary>
        /// Awaits a task of <see cref="Option{T}" />, then applies an asynchronous
        /// map function to a <see cref="Some{T}" /> value or returns a default for
        /// a <see cref="None{T}" />.
        /// </summary>
        /// <remarks>
        /// The map function is not invoked on a <see cref="None{T}" />, so that
        /// path awaits the receiver only.
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
            Option<T> option = await optionTask.ConfigureAwait(false);

            if (option.IsNone) return defaultValue;

            T some = option.Expect("Expected Some but found None.");

            return await map.Invoke(some).ConfigureAwait(false);
        }
    }

    extension<T>(ValueTask<Option<T>> optionTask) where T : notnull
    {
        /// <summary>
        /// Awaits a value task of <see cref="Option{T}" />, then applies a
        /// synchronous map function to a <see cref="Some{T}" /> value or returns a
        /// default for a <see cref="None{T}" />.
        /// </summary>
        /// <remarks>
        /// The map function is not invoked on a <see cref="None{T}" />. Only the
        /// receiver is awaited.
        /// </remarks>
        /// <typeparam name="TOut">The type of the output value.</typeparam>
        /// <param name="defaultValue">
        /// The value to return when the option is a <see cref="None{T}" />.
        /// </param>
        /// <param name="map">
        /// The transform applied to the <see cref="Some{T}" /> value.
        /// </param>
        /// <returns>
        /// The result of <paramref name="map" /> on a <see cref="Some{T}" />, or
        /// <paramref name="defaultValue" /> on a <see cref="None{T}" />.
        /// </returns>
        public async ValueTask<TOut> MapOrAsync<TOut>(
            TOut defaultValue,
            Func<T, TOut> map)
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            if (option.IsNone) return defaultValue;

            T some = option.Expect("Expected Some but found None.");

            return map.Invoke(some);
        }

        /// <summary>
        /// Awaits a value task of <see cref="Option{T}" />, then applies an
        /// asynchronous map function to a <see cref="Some{T}" /> value or returns a
        /// default for a <see cref="None{T}" />.
        /// </summary>
        /// <remarks>
        /// The map function is not invoked on a <see cref="None{T}" />, so that
        /// path awaits the receiver only.
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
            Option<T> option = await optionTask.ConfigureAwait(false);

            if (option.IsNone) return defaultValue;

            T some = option.Expect("Expected Some but found None.");

            return await map.Invoke(some).ConfigureAwait(false);
        }
    }
}
