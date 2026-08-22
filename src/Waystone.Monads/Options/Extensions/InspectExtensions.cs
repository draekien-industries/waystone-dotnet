namespace Waystone.Monads.Options.Extensions;

using System;
using System.Threading.Tasks;
using Waystone.SourceGenerators;

[GenerateAwaitedReceivers(typeof(Option<>))]
[GenerateAwaitedMember(nameof(Option<>.Inspect))]
public static partial class InspectExtensions
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
}
