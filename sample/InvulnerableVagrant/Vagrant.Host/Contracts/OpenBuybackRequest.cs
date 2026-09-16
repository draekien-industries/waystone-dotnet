namespace Vagrant.Host.Contracts;

using Waystone.Monads.Options;

/// <summary>What a clerk sends to take something in off a patron.</summary>
/// <remarks>
/// The figure is on the request because it is Pumat's, named at the counter after
/// looking at the thing. The shop has no shelf label to read for something it has never
/// stocked.
/// </remarks>
internal sealed record OpenBuybackRequest
{
    /// <summary>Who is selling.</summary>
    public Option<Guid> Patron { get; init; } = Option.None<Guid>();

    /// <summary>What they brought in.</summary>
    public Option<string> Description { get; init; } = Option.None<string>();

    /// <summary>What the shop offered for it.</summary>
    public Option<CoinRequest> Offered { get; init; } = Option.None<CoinRequest>();
}
