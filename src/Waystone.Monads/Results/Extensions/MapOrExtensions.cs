namespace Waystone.Monads.Results.Extensions;

using System;
using System.Threading.Tasks;

public static class MapOrExtensions
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

    extension<TOk, TErr>(Task<Result<TOk, TErr>> resultTask)
        where TOk : notnull where TErr : notnull
    {
        public async Task<TOut> MapOrAsync<TOut>(
            TOut defaultValue,
            Func<TOk, Task<TOut>> map)
            where TOut : notnull
        {
            Result<TOk, TErr> result = await resultTask.ConfigureAwait(false);

            return await result.MapOrAsync(defaultValue, map)
               .ConfigureAwait(false);
        }

        public async Task<TOut> MapOrAsync<TOut>(
            TOut defaultValue,
            Func<TOk, TOut> map)
            where TOut : notnull
        {
            Result<TOk, TErr> result = await resultTask.ConfigureAwait(false);

            return result.MapOr(defaultValue, map);
        }
    }

    extension<TOk, TErr>(ValueTask<Result<TOk, TErr>> resultTask)
        where TOk : notnull where TErr : notnull
    {
        public async Task<TOut> MapOrAsync<TOut>(
            TOut defaultValue,
            Func<TOk, Task<TOut>> map)
            where TOut : notnull
        {
            Result<TOk, TErr> result = await resultTask.ConfigureAwait(false);

            return await result.MapOrAsync(defaultValue, map)
               .ConfigureAwait(false);
        }

        public async Task<TOut> MapOrAsync<TOut>(
            TOut defaultValue,
            Func<TOk, TOut> map)
            where TOut : notnull
        {
            Result<TOk, TErr> result = await resultTask.ConfigureAwait(false);

            return result.MapOr(defaultValue, map);
        }
    }
}
