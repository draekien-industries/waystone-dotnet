namespace Vagrant.Host.Contracts;

using Vagrant.Catalog;
using Vagrant.SharedKernel;

/// <summary>A patron at the counter with things they mean to buy.</summary>
/// <remarks>
/// The only schema here that does not produce a domain type.
/// <see cref="OpenBuybackSchema" /> produces a <c>Buyback</c>, because the figure on a
/// buyback is Pumat's and arrives with the request. A purchase is opened on the shop's
/// own prices, which are the Catalog's to state and are not in the body at all, so what
/// the parse can honestly produce is the request understood — a patron and a list of
/// shelf labels — and the endpoint takes it from there.
/// </remarks>
/// <param name="Patron">Who is buying.</param>
/// <param name="Items">What they have asked for.</param>
internal sealed record OpenPurchase(
    PatronId Patron,
    IReadOnlyList<Wanted> Items);
