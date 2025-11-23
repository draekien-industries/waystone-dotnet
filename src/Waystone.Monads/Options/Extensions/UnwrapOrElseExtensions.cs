namespace Waystone.Monads.Options.Extensions;

using System;
using System.Threading.Tasks;

public static class UnwrapOrElseExtensions
{
    extension<T>(Option<T> option) where T : notnull
    {
        public async ValueTask<T> UnwrapOrElse(Func<Task<T>> elseFunc) =>
            option.IsSome
                ? option.Expect("Expected Some but found None.")
                : await elseFunc.Invoke().ConfigureAwait(false);
    }

    extension<T>(Task<Option<T>> optionTask) where T : notnull
    {
        public async Task<T> UnwrapOrElse(Func<T> elseFunc)
        {
            Option<T>? option = await optionTask.ConfigureAwait(false);

            return option.IsSome
                ? option.Expect("Expected Some but found None.")
                : elseFunc.Invoke();
        }

        public async Task<T> UnwrapOrElse(Func<Task<T>> elseFunc)
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            return option.IsSome
                ? option.Expect("Expected Some but found None.")
                : await elseFunc.Invoke().ConfigureAwait(false);
        }
    }

    extension<T>(ValueTask<Option<T>> optionTask) where T : notnull
    {
        public async Task<T> UnwrapOrElse(Func<T> elseFunc)
        {
            Option<T>? option = await optionTask.ConfigureAwait(false);

            return option.IsSome
                ? option.Expect("Expected Some but found None.")
                : elseFunc.Invoke();
        }

        public async Task<T> UnwrapOrElse(Func<Task<T>> elseFunc)
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            return option.IsSome
                ? option.Expect("Expected Some but found None.")
                : await elseFunc.Invoke().ConfigureAwait(false);
        }
    }
}
