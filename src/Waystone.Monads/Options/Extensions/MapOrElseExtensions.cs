namespace Waystone.Monads.Options.Extensions;

using System;
using System.Threading.Tasks;
using Waystone.SourceGenerators;

[GenerateAwaitedReceivers(typeof(Option<>))]
[GenerateAwaitedMember(nameof(Option<>.MapOrElse))]
public static partial class MapOrElseExtensions
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
}
