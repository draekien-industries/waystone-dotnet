namespace Waystone.Monads.Results.Extensions;

using System;
using System.Threading.Tasks;
using Waystone.SourceGenerators;

[GenerateAwaitedReceivers(typeof(Result<,>))]
[GenerateAwaitedMember(nameof(Result<,>.MapErr))]
public static partial class MapErrExtensions
{
    extension<TOk, TErr>(Result<TOk, TErr> result)
        where TOk : notnull
        where TErr : notnull
    {
        public async ValueTask<Result<TOk, TOut>> MapErrAsync<TOut>(
            Func<TErr, Task<TOut>> map)
            where TOut : notnull
        {
            if (result.IsOk)
            {
                TOk ok = result.Expect("Expected Ok but found Err.");

                return Result.Ok<TOk, TOut>(ok);
            }

            TErr err = result.ExpectErr("Expected Err but found Ok.");
            TOut output = await map.Invoke(err).ConfigureAwait(false);

            return Result.Err<TOk, TOut>(output);
        }
    }
}
