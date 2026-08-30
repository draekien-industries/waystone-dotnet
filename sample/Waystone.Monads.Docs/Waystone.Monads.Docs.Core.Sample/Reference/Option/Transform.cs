using Waystone.Monads.Options;
using Waystone.Monads.Options.Extensions;

namespace Waystone.Monads.Docs.Core.Sample.Reference.OptionApi;

/// <summary>reference/option/transform.md</summary>
internal static class OptionTransform
{
    internal static void Map()
    {
        Option<string> maybeName = Option.Some("Henry Crabgrass");
        Option<int> maybeLength = maybeName.Map(name => name.Length);

        _ = maybeLength;
    }

    internal static void AndThen(Option<string> maybeSigil)
    {
        Option<string> maybeDomain = maybeSigil.AndThen(TryExtractDomain);

        _ = maybeDomain;
    }

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

        Option<(string, string)> twins = vex.Zip(vax);     // Some(("Vex'ahlia", "Vax'ildan"))
        Option<(string, string)> alone = vex.Zip(missing); // None

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

        _ = twins.Unzip(); // (Some("Vex'ahlia"), Some("Vax'ildan"))
        _ = none.Unzip();  // (None, None)

        _ = Option.Some((0, "x")).Unzip(); // (Some(0), Some("x"))
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

        _ = firstRoll.Reduce(secondRoll, (a, b) => a + b);          // Some(7)
        _ = firstRoll.Reduce(Option.None<int>(), (a, b) => a + b);  // Some(3)
        _ = Option.None<int>().Reduce(secondRoll, (a, b) => a + b); // Some(4)
    }

    internal static void Or()
    {
        Option<string> chosen = Option.Some("Keyleth");
        Option<string> absent = Option.None<string>();
        Option<string> fallback = Option.Some("The understudy");

        Option<string> result = chosen.Or(absent).Or(fallback);
        //             ^? Some("Keyleth")

        _ = result;
    }

    internal static void OrElse()
    {
        Option<string> first = Option.None<string>();

        Option<string> result = first
            .OrElse(() => RollForAnother())
            .OrElse(() => SendInTheHireling());
        //     ^? Some("The understudy")

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

    private static Option<string> TryExtractDomain(string sigil) =>
        Option.Try(() => sigil.Split('@')[1]);

    private static Option<string> RollForAnother() => Option.None<string>();

    private static Option<string> SendInTheHireling() => Option.Some("The understudy");
}
