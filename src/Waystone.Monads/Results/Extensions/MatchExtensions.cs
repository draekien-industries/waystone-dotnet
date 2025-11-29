namespace Waystone.Monads.Results.Extensions;

using System;
using System.Threading.Tasks;

public static class MatchExtensions
{
    extension<TOk, TErr>(Result<TOk, TErr> result)
        where TOk : notnull where TErr : notnull
    {
        public async Task MatchAsync(
            Func<TOk, Task> onOk,
            Func<TErr, Task> onErr)
        {
            if (result.IsOk)
            {
                TOk ok = result.Expect("Expected Ok but found Err.");

                await onOk.Invoke(ok).ConfigureAwait(false);

                return;
            }

            TErr err = result.ExpectErr("Expected Err but found Ok.");

            await onErr.Invoke(err).ConfigureAwait(false);
        }

        public async Task MatchAsync(
            Func<TOk, Task> onOk,
            Action<TErr> onErr)
        {
            if (result.IsOk)
            {
                TOk ok = result.Expect("Expected Ok but found Err.");

                await onOk.Invoke(ok).ConfigureAwait(false);

                return;
            }

            TErr err = result.ExpectErr("Expected Err but found Ok.");

            onErr.Invoke(err);
        }

        public async Task MatchAsync(
            Action<TOk> onOk,
            Func<TErr, Task> onErr)
        {
            if (result.IsOk)
            {
                TOk ok = result.Expect("Expected Ok but found Err.");

                onOk.Invoke(ok);

                return;
            }

            TErr err = result.ExpectErr("Expected Err but found Ok.");

            await onErr.Invoke(err).ConfigureAwait(false);
        }

        public async Task<TOut> MatchAsync<TOut>(
            Func<TOk, Task<TOut>> onOk,
            Func<TErr, Task<TOut>> onErr)
        {
            if (result.IsOk)
            {
                TOk ok = result.Expect("Expected Ok but found Err.");

                return await onOk.Invoke(ok).ConfigureAwait(false);
            }

            TErr err = result.ExpectErr("Expected Err but found Ok.");

            return await onErr.Invoke(err).ConfigureAwait(false);
        }
    }

    extension<TOk, TErr>(Task<Result<TOk, TErr>> resultTask)
        where TOk : notnull where TErr : notnull
    {
        public async Task MatchAsync(
            Func<TOk, Task> onOk,
            Func<TErr, Task> onErr)
        {
            Result<TOk, TErr> result = await resultTask.ConfigureAwait(false);

            await result.MatchAsync(onOk, onErr).ConfigureAwait(false);
        }

        public async Task MatchAsync(
            Func<TOk, Task> onOk,
            Action<TErr> onErr)
        {
            Result<TOk, TErr> result = await resultTask.ConfigureAwait(false);

            await result.MatchAsync(onOk, onErr).ConfigureAwait(false);
        }

        public async Task MatchAsync(
            Action<TOk> onOk,
            Func<TErr, Task> onErr)
        {
            Result<TOk, TErr> result = await resultTask.ConfigureAwait(false);

            await result.MatchAsync(onOk, onErr).ConfigureAwait(false);
        }

        public async Task<TOut> MatchAsync<TOut>(
            Func<TOk, Task<TOut>> onOk,
            Func<TErr, Task<TOut>> onErr)
        {
            Result<TOk, TErr> result = await resultTask.ConfigureAwait(false);

            return await result.MatchAsync(onOk, onErr).ConfigureAwait(false);
        }

        public async Task MatchAsync(
            Action<TOk> onOk,
            Action<TErr> onErr)
        {
            Result<TOk, TErr> result = await resultTask.ConfigureAwait(false);

            if (result.IsOk)
            {
                TOk ok = result.Expect("Expected Ok but found Err.");

                onOk.Invoke(ok);

                return;
            }

            TErr err = result.ExpectErr("Expected Err but found Ok.");

            onErr.Invoke(err);
        }
    }

    extension<TOk, TErr>(ValueTask<Result<TOk, TErr>> resultTask)
        where TOk : notnull where TErr : notnull
    {
        public async Task MatchAsync(
            Func<TOk, Task> onOk,
            Func<TErr, Task> onErr)
        {
            Result<TOk, TErr> result = await resultTask.ConfigureAwait(false);

            await result.MatchAsync(onOk, onErr).ConfigureAwait(false);
        }

        public async Task MatchAsync(
            Func<TOk, Task> onOk,
            Action<TErr> onErr)
        {
            Result<TOk, TErr> result = await resultTask.ConfigureAwait(false);

            await result.MatchAsync(onOk, onErr).ConfigureAwait(false);
        }

        public async Task MatchAsync(
            Action<TOk> onOk,
            Func<TErr, Task> onErr)
        {
            Result<TOk, TErr> result = await resultTask.ConfigureAwait(false);

            await result.MatchAsync(onOk, onErr).ConfigureAwait(false);
        }

        public async Task<TOut> MatchAsync<TOut>(
            Func<TOk, Task<TOut>> onOk,
            Func<TErr, Task<TOut>> onErr)
        {
            Result<TOk, TErr> result = await resultTask.ConfigureAwait(false);

            return await result.MatchAsync(onOk, onErr).ConfigureAwait(false);
        }

        public async Task MatchAsync(
            Action<TOk> onOk,
            Action<TErr> onErr)
        {
            Result<TOk, TErr> result = await resultTask.ConfigureAwait(false);

            if (result.IsOk)
            {
                TOk ok = result.Expect("Expected Ok but found Err.");

                onOk.Invoke(ok);

                return;
            }

            TErr err = result.ExpectErr("Expected Err but found Ok.");

            onErr.Invoke(err);
        }
    }
}
