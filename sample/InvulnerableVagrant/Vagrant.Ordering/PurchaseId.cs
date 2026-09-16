namespace Vagrant.Ordering;

/// <summary>Identifies one patron's purchase.</summary>
/// <param name="Value">The underlying identifier.</param>
public readonly record struct PurchaseId(Guid Value)
{
    /// <summary>Mints an identifier for a purchase that has not been opened before.</summary>
    /// <returns>An identifier no existing purchase holds.</returns>
    /// <remarks>
    /// Version 7, so the value sorts by the moment it was minted. The identifier is the
    /// primary key of a SQLite table, and a random one scatters inserts across the
    /// index instead of appending to it.
    /// </remarks>
    public static PurchaseId New() => new(Guid.CreateVersion7());

    /// <inheritdoc />
    public override string ToString() => Value.ToString();
}
