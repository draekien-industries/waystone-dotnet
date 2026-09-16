namespace Vagrant.Host.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Vagrant.Catalog;
using Vagrant.SharedKernel;
using Waystone.Monads.Results;
using Waystone.Monads.Results.Errors;

/// <summary>What is on the shelves when the shop opens for the first time.</summary>
/// <remarks>
/// <para>
/// Prices are the Dungeon Master's Guide's, and the floor is Pumat's: he will come down
/// about a tenth on most things and not at all on the two he had to go looking for.
/// </para>
/// <para>
/// Nothing here throws on a bad price. <see cref="PriceBand.Between" /> returns a
/// <c>Result</c>, so the seed returns one too, and it travels to <c>Program</c> where a
/// shop that cannot open becomes a log line and an exit code. That is the whole point of
/// the return type: a failure nobody can act on locally is passed to somebody who can.
/// </para>
/// </remarks>
internal static class CatalogSeed
{
    private static readonly Shelf[] Shelves =
    [
        new("Potion of Healing", 50, 45, 12),
        new("Potion of Greater Healing", 250, 225, 4),
        new("Driftglobe", 750, 700, 3),
        new("Rope of Climbing", 2000, 1800, 2),
        new("Bag of Holding", 4000, 4000, 1),
        new("Cloak of Elvenkind", 5000, 4500, 1),
        new("Immovable Rod", 5000, 5000, 1),
    ];

    /// <summary>Stocks the shelves, unless they are stocked already.</summary>
    /// <param name="db">The catalog's rows.</param>
    /// <param name="ct">Cancels the work.</param>
    /// <returns>
    /// How many lines were added — zero when the shop was already stocked — or the first
    /// line whose floor price sits above its asking price.
    /// </returns>
    public static async Task<Result<int, Error>> StockTheShelvesAsync(
        CatalogDbContext db,
        CancellationToken ct)
    {
        if (await db.StockedItems.AnyAsync(ct).ConfigureAwait(false))
        {
            return Result.Ok<int, Error>(0);
        }

        Result<int, Error> priced = Price(Shelves)
           .Inspect(db, static (lines, context) => context.StockedItems.AddRange(lines))
           .Map(static lines => lines.Count);

        if (priced.IsErr) return priced;

        await db.SaveChangesAsync(ct).ConfigureAwait(false);

        return priced;
    }

    /// <remarks>
    /// Stops at the first bad line rather than reporting every one. A shop that will not
    /// open needs one reason, and the next line's price is not evidence about this one.
    /// </remarks>
    private static Result<List<StockedItem>, Error> Price(Shelf[] shelves)
    {
        List<StockedItem> priced = new(shelves.Length);

        foreach (Shelf shelf in shelves)
        {
            Result<List<StockedItem>, Error> step = Line(shelf)
               .AndThen(
                    priced,
                    static (line, into) =>
                    {
                        into.Add(line);

                        return Result.Ok<List<StockedItem>, Error>(into);
                    });

            if (step.IsErr) return step;
        }

        return Result.Ok<List<StockedItem>, Error>(priced);
    }

    private static Result<StockedItem, Error> Line(Shelf shelf) =>
        PriceBand
           .Between(Coin.FromGold(shelf.Asking), Coin.FromGold(shelf.Floor))
           .Match(
                shelf,
                static (band, line) => Result.Ok<StockedItem, Error>(
                    StockedItem.Stock(
                        StockedItemId.New(),
                        line.Name,
                        band,
                        line.OnHand)),
                static (error, _) => Result.Err<StockedItem, Error>(error));

    private readonly record struct Shelf(
        string Name,
        uint Asking,
        uint Floor,
        uint OnHand);
}
