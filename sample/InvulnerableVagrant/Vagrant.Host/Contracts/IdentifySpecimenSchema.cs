namespace Vagrant.Host.Contracts;

using Vagrant.Appraisal;
using Waystone.Monads.Results;
using Waystone.Monads.Schemas;

/// <summary>Turns an identify request into a check, or says why it is not one.</summary>
/// <remarks>
/// No bound on the total. An arcana check can land below zero, so the schema's whole job
/// here is to insist the field was sent at all.
/// </remarks>
internal sealed partial class IdentifySpecimenSchema
    : SchemaConfig<IdentifySpecimenRequest, ArcanaCheck>
{
    /// <inheritdoc />
    protected override Result<ArcanaCheck, SchemaViolation> Configure(
        IdentifySpecimenRequest subject)
    {
        ArgumentNullException.ThrowIfNull(subject);

        return Schema.Fields(
                          Schema.Required(subject.Check, Schema.Number.Int32)
                                .Named("check"))
                     .Into(total => new ArcanaCheck(total));
    }
}
