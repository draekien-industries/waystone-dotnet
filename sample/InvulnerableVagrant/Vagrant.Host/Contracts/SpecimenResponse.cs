namespace Vagrant.Host.Contracts;

using Vagrant.Appraisal;
using Waystone.Monads.Options;

/// <summary>An item on the examination shelf as a patron sees it.</summary>
/// <remarks>
/// The enchantment is an <c>Option</c> on the wire, serialized by
/// <c>AddMonadConverters()</c>. A specimen nobody has read yet is a 200 with a null
/// field, not a 404 and not an error body — the shop holds the item and has nothing to
/// say about it yet.
/// </remarks>
/// <param name="Id">Which specimen this is.</param>
/// <param name="Description">What it looks like.</param>
/// <param name="Enchantment">What it does, once the shop has read it.</param>
internal sealed record SpecimenResponse(
    Guid Id,
    string Description,
    Option<Enchantment> Enchantment)
{
    /// <summary>Reads a specimen into the shape that goes on the wire.</summary>
    /// <param name="specimen">The item on the shelf.</param>
    /// <returns>The response.</returns>
    public static SpecimenResponse From(Specimen specimen)
    {
        ArgumentNullException.ThrowIfNull(specimen);

        return new SpecimenResponse(
            specimen.Id.Value,
            specimen.Description,
            specimen.Enchantment);
    }
}
