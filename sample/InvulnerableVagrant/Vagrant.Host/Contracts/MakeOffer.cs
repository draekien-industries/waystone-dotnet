namespace Vagrant.Host.Contracts;

using Vagrant.Ordering;

/// <summary>A price a patron has named for one line of a purchase.</summary>
/// <param name="Subject">Which line they are haggling over.</param>
/// <param name="Offer">What they said they would pay for the whole line.</param>
internal readonly record struct MakeOffer(LineItemSubject Subject, Offer Offer);
