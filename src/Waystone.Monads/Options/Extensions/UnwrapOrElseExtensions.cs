namespace Waystone.Monads.Options.Extensions;

using System;
using System.Threading.Tasks;
using Waystone.SourceGenerators;

[GenerateAwaitedReceivers(typeof(Option<>))]
[GenerateAwaitedMember(nameof(Option<>.UnwrapOrElse))]
public static partial class UnwrapOrElseExtensions
{
    extension<T>(Option<T> option) where T : notnull
    {
        public async ValueTask<T> UnwrapOrElseAsync(Func<Task<T>> valueFactory) =>
            option.IsSome
                ? option.Expect("Expected Some but found None.")
                : await valueFactory.Invoke().ConfigureAwait(false);
    }
}
