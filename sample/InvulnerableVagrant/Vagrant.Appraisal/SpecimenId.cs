namespace Vagrant.Appraisal;

/// <summary>Identifies one item the shop has been asked to examine.</summary>
/// <param name="Value">The underlying identifier.</param>
public readonly record struct SpecimenId(Guid Value)
{
    /// <summary>Mints an identifier for an item the shop has not seen before.</summary>
    /// <returns>An identifier no specimen on the shelf holds.</returns>
    /// <remarks>
    /// Version 7, so the value sorts by the moment it was minted. The identifier is the
    /// primary key of a SQLite table, and a random one scatters inserts across the
    /// index instead of appending to it.
    /// </remarks>
    public static SpecimenId New() => new(Guid.CreateVersion7());

    /// <inheritdoc />
    public override string ToString() => Value.ToString();
}
