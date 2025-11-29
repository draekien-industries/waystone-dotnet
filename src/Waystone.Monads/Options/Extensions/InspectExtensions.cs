namespace Waystone.Monads.Options.Extensions;

using System;
using System.Threading.Tasks;

public static class InspectExtensions
{
    extension<T>(Option<T> option) where T : notnull
    {
        public async ValueTask<Option<T>> InspectAsync(Func<T, Task> action)
        {
            if (option.IsNone) return option;

            T some = option.Expect("Expected Some but found None.");
            await action.Invoke(some).ConfigureAwait(false);

            return option;
        }
    }

    extension<T>(Task<Option<T>> optionTask) where T : notnull
    {
        public async Task<Option<T>> InspectAsync(Func<T, Task> action)
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            if (option.IsNone) return option;

            T some = option.Expect("Expected Some but found None.");
            await action.Invoke(some).ConfigureAwait(false);

            return option;
        }

        public async Task<Option<T>> InspectAsync(Action<T> action)
        {
            Option<T>? option = await optionTask.ConfigureAwait(false);

            if (option.IsNone) return option;

            T some = option.Expect("Expected Some but found None.");
            action.Invoke(some);

            return option;
        }
    }

    extension<T>(ValueTask<Option<T>> optionTask) where T : notnull
    {
        public async Task<Option<T>> InspectAsync(Func<T, Task> action)
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            if (option.IsNone) return option;

            T some = option.Expect("Expected Some but found None.");
            await action.Invoke(some).ConfigureAwait(false);

            return option;
        }

        public async Task<Option<T>> InspectAsync(Action<T> action)
        {
            Option<T>? option = await optionTask.ConfigureAwait(false);

            if (option.IsNone) return option;

            T some = option.Expect("Expected Some but found None.");
            action.Invoke(some);

            return option;
        }
    }
}
