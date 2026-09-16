namespace Vagrant.Host.Contracts;

using Vagrant.Catalog;

/// <summary>A line of stock as a patron sees it.</summary>
/// <remarks>
/// The floor price is not here. It is what Pumat will come down to if pressed, and a
/// patron who could read it off the response would never offer anything else.
/// </remarks>
/// <param name="Id">Which line of stock this is.</param>
/// <param name="Name">What Pumat calls it.</param>
/// <param name="AskingPrice">What the shelf label says, as it is written there.</param>
/// <param name="OnHand">How many are on the shelf.</param>
internal sealed record StockedItemResponse(
    Guid Id,
    string Name,
    string AskingPrice,
    uint OnHand)
{
    /// <summary>Reads a line of stock into the shape that goes on the wire.</summary>
    /// <param name="item">The line of stock.</param>
    /// <returns>The response.</returns>
    public static StockedItemResponse From(StockedItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        return new StockedItemResponse(
            item.Id.Value,
            item.Name,
            item.Band.AskingPrice.ToString(),
            item.OnHand);
    }
}
