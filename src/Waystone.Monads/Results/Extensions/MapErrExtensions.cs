namespace Waystone.Monads.Results.Extensions;

using System;
using System.Threading.Tasks;

public static class MapErrExtensions
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

    extension<TOk, TErr>(Task<Result<TOk, TErr>> resultTask)
        where TOk : notnull
        where TErr : notnull
    {
        public async Task<Result<TOk, TOut>> MapErrAsync<TOut>(
            Func<TErr, Task<TOut>> map)
            where TOut : notnull
        {
            Result<TOk, TErr> result = await resultTask.ConfigureAwait(false);

            return await result.MapErrAsync(map).ConfigureAwait(false);
        }

        public async Task<Result<TOk, TOut>> MapErrAsync<TOut>(
            Func<TErr, TOut> map)
            where TOut : notnull
        {
            Result<TOk, TErr> result = await resultTask.ConfigureAwait(false);

            return result.MapErr(map);
        }
    }

    extension<TOk, TErr>(ValueTask<Result<TOk, TErr>> resultTask)
        where TOk : notnull
        where TErr : notnull
    {
        public async Task<Result<TOk, TOut>> MapErrAsync<TOut>(
            Func<TErr, Task<TOut>> map)
            where TOut : notnull
        {
            Result<TOk, TErr> result = await resultTask.ConfigureAwait(false);

            return await result.MapErrAsync(map).ConfigureAwait(false);
        }

        public async Task<Result<TOk, TOut>> MapErrAsync<TOut>(
            Func<TErr, TOut> map)
            where TOut : notnull
        {
            Result<TOk, TErr> result = await resultTask.ConfigureAwait(false);

            return result.MapErr(map);
        }
    }
}
