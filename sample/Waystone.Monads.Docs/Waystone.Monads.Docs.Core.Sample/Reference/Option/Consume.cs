using System.Diagnostics;
using Waystone.Monads.Options;
using Waystone.Monads.Options.Extensions;

namespace Waystone.Monads.Docs.Core.Sample.Reference.OptionApi;

/// <summary>reference/option/consume.md</summary>
internal static class OptionConsume
{
    internal sealed record Adventurer(Uri Portrait);

    internal static void StateChecks()
    {
        Option<string> maybeName = Option.Some("Laudna");

        _ = maybeName.IsSome; // true
        _ = maybeName.IsNone; // false
    }

    internal static void IsSomeAnd()
    {
        Option<string> maybePatron = Option.Some("The Raven Queen");
        _ = maybePatron.IsSomeAnd(patron => patron.Length > 0); // true
    }

    internal static void IsNoneOr()
    {
        Option<string> maybePatron = Option.Some("The Raven Queen");
        _ = maybePatron.IsNoneOr(patron => patron.Length > 0);                 // true
        _ = maybePatron.IsNoneOr(patron => string.IsNullOrWhiteSpace(patron)); // false
    }

    internal static void Match()
    {
        Option<string> maybeName = Option.Some("Travis");

        int length = maybeName.Match(
            name => name.Length,
            () => 0);

        _ = length;
    }

    internal static void Deconstructing(Option<string> maybeName)
    {
        if (maybeName is Some<string>(var name))
        {
            Console.WriteLine(name.Length);
        }
    }

    // The page also shows `var (a, b) = maybeName;` to say it does not compile
    // -- Option deconstructs to one value, not two. A sample that fails to
    // build is the claim itself, so there is nothing to pin here. The same goes
    // for `option is None<string>()`, with the parentheses.

    internal static int SwitchingOverBothStates(Option<string> maybeName) =>
        maybeName switch
        {
            Some<string>(var name) => name.Length,
            None<string> => 0,
            _ => throw new UnreachableException(),
        };

    internal static void Unwrap()
    {
        Option<string> maybeName = Option.Some("Lorekeeper");
        string name = maybeName.Unwrap();

        _ = name;
    }

    internal static void UnwrapOr()
    {
        Option<string> maybeNickname = Option.None<string>();
        string nickname = maybeNickname.UnwrapOr("Lautna");
        //     ^? "Lautna"

        _ = nickname;
    }

    internal static void UnwrapOrElse()
    {
        Option<Uri> maybePortrait = Option.None<Uri>();
        Uri portrait = maybePortrait.UnwrapOrElse(() => GeneratePortrait());
        //  ^? generated portrait

        _ = portrait;
    }

    internal static void UnwrapOrDefault()
    {
        Option<string> maybeName = Option.None<string>();
        string? name = maybeName.UnwrapOrDefault();
        //      ^? null

        _ = name;
    }

    internal static void UnwrapOrNull()
    {
        Option<int> maybeCount = Option.None<int>();
        int? count = maybeCount.UnwrapOrNull();
        //   ^? null, where UnwrapOrDefault would have given you 0

        _ = count;
    }

    internal static void Expect()
    {
        Option<string> maybeName = Option.Some("Greymore");
        string name = maybeName.Expect("Expected a name, but got nothing.");

        _ = name;
    }

    internal static void MapOr()
    {
        Option<string> maybeName = Option.None<string>();
        int length = maybeName.MapOr(0, name => name.Length);
        //  ^? 0

        _ = length;
    }

    internal static void MapOrElse()
    {
        Option<Adventurer> maybeAdventurer = Option.None<Adventurer>();

        Uri portrait = maybeAdventurer.MapOrElse(
            () => GeneratePortrait(),
            adventurer => adventurer.Portrait);

        _ = portrait;
    }

    internal static void MapOrDefault()
    {
        Option<string> maybeName = Option.None<string>();
        int length = maybeName.MapOrDefault(name => name.Length);
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

    private static Uri GeneratePortrait() =>
        new("https://example.test/generated.png");
}
