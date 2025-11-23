namespace Waystone.Monads.Options.Extensions;

using System.Threading.Tasks;
using Results;

public static class OkOrExtensions
{
    extension<T>(Task<Option<T>> optionTask) where T : notnull
    {
        public async Task<Result<T, TErr>> OkOr<TErr>(TErr err)
            where TErr : notnull
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            return option.OkOr(err);
        }
    }

    extension<T>(ValueTask<Option<T>> optionTask) where T : notnull
    {
        public async Task<Result<T, TErr>> OkOr<TErr>(TErr err)
            where TErr : notnull
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            return option.OkOr(err);
        }
    }
}
