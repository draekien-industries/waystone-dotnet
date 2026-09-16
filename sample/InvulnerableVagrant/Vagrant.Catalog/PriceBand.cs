namespace Vagrant.Catalog;

using Vagrant.SharedKernel;
using Waystone.Monads.Results;
using Waystone.Monads.Results.Errors;

/// <summary>What the shop asks for an item, and the least it will take.</summary>
/// <remarks>
/// The two prices travel together because neither is meaningful alone: an asking price
/// with no floor says nothing about what haggling can reach, and a floor with no asking
/// price is not a price at all. Holding them as one value makes
/// <c>FloorPrice &lt;= AskingPrice</c> an invariant of the type rather than a rule every
/// caller has to remember.
/// </remarks>
public readonly record struct PriceBand
{
    private PriceBand(Coin askingPrice, Coin floorPrice)
    {
        AskingPrice = askingPrice;
        FloorPrice = floorPrice;
    }

    /// <summary>What the shop puts on the shelf label.</summary>
    public Coin AskingPrice { get; }

    /// <summary>The least the shop will take, however well a patron haggles.</summary>
    public Coin FloorPrice { get; }

    /// <summary>Sets what an item is worth between two prices.</summary>
    /// <param name="askingPrice">What the shop asks.</param>
    /// <param name="floorPrice">
    /// The least it will take. Equal to <paramref name="askingPrice" /> for an item the
    /// shop will not discount.
    /// </param>
    /// <returns>
    /// The band, or <see cref="CatalogError.FloorAboveAsking" /> when
    /// <paramref name="floorPrice" /> exceeds <paramref name="askingPrice" />.
    /// </returns>
    /// <remarks>
    /// The only way to build one. A band whose floor sits above its asking price would
    /// admit no offer the shelf label invites, so it cannot be constructed rather than
    /// being rejected later by whoever first notices.
    /// </remarks>
    public static Result<PriceBand, Error> Between(Coin askingPrice, Coin floorPrice) =>
        floorPrice > askingPrice
            ? Result.Err<PriceBand, Error>(
                CatalogErrorCatalog.Errors.FloorAboveAsking(
                    $"a floor of {floorPrice} is above an asking price of {askingPrice}"))
            : Result.Ok<PriceBand, Error>(new PriceBand(askingPrice, floorPrice));

    /// <summary>Whether the shop would take this much for the item.</summary>
    /// <param name="offer">What a patron has named.</param>
    /// <returns>
    /// <see langword="true" /> when <paramref name="offer" /> reaches
    /// <see cref="FloorPrice" />.
    /// </returns>
    /// <remarks>
    /// An offer above the asking price is admitted. Pumat does not argue a patron down
    /// to the label.
    /// </remarks>
    public bool Admits(Coin offer) => offer.IsAtLeast(FloorPrice);

    /// <inheritdoc />
    public override string ToString() =>
        AskingPrice == FloorPrice
            ? AskingPrice.ToString()
            : $"{AskingPrice} (floor {FloorPrice})";
}
