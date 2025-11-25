namespace Waystone.Monads.Results.Extensions;

using System;
using System.Threading.Tasks;

public static class MapExtensions
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

    extension<TOk, TErr>(Task<Result<TOk, TErr>> resultTask)
        where TOk : notnull where TErr : notnull
    {
        public async Task<Result<TOut, TErr>> MapAsync<TOut>(
            Func<TOk, Task<TOut>> map)
            where TOut : notnull
        {
            Result<TOk, TErr> result = await resultTask.ConfigureAwait(false);

            return await result.MapAsync(map);
        }

        public async Task<Result<TOut, TErr>> MapAsync<TOut>(
            Func<TOk, TOut> map)
            where TOut : notnull
        {
            Result<TOk, TErr> result = await resultTask.ConfigureAwait(false);

            return result.Map(map);
        }
    }

    extension<TOk, TErr>(ValueTask<Result<TOk, TErr>> resultTask)
        where TOk : notnull where TErr : notnull
    {
        public async ValueTask<Result<TOut, TErr>> MapAsync<TOut>(
            Func<TOk, Task<TOut>> map)
            where TOut : notnull
        {
            Result<TOk, TErr> result = await resultTask.ConfigureAwait(false);

            return await result.MapAsync(map);
        }

        public async ValueTask<Result<TOut, TErr>> MapAsync<TOut>(
            Func<TOk, TOut> map)
            where TOut : notnull
        {
            Result<TOk, TErr> result = await resultTask.ConfigureAwait(false);

            return result.Map(map);
        }
    }
}
