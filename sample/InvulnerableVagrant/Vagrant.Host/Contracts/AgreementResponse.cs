namespace Vagrant.Host.Contracts;

/// <summary>What the shop said to an offer.</summary>
/// <remarks>
/// The agreement, not the whole purchase. A patron who asks "will you take four hundred
/// for the potions" is owed an answer about the potions; re-reading the purchase to
/// return it would be a second query answering a question nobody asked.
/// </remarks>
/// <param name="Item">Which line was haggled over.</param>
/// <param name="AgreedPrice">What the shop will take for the whole line.</param>
internal sealed record AgreementResponse(Guid Item, string AgreedPrice);
