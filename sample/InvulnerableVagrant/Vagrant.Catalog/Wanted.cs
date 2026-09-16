namespace Vagrant.Catalog;

/// <summary>How many of one line of stock someone has asked the shop for.</summary>
/// <remarks>
/// What goes in where <see cref="Withdrawal" /> comes out. The shop is told which line
/// and how many; what it was asking is the shop's to add.
/// </remarks>
/// <param name="Item">Which line of stock.</param>
/// <param name="Quantity">How many of it.</param>
public readonly record struct Wanted(StockedItemId Item, uint Quantity);
