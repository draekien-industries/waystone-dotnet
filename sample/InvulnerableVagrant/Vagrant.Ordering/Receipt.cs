namespace Vagrant.Ordering;

using Vagrant.SharedKernel;

/// <summary>A record of coin having moved, in either direction.</summary>
/// <remarks>
/// The same shape for a purchase and a buyback, because the shop's books record the same
/// facts either way. <see cref="Moved" /> is an amount, not a signed balance: which way
/// it went is which aggregate wrote the receipt.
/// </remarks>
/// <param name="Id">Which receipt this is.</param>
/// <param name="Patron">Who was across the counter.</param>
/// <param name="Moved">How much changed hands.</param>
/// <param name="At">When it did.</param>
public readonly record struct Receipt(
    ReceiptId Id,
    PatronId Patron,
    Coin Moved,
    DateTimeOffset At);
