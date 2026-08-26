namespace Waystone.Monads.Options.Extensions;

using System;
using System.Threading.Tasks;
using Results;
using Waystone.SourceGenerators;

[GenerateAwaitedReceivers(typeof(Option<>))]
[GenerateAwaitedMember(nameof(Option<>.OkOrElse))]
public static partial class OkOrElseExtensions
{
    extension<T>(Option<T> option) where T : notnull
    {
        public async ValueTask<Result<T, TErr>> OkOrElseAsync<TErr>(
            Func<Task<TErr>> errorFactory)
            where TErr : notnull
        {
            if (option.IsSome)
            {
                T some = option.Expect("Expected Some but found None.");

                return Result.Ok<T, TErr>(some);
            }

            TErr err = await errorFactory.Invoke().ConfigureAwait(false);

            return Result.Err<T, TErr>(err);
        }
    }
}
