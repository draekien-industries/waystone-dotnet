using Waystone.Monads.Linq;
using Waystone.Monads.Options;

namespace Waystone.Monads.Docs.Linq.Sample;

/// <summary>packages/linq.md</summary>
internal static class LinqPage
{
    internal sealed record Adventurer(string Name, Option<string> HomeCity);

    internal sealed record TravelCost(decimal Gold);

    internal static Option<TravelCost> QuerySyntax(string name) =>
        from adventurer in FindAdventurer(name)
        from city in adventurer.HomeCity
        from rate in TeleportRateFor(city)
        select Price(adventurer, rate);

    internal static Option<TravelCost> TheSameChainByHand(string name) =>
        FindAdventurer(name)
            .AndThen(adventurer => adventurer.HomeCity
                .AndThen(city => TeleportRateFor(city)
                    .Map(rate => Price(adventurer, rate))));

    // The page also shows a `where` clause on a Result, to say it does not
    // compile. There is nothing to pin here: a sample that fails to build is
    // the claim itself, and this project would stop building if it were added.

    private static Option<Adventurer> FindAdventurer(string name) =>
        Option.Some(new Adventurer("Keyleth", Option.Some("Zephrah")));

    private static Option<decimal> TeleportRateFor(string city) =>
        Option.Some(25m);

    private static TravelCost Price(Adventurer adventurer, decimal rate) =>
        new(rate);
}
