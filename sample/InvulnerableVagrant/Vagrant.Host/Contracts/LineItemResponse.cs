namespace Vagrant.Host.Contracts;

using Vagrant.Ordering;
using Waystone.Monads.Options;

/// <summary>One line of a purchase as a patron sees it.</summary>
/// <remarks>
/// <para>
/// The agreed price is the <c>Option</c> that makes the case for putting one on the
/// wire. A line nobody haggled over has no agreed price, and that is a different fact
/// from an agreed price that happens to equal the asking price — a nullable decimal
/// would render both as the same number.
/// </para>
/// <para>
/// The floor is not here, for the same reason it is not on
/// <see cref="StockedItemResponse" />: a patron who could read it would never offer
/// anything else.
/// </para>
/// </remarks>
/// <param name="Item">Which line this is, for haggling over.</param>
/// <param name="Quantity">How many.</param>
/// <param name="AskingPrice">What the shop asked, per one of them.</param>
/// <param name="AgreedPrice">What was settled on, once anyone has haggled.</param>
/// <param name="Due">What the line comes to as it stands.</param>
internal sealed record LineItemResponse(
    Guid Item,
    uint Quantity,
    string AskingPrice,
    Option<string> AgreedPrice,
    string Due)
{
    /// <summary>Reads a line into the shape that goes on the wire.</summary>
    /// <param name="line">The line of the purchase.</param>
    /// <returns>The response.</returns>
    public static LineItemResponse From(LineItem line)
    {
        ArgumentNullException.ThrowIfNull(line);

        return new LineItemResponse(
            line.Subject.Value,
            line.Quantity,
            line.Asking.ToString(),
            line.Agreed.Map(static agreed => agreed.Settled.ToString()),
            line.Due.ToString());
    }
}
