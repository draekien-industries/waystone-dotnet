namespace Vagrant.Host.Contracts;

using Waystone.Monads.Options;

/// <summary>One entry in what a patron has brought to the counter.</summary>
internal sealed record ItemWantedRequest
{
    /// <summary>Which line of stock, by the identifier on the shelf label.</summary>
    public Option<Guid> Item { get; init; } = Option.None<Guid>();

    /// <summary>How many of it.</summary>
    public Option<int> Quantity { get; init; } = Option.None<int>();
}
