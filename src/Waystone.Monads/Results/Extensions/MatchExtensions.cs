namespace Waystone.Monads.Results.Extensions;

using System;
using System.Threading.Tasks;
using Waystone.SourceGenerators;

[GenerateAwaitedReceivers(typeof(Result<,>))]
[GenerateAwaitedMember(nameof(Result<,>.Match))]
public static partial class MatchExtensions
{
    extension<TOk, TErr>(Result<TOk, TErr> result)
        where TOk : notnull where TErr : notnull
    {
        public async ValueTask MatchAsync(
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

        public async ValueTask MatchAsync(
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

        public async ValueTask MatchAsync(
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

        public async ValueTask<TOut> MatchAsync<TOut>(
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
}
