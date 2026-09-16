namespace Vagrant.Catalog;

using Waystone.Monads.Results;
using Waystone.Monads.Results.Errors;

/// <summary>One line of the shop's stock: a thing, a price, and a count of it.</summary>
/// <remarks>
/// The aggregate root of this context. <see cref="OnHand" /> changes only through
/// <see cref="Withdraw" />, so no caller can set a count the shop does not have.
/// </remarks>
public sealed class StockedItem
{
    /// <remarks>
    /// For rehydration. Something outside this project reads a line of stock back from
    /// wherever it was kept and fills the properties in; it cannot call
    /// <see cref="Stock" />, because that mints a line the shop has never held.
    /// Private, so nothing here can reach it either.
    /// </remarks>
    private StockedItem()
    {
    }

    private StockedItem(StockedItemId id, string name, PriceBand band, uint onHand)
    {
        Id = id;
        Name = name;
        Band = band;
        OnHand = onHand;
    }

    /// <summary>Which line of stock this is.</summary>
    public StockedItemId Id { get; private init; }

    /// <summary>What Pumat calls it.</summary>
    public string Name { get; private init; } = string.Empty;

    /// <summary>What the shop asks, and the least it will take.</summary>
    public PriceBand Band { get; private init; }

    /// <summary>How many are on the shelf right now.</summary>
    public uint OnHand { get; private set; }

    /// <summary>Puts a line of stock on the shelf.</summary>
    /// <param name="id">Which line of stock this is.</param>
    /// <param name="name">What Pumat calls it.</param>
    /// <param name="band">What the shop asks, and the least it will take.</param>
    /// <param name="onHand">How many are on the shelf. Zero is allowed.</param>
    /// <returns>The line of stock.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="name" /> is blank.
    /// </exception>
    /// <remarks>
    /// Returns the item rather than a <c>Result</c>. The one thing that can be wrong
    /// about a price is caught by <see cref="PriceBand.Between" /> before a band exists
    /// to pass here, a count on hand is unsigned so it cannot be negative, and a blank
    /// name is a call site that stocked the shop badly.
    /// </remarks>
    public static StockedItem Stock(
        StockedItemId id,
        string name,
        PriceBand band,
        uint onHand)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new StockedItem(id, name.Trim(), band, onHand);
    }

    /// <summary>Takes some of this line off the shelf.</summary>
    /// <param name="quantity">How many to take.</param>
    /// <returns>
    /// What came off the shelf, or <see cref="CatalogError.NotEnoughOnHand" /> when
    /// fewer than <paramref name="quantity" /> are there.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="quantity" /> is zero.
    /// </exception>
    /// <remarks>
    /// A shortfall is a <c>Result</c> because a patron is owed the reason: they asked
    /// for four and the shop has one. Asking for none is not a shortfall — it is a
    /// caller that has not decided what it wants.
    /// </remarks>
    public Result<Withdrawal, Error> Withdraw(uint quantity)
    {
        ArgumentOutOfRangeException.ThrowIfZero(quantity);

        if (quantity > OnHand)
        {
            return Result.Err<Withdrawal, Error>(
                CatalogErrorCatalog.Errors.NotEnoughOnHand(
                    $"{Name}: {quantity} asked for, {OnHand} on hand"));
        }

        OnHand -= quantity;

        return Result.Ok<Withdrawal, Error>(new Withdrawal(Id, quantity, Band));
    }
}
