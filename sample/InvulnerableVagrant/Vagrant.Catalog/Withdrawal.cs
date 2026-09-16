namespace Vagrant.Catalog;

/// <summary>What came off the shelf, and what the shop was asking for it.</summary>
/// <remarks>
/// The band travels with the withdrawal because the price an item was taken at is the
/// price in force at that moment. Re-reading the shelf label afterwards would price a
/// purchase at whatever the shop is asking by the time it settles.
/// </remarks>
/// <param name="Item">Which line of stock it came from.</param>
/// <param name="Quantity">How many were taken.</param>
/// <param name="Band">What the shop was asking, and the least it would take.</param>
public readonly record struct Withdrawal(
    StockedItemId Item,
    uint Quantity,
    PriceBand Band);
