namespace Waystone.Monads.Results.Extensions;

using System;
using System.Threading.Tasks;
using Waystone.SourceGenerators;

[GenerateAwaitedReceivers(typeof(Result<,>))]
[GenerateAwaitedMember(nameof(Result<,>.OrElse))]
public static partial class OrElseExtensions
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
}
