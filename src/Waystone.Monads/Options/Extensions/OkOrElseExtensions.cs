namespace Waystone.Monads.Options.Extensions;

using System;
using System.Threading.Tasks;
using Results;

public static class OkOrElseExtensions
{
    extension<T>(Task<Option<T>> optionTask) where T : notnull
    {
        public async Task<Result<T, TErr>> OkOrElse<TErr>(Func<TErr> errFunc)
            where TErr : notnull
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            return option.OkOrElse(errFunc);
        }

        public Task<Result<T, TErr>> OkOrElse<TErr>(Func<Task<TErr>> errFunc)
            where TErr : notnull =>
            optionTask.Match(
                Result.Ok<T, TErr>,
                async () =>
                {
                    TErr? err = await errFunc.Invoke().ConfigureAwait(false);

                    return Result.Err<T, TErr>(err);
                });
    }

    extension<T>(ValueTask<Option<T>> optionTask) where T : notnull
    {
        public async Task<Result<T, TErr>> OkOrElse<TErr>(Func<TErr> errFunc)
            where TErr : notnull
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            return option.OkOrElse(errFunc);
        }

        public Task<Result<T, TErr>> OkOrElse<TErr>(Func<Task<TErr>> errFunc)
            where TErr : notnull =>
            optionTask.Match(
                Result.Ok<T, TErr>,
                async () =>
                {
                    TErr? err = await errFunc.Invoke().ConfigureAwait(false);

                    return Result.Err<T, TErr>(err);
                });
    }
}
