namespace Waystone.Monads.Options.Extensions;

using System;
using System.Threading.Tasks;

public static class MapOrNullExtensions
{
    extension<T>(Option<T> option) where T : notnull
    {
        /// <summary>
        /// Applies <paramref name="map" /> to the contained value if the option is a
        /// <see cref="Some{T}" />, otherwise returns <see langword="null" />.
        /// </summary>
        /// <remarks>
        /// Prefer this to <see cref="Option{T}.MapOrDefault{T2}" /> when
        /// <typeparamref name="TOut" /> is a value type. <c>MapOrDefault</c> returns the
        /// default of <typeparamref name="TOut" /> for a <see cref="None{T}" />, which is
        /// indistinguishable from a legitimate zero.
        /// </remarks>
        /// <typeparam name="TOut">The type of the output value.</typeparam>
        /// <param name="map">
        /// A function to transform the value inside the option if it is
        /// a <see cref="Some{T}" />.
        /// </param>
        /// <returns>
        /// The transformed value if the option was a <see cref="Some{T}" />,
        /// otherwise <see langword="null" />.
        /// </returns>
        public TOut? MapOrNull<TOut>(Func<T, TOut> map) where TOut : struct =>
            option.Match<TOut?>(value => map.Invoke(value), () => null);

        /// <summary>
        /// Awaits <paramref name="map" /> against the contained value if the option is
        /// a <see cref="Some{T}" />, otherwise returns <see langword="null" />.
        /// </summary>
        /// <typeparam name="TOut">The type of the output value.</typeparam>
        /// <param name="map">
        /// A function to asynchronously transform the value inside the
        /// option if it is a <see cref="Some{T}" />.
        /// </param>
        /// <returns>
        /// A <see cref="ValueTask{TResult}" /> containing the transformed value if the
        /// option was a <see cref="Some{T}" />, otherwise <see langword="null" />.
        /// </returns>
        public async ValueTask<TOut?> MapOrNullAsync<TOut>(
            Func<T, Task<TOut>> map)
            where TOut : struct
        {
            if (option.IsNone) return null;

            T some = option.Expect("Expected Some but found None.");

            return await map.Invoke(some).ConfigureAwait(false);
        }
    }

    extension<T>(Task<Option<T>> optionTask) where T : notnull
    {
        /// <summary>
        /// Awaits the <see cref="Task{TResult}" /> and applies <paramref name="map" /> to
        /// the contained value if the option is a <see cref="Some{T}" />, otherwise
        /// returns <see langword="null" />.
        /// </summary>
        /// <typeparam name="TOut">The type of the output value.</typeparam>
        /// <param name="map">
        /// A function to transform the value inside the option if it is
        /// a <see cref="Some{T}" />.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}" /> containing the transformed value if the awaited
        /// option was a <see cref="Some{T}" />, otherwise <see langword="null" />.
        /// </returns>
        public async Task<TOut?> MapOrNullAsync<TOut>(Func<T, TOut> map)
            where TOut : struct
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            return option.MapOrNull(map);
        }

        /// <summary>
        /// Awaits the <see cref="Task{TResult}" /> and awaits <paramref name="map" />
        /// against the contained value if the option is a <see cref="Some{T}" />,
        /// otherwise returns <see langword="null" />.
        /// </summary>
        /// <typeparam name="TOut">The type of the output value.</typeparam>
        /// <param name="map">
        /// A function to asynchronously transform the value inside the
        /// option if it is a <see cref="Some{T}" />.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}" /> containing the transformed value if the awaited
        /// option was a <see cref="Some{T}" />, otherwise <see langword="null" />.
        /// </returns>
        public async Task<TOut?> MapOrNullAsync<TOut>(Func<T, Task<TOut>> map)
            where TOut : struct
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            if (option.IsNone) return null;

            T some = option.Expect("Expected Some but found None.");

            return await map.Invoke(some).ConfigureAwait(false);
        }
    }

    extension<T>(ValueTask<Option<T>> optionTask) where T : notnull
    {
        /// <summary>
        /// Awaits the <see cref="ValueTask{TResult}" /> and applies
        /// <paramref name="map" /> to the contained value if the option is a
        /// <see cref="Some{T}" />, otherwise returns <see langword="null" />.
        /// </summary>
        /// <typeparam name="TOut">The type of the output value.</typeparam>
        /// <param name="map">
        /// A function to transform the value inside the option if it is
        /// a <see cref="Some{T}" />.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}" /> containing the transformed value if the awaited
        /// option was a <see cref="Some{T}" />, otherwise <see langword="null" />.
        /// </returns>
        public async Task<TOut?> MapOrNullAsync<TOut>(Func<T, TOut> map)
            where TOut : struct
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            return option.MapOrNull(map);
        }

        /// <summary>
        /// Awaits the <see cref="ValueTask{TResult}" /> and awaits
        /// <paramref name="map" /> against the contained value if the option is a
        /// <see cref="Some{T}" />, otherwise returns <see langword="null" />.
        /// </summary>
        /// <typeparam name="TOut">The type of the output value.</typeparam>
        /// <param name="map">
        /// A function to asynchronously transform the value inside the
        /// option if it is a <see cref="Some{T}" />.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}" /> containing the transformed value if the awaited
        /// option was a <see cref="Some{T}" />, otherwise <see langword="null" />.
        /// </returns>
        public async Task<TOut?> MapOrNullAsync<TOut>(Func<T, Task<TOut>> map)
            where TOut : struct
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            if (option.IsNone) return null;

            T some = option.Expect("Expected Some but found None.");

            return await map.Invoke(some).ConfigureAwait(false);
        }
    }
}
