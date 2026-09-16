namespace Vagrant.Catalog;

/// <summary>Identifies one line of the shop's stock.</summary>
/// <param name="Value">The underlying identifier.</param>
public readonly record struct StockedItemId(Guid Value)
{
    /// <summary>Mints an identifier for stock the shop has not held before.</summary>
    /// <returns>An identifier no existing line holds.</returns>
    /// <remarks>
    /// Version 7, so the value sorts by the moment it was minted. The identifier is the
    /// primary key of a SQLite table, and a random one scatters inserts across the
    /// index instead of appending to it.
    /// </remarks>
    public static StockedItemId New() => new(Guid.CreateVersion7());

    /// <inheritdoc />
    public override string ToString() => Value.ToString();
}
