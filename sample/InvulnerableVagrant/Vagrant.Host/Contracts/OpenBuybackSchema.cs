namespace Vagrant.Host.Contracts;

using Vagrant.Ordering;
using Vagrant.SharedKernel;
using Waystone.Monads.Results;
using Waystone.Monads.Schemas;

/// <summary>Turns a request to buy something in into a buyback, or says why not.</summary>
/// <remarks>
/// Produces the aggregate rather than a parsed request, because the body holds
/// everything <c>Buyback.Open</c> needs. Nothing downstream of the parse accepts an
/// unparsed body, and there is no second validation step.
/// </remarks>
internal sealed partial class OpenBuybackSchema
    : SchemaConfig<OpenBuybackRequest, Buyback>
{
    /// <inheritdoc />
    protected override Result<Buyback, SchemaViolation> Configure(
        OpenBuybackRequest subject)
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
                                     subject.Description,
                                     Schema.Text.Trim().LengthBetween(3, 200))
                                .Named("description"),
                          Schema.Required(subject.Offered, CoinSchema.Instance)
                                .Named("offered"))
                     .Into(
                          (patron, description, offered) => Buyback.Open(
                              BuybackId.New(),
                              patron,
                              description,
                              offered));
    }
}
