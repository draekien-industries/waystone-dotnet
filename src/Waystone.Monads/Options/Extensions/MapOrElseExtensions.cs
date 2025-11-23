namespace Waystone.Monads.Options.Extensions;

using System;
using System.Threading.Tasks;

public static class MapOrElseExtensions
{
    extension<T>(Task<Option<T>> optionTask) where T : notnull
    {
        public async Task<TOut> MapOrElse<TOut>(
            Func<TOut> defaultFunc,
            Func<T, TOut> map)
        {
            Option<T>? option = await optionTask.ConfigureAwait(false);

            if (option.IsNone)
            {
                return defaultFunc.Invoke();
            }

            T some = option.Expect("Expected Some but found None.");

            return map.Invoke(some);
        }

        public async Task<TOut> MapOrElse<TOut>(
            Func<Task<TOut>> defaultFunc,
            Func<T, Task<TOut>> map)
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            if (option.IsNone)
            {
                return await defaultFunc.Invoke().ConfigureAwait(false);
            }

            T some = option.Expect("Expected Some but found None.");

            return await map.Invoke(some).ConfigureAwait(false);
        }

        public async Task<TOut> MapOrElse<TOut>(
            Func<TOut> defaultFunc,
            Func<T, Task<TOut>> map)
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            if (option.IsNone)
            {
                return defaultFunc.Invoke();
            }

            T some = option.Expect("Expected Some but found None.");

            return await map.Invoke(some).ConfigureAwait(false);
        }

        public async Task<TOut> MapOrElse<TOut>(
            Func<Task<TOut>> defaultFunc,
            Func<T, TOut> map)
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            if (option.IsNone)
            {
                return await defaultFunc.Invoke().ConfigureAwait(false);
            }

            T some = option.Expect("Expected Some but found None.");

            return map.Invoke(some);
        }
    }

    extension<T>(ValueTask<Option<T>> optionTask) where T : notnull
    {
        public async Task<TOut> MapOrElse<TOut>(
            Func<TOut> defaultFunc,
            Func<T, TOut> map)
        {
            Option<T>? option = await optionTask.ConfigureAwait(false);

            if (option.IsNone)
            {
                return defaultFunc.Invoke();
            }

            T some = option.Expect("Expected Some but found None.");

            return map.Invoke(some);
        }

        public async Task<TOut> MapOrElse<TOut>(
            Func<Task<TOut>> defaultFunc,
            Func<T, Task<TOut>> map)
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            if (option.IsNone)
            {
                return await defaultFunc.Invoke().ConfigureAwait(false);
            }

            T some = option.Expect("Expected Some but found None.");

            return await map.Invoke(some).ConfigureAwait(false);
        }

        public async Task<TOut> MapOrElse<TOut>(
            Func<TOut> defaultFunc,
            Func<T, Task<TOut>> map)
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            if (option.IsNone)
            {
                return defaultFunc.Invoke();
            }

            T some = option.Expect("Expected Some but found None.");

            return await map.Invoke(some).ConfigureAwait(false);
        }

        public async Task<TOut> MapOrElse<TOut>(
            Func<Task<TOut>> defaultFunc,
            Func<T, TOut> map)
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            if (option.IsNone)
            {
                return await defaultFunc.Invoke().ConfigureAwait(false);
            }

            T some = option.Expect("Expected Some but found None.");

            return map.Invoke(some);
        }
    }
}
