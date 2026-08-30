using Waystone.Monads.Options;
using Waystone.Monads.Results;

namespace Waystone.Monads.Docs.Core.Sample.Reference;

/// <summary>
/// reference/*/transform.md and reference/*/side-effects.md. Two short
/// categories that the source page also keeps close together.
/// </summary>
internal static class TransformAndSideEffects
{
    internal static void MapAnOption()
    {
        Option<string> maybeName = Option.Some("Henry Crabgrass");
        Option<int> maybeLength = maybeName.Map(name => name.Length);

        _ = maybeLength;
    }

    internal static void MapAResult()
    {
        Result<string, string> nameResult = Result.Ok<string, string>("Consent");
        Result<int, string> lengthResult = nameResult.Map(name => name.Length);

        _ = lengthResult;
    }

    internal static void InspectAnOption()
    {
        Option<string> maybeName = Option.Some("Geladon");
        maybeName.Inspect(name => Console.WriteLine(name.Length));
    }

    internal static void InspectAResult()
    {
        Result<string, string> nameResult = Result.Ok<string, string>("Percival");
        nameResult.Inspect(name => Console.WriteLine(name.Length));
    }
}
