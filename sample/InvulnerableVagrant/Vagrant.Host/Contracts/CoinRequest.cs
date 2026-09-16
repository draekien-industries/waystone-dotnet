namespace Vagrant.Host.Contracts;

using Waystone.Monads.Options;

/// <summary>An amount of money as a patron counts it out.</summary>
/// <remarks>
/// Four denominations rather than the single count of copper the database stores. A
/// patron putting nine hundred gold on the counter says so; converting that to ninety
/// thousand copper before sending it is the client doing the shop's arithmetic.
/// </remarks>
internal sealed record CoinRequest
{
    /// <summary>How many platinum pieces.</summary>
    public Option<int> Platinum { get; init; } = Option.None<int>();

    /// <summary>How many gold pieces.</summary>
    public Option<int> Gold { get; init; } = Option.None<int>();

    /// <summary>How many silver pieces.</summary>
    public Option<int> Silver { get; init; } = Option.None<int>();

    /// <summary>How many copper pieces.</summary>
    public Option<int> Copper { get; init; } = Option.None<int>();
}
