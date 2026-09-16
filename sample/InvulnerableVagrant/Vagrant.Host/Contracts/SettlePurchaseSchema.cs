namespace Vagrant.Host.Contracts;

using Vagrant.SharedKernel;
using Waystone.Monads.Results;
using Waystone.Monads.Schemas;

/// <summary>Turns a payment into an amount, or says why it is not one.</summary>
/// <remarks>
/// Whether the amount covers the purchase is not asked here. The total is the
/// purchase's, the schema cannot see it, and a patron who is short is owed
/// <c>InsufficientCoin</c> and the two figures — not a field-level rejection.
/// </remarks>
internal sealed partial class SettlePurchaseSchema
    : SchemaConfig<SettlePurchaseRequest, Coin>
{
    /// <inheritdoc />
    protected override Result<Coin, SchemaViolation> Configure(
        SettlePurchaseRequest subject)
    {
        ArgumentNullException.ThrowIfNull(subject);

        return Schema.Fields(
                          Schema.Required(subject.Tendered, CoinSchema.Instance)
                                .Named("tendered"))
                     .Into(tendered => tendered);
    }
}
