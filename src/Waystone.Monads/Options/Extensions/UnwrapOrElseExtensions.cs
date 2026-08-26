namespace Waystone.Monads.Options.Extensions;

using System;
using System.Threading.Tasks;

public static class UnwrapOrElseExtensions
{
    extension<T>(Option<T> option) where T : notnull
    {
        public async ValueTask<T> UnwrapOrElseAsync(Func<Task<T>> valueFactory) =>
            option.IsSome
                ? option.Expect("Expected Some but found None.")
                : await valueFactory.Invoke().ConfigureAwait(false);
    }

    extension<T>(Task<Option<T>> optionTask) where T : notnull
    {
        public async ValueTask<T> UnwrapOrElseAsync(Func<T> valueFactory)
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            return option.IsSome
                ? option.Expect("Expected Some but found None.")
                : valueFactory.Invoke();
        }

        public async ValueTask<T> UnwrapOrElseAsync(Func<Task<T>> valueFactory)
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            return option.IsSome
                ? option.Expect("Expected Some but found None.")
                : await valueFactory.Invoke().ConfigureAwait(false);
        }
    }

    extension<T>(ValueTask<Option<T>> optionTask) where T : notnull
    {
        public async ValueTask<T> UnwrapOrElseAsync(Func<T> valueFactory)
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            return option.IsSome
                ? option.Expect("Expected Some but found None.")
                : valueFactory.Invoke();
        }

        public async ValueTask<T> UnwrapOrElseAsync(Func<Task<T>> valueFactory)
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            return option.IsSome
                ? option.Expect("Expected Some but found None.")
                : await valueFactory.Invoke().ConfigureAwait(false);
        }
    }
}
