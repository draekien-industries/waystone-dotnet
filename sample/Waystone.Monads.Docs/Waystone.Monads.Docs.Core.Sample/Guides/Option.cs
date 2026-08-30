using Waystone.Monads.Options;
using Waystone.Monads.Options.Extensions;

namespace Waystone.Monads.Docs.Core.Sample.Guides;

/// <summary>guides/option.md</summary>
internal static class OptionGuide
{
    internal sealed record Character(string Name, Option<string> Patron);

    private interface ILogger
    {
        void LogInformation(string message, params object?[] args);
    }

    private static readonly ILogger logger = null!;

    internal static void PrintingAndLogging()
    {
        _ = Option.Some("Vex'ahlia").ToString(); // "Some { IsSome = True, IsNone = False }"
        _ = Option.None<string>().ToString();    // "None { IsSome = False, IsNone = True }"
    }

    internal static Option<Character> Inspecting(string name) =>
        FindCharacter(name)
            .Inspect(c => logger.LogInformation("Found character {Name}", c.Name));

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

    internal static Option<string> AndThen(string name) =>
        FindPatron(name).AndThen(TryExtractDomain);

    internal static void Filter()
    {
        Option<string> maybeName = Option.Some("Thordak");

        Option<string> nonEmpty = maybeName.Filter(name => name.Length > 0); // Some("Thordak")
        Option<string> blank = maybeName.Filter(name => name.Length == 0);   // None

        _ = (nonEmpty, blank);
    }

    internal static void Zip()
    {
        Option<string> vex = Option.Some("Vex'ahlia");
        Option<string> vax = Option.Some("Vax'ildan");
        Option<string> missing = Option.None<string>();

        Option<(string, string)> twins = vex.Zip(vax);      // Some(("Vex'ahlia", "Vax'ildan"))
        Option<(string, string)> alone = vex.Zip(missing);  // None

        _ = (twins, alone);
    }

    internal static void ZipWith()
    {
        Option<int> fireball = Option.Some(24);
        Option<int> sneakAttack = Option.Some(18);
        Option<int> total = fireball.ZipWith(sneakAttack, (a, b) => a + b);
        //         ^? Some(42)

        _ = total;
    }

    internal static void Unzip()
    {
        Option<(string, string)> twins = Option.Some(("Vex'ahlia", "Vax'ildan"));
        Option<(string, string)> none = Option.None<(string, string)>();

        (Option<string>, Option<string>) unzippedTwins = twins.Unzip(); // (Some("Vex'ahlia"), Some("Vax'ildan"))
        (Option<string>, Option<string>) unzippedNone = none.Unzip();   // (None, None)

        _ = (unzippedTwins, unzippedNone);
    }

    internal static void And()
    {
        Option<string> maybeName = Option.Some("Grog");
        Option<int> maybeLevel = Option.Some(19);

        Option<int> both = maybeName.And(maybeLevel);                // Some(19)
        Option<int> neither = Option.None<string>().And(maybeLevel); // None

        _ = (both, neither);
    }

    internal static void Reduce()
    {
        Option<int> firstRoll = Option.Some(3);
        Option<int> secondRoll = Option.Some(4);

        _ = firstRoll.Reduce(secondRoll, (a, b) => a + b);             // Some(7)
        _ = firstRoll.Reduce(Option.None<int>(), (a, b) => a + b);     // Some(3)
        _ = Option.None<int>().Reduce(secondRoll, (a, b) => a + b);    // Some(4)
    }

    internal static void Or()
    {
        Option<string> chosen = Option.Some("Keyleth");
        Option<string> absent = Option.None<string>();
        Option<string> fallback = Option.Some("The understudy");

        Option<string> result = chosen.Or(absent).Or(fallback); // Some("Keyleth")

        _ = result;
    }

    internal static void OrElse()
    {
        Option<string> first = Option.None<string>();

        Option<string> RollForAnother() => Option.None<string>();
        Option<string> SendInTheHireling() => Option.Some("The understudy");

        Option<string> result = first
            .OrElse(() => RollForAnother())
            .OrElse(() => SendInTheHireling()); // Some("The understudy")

        _ = result;
    }

    internal static void Xor()
    {
        Option<string> bardsong = Option.Some("Scanlan");
        Option<string> silence = Option.None<string>();
        Option<string> secondBard = Option.Some("Fig");

        Option<string> result = bardsong
            .Xor(silence)     // Some("Scanlan")
            .Xor(secondBard); // None

        _ = result;
    }

    internal static void AsEnumerable()
    {
        Option<string> maybeName = Option.Some("Pike");

        IEnumerable<string> sequence = maybeName.AsEnumerable();
        //                  ^? ["Pike"], and [] for a None

        _ = sequence;
    }

    private static Option<Character> FindCharacter(string name) =>
        Option.Some(new Character("Vax'ildan", Option.Some("The Raven Queen")));

    private static Option<string> FindPatron(string name) =>
        Option.Some("matron@ravenqueen.divine");

    private static Option<string> TryExtractDomain(string sigil) =>
        Option.Try(() => sigil.Split('@')[1]);
}
