namespace Vagrant.SharedKernel;

/// <summary>Identifies someone who trades with the Invulnerable Vagrant.</summary>
/// <remarks>
/// Only the identity is shared. Each context holds whatever else it needs to know about
/// a patron itself, so nothing here grows a field because one context wanted it.
/// </remarks>
/// <param name="Value">The underlying identifier.</param>
public readonly record struct PatronId(Guid Value)
{
    /// <summary>Mints an identifier for a patron the shop has not seen before.</summary>
    /// <returns>An identifier no existing patron holds.</returns>
    /// <remarks>
    /// Version 7, so the value sorts by the moment it was minted. Identifiers here are
    /// stored keys, and a random one scatters inserts across the index instead of
    /// appending to it.
    /// </remarks>
    public static PatronId New() => new(Guid.CreateVersion7());

    /// <inheritdoc />
    public override string ToString() => Value.ToString();
}
