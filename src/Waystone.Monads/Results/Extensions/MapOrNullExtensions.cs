namespace Waystone.Monads.Results.Extensions;

using System;
using System.Threading.Tasks;

public static class MapOrNullExtensions
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
        /// A function to transform the <see cref="Ok{TOk,TErr}" />
        /// value.
        /// </param>
        /// <returns>
        /// The transformed value if the result was an <see cref="Ok{TOk,TErr}" />,
        /// otherwise <see langword="null" />.
        /// </returns>
        public TOut? MapOrNull<TOut>(Func<TOk, TOut> map) where TOut : struct =>
            result.Match<TOut?>(value => map.Invoke(value), _ => null);

        /// <summary>
        /// Awaits <paramref name="map" /> against the contained value if the result is
        /// an <see cref="Ok{TOk,TErr}" />, otherwise returns <see langword="null" />.
        /// </summary>
        /// <typeparam name="TOut">The mapped result value type</typeparam>
        /// <param name="map">
        /// A function to asynchronously transform the
        /// <see cref="Ok{TOk,TErr}" /> value.
        /// </param>
        /// <returns>
        /// A <see cref="ValueTask{TResult}" /> containing the transformed value if the
        /// result was an <see cref="Ok{TOk,TErr}" />, otherwise
        /// <see langword="null" />.
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

    extension<TOk, TErr>(Task<Result<TOk, TErr>> resultTask)
        where TOk : notnull where TErr : notnull
    {
        /// <summary>
        /// Awaits the <see cref="Task{TResult}" /> and applies <paramref name="map" /> to
        /// the contained value if the result is an <see cref="Ok{TOk,TErr}" />, otherwise
        /// returns <see langword="null" />.
        /// </summary>
        /// <typeparam name="TOut">The mapped result value type</typeparam>
        /// <param name="map">
        /// A function to transform the <see cref="Ok{TOk,TErr}" />
        /// value.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}" /> containing the transformed value if the awaited
        /// result was an <see cref="Ok{TOk,TErr}" />, otherwise
        /// <see langword="null" />.
        /// </returns>
        public async Task<TOut?> MapOrNullAsync<TOut>(Func<TOk, TOut> map)
            where TOut : struct
        {
            Result<TOk, TErr> result = await resultTask.ConfigureAwait(false);

            return result.MapOrNull(map);
        }

        /// <summary>
        /// Awaits the <see cref="Task{TResult}" /> and awaits <paramref name="map" />
        /// against the contained value if the result is an <see cref="Ok{TOk,TErr}" />,
        /// otherwise returns <see langword="null" />.
        /// </summary>
        /// <typeparam name="TOut">The mapped result value type</typeparam>
        /// <param name="map">
        /// A function to asynchronously transform the
        /// <see cref="Ok{TOk,TErr}" /> value.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}" /> containing the transformed value if the awaited
        /// result was an <see cref="Ok{TOk,TErr}" />, otherwise
        /// <see langword="null" />.
        /// </returns>
        public async Task<TOut?> MapOrNullAsync<TOut>(Func<TOk, Task<TOut>> map)
            where TOut : struct
        {
            Result<TOk, TErr> result = await resultTask.ConfigureAwait(false);

            if (result.IsErr) return null;

            TOk ok = result.Expect("Expected Ok but found Err.");

            return await map.Invoke(ok).ConfigureAwait(false);
        }
    }

    extension<TOk, TErr>(ValueTask<Result<TOk, TErr>> resultTask)
        where TOk : notnull where TErr : notnull
    {
        /// <summary>
        /// Awaits the <see cref="ValueTask{TResult}" /> and applies
        /// <paramref name="map" /> to the contained value if the result is an
        /// <see cref="Ok{TOk,TErr}" />, otherwise returns <see langword="null" />.
        /// </summary>
        /// <typeparam name="TOut">The mapped result value type</typeparam>
        /// <param name="map">
        /// A function to transform the <see cref="Ok{TOk,TErr}" />
        /// value.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}" /> containing the transformed value if the awaited
        /// result was an <see cref="Ok{TOk,TErr}" />, otherwise
        /// <see langword="null" />.
        /// </returns>
        public async Task<TOut?> MapOrNullAsync<TOut>(Func<TOk, TOut> map)
            where TOut : struct
        {
            Result<TOk, TErr> result = await resultTask.ConfigureAwait(false);

            return result.MapOrNull(map);
        }

        /// <summary>
        /// Awaits the <see cref="ValueTask{TResult}" /> and awaits
        /// <paramref name="map" /> against the contained value if the result is an
        /// <see cref="Ok{TOk,TErr}" />, otherwise returns <see langword="null" />.
        /// </summary>
        /// <typeparam name="TOut">The mapped result value type</typeparam>
        /// <param name="map">
        /// A function to asynchronously transform the
        /// <see cref="Ok{TOk,TErr}" /> value.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}" /> containing the transformed value if the awaited
        /// result was an <see cref="Ok{TOk,TErr}" />, otherwise
        /// <see langword="null" />.
        /// </returns>
        public async Task<TOut?> MapOrNullAsync<TOut>(Func<TOk, Task<TOut>> map)
            where TOut : struct
        {
            Result<TOk, TErr> result = await resultTask.ConfigureAwait(false);

            if (result.IsErr) return null;

            TOk ok = result.Expect("Expected Ok but found Err.");

            return await map.Invoke(ok).ConfigureAwait(false);
        }
    }
}
