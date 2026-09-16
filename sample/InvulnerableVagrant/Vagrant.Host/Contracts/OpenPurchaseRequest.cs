namespace Vagrant.Host.Contracts;

using Waystone.Monads.Options;

/// <summary>What a patron sends to start buying.</summary>
/// <remarks>
/// No prices. The body names what the patron wants and the shop reads its own shelf
/// labels, so a client cannot state what it intends to be charged.
/// </remarks>
internal sealed record OpenPurchaseRequest
{
    /// <summary>Who is buying.</summary>
    public Option<Guid> Patron { get; init; } = Option.None<Guid>();

    /// <summary>What they have brought to the counter.</summary>
    public Option<IReadOnlyList<ItemWantedRequest>> Items { get; init; } =
        Option.None<IReadOnlyList<ItemWantedRequest>>();
}
