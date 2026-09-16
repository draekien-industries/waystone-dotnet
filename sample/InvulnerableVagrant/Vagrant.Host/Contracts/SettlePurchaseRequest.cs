namespace Vagrant.Host.Contracts;

using Waystone.Monads.Options;

/// <summary>What a patron sends to pay for a purchase.</summary>
/// <remarks>
/// The coin and nothing else. A body carrying the total as well would let a client state
/// what it believes it owes, and the shop would then have two figures to reconcile
/// instead of one to compare against.
/// </remarks>
internal sealed record SettlePurchaseRequest
{
    /// <summary>What is being put on the counter.</summary>
    public Option<CoinRequest> Tendered { get; init; } = Option.None<CoinRequest>();
}
