namespace Vagrant.Host.Translation;

using Vagrant.Catalog;
using Vagrant.Ordering;
using Waystone.Monads.Results;
using Waystone.Monads.Results.Errors;

/// <summary>Turns what came off the shelf into what goes on a purchase.</summary>
/// <remarks>
/// <para>
/// The seam between two contexts that cannot see each other. Catalog knows a
/// <see cref="Withdrawal" /> and Ordering knows a <see cref="LineItem" />; the two are
/// the same physical thing under different names, and this is the one place that says
/// so.
/// </para>
/// <para>
/// A <see cref="LineItemSubject" /> is built from the <c>StockedItemId</c>'s value
/// rather than being one. Ordering would otherwise reference Catalog, and a purchase
/// would stop being something the shop could sell a thing it never stocked.
/// </para>
/// <para>
/// The prices travel with the withdrawal. Reading the shelf label again here would price
/// a purchase at whatever the shop is asking by the time it settles.
/// </para>
/// </remarks>
internal static class PurchaseLines
{
    /// <summary>Reads a set of withdrawals as the lines of a purchase.</summary>
    /// <param name="withdrawals">What came off the shelf.</param>
    /// <returns>
    /// The lines, or <c>NoLineItems</c> when nothing came off the shelf at all.
    /// </returns>
    public static Result<LineItems, Error> From(IReadOnlyList<Withdrawal> withdrawals)
    {
        ArgumentNullException.ThrowIfNull(withdrawals);

        return LineItems.Of(withdrawals.Select(Line));
    }

    private static LineItem Line(Withdrawal withdrawal) =>
        LineItem.For(
            new LineItemSubject(withdrawal.Item.Value),
            withdrawal.Quantity,
            withdrawal.Band.AskingPrice,
            withdrawal.Band.FloorPrice);
}
