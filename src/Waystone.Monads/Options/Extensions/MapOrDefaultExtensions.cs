namespace Waystone.Monads.Options.Extensions;

using System;
using System.Threading.Tasks;

public static class MapOrDefaultExtensions
{
    extension<T>(Option<T> option) where T : notnull
    {
        /// <summary>
        /// Awaits <paramref name="map" /> against the contained value if the option is
        /// a <see cref="Some{T}" />, otherwise returns the <see langword="default" /> of
        /// <typeparamref name="TOut" />.
        /// </summary>
        /// <typeparam name="TOut">The type of the output value.</typeparam>
        /// <param name="map">
        /// A function to asynchronously transform the value inside the
        /// option if it is a <see cref="Some{T}" />.
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

    extension<T>(Task<Option<T>> optionTask) where T : notnull
    {
        /// <summary>
        /// Awaits the <see cref="Task{TResult}" /> and applies <paramref name="map" /> to
        /// the contained value if the option is a <see cref="Some{T}" />, otherwise
        /// returns the <see langword="default" /> of <typeparamref name="TOut" />.
        /// </summary>
        /// <typeparam name="TOut">The type of the output value.</typeparam>
        /// <param name="map">
        /// A function to transform the value inside the option if it is
        /// a <see cref="Some{T}" />.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}" /> containing the transformed value if the awaited
        /// option was a <see cref="Some{T}" />, or the <see langword="default" /> of
        /// <typeparamref name="TOut" /> otherwise.
        /// </returns>
        public async Task<TOut?> MapOrDefaultAsync<TOut>(Func<T, TOut> map)
            where TOut : notnull
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            return option.MapOrDefault(map);
        }

        /// <summary>
        /// Awaits the <see cref="Task{TResult}" /> and awaits <paramref name="map" />
        /// against the contained value if the option is a <see cref="Some{T}" />,
        /// otherwise returns the <see langword="default" /> of
        /// <typeparamref name="TOut" />.
        /// </summary>
        /// <typeparam name="TOut">The type of the output value.</typeparam>
        /// <param name="map">
        /// A function to asynchronously transform the value inside the
        /// option if it is a <see cref="Some{T}" />.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}" /> containing the transformed value if the awaited
        /// option was a <see cref="Some{T}" />, or the <see langword="default" /> of
        /// <typeparamref name="TOut" /> otherwise.
        /// </returns>
        public async Task<TOut?> MapOrDefaultAsync<TOut>(
            Func<T, Task<TOut>> map)
            where TOut : notnull
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            if (option.IsNone) return default;

            T some = option.Expect("Expected Some but found None.");

            return await map.Invoke(some).ConfigureAwait(false);
        }
    }

    extension<T>(ValueTask<Option<T>> optionTask) where T : notnull
    {
        /// <summary>
        /// Awaits the <see cref="ValueTask{TResult}" /> and applies
        /// <paramref name="map" /> to the contained value if the option is a
        /// <see cref="Some{T}" />, otherwise returns the <see langword="default" /> of
        /// <typeparamref name="TOut" />.
        /// </summary>
        /// <typeparam name="TOut">The type of the output value.</typeparam>
        /// <param name="map">
        /// A function to transform the value inside the option if it is
        /// a <see cref="Some{T}" />.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}" /> containing the transformed value if the awaited
        /// option was a <see cref="Some{T}" />, or the <see langword="default" /> of
        /// <typeparamref name="TOut" /> otherwise.
        /// </returns>
        public async Task<TOut?> MapOrDefaultAsync<TOut>(Func<T, TOut> map)
            where TOut : notnull
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            return option.MapOrDefault(map);
        }

        /// <summary>
        /// Awaits the <see cref="ValueTask{TResult}" /> and awaits
        /// <paramref name="map" /> against the contained value if the option is a
        /// <see cref="Some{T}" />, otherwise returns the <see langword="default" /> of
        /// <typeparamref name="TOut" />.
        /// </summary>
        /// <typeparam name="TOut">The type of the output value.</typeparam>
        /// <param name="map">
        /// A function to asynchronously transform the value inside the
        /// option if it is a <see cref="Some{T}" />.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}" /> containing the transformed value if the awaited
        /// option was a <see cref="Some{T}" />, or the <see langword="default" /> of
        /// <typeparamref name="TOut" /> otherwise.
        /// </returns>
        public async Task<TOut?> MapOrDefaultAsync<TOut>(
            Func<T, Task<TOut>> map)
            where TOut : notnull
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            if (option.IsNone) return default;

            T some = option.Expect("Expected Some but found None.");

            return await map.Invoke(some).ConfigureAwait(false);
        }
    }
}
