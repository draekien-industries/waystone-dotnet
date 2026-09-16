namespace Vagrant.Appraisal;

using Vagrant.SharedKernel;
using Waystone.Monads.Options;

/// <summary>An item handed to the shop whose magical nature is not yet known.</summary>
/// <remarks>
/// A specimen carries its enchantment from the moment it arrives. Identification changes
/// what the shop knows, not what the item is, so <see cref="Identify" /> flips a flag and
/// <see cref="Enchantment" /> reads the value that was there all along.
/// </remarks>
public sealed class Specimen
{
    /// <remarks>
    /// For rehydration. Something outside this project reads a specimen back from
    /// wherever it was kept and fills the properties in; it cannot call
    /// <see cref="HandedIn" />, because that mints an item the shop has never been given.
    /// Private, so nothing here can reach it either.
    /// </remarks>
    private Specimen()
    { }

    private Specimen(
        SpecimenId id,
        PatronId handedInBy,
        string description,
        Aura aura,
        Enchantment carries)
    {
        Id = id;
        HandedInBy = handedInBy;
        Description = description;
        Aura = aura;
        Carries = carries;
    }

    /// <summary>Which item on the shelf this is.</summary>
    public SpecimenId Id { get; private init; }

    /// <summary>Who left it to be examined.</summary>
    public PatronId HandedInBy { get; private init; }

    /// <summary>What it looks like to someone who cannot read its aura.</summary>
    public string Description { get; private init; } = string.Empty;

    /// <summary>The magical signature it gives off.</summary>
    public Aura Aura { get; private init; }

    /// <summary>What it does, once the shop has read it.</summary>
    /// <remarks>
    /// <c>None</c> until an examination succeeds, and never <c>None</c> again after one
    /// does. This is the sample's plainest <c>Option</c>: a specimen nobody could read
    /// is not a failure to report, it is Pumat saying he could not tell.
    /// </remarks>
    public Option<Enchantment> Enchantment =>
        Identified
            ? Option.Some(Carries)
            : Option.None<Enchantment>();

    /// <summary>The enchantment the item has whether or not anyone has read it.</summary>
    /// <remarks>
    /// Internal, not public. The host persists it and the tests arrange it; a caller
    /// inside the domain reads <see cref="Enchantment" /> and gets nothing until the
    /// shop has earned it.
    /// </remarks>
    internal Enchantment Carries { get; private init; }

    /// <summary>Whether an examination has read the aura.</summary>
    internal bool Identified { get; private set; }

    /// <summary>Takes in an item a patron wants examined.</summary>
    /// <param name="id">Which specimen this becomes.</param>
    /// <param name="handedInBy">Who left it.</param>
    /// <param name="description">What it looks like.</param>
    /// <param name="aura">How hard it will be to read.</param>
    /// <param name="carries">What it actually does.</param>
    /// <returns>A specimen the shop holds and has not yet read.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="description" /> is null, empty or whitespace.
    /// </exception>
    /// <remarks>
    /// Not a <c>Result</c>. Every way this can be called wrongly is a mistake in the
    /// caller rather than something a patron is owed an explanation for, and the one
    /// remaining argument is checked by the schema before it reaches here.
    /// </remarks>
    public static Specimen HandedIn(
        SpecimenId id,
        PatronId handedInBy,
        string description,
        Aura aura,
        Enchantment carries)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        return new Specimen(id, handedInBy, description.Trim(), aura, carries);
    }

    /// <summary>Reads the specimen's aura and keeps whatever it gives up.</summary>
    /// <param name="check">How well the examination went.</param>
    /// <remarks>
    /// Returns nothing, and succeeds whatever happens. A check that falls short leaves
    /// <see cref="Enchantment" /> as <c>None</c>, and a second look at an item already
    /// read changes nothing — once the shop knows what something does it does not
    /// unlearn it, so there is no "already identified" failure for a caller to handle.
    /// </remarks>
    public void Identify(ArcanaCheck check)
    {
        if (Identified) return;

        Identified = Aura.YieldsTo(check);
    }
}
