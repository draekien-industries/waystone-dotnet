namespace Waystone.Monads.Results.Extensions;

using System;
using System.Threading.Tasks;
using Waystone.SourceGenerators;

[GenerateAwaitedReceivers(typeof(Result<,>))]
[GenerateAwaitedMember(nameof(Result<,>.UnwrapOrElse))]
public static partial class UnwrapOrElseExtensions
{
    extension<TOk, TErr>(Result<TOk, TErr> result)
        where TOk : notnull
        where TErr : notnull
    {
        public async ValueTask<TOk> UnwrapOrElseAsync(
            Func<TErr, Task<TOk>> valueFactory)
        {
            if (result.IsOk)
            {
                return result.Expect("Expected Ok but found Err.");
            }

            TErr err = result.ExpectErr("Expected Err but found Ok.");

            TOk output = await valueFactory.Invoke(err)
               .ConfigureAwait(false);

            return output;
        }
    }
}
