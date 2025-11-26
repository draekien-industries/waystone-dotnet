namespace Waystone.Monads.Results.Extensions;

using System;
using System.Threading.Tasks;

public static class MapOrElseExtensions
{
    extension<TOk, TErr>(Result<TOk, TErr> result)
        where TOk : notnull where TErr : notnull
    {
        public async ValueTask<TOut> MapOrElseAsync<TOut>(
            Func<TErr, Task<TOut>> factory,
            Func<TOk, Task<TOut>> map)
            where TOut : notnull
        {
            if (result.IsErr)
            {
                TErr err = result.ExpectErr("Expected Err but found Ok.");

                return await factory.Invoke(err).ConfigureAwait(false);
            }

            TOk ok = result.Expect("Expected Ok but found Err.");
            TOut output = await map.Invoke(ok).ConfigureAwait(false);

            return output;
        }

        public async ValueTask<TOut> MapOrElseAsync<TOut>(
            Func<TErr, TOut> factory,
            Func<TOk, Task<TOut>> map)
            where TOut : notnull
        {
            if (result.IsErr)
            {
                TErr err = result.ExpectErr("Expected Err but found Ok.");

                return factory.Invoke(err);
            }

            TOk ok = result.Expect("Expected Ok but found Err.");
            TOut output = await map.Invoke(ok).ConfigureAwait(false);

            return output;
        }

        public async ValueTask<TOut> MapOrElseAsync<TOut>(
            Func<TErr, Task<TOut>> factory,
            Func<TOk, TOut> map)
            where TOut : notnull
        {
            if (result.IsErr)
            {
                TErr err = result.ExpectErr("Expected Err but found Ok.");

                return await factory.Invoke(err).ConfigureAwait(false);
            }

            TOk ok = result.Expect("Expected Ok but found Err.");
            TOut output = map.Invoke(ok);

            return output;
        }
    }

    extension<TOk, TErr>(Task<Result<TOk, TErr>> resultTask)
        where TOk : notnull where TErr : notnull
    {
        public async Task<TOut> MapOrElseAsync<TOut>(
            Func<TErr, Task<TOut>> factory,
            Func<TOk, Task<TOut>> map)
            where TOut : notnull
        {
            Result<TOk, TErr> result = await resultTask.ConfigureAwait(false);

            return await result.MapOrElseAsync(factory, map)
               .ConfigureAwait(false);
        }

        public async Task<TOut> MapOrElseAsync<TOut>(
            Func<TErr, TOut> factory,
            Func<TOk, Task<TOut>> map)
            where TOut : notnull
        {
            Result<TOk, TErr> result = await resultTask.ConfigureAwait(false);

            return await result.MapOrElseAsync(factory, map)
               .ConfigureAwait(false);
        }

        public async Task<TOut> MapOrElseAsync<TOut>(
            Func<TErr, Task<TOut>> factory,
            Func<TOk, TOut> map)
            where TOut : notnull
        {
            Result<TOk, TErr> result = await resultTask.ConfigureAwait(false);

            return await result.MapOrElseAsync(factory, map)
               .ConfigureAwait(false);
        }

        public async Task<TOut> MapOrElseAsync<TOut>(
            Func<TErr, TOut> factory,
            Func<TOk, TOut> map)
            where TOut : notnull
        {
            Result<TOk, TErr> result = await resultTask.ConfigureAwait(false);

            return result.MapOrElse(factory, map);
        }
    }

    extension<TOk, TErr>(ValueTask<Result<TOk, TErr>> resultTask)
        where TOk : notnull where TErr : notnull
    {
        public async Task<TOut> MapOrElseAsync<TOut>(
            Func<TErr, Task<TOut>> factory,
            Func<TOk, Task<TOut>> map)
            where TOut : notnull
        {
            Result<TOk, TErr> result = await resultTask.ConfigureAwait(false);

            return await result.MapOrElseAsync(factory, map)
               .ConfigureAwait(false);
        }

        public async Task<TOut> MapOrElseAsync<TOut>(
            Func<TErr, TOut> factory,
            Func<TOk, Task<TOut>> map)
            where TOut : notnull
        {
            Result<TOk, TErr> result = await resultTask.ConfigureAwait(false);

            return await result.MapOrElseAsync(factory, map)
               .ConfigureAwait(false);
        }

        public async Task<TOut> MapOrElseAsync<TOut>(
            Func<TErr, Task<TOut>> factory,
            Func<TOk, TOut> map)
            where TOut : notnull
        {
            Result<TOk, TErr> result = await resultTask.ConfigureAwait(false);

            return await result.MapOrElseAsync(factory, map)
               .ConfigureAwait(false);
        }

        public async Task<TOut> MapOrElseAsync<TOut>(
            Func<TErr, TOut> factory,
            Func<TOk, TOut> map)
            where TOut : notnull
        {
            Result<TOk, TErr> result = await resultTask.ConfigureAwait(false);

            return result.MapOrElse(factory, map);
        }
    }
}
