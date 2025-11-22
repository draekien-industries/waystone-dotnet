namespace Waystone.Monads.Options.Extensions;

using System;
using System.Threading.Tasks;

public static class MapOrExtensions
{
    extension<T>(Task<Option<T>> optionTask) where T : notnull
    {
        /// <summary>
        /// Maps an <see cref="Option{T}" /> to a value of type
        /// <typeparamref name="TOut" />
        /// by applying the given mapping function if the option is
        /// <see cref="Option{T}.IsSome" />,
        /// or returns a specified default value if the option is
        /// <see cref="Option{T}.IsNone" />.
        /// </summary>
        /// <typeparam name="TOut">The type of the output value.</typeparam>
        /// <param name="defaultValue">
        /// The default value to return if the option is
        /// <see cref="None{T}" />
        /// </param>
        /// <param name="map">
        /// A function to transform the value inside the option if it is
        /// <see cref="Option{T}.IsSome" />.
        /// </param>
        /// <returns>
        /// A task representing the operation. The task result is either the transformed
        /// value
        /// of type <typeparamref name="TOut" /> if the option is <see cref="Some{T}" />,
        /// or the provided default value if the option is <see cref="None{T}" />
        /// </returns>
        public async Task<TOut> MapOr<TOut>(
            TOut defaultValue,
            Func<T, TOut> map)
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            if (option.IsNone) return defaultValue;

            T some = option.Expect("Expected Some but found None.");

            return map.Invoke(some);
        }

        /// <summary>
        /// Maps an <see cref="Option{T}" /> wrapped in a task to a value of type
        /// <typeparamref name="TOut" />
        /// by applying the given mapping function asynchronously if the option is
        /// <see cref="Option{T}.IsSome" />,
        /// or returns a specified default value if the option is
        /// <see cref="Option{T}.IsNone" />.
        /// </summary>
        /// <typeparam name="TOut">The type of the output value.</typeparam>
        /// <param name="defaultValue">
        /// The default value to return if the option is
        /// <see cref="None{T}" />
        /// </param>
        /// <param name="map">
        /// A function to asynchronously transform the value inside the option if it is
        /// <see cref="Option{T}.IsSome" />.
        /// </param>
        /// <returns>
        /// A task representing the operation. The task result is either the asynchronously
        /// transformed value
        /// of type <typeparamref name="TOut" /> if the option is <see cref="Some{T}" />,
        /// or the provided default value if the option is <see cref="None{T}" />
        /// </returns>
        public async Task<TOut> MapOr<TOut>(
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
        /// Maps an awaited <see cref="Option{T}" /> to a value of type
        /// <typeparamref name="TOut" />
        /// by applying the given mapping function if the option is
        /// <see cref="Option{T}.IsSome" />,
        /// or returns a specified default value if the option is
        /// <see cref="Option{T}.IsNone" />.
        /// </summary>
        /// <typeparam name="TOut">The type of the output value.</typeparam>
        /// <param name="defaultValue">
        /// The default value to return if the option is
        /// <see cref="None{T}" />
        /// </param>
        /// <param name="map">
        /// A function to transform the value inside the option if it is
        /// <see cref="Option{T}.IsSome" />.
        /// </param>
        /// <returns>
        /// A task representing the operation. The task result is either the transformed
        /// value
        /// of type <typeparamref name="TOut" /> if the option is <see cref="Some{T}" />,
        /// or the provided default value if the option is <see cref="None{T}" />.
        /// </returns>
        public async Task<TOut> MapOr<TOut>(
            TOut defaultValue,
            Func<T, TOut> map)
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            if (option.IsNone) return defaultValue;

            T some = option.Expect("Expected Some but found None.");

            return map.Invoke(some);
        }

        /// <summary>
        /// Maps an <see cref="Option{T}" /> to a value of type
        /// <typeparamref name="TOut" />
        /// by applying the given mapping function if the option is
        /// <see cref="Option{T}.IsSome" />,
        /// or returns a specified default value if the option is
        /// <see cref="Option{T}.IsNone" />.
        /// </summary>
        /// <typeparam name="TOut">The type of the output value.</typeparam>
        /// <param name="defaultValue">
        /// The default value to return if the option is
        /// <see cref="None{T}" />
        /// </param>
        /// <param name="map">
        /// A function to transform the value inside the option if it is
        /// <see cref="Option{T}.IsSome" />.
        /// </param>
        /// <returns>
        /// A task representing the operation. The task result is either the transformed
        /// value
        /// of type <typeparamref name="TOut" /> if the option is <see cref="Some{T}" />,
        /// or the provided default value if the option is <see cref="None{T}" />
        /// </returns>
        public async Task<TOut> MapOr<TOut>(
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
