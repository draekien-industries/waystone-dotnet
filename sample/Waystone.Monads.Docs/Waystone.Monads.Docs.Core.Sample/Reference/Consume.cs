using System.Diagnostics;
using Waystone.Monads.Options;
using Waystone.Monads.Options.Extensions;
using Waystone.Monads.Results;
using Waystone.Monads.Results.Extensions;
using Waystone.Monads.Results.Errors;

namespace Waystone.Monads.Docs.Core.Sample.Reference;

/// <summary>
/// reference/option/consume.md and reference/result/consume.md. Covers the
/// state checks, Match, the deconstruction forms, the Unwrap family, Expect,
/// and the MapOr family — which end the chain by returning a raw value, which
/// is why they are here rather than on Transform.
/// </summary>
internal static class Consume
{
    internal sealed record Adventurer(Uri Portrait);

    internal sealed record Loadout(string Weapon);

    private static class ErrorCodes
    {
        internal const string MissingName = "name.missing";
    }

    internal static void StateChecksOnAResult()
    {
        Result<DateTime, Error> safeParseResult = SafeParse("2025-01-01");

        _ = safeParseResult.IsOk;  // true
        _ = safeParseResult.IsErr; // false
    }

    internal static void StateChecksOnAnOption()
    {
        Option<string> maybeName = Option.Some("Laudna");

        _ = maybeName.IsSome; // true
        _ = maybeName.IsNone; // false
    }

    internal static void MatchOnAnOption()
    {
        Option<string> maybeName = Option.Some("Travis");

        int length = maybeName.Match(
            name => name.Length,
            () => 0);

        _ = length;
    }

    internal static void MatchOnAResult()
    {
        Result<string, string> nameResult = Result.Ok<string, string>("Sam");

        int length = nameResult.Match(
            name => name.Length,
            _ => 0);

        _ = length;
    }

    internal static void DeconstructingAnOption(Option<string> maybeName)
    {
        if (maybeName is Some<string>(var name))
        {
            Console.WriteLine(name.Length);
        }
    }

    internal static void DeconstructingAResult(Result<string, string> nameResult)
    {
        if (nameResult is Ok<string, string>(var name)) { _ = name; }
        if (nameResult is Err<string, string>(var error)) { _ = error; }
    }

    // The page also shows `var (a, b) = maybeName;` to say it does not compile
    // -- Option deconstructs to one value, not two. A sample that fails to
    // build is the claim itself, so there is nothing to pin here.

    internal static int SwitchingOverBothStates(Option<string> maybeName) =>
        maybeName switch
        {
            Some<string>(var name) => name.Length,
            None<string> => 0,
            _ => throw new UnreachableException(),
        };

    internal static void UnwrapAnOption()
    {
        Option<string> maybeName = Option.Some("Lorekeeper");
        string name = maybeName.Unwrap();

        _ = name;
    }

    internal static void UnwrapAResult()
    {
        Result<string, string> nameResult = Result.Ok<string, string>("Danny");
        string name = nameResult.Unwrap();

        _ = name;
    }

    internal static void UnwrapOrOnAnOption()
    {
        Option<string> maybeNickname = Option.None<string>();
        string nickname = maybeNickname.UnwrapOr("Lautna");
        //     ^? "Lautna"

        _ = nickname;
    }

    internal static void UnwrapOrOnAResult()
    {
        Result<string, Error> nameResult =
            Result.Err<string, Error>(
                new Error(ErrorCodes.MissingName, "no name was supplied"));

        string name = nameResult.UnwrapOr("Unknown");
        //     ^? "Unknown"

        _ = name;
    }

    internal static void UnwrapOrElseOnAnOption()
    {
        Option<Uri> maybePortrait = Option.None<Uri>();
        Uri portrait = maybePortrait.UnwrapOrElse(() => GeneratePortrait());
        //  ^? generated portrait

        _ = portrait;
    }

    internal static void UnwrapOrElseOnAResult()
    {
        Result<Loadout, Error> getLoadoutResult = GetLoadout("Ashton");
        //                     ^? Err<Loadout, Error>

        Loadout loadout = getLoadoutResult.UnwrapOrElse(error => GenerateDefaultLoadout());
        //      ^? generated loadout

        _ = loadout;
    }

    internal static void UnwrapOrDefaultOnAnOption()
    {
        Option<string> maybeName = Option.None<string>();
        string? name = maybeName.UnwrapOrDefault();
        //      ^? null

        _ = name;
    }

    internal static void UnwrapOrDefaultOnAResult()
    {
        Result<int, string> numberResult = Result.Err<int, string>("Error");
        int number = numberResult.UnwrapOrDefault();
        //  ^? 0

        _ = number;
    }

    internal static void UnwrapOrNullOnAnOption()
    {
        Option<int> maybeCount = Option.None<int>();
        int? count = maybeCount.UnwrapOrNull();
        //   ^? null, where UnwrapOrDefault would have given you 0

        _ = count;
    }

    internal static void UnwrapOrNullOnAResult()
    {
        Result<int, string> countResult = Result.Err<int, string>("Error");
        int? count = countResult.UnwrapOrNull();
        //   ^? null

        _ = count;
    }

    internal static void ExpectOnAnOption()
    {
        Option<string> maybeName = Option.Some("Greymore");
        string name = maybeName.Expect("Expected a name, but got nothing.");

        _ = name;
    }

    internal static void ExpectOnAResult()
    {
        Result<string, string> nameResult = Result.Ok<string, string>("Pelor");
        string name = nameResult.Expect("Expected a name, but got an error");

        _ = name;
    }

    internal static void MapOr()
    {
        Result<string, string> nameResult = Result.Err<string, string>("Error");

        int length = nameResult.MapOr(0, name => name.Length);
        //  ^? 0

        _ = length;
    }

    internal static void MapOrElseOnAnOption()
    {
        Option<Adventurer> maybeAdventurer = Option.None<Adventurer>();

        Uri portrait = maybeAdventurer.MapOrElse(
            () => GeneratePortrait(),
            adventurer => adventurer.Portrait);

        _ = portrait;
    }

    internal static void MapOrElseOnAResult()
    {
        Result<Adventurer, Error> getAdventurerResult = GetAdventurer("Changebringer");

        Uri portrait = getAdventurerResult.MapOrElse(
            error => GeneratePortrait(),
            adventurer => adventurer.Portrait);

        _ = portrait;
    }

    internal static void MapOrDefaultOnAnOption()
    {
        Option<string> maybeName = Option.None<string>();
        int length = maybeName.MapOrDefault(name => name.Length);
        //  ^? 0

        _ = length;
    }

    internal static void MapOrDefaultOnAResult()
    {
        Result<string, string> nameResult = Result.Err<string, string>("Error");
        int length = nameResult.MapOrDefault(name => name.Length);
        //  ^? 0

        _ = length;
    }

    internal static void MapOrNull()
    {
        Option<string> maybeName = Option.None<string>();
        int? length = maybeName.MapOrNull(name => name.Length);
        //   ^? null, where MapOrDefault would have given you 0

        _ = length;
    }

    private static Result<DateTime, Error> SafeParse(string input) =>
        DateTime.TryParse(input, out DateTime parsed)
            ? Result.Ok<DateTime, Error>(parsed)
            : Result.Err<DateTime, Error>(new Error("date.malformed", "not a date"));

    private static Result<Loadout, Error> GetLoadout(string name) =>
        Result.Err<Loadout, Error>(new Error("loadout.missing", "no loadout"));

    private static Result<Adventurer, Error> GetAdventurer(string name) =>
        Result.Ok<Adventurer, Error>(
            new Adventurer(new Uri("https://example.test/portrait.png")));

    private static Uri GeneratePortrait() =>
        new("https://example.test/generated.png");

    private static Loadout GenerateDefaultLoadout() => new("Rusty dagger");
}
