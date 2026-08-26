namespace Waystone.Monads.Results.Extensions;

using System;
using System.Threading.Tasks;
using Waystone.SourceGenerators;

[GenerateAwaitedReceivers(typeof(Result<,>))]
[GenerateAwaitedMember(nameof(Result<,>.MapOr))]
public static partial class MapOrExtensions
{
    extension<TOk, TErr>(Result<TOk, TErr> result)
        where TOk : notnull where TErr : notnull
    {
        public async ValueTask<TOut> MapOrAsync<TOut>(
            TOut defaultValue,
            Func<TOk, Task<TOut>> map)
            where TOut : notnull
        {
            if (result.IsErr) return defaultValue;

            TOk ok = result.Expect("Expected Ok but found Err.");
            TOut output = await map.Invoke(ok).ConfigureAwait(false);

            return output;
        }
    }
}
