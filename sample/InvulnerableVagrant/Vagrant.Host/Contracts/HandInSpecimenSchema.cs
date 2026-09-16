namespace Vagrant.Host.Contracts;

using Vagrant.Appraisal;
using Vagrant.SharedKernel;
using Waystone.Monads.Results;
using Waystone.Monads.Schemas;

/// <summary>Turns a hand-in request into a specimen, or says why it is not one.</summary>
/// <remarks>
/// The only route from the wire to <see cref="Specimen.HandedIn" />. Obscurity arrives
/// as a signed number because JSON has no other kind, and leaves as the <c>uint</c> the
/// domain takes — <c>AtLeast(0)</c> is what makes that cast safe, so the domain needs no
/// guard against a negative it can no longer be given.
/// </remarks>
internal sealed partial class HandInSpecimenSchema
    : SchemaConfig<HandInSpecimenRequest, Specimen>
{
    /// <inheritdoc />
    protected override Result<Specimen, SchemaViolation> Configure(
        HandInSpecimenRequest subject)
    {
        ArgumentNullException.ThrowIfNull(subject);

        return Schema.Fields(
                          Schema.Required(
                                     subject.Patron,
                                     Schema.Uuid.NotEmpty()
                                           .Transform(value => new PatronId(value)))
                                .Named("patron"),
                          Schema.Required(
                                     subject.Description,
                                     Schema.Text.Trim().LengthBetween(3, 200))
                                .Named("description"),
                          Schema.Required(
                                     subject.Obscurity,
                                     Schema.Number.Int32.AtLeast(0)
                                           .Transform(value => new Aura((uint)value)))
                                .Named("obscurity"),
                          Schema.Required(
                                     subject.EnchantmentName,
                                     Schema.Text.Trim().NotEmpty())
                                .Named("enchantmentName"),
                          Schema.Required(
                                     subject.EnchantmentEffect,
                                     Schema.Text.Trim().NotEmpty())
                                .Named("enchantmentEffect"))
                     .Into(
                          (patron, description, aura, name, effect) =>
                              Specimen.HandedIn(
                                  SpecimenId.New(),
                                  patron,
                                  description,
                                  aura,
                                  new Enchantment(name, effect)));
    }
}
