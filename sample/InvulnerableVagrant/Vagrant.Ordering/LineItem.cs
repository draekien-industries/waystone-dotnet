namespace Vagrant.Ordering;

using Vagrant.SharedKernel;
using Waystone.Monads.Options;
using Waystone.Monads.Results;
using Waystone.Monads.Results.Errors;

/// <summary>One thing on a purchase, and what it will cost.</summary>
/// <remarks>
/// A class rather than a value object, because haggling changes a line without changing
/// which line it is. It has no life outside the <see cref="Purchase" /> that carries it.
///
/// <see cref="Asking" /> and <see cref="Floor" /> are per unit, and both are
/// <c>Coin</c> rather than Catalog's <c>PriceBand</c> — <c>Coin</c> is shared, a
/// <c>PriceBand</c> is not, and the invariant it carries was enforced when the item was
/// stocked.
/// </remarks>
public sealed class LineItem
{
    /// <remarks>
    /// For rehydration. Something outside this project reads a line back from wherever it
    /// was kept and fills the properties in; it cannot call <see cref="For" />, because
    /// that mints a line nobody put on a purchase. Private, so nothing here can reach it
    /// either.
    /// </remarks>
    private LineItem()
    { }

    private LineItem(LineItemSubject subject, uint quantity, Coin asking, Coin floor)
    {
        Subject = subject;
        Quantity = quantity;
        Asking = asking;
        Floor = floor;
    }

    /// <summary>What the line is for.</summary>
    public LineItemSubject Subject { get; private init; }

    /// <summary>How many of it.</summary>
    public uint Quantity { get; private init; }

    /// <summary>What the shop asks for one of them.</summary>
    public Coin Asking { get; private init; }

    /// <summary>The least the shop will take for one of them.</summary>
    public Coin Floor { get; private init; }

    /// <summary>What was settled on, if anyone haggled.</summary>
    /// <remarks>
    /// <c>None</c> is not the same as an agreed price equal to the asking price. One
    /// means nobody haggled; the other means somebody did and got nowhere.
    /// </remarks>
    public Option<AgreedPrice> Agreed =>
        Haggled
            ? Option.Some(Settled)
            : Option.None<AgreedPrice>();

    /// <summary>What this line adds to the purchase.</summary>
    public Coin Due =>
        Agreed.Match(
            agreed => agreed.Settled,
            () => Asking * Quantity);

    /// <summary>What was settled on, whether or not anyone has.</summary>
    internal AgreedPrice Settled { get; private set; }

    /// <summary>Whether anyone haggled over this line.</summary>
    internal bool Haggled { get; private set; }

    /// <summary>Puts a quantity of something on a purchase at a price.</summary>
    /// <param name="subject">What the line is for.</param>
    /// <param name="quantity">How many.</param>
    /// <param name="asking">What the shop asks for one.</param>
    /// <param name="floor">The least the shop will take for one.</param>
    /// <returns>A line nobody has haggled over.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="quantity" /> is zero. A line for none of something is a line
    /// that should not have been added.
    /// </exception>
    public static LineItem For(
        LineItemSubject subject,
        uint quantity,
        Coin asking,
        Coin floor)
    {
        ArgumentOutOfRangeException.ThrowIfZero(quantity);

        return new LineItem(subject, quantity, asking, floor);
    }

    /// <summary>Takes an offer for this line.</summary>
    /// <param name="offer">What the patron said they would pay for the line.</param>
    /// <returns>
    /// What was agreed, or <see cref="OrderingError.OfferBelowFloor" /> when the offer
    /// is under what the shop will take for this many.
    /// </returns>
    /// <remarks>
    /// Haggling again replaces the last figure. The shop has no rule against a patron
    /// trying twice, so there is no failure to report for it.
    /// </remarks>
    internal Result<AgreedPrice, Error> Agree(Offer offer)
    {
        Coin least = Floor * Quantity;

        if (!offer.Named.IsAtLeast(least))
        {
            return Result.Err<AgreedPrice, Error>(
                OrderingErrorCatalog.Errors.OfferBelowFloor(
                    $"{offer.Named} offered for {Quantity}, and the shop will not go below {least}"));
        }

        Settled = new AgreedPrice(offer.Named);
        Haggled = true;

        return Result.Ok<AgreedPrice, Error>(Settled);
    }
}
