namespace Vagrant.Ordering;

using Vagrant.SharedKernel;

/// <summary>What a patron said they would pay.</summary>
/// <param name="Named">The amount offered.</param>
public readonly record struct Offer(Coin Named);
