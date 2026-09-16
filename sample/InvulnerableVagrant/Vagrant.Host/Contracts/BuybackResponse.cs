namespace Vagrant.Host.Contracts;

using Vagrant.Ordering;

/// <summary>Something the shop has agreed to take in, as a patron sees it.</summary>
/// <param name="Id">Which buyback this is.</param>
/// <param name="Patron">Who is selling.</param>
/// <param name="Description">What they brought in.</param>
/// <param name="Offered">What the shop said it would give.</param>
internal sealed record BuybackResponse(
    Guid Id,
    Guid Patron,
    string Description,
    string Offered)
{
    /// <summary>Reads a buyback into the shape that goes on the wire.</summary>
    /// <param name="buyback">The buyback.</param>
    /// <returns>The response.</returns>
    public static BuybackResponse From(Buyback buyback)
    {
        ArgumentNullException.ThrowIfNull(buyback);

        return new BuybackResponse(
            buyback.Id.Value,
            buyback.Patron.Value,
            buyback.Description,
            buyback.Offered.ToString());
    }
}
