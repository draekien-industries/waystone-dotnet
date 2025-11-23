namespace Waystone.Monads.Options.Extensions;

using System;
using System.Threading.Tasks;

public static class OrElseExtensions
{
    extension<T>(Option<T> option) where T : notnull
    {
        public async ValueTask<Option<T>> OrElse(Func<Task<Option<T>>> elseFunc)
        {
            if (option.IsSome) return option;

            return await elseFunc.Invoke().ConfigureAwait(false);
        }
    }

    extension<T>(Task<Option<T>> optionTask) where T : notnull
    {
        public async Task<Option<T>> OrElse(Func<Option<T>> elseFunc)
        {
            Option<T>? option = await optionTask.ConfigureAwait(false);

            return option.IsSome ? option : elseFunc.Invoke();
        }

        public async Task<Option<T>> OrElse(Func<Task<Option<T>>> elseFunc)
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            if (option.IsSome) return option;

            return await elseFunc.Invoke().ConfigureAwait(false);
        }
    }

    extension<T>(ValueTask<Option<T>> optionTask) where T : notnull
    {
        public async Task<Option<T>> OrElse(Func<Option<T>> elseFunc)
        {
            Option<T>? option = await optionTask.ConfigureAwait(false);

            return option.IsSome ? option : elseFunc.Invoke();
        }

        public async Task<Option<T>> OrElse(Func<Task<Option<T>>> elseFunc)
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            if (option.IsSome) return option;

            return await elseFunc.Invoke().ConfigureAwait(false);
        }
    }
}
