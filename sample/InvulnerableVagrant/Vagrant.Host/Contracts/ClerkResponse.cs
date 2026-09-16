namespace Vagrant.Host.Contracts;

using Vagrant.Staffing;
using Waystone.Monads.Options;

/// <summary>A clerk behind the counter as a patron sees them.</summary>
/// <remarks>
/// <para>
/// <c>Holding</c> is an <c>Option</c> on the wire, serialized by
/// <c>AddMonadConverters()</c>: a free clerk is a <c>null</c>, not a zero GUID and not
/// an omitted property. The same reason <c>SpecimenResponse.Enchantment</c> is one — the
/// shop has nothing to say rather than something empty to say.
/// </para>
/// <para>
/// Three of the four rows come back named "Pumat Sol". That is the roster, not a bug:
/// the simulacra share a name and are told apart by <c>Id</c>.
/// </para>
/// </remarks>
/// <param name="Id">Which clerk this is.</param>
/// <param name="Name">What they introduce themselves as.</param>
/// <param name="Holding">What they are working on, if anything.</param>
internal sealed record ClerkResponse(Guid Id, string Name, Option<Guid> Holding)
{
    /// <summary>Reads a clerk into the shape that goes on the wire.</summary>
    /// <param name="clerk">Whoever is behind the counter.</param>
    /// <returns>The response.</returns>
    public static ClerkResponse From(Clerk clerk)
    {
        ArgumentNullException.ThrowIfNull(clerk);

        return new ClerkResponse(
            clerk.Id.Value,
            clerk.Name,
            clerk.Engagement.Map(
                static assignment => assignment.Errand.Subject.Value));
    }
}
