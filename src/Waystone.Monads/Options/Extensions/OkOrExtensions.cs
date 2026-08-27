namespace Waystone.Monads.Options.Extensions;

using System.Threading.Tasks;
using Results;

public static class OkOrExtensions
{
    extension<T>(Task<Option<T>> optionTask) where T : notnull
    {
        public async ValueTask<Result<T, TErr>> OkOrAsync<TErr>(TErr error)
            where TErr : notnull
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            return option.OkOr(error);
        }
    }

    extension<T>(ValueTask<Option<T>> optionTask) where T : notnull
    {
        public async ValueTask<Result<T, TErr>> OkOrAsync<TErr>(TErr error)
            where TErr : notnull
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            return option.OkOr(error);
        }
    }
}
