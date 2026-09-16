namespace Vagrant.Ordering;

using Vagrant.SharedKernel;

/// <summary>What the shop and a patron settled on for one line.</summary>
/// <param name="Settled">The amount agreed.</param>
public readonly record struct AgreedPrice(Coin Settled);
