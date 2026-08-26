namespace Waystone.Monads.Options.Extensions;

using System;
using System.Threading.Tasks;

public static class OrElseExtensions
{
    extension<T>(Option<T> option) where T : notnull
    {
        public async ValueTask<Option<T>> OrElseAsync(
            Func<ValueTask<Option<T>>> optionFactory)
        {
            if (option.IsSome) return option;

            return await optionFactory.Invoke().ConfigureAwait(false);
        }
    }

    extension<T>(Task<Option<T>> optionTask) where T : notnull
    {
        public async ValueTask<Option<T>> OrElseAsync(Func<Option<T>> optionFactory)
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            return option.IsSome ? option : optionFactory.Invoke();
        }

        public async ValueTask<Option<T>> OrElseAsync(
            Func<ValueTask<Option<T>>> optionFactory)
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            if (option.IsSome) return option;

            return await optionFactory.Invoke().ConfigureAwait(false);
        }
    }

    extension<T>(ValueTask<Option<T>> optionTask) where T : notnull
    {
        public async ValueTask<Option<T>> OrElseAsync(Func<Option<T>> optionFactory)
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            return option.IsSome ? option : optionFactory.Invoke();
        }

        public async ValueTask<Option<T>> OrElseAsync(
            Func<ValueTask<Option<T>>> optionFactory)
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            if (option.IsSome) return option;

            return await optionFactory.Invoke().ConfigureAwait(false);
        }
    }
}
