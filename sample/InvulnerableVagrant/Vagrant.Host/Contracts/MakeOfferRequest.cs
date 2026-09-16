namespace Vagrant.Host.Contracts;

using Waystone.Monads.Options;

/// <summary>What a patron sends to haggle over one line.</summary>
internal sealed record MakeOfferRequest
{
    /// <summary>Which line, by the identifier the purchase carries for it.</summary>
    public Option<Guid> Item { get; init; } = Option.None<Guid>();

    /// <summary>What they will pay for the whole line, not for one of it.</summary>
    public Option<CoinRequest> Offer { get; init; } = Option.None<CoinRequest>();
}
