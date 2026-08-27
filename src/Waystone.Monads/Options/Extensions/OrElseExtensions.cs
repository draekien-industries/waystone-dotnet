namespace Waystone.Monads.Options.Extensions;

using System;
using System.Threading.Tasks;
using Waystone.SourceGenerators;

[GenerateAwaitedReceivers(typeof(Option<>))]
[GenerateAwaitedMember(nameof(Option<>.OrElse))]
public static partial class OrElseExtensions
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
}
