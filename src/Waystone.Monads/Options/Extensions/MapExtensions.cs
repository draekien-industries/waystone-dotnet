namespace Waystone.Monads.Options.Extensions;

using System;
using System.Threading.Tasks;

public static class MapExtensions
{
    extension<T>(Option<T> option) where T : notnull
    {
        public async ValueTask<Option<TOut>> Map<TOut>(Func<T, Task<TOut>> map)
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
        /// Asynchronously transforms the value contained in the <see cref="Option{T}" />
        /// instance using the provided mapping function.
        /// </summary>
        /// <typeparam name="TOut">The type of the value in the resulting option.</typeparam>
        /// <param name="map">
        /// A function that takes the value inside the option and returns
        /// a task containing the transformed value.
        /// </param>
        /// <returns>
        /// A task containing an <see cref="Option{TOut}" /> with the transformed value if
        /// the original option was in a "Some" state; otherwise, an empty option.
        /// </returns>
        public async Task<Option<TOut>> Map<TOut>(Func<T, Task<TOut>> map)
            where TOut : notnull
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            if (option.IsNone) return Option.None<TOut>();

            T some = option.Expect("Expected Some but found None.");

            TOut mapped = await map.Invoke(some).ConfigureAwait(false);

            return Option.Some(mapped);
        }

        /// <summary>
        /// Asynchronously transforms the value contained in the <see cref="Option{T}" />
        /// instance using the provided mapping function.
        /// </summary>
        /// <typeparam name="TOut">
        /// The type of the value in the resulting
        /// <see cref="Option{TOut}" />.
        /// </typeparam>
        /// <param name="map">
        /// A function that takes the value inside the option and returns a task containing
        /// the transformed value.
        /// </param>
        /// <returns>
        /// A task containing an <see cref="Option{TOut}" /> with the transformed value if
        /// the original option was in a "Some" state; otherwise, an empty option.
        /// </returns>
        public async Task<Option<TOut>> Map<TOut>(Func<T, TOut> map)
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
        /// Asynchronously transforms the value contained in the <see cref="Option{T}" />
        /// instance using the provided mapping function.
        /// </summary>
        /// <typeparam name="TOut">The type of the value in the resulting option.</typeparam>
        /// <param name="map">
        /// A function that takes the value inside the option and returns
        /// a task containing the transformed value.
        /// </param>
        /// <returns>
        /// A task containing an <see cref="Option{TOut}" /> with the transformed value if
        /// the original <see cref="Option{T}" /> was in a "Some" state; otherwise, an
        /// empty option.
        /// </returns>
        public async Task<Option<TOut>> Map<TOut>(Func<T, Task<TOut>> map)
            where TOut : notnull
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            if (option.IsNone) return Option.None<TOut>();

            T some = option.Expect("Expected Some but found None.");

            TOut mapped = await map.Invoke(some).ConfigureAwait(false);

            return Option.Some(mapped);
        }

        /// <summary>
        /// Asynchronously transforms the value contained within the
        /// <see cref="Option{T}" />
        /// returned by the <see cref="ValueTask{T}" /> using the provided mapping
        /// function.
        /// </summary>
        /// <typeparam name="TOut">The type of the value in the resulting option.</typeparam>
        /// <param name="map">
        /// A function that takes the value inside the option and maps it to a new value.
        /// </param>
        /// <returns>
        /// A task that, when completed, contains an <see cref="Option{TOut}" /> with the
        /// transformed value
        /// if the original option was in a "Some" state; otherwise, an empty option.
        /// </returns>
        public async Task<Option<TOut>> Map<TOut>(Func<T, TOut> map)
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
