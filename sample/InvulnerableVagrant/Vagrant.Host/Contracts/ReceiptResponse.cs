namespace Vagrant.Host.Contracts;

using Vagrant.Ordering;

/// <summary>What a patron walks away with once the coin has moved.</summary>
/// <remarks>
/// The shop keeps no receipts. This is derived from the settlement and handed over once;
/// what the shop keeps is the purchase or the buyback, and that it is settled.
/// </remarks>
/// <param name="Id">Which receipt this is.</param>
/// <param name="Patron">Whose it is.</param>
/// <param name="Moved">How much coin changed hands.</param>
/// <param name="At">When it did.</param>
internal sealed record ReceiptResponse(
    Guid Id,
    Guid Patron,
    string Moved,
    DateTimeOffset At)
{
    /// <summary>Reads a receipt into the shape that goes on the wire.</summary>
    /// <param name="receipt">The receipt.</param>
    /// <returns>The response.</returns>
    public static ReceiptResponse From(Receipt receipt) =>
        new(receipt.Id.Value, receipt.Patron.Value, receipt.Moved.ToString(), receipt.At);
}
