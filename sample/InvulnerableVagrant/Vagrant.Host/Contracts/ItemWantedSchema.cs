namespace Vagrant.Host.Contracts;

using Vagrant.Catalog;
using Waystone.Monads.Results;
using Waystone.Monads.Schemas;

/// <summary>Turns one counter entry into something to take off the shelf.</summary>
/// <remarks>
/// The quantity is bounded at one here rather than checked again later.
/// <c>LineItem.For</c> throws on a line of none, and a request asking for none of
/// something is a client that has not decided what it wants — so it is refused at the
/// edge, with the field path, and the aggregate never sees it.
/// </remarks>
internal sealed partial class ItemWantedSchema
    : SchemaConfig<ItemWantedRequest, Wanted>
{
    /// <inheritdoc />
    protected override Result<Wanted, SchemaViolation> Configure(
        ItemWantedRequest subject)
    {
        ArgumentNullException.ThrowIfNull(subject);

        return Schema.Fields(
                          Schema.Required(
                                     subject.Item,
                                     Schema.Uuid
                                           .NotEmpty()
                                           .Transform(
                                                value => new StockedItemId(value)))
                                .Named("item"),
                          Schema.Required(
                                     subject.Quantity,
                                     Schema.Number.Int32
                                           .AtLeast(1)
                                           .Transform(value => (uint)value))
                                .Named("quantity"))
                     .Into((item, quantity) => new Wanted(item, quantity));
    }
}
