namespace Vagrant.Host.Contracts;

using Waystone.Monads.Options;

/// <summary>What a clerk sends to record an attempt at reading an item.</summary>
internal sealed record IdentifySpecimenRequest
{
    /// <summary>The roll and its modifiers, added up.</summary>
    public Option<int> Check { get; init; } = Option.None<int>();
}
