namespace Waystone.Monads.Results.Extensions;

using System;
using System.Threading.Tasks;

public static class OrElseExtensions
{
    extension<TOk, TErr>(Result<TOk, TErr> result)
        where TOk : notnull
        where TErr : notnull
    {
        public async ValueTask<Result<TOk, TOut>> OrElseAsync<TOut>(
            Func<TErr, ValueTask<Result<TOk, TOut>>> resultFactory)
            where TOut : notnull
        {
            if (result.IsOk)
            {
                TOk ok = result.Expect("Expected Ok but found Err.");

                return Result.Ok<TOk, TOut>(ok);
            }

            TErr err = result.ExpectErr("Expected Err but found Ok.");

            Result<TOk, TOut> output = await resultFactory.Invoke(err)
               .ConfigureAwait(false);

            return output;
        }
    }

    extension<TOk, TErr>(Task<Result<TOk, TErr>> resultTask)
        where TOk : notnull
        where TErr : notnull
    {
        public async ValueTask<Result<TOk, TOut>> OrElseAsync<TOut>(
            Func<TErr, Result<TOk, TOut>> resultFactory)
            where TOut : notnull
        {
            Result<TOk, TErr> result = await resultTask.ConfigureAwait(false);

            return result.OrElse(resultFactory);
        }

        public async ValueTask<Result<TOk, TOut>> OrElseAsync<TOut>(
            Func<TErr, ValueTask<Result<TOk, TOut>>> resultFactory)
            where TOut : notnull
        {
            Result<TOk, TErr> result = await resultTask.ConfigureAwait(false);

            return await result.OrElseAsync(resultFactory).ConfigureAwait(false);
        }
    }

    extension<TOk, TErr>(ValueTask<Result<TOk, TErr>> resultTask)
        where TOk : notnull
        where TErr : notnull
    {
        public async ValueTask<Result<TOk, TOut>> OrElseAsync<TOut>(
            Func<TErr, Result<TOk, TOut>> resultFactory)
            where TOut : notnull
        {
            Result<TOk, TErr> result = await resultTask.ConfigureAwait(false);

            return result.OrElse(resultFactory);
        }

        public async ValueTask<Result<TOk, TOut>> OrElseAsync<TOut>(
            Func<TErr, ValueTask<Result<TOk, TOut>>> resultFactory)
            where TOut : notnull
        {
            Result<TOk, TErr> result = await resultTask.ConfigureAwait(false);

            return await result.OrElseAsync(resultFactory).ConfigureAwait(false);
        }
    }
}
