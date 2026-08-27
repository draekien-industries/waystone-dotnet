namespace Waystone.Monads.PreviousMajor.Declarations.Sample;

using System.Threading.Tasks;
using Waystone.Monads.Results;
using Waystone.Monads.Results.Errors;
using static Waystone.Monads.Results.Extensions.MapExtensions;

/// <summary>
/// A <c>using static</c> on an extension class, deliberately load-bearing: this
/// file imports no extension namespace, so the reduced call below binds only
/// through the import above. There are zero <c>using static</c> occurrences in the
/// rest of this repository, which is why the deliberate one is here — a consumer
/// who wrote it gets <c>CS0246</c> from <c>DRA-111</c> and no code fix can guess
/// what to import instead.
/// </summary>
internal static class UsingStaticExtension
{
    internal static ValueTask<Result<int, Error>> Doubled(
        Result<int, Error> result) =>
        result.MapAsync(value => Task.FromResult(value * 2));
}
