namespace Waystone.Monads.Results.Extensions;

using System;
using System.Threading.Tasks;

public static class UnwrapOrElseExtensions
{
    extension<TOk, TErr>(Result<TOk, TErr> result)
        where TOk : notnull
        where TErr : notnull
    {
        public async ValueTask<TOk> UnwrapOrElseAsync(
            Func<TErr, Task<TOk>> factory)
        {
            if (result.IsOk)
            {
                return result.Expect("Expected Ok but found Err.");
            }

            TErr err = result.ExpectErr("Expected Err but found Ok.");

            TOk output = await factory.Invoke(err)
               .ConfigureAwait(false);

            return output;
        }
    }

    extension<TOk, TErr>(Task<Result<TOk, TErr>> resultTask)
        where TOk : notnull
        where TErr : notnull
    {
        public async Task<TOk> UnwrapOrElseAsync(Func<TErr, TOk> factory)
        {
            Result<TOk, TErr> result = await resultTask.ConfigureAwait(false);

            return result.UnwrapOrElse(factory);
        }

        public async Task<TOk> UnwrapOrElseAsync(Func<TErr, Task<TOk>> factory)
        {
            Result<TOk, TErr> result = await resultTask.ConfigureAwait(false);

            return await result.UnwrapOrElseAsync(factory)
               .ConfigureAwait(false);
        }
    }

    extension<TOk, TErr>(ValueTask<Result<TOk, TErr>> resultTask)
        where TOk : notnull
        where TErr : notnull
    {
        public async Task<TOk> UnwrapOrElseAsync(Func<TErr, TOk> factory)
        {
            Result<TOk, TErr> result = await resultTask.ConfigureAwait(false);

            return result.UnwrapOrElse(factory);
        }

        public async Task<TOk> UnwrapOrElseAsync(Func<TErr, Task<TOk>> factory)
        {
            Result<TOk, TErr> result = await resultTask.ConfigureAwait(false);

            return await result.UnwrapOrElseAsync(factory)
               .ConfigureAwait(false);
        }
    }
}
