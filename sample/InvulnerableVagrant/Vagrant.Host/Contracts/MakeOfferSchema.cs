namespace Vagrant.Host.Contracts;

using Vagrant.Ordering;
using Waystone.Monads.Results;
using Waystone.Monads.Schemas;

/// <summary>Turns a haggling request into a named price, or says why it is not one.</summary>
/// <remarks>
/// No lower bound on the offer. Whether a figure is too low is the line's floor to
/// decide, and the floor is not something a schema at the edge can see — so an offer of
/// one copper parses and is then refused by <c>LineItem.Agree</c>, with the reason.
/// </remarks>
internal sealed partial class MakeOfferSchema : SchemaConfig<MakeOfferRequest, MakeOffer>
{
    /// <inheritdoc />
    protected override Result<MakeOffer, SchemaViolation> Configure(
        MakeOfferRequest subject)
    {
        ArgumentNullException.ThrowIfNull(subject);

        return Schema.Fields(
                          Schema.Required(
                                     subject.Item,
                                     Schema.Uuid
                                           .NotEmpty()
                                           .Transform(
                                                value => new LineItemSubject(value)))
                                .Named("item"),
                          Schema.Required(subject.Offer, CoinSchema.Instance)
                                .Named("offer"))
                     .Into((item, offer) => new MakeOffer(item, new Offer(offer)));
    }
}
