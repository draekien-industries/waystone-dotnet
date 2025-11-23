namespace Waystone.Monads.Options.Extensions;

using System;
using System.Threading.Tasks;

public static class ZipWithExtensions
{
    extension<TSelf>(Option<TSelf> option) where TSelf : notnull
    {
        public async ValueTask<Option<TOut>> ZipWithAsync<TOther, TOut>(
            Option<TOther> otherOption,
            Func<TSelf, TOther, Task<TOut>> zip)
            where TOther : notnull
            where TOut : notnull
        {
            if (option.IsNone || otherOption.IsNone) return Option.None<TOut>();

            TSelf self = option.Expect("Expected Some but found None.");
            TOther other = otherOption.Expect("Expected Some but found None.");

            TOut result = await zip.Invoke(self, other).ConfigureAwait(false);

            return Option.Some(result);
        }
    }

    extension<TSelf>(Task<Option<TSelf>> optionTask) where TSelf : notnull
    {
        public async Task<Option<TOut>> ZipWithAsync<TOther, TOut>(
            Option<TOther> otherOption,
            Func<TSelf, TOther, Task<TOut>> zip)
            where TOther : notnull
            where TOut : notnull
        {
            Option<TSelf> option = await optionTask.ConfigureAwait(false);

            if (option.IsNone || otherOption.IsNone) return Option.None<TOut>();

            TSelf self = option.Expect("Expected Some but found None.");
            TOther other = otherOption.Expect("Expected Some but found None.");

            TOut result = await zip.Invoke(self, other).ConfigureAwait(false);

            return Option.Some(result);
        }

        public async Task<Option<TOut>> ZipWithAsync<TOther, TOut>(
            Task<Option<TOther>> otherOptionTask,
            Func<TSelf, TOther, Task<TOut>> zip)
            where TOther : notnull
            where TOut : notnull
        {
            Option<TSelf> option = await optionTask.ConfigureAwait(false);

            Option<TOther> otherOption =
                await otherOptionTask.ConfigureAwait(false);

            if (option.IsNone || otherOption.IsNone) return Option.None<TOut>();

            TSelf self = option.Expect("Expected Some but found None.");
            TOther other = otherOption.Expect("Expected Some but found None.");

            TOut result = await zip.Invoke(self, other).ConfigureAwait(false);

            return Option.Some(result);
        }
    }

    extension<TSelf>(ValueTask<Option<TSelf>> optionTask) where TSelf : notnull
    {
        public async Task<Option<TOut>> ZipWithAsync<TOther, TOut>(
            Option<TOther> otherOption,
            Func<TSelf, TOther, Task<TOut>> zip)
            where TOther : notnull
            where TOut : notnull
        {
            Option<TSelf> option = await optionTask.ConfigureAwait(false);

            if (option.IsNone || otherOption.IsNone) return Option.None<TOut>();

            TSelf self = option.Expect("Expected Some but found None.");
            TOther other = otherOption.Expect("Expected Some but found None.");

            TOut result = await zip.Invoke(self, other).ConfigureAwait(false);

            return Option.Some(result);
        }

        public async Task<Option<TOut>> ZipWithAsync<TOther, TOut>(
            ValueTask<Option<TOther>> otherOptionTask,
            Func<TSelf, TOther, Task<TOut>> zip)
            where TOther : notnull
            where TOut : notnull
        {
            Option<TSelf> option = await optionTask.ConfigureAwait(false);

            Option<TOther> otherOption =
                await otherOptionTask.ConfigureAwait(false);

            if (option.IsNone || otherOption.IsNone) return Option.None<TOut>();

            TSelf self = option.Expect("Expected Some but found None.");
            TOther other = otherOption.Expect("Expected Some but found None.");

            TOut result = await zip.Invoke(self, other).ConfigureAwait(false);

            return Option.Some(result);
        }
    }
}
