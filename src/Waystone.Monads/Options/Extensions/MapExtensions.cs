namespace Waystone.Monads.Options.Extensions;

using System;
using System.Threading.Tasks;

/// <summary>
/// Applies an asynchronous map to an <see cref="Option{T}" />, and applies
/// <c>MapAsync</c> to an <see cref="Option{T}" /> that is still inside a
/// <see cref="Task{TResult}" /> or <see cref="ValueTask{TResult}" />.
/// </summary>
public static class MapExtensions
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

    extension<T>(Task<Option<T>> optionTask) where T : notnull
    {
        /// <summary>
        /// Awaits the <see cref="Task{TResult}" />, then awaits
        /// <paramref name="map" /> against the contained value if the option is a
        /// <see cref="Some{T}" />.
        /// </summary>
        /// <typeparam name="TOut">The type of the value in the resulting option.</typeparam>
        /// <param name="map">
        /// A function that asynchronously transforms the contained value. It is not
        /// invoked when the awaited option is a <see cref="None{T}" />.
        /// </param>
        /// <returns>
        /// A <see cref="ValueTask{TResult}" /> containing <see cref="Some{T}" /> of
        /// the mapped value if the awaited option was a <see cref="Some{T}" />,
        /// otherwise <see cref="None{T}" /> of <typeparamref name="TOut" />.
        /// </returns>
        public async ValueTask<Option<TOut>> MapAsync<TOut>(Func<T, Task<TOut>> map)
            where TOut : notnull
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            if (option.IsNone) return Option.None<TOut>();

            T some = option.Expect("Expected Some but found None.");

            TOut mapped = await map.Invoke(some).ConfigureAwait(false);

            return Option.Some(mapped);
        }

        /// <summary>
        /// Awaits the <see cref="Task{TResult}" />, then applies a synchronous
        /// <paramref name="map" /> to the contained value if the option is a
        /// <see cref="Some{T}" />.
        /// </summary>
        /// <typeparam name="TOut">
        /// The type of the value in the resulting
        /// <see cref="Option{TOut}" />.
        /// </typeparam>
        /// <param name="map">
        /// A function that transforms the contained value. It is not invoked when
        /// the awaited option is a <see cref="None{T}" />.
        /// </param>
        /// <returns>
        /// A <see cref="ValueTask{TResult}" /> containing <see cref="Some{T}" /> of
        /// the mapped value if the awaited option was a <see cref="Some{T}" />,
        /// otherwise <see cref="None{T}" /> of <typeparamref name="TOut" />.
        /// </returns>
        public async ValueTask<Option<TOut>> MapAsync<TOut>(Func<T, TOut> map)
            where TOut : notnull
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            if (option.IsNone) return Option.None<TOut>();

            T some = option.Expect("Expected Some but found None.");

            TOut mapped = map.Invoke(some);

            return Option.Some(mapped);
        }
    }

    extension<T>(ValueTask<Option<T>> optionTask) where T : notnull
    {
        /// <summary>
        /// Awaits the <see cref="ValueTask{TResult}" />, then awaits
        /// <paramref name="map" /> against the contained value if the option is a
        /// <see cref="Some{T}" />.
        /// </summary>
        /// <typeparam name="TOut">The type of the value in the resulting option.</typeparam>
        /// <param name="map">
        /// A function that asynchronously transforms the contained value. It is not
        /// invoked when the awaited option is a <see cref="None{T}" />.
        /// </param>
        /// <returns>
        /// A <see cref="ValueTask{TResult}" /> containing <see cref="Some{T}" /> of
        /// the mapped value if the awaited option was a <see cref="Some{T}" />,
        /// otherwise <see cref="None{T}" /> of <typeparamref name="TOut" />.
        /// </returns>
        public async ValueTask<Option<TOut>> MapAsync<TOut>(Func<T, Task<TOut>> map)
            where TOut : notnull
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            if (option.IsNone) return Option.None<TOut>();

            T some = option.Expect("Expected Some but found None.");

            TOut mapped = await map.Invoke(some).ConfigureAwait(false);

            return Option.Some(mapped);
        }

        /// <summary>
        /// Awaits the <see cref="ValueTask{TResult}" />, then applies a synchronous
        /// <paramref name="map" /> to the contained value if the option is a
        /// <see cref="Some{T}" />.
        /// </summary>
        /// <typeparam name="TOut">The type of the value in the resulting option.</typeparam>
        /// <param name="map">
        /// A function that transforms the contained value. It is not invoked when
        /// the awaited option is a <see cref="None{T}" />.
        /// </param>
        /// <returns>
        /// A <see cref="ValueTask{TResult}" /> containing <see cref="Some{T}" /> of
        /// the mapped value if the awaited option was a <see cref="Some{T}" />,
        /// otherwise <see cref="None{T}" /> of <typeparamref name="TOut" />.
        /// </returns>
        public async ValueTask<Option<TOut>> MapAsync<TOut>(Func<T, TOut> map)
            where TOut : notnull
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            if (option.IsNone) return Option.None<TOut>();

            T some = option.Expect("Expected Some but found None.");

            TOut mapped = map.Invoke(some);

            return Option.Some(mapped);
        }
    }
}
