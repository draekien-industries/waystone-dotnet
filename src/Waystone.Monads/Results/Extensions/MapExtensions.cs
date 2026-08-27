namespace Waystone.Monads.Results.Extensions;

using System;
using System.Threading.Tasks;
using Waystone.SourceGenerators;

[GenerateAwaitedReceivers(typeof(Result<,>))]
[GenerateAwaitedMember(nameof(Result<,>.Map))]
public static partial class MapExtensions
{
    extension<TOk, TErr>(Result<TOk, TErr> result)
        where TOk : notnull where TErr : notnull
    {
        public async ValueTask<Result<TOut, TErr>> MapAsync<TOut>(
            Func<TOk, Task<TOut>> map)
            where TOut : notnull
        {
            if (result.IsErr)
            {
                TErr err = result.ExpectErr("Expected Err but found Ok.");

                return Result.Err<TOut, TErr>(err);
            }

            TOk ok = result.Expect("Expected Ok but found Err.");
            TOut? output = await map.Invoke(ok).ConfigureAwait(false);

            return Result.Ok<TOut, TErr>(output);
        }
    }
}
