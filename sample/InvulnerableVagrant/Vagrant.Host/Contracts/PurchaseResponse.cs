namespace Vagrant.Host.Contracts;

using Vagrant.Ordering;

/// <summary>A purchase as a patron sees it.</summary>
/// <remarks>
/// Prices are rendered as the shop writes them — <c>900gp</c>, not a count of copper.
/// The database stores one integer for the same amount, and neither shape is the other's
/// business.
/// </remarks>
/// <param name="Id">Which purchase this is.</param>
/// <param name="Patron">Who is buying.</param>
/// <param name="Lines">Everything on it.</param>
/// <param name="Total">What it comes to as it stands.</param>
internal sealed record PurchaseResponse(
    Guid Id,
    Guid Patron,
    IReadOnlyList<LineItemResponse> Lines,
    string Total)
{
    /// <summary>Reads a purchase into the shape that goes on the wire.</summary>
    /// <param name="purchase">The purchase.</param>
    /// <returns>The response.</returns>
    public static PurchaseResponse From(Purchase purchase)
    {
        ArgumentNullException.ThrowIfNull(purchase);

        return new PurchaseResponse(
            purchase.Id.Value,
            purchase.Patron.Value,
            [.. purchase.Lines.Select(LineItemResponse.From)],
            purchase.Total.ToString());
    }
}
