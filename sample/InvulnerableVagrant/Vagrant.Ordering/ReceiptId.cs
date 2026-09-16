namespace Vagrant.Ordering;

/// <summary>Identifies one record of coin having moved.</summary>
/// <param name="Value">The underlying identifier.</param>
public readonly record struct ReceiptId(Guid Value)
{
    /// <summary>Mints an identifier for a receipt not yet written.</summary>
    /// <returns>An identifier no existing receipt holds.</returns>
    /// <remarks>
    /// Version 7, so the value sorts by the moment it was minted, which is also the
    /// order the shop's takings are read in.
    /// </remarks>
    public static ReceiptId New() => new(Guid.CreateVersion7());

    /// <inheritdoc />
    public override string ToString() => Value.ToString();
}
