namespace Vagrant.Staffing;

/// <summary>Identifies one of the shop's clerks.</summary>
/// <remarks>
/// The only thing that tells the clerks apart. Three of the four answer to the same
/// name, so a model keyed on <see cref="Clerk.Name" /> would collapse them into one.
/// </remarks>
/// <param name="Value">The underlying identifier.</param>
public readonly record struct ClerkId(Guid Value)
{
    /// <summary>Mints an identifier for a clerk the shop has not had before.</summary>
    /// <returns>An identifier no clerk holds.</returns>
    /// <remarks>
    /// Version 7, so the value sorts by the moment it was minted. The identifier is the
    /// primary key of a SQLite table, and a random one scatters inserts across the
    /// index instead of appending to it.
    /// </remarks>
    public static ClerkId New() => new(Guid.CreateVersion7());

    /// <inheritdoc />
    public override string ToString() => Value.ToString();
}
