namespace Waystone.Monads.Options.Extensions;

using System;
using System.Threading.Tasks;

public static class MapOrElseExtensions
{
    extension<T>(Option<T> option) where T : notnull
    {
        public async ValueTask<TOut> MapOrElseAsync<TOut>(
            Func<Task<TOut>> defaultFactory,
            Func<T, Task<TOut>> map)
        {
            if (option.IsNone)
            {
                return await defaultFactory.Invoke().ConfigureAwait(false);
            }

            T some = option.Expect("Expected Some but found None.");

            return await map.Invoke(some).ConfigureAwait(false);
        }

        public async ValueTask<TOut> MapOrElseAsync<TOut>(
            Func<TOut> defaultFactory,
            Func<T, Task<TOut>> map)
        {
            if (option.IsNone)
            {
                return defaultFactory.Invoke();
            }

            T some = option.Expect("Expected Some but found None.");

            return await map.Invoke(some).ConfigureAwait(false);
        }

        public async ValueTask<TOut> MapOrElseAsync<TOut>(
            Func<Task<TOut>> defaultFactory,
            Func<T, TOut> map)
        {
            if (option.IsNone)
            {
                return await defaultFactory.Invoke().ConfigureAwait(false);
            }

            T some = option.Expect("Expected Some but found None.");

            return map.Invoke(some);
        }
    }

    extension<T>(Task<Option<T>> optionTask) where T : notnull
    {
        public async ValueTask<TOut> MapOrElseAsync<TOut>(
            Func<TOut> defaultFactory,
            Func<T, TOut> map)
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            if (option.IsNone)
            {
                return defaultFactory.Invoke();
            }

            T some = option.Expect("Expected Some but found None.");

            return map.Invoke(some);
        }

        public async ValueTask<TOut> MapOrElseAsync<TOut>(
            Func<Task<TOut>> defaultFactory,
            Func<T, Task<TOut>> map)
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            if (option.IsNone)
            {
                return await defaultFactory.Invoke().ConfigureAwait(false);
            }

            T some = option.Expect("Expected Some but found None.");

            return await map.Invoke(some).ConfigureAwait(false);
        }

        public async ValueTask<TOut> MapOrElseAsync<TOut>(
            Func<TOut> defaultFactory,
            Func<T, Task<TOut>> map)
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            if (option.IsNone)
            {
                return defaultFactory.Invoke();
            }

            T some = option.Expect("Expected Some but found None.");

            return await map.Invoke(some).ConfigureAwait(false);
        }

        public async ValueTask<TOut> MapOrElseAsync<TOut>(
            Func<Task<TOut>> defaultFactory,
            Func<T, TOut> map)
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            if (option.IsNone)
            {
                return await defaultFactory.Invoke().ConfigureAwait(false);
            }

            T some = option.Expect("Expected Some but found None.");

            return map.Invoke(some);
        }
    }

    extension<T>(ValueTask<Option<T>> optionTask) where T : notnull
    {
        public async ValueTask<TOut> MapOrElseAsync<TOut>(
            Func<TOut> defaultFactory,
            Func<T, TOut> map)
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            if (option.IsNone)
            {
                return defaultFactory.Invoke();
            }

            T some = option.Expect("Expected Some but found None.");

            return map.Invoke(some);
        }

        public async ValueTask<TOut> MapOrElseAsync<TOut>(
            Func<Task<TOut>> defaultFactory,
            Func<T, Task<TOut>> map)
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            if (option.IsNone)
            {
                return await defaultFactory.Invoke().ConfigureAwait(false);
            }

            T some = option.Expect("Expected Some but found None.");

            return await map.Invoke(some).ConfigureAwait(false);
        }

        public async ValueTask<TOut> MapOrElseAsync<TOut>(
            Func<TOut> defaultFactory,
            Func<T, Task<TOut>> map)
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            if (option.IsNone)
            {
                return defaultFactory.Invoke();
            }

            T some = option.Expect("Expected Some but found None.");

            return await map.Invoke(some).ConfigureAwait(false);
        }

        public async ValueTask<TOut> MapOrElseAsync<TOut>(
            Func<Task<TOut>> defaultFactory,
            Func<T, TOut> map)
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            if (option.IsNone)
            {
                return await defaultFactory.Invoke().ConfigureAwait(false);
            }

            T some = option.Expect("Expected Some but found None.");

            return map.Invoke(some);
        }
    }
}
