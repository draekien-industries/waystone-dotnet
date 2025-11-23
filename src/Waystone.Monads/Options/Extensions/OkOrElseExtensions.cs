namespace Waystone.Monads.Options.Extensions;

using System;
using System.Threading.Tasks;
using Results;

public static class OkOrElseExtensions
{
    extension<T>(Option<T> option) where T : notnull
    {
        public async ValueTask<Result<T, TErr>> OkOrElseAsync<TErr>(
            Func<Task<TErr>> errFunc)
            where TErr : notnull
        {
            if (option.IsSome)
            {
                T some = option.Expect("Expected Some but found None.");

                return Result.Ok<T, TErr>(some);
            }

            TErr err = await errFunc.Invoke().ConfigureAwait(false);

            return Result.Err<T, TErr>(err);
        }
    }

    extension<T>(Task<Option<T>> optionTask) where T : notnull
    {
        public async Task<Result<T, TErr>> OkOrElseAsync<TErr>(
            Func<TErr> errFunc)
            where TErr : notnull
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            return option.OkOrElse(errFunc);
        }

        public Task<Result<T, TErr>> OkOrElseAsync<TErr>(
            Func<Task<TErr>> errFunc)
            where TErr : notnull =>
            optionTask.MatchAsync(
                Result.Ok<T, TErr>,
                async () =>
                {
                    TErr? err = await errFunc.Invoke().ConfigureAwait(false);

                    return Result.Err<T, TErr>(err);
                });
    }

    extension<T>(ValueTask<Option<T>> optionTask) where T : notnull
    {
        public async Task<Result<T, TErr>> OkOrElseAsync<TErr>(
            Func<TErr> errFunc)
            where TErr : notnull
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            return option.OkOrElse(errFunc);
        }

        public Task<Result<T, TErr>> OkOrElseAsync<TErr>(
            Func<Task<TErr>> errFunc)
            where TErr : notnull =>
            optionTask.MatchAsync(
                Result.Ok<T, TErr>,
                async () =>
                {
                    TErr? err = await errFunc.Invoke().ConfigureAwait(false);

                    return Result.Err<T, TErr>(err);
                });
    }
}
