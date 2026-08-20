namespace Waystone.Monads.Results.Extensions;

using System;
using System.Threading.Tasks;

public static class MapOrDefaultExtensions
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

    extension<TOk, TErr>(Task<Result<TOk, TErr>> resultTask)
        where TOk : notnull where TErr : notnull
    {
        /// <summary>
        /// Awaits the <see cref="Task{TResult}" /> and applies <paramref name="map" /> to
        /// the contained value if the result is an <see cref="Ok{TOk,TErr}" />, otherwise
        /// returns the <see langword="default" /> of <typeparamref name="TOut" />.
        /// </summary>
        /// <typeparam name="TOut">The mapped result value type</typeparam>
        /// <param name="map">
        /// A function to transform the <see cref="Ok{TOk,TErr}" />
        /// value.
        /// </param>
        /// <returns>
        /// A <see cref="ValueTask{TResult}" /> containing the transformed value if the awaited
        /// result was an <see cref="Ok{TOk,TErr}" />, or the <see langword="default" /> of
        /// <typeparamref name="TOut" /> otherwise.
        /// </returns>
        public async ValueTask<TOut?> MapOrDefaultAsync<TOut>(Func<TOk, TOut> map)
            where TOut : notnull
        {
            Result<TOk, TErr> result =
                await resultTask.ConfigureAwait(false);

            return result.MapOrDefault(map);
        }

        /// <summary>
        /// Awaits the <see cref="Task{TResult}" /> and awaits <paramref name="map" />
        /// against the contained value if the result is an <see cref="Ok{TOk,TErr}" />,
        /// otherwise returns the <see langword="default" /> of
        /// <typeparamref name="TOut" />.
        /// </summary>
        /// <typeparam name="TOut">The mapped result value type</typeparam>
        /// <param name="map">
        /// A function to asynchronously transform the
        /// <see cref="Ok{TOk,TErr}" /> value.
        /// </param>
        /// <returns>
        /// A <see cref="ValueTask{TResult}" /> containing the transformed value if the awaited
        /// result was an <see cref="Ok{TOk,TErr}" />, or the <see langword="default" /> of
        /// <typeparamref name="TOut" /> otherwise.
        /// </returns>
        public async ValueTask<TOut?> MapOrDefaultAsync<TOut>(
            Func<TOk, Task<TOut>> map)
            where TOut : notnull
        {
            Result<TOk, TErr> result =
                await resultTask.ConfigureAwait(false);

            return await result.MapOrDefaultAsync(map)
               .ConfigureAwait(false);
        }
    }

    extension<TOk, TErr>(ValueTask<Result<TOk, TErr>> resultTask)
        where TOk : notnull where TErr : notnull
    {
        /// <summary>
        /// Awaits the <see cref="ValueTask{TResult}" /> and applies
        /// <paramref name="map" /> to the contained value if the result is an
        /// <see cref="Ok{TOk,TErr}" />, otherwise returns the
        /// <see langword="default" /> of <typeparamref name="TOut" />.
        /// </summary>
        /// <typeparam name="TOut">The mapped result value type</typeparam>
        /// <param name="map">
        /// A function to transform the <see cref="Ok{TOk,TErr}" />
        /// value.
        /// </param>
        /// <returns>
        /// A <see cref="ValueTask{TResult}" /> containing the transformed value if the awaited
        /// result was an <see cref="Ok{TOk,TErr}" />, or the <see langword="default" /> of
        /// <typeparamref name="TOut" /> otherwise.
        /// </returns>
        public async ValueTask<TOut?> MapOrDefaultAsync<TOut>(Func<TOk, TOut> map)
            where TOut : notnull
        {
            Result<TOk, TErr> result =
                await resultTask.ConfigureAwait(false);

            return result.MapOrDefault(map);
        }

        /// <summary>
        /// Awaits the <see cref="ValueTask{TResult}" /> and awaits
        /// <paramref name="map" /> against the contained value if the result is an
        /// <see cref="Ok{TOk,TErr}" />, otherwise returns the
        /// <see langword="default" /> of <typeparamref name="TOut" />.
        /// </summary>
        /// <typeparam name="TOut">The mapped result value type</typeparam>
        /// <param name="map">
        /// A function to asynchronously transform the
        /// <see cref="Ok{TOk,TErr}" /> value.
        /// </param>
        /// <returns>
        /// A <see cref="ValueTask{TResult}" /> containing the transformed value if the awaited
        /// result was an <see cref="Ok{TOk,TErr}" />, or the <see langword="default" /> of
        /// <typeparamref name="TOut" /> otherwise.
        /// </returns>
        public async ValueTask<TOut?> MapOrDefaultAsync<TOut>(
            Func<TOk, Task<TOut>> map)
            where TOut : notnull
        {
            Result<TOk, TErr> result =
                await resultTask.ConfigureAwait(false);

            return await result.MapOrDefaultAsync(map)
               .ConfigureAwait(false);
        }
    }
}
