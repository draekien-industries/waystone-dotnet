namespace Vagrant.Host.Contracts;

using Vagrant.SharedKernel;
using Waystone.Monads.Results;
using Waystone.Monads.Schemas;

/// <summary>Turns a request to buy into one the shop can act on.</summary>
/// <remarks>
/// <c>MinCount(1)</c> says the same thing <c>LineItems.Of</c> says, and both stay.
/// The schema refuses an empty counter with a field path a client can fix; the aggregate
/// refuses it however it was reached, including from code no request ever touches.
/// </remarks>
internal sealed partial class OpenPurchaseSchema
    : SchemaConfig<OpenPurchaseRequest, OpenPurchase>
{
    /// <inheritdoc />
    protected override Result<OpenPurchase, SchemaViolation> Configure(
        OpenPurchaseRequest subject)
    {
        ArgumentNullException.ThrowIfNull(subject);

        return Schema.Fields(
                          Schema.Required(
                                     subject.Patron,
                                     Schema.Uuid
                                           .NotEmpty()
                                           .Transform(value => new PatronId(value)))
                                .Named("patron"),
                          Schema.Required(
                                     subject.Items,
                                     Schema.List(ItemWantedSchema.Instance).MinCount(1))
                                .Named("items"))
                     .Into((patron, items) => new OpenPurchase(patron, items));
    }
}
