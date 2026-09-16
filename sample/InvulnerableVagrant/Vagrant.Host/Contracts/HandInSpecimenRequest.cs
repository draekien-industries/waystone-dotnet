namespace Vagrant.Host.Contracts;

using Waystone.Monads.Options;

/// <summary>What a patron sends to leave an item for examination.</summary>
/// <remarks>
/// <para>
/// Every field is an <c>Option</c>, not a nullable, and each is initialized to
/// <c>None</c>. A body that omits a field leaves the initializer in place, so the
/// absent case arrives as <c>None</c> rather than as a null nobody checked.
/// </para>
/// <para>
/// Obscurity is signed here and unsigned in the domain. JSON has one kind of number,
/// and <see cref="HandInSpecimenSchema" /> is where the two meet.
/// </para>
/// </remarks>
internal sealed record HandInSpecimenRequest
{
    /// <summary>Who is leaving the item.</summary>
    public Option<Guid> Patron { get; init; } = Option.None<Guid>();

    /// <summary>What it looks like.</summary>
    public Option<string> Description { get; init; } = Option.None<string>();

    /// <summary>How hard the shop expects it to be to read.</summary>
    public Option<int> Obscurity { get; init; } = Option.None<int>();

    /// <summary>What the item's effect is called.</summary>
    public Option<string> EnchantmentName { get; init; } = Option.None<string>();

    /// <summary>What the effect does.</summary>
    public Option<string> EnchantmentEffect { get; init; } = Option.None<string>();
}
