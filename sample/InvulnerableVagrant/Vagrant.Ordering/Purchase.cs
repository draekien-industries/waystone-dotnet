namespace Vagrant.Ordering;

using Vagrant.SharedKernel;
using Waystone.Monads.Results;
using Waystone.Monads.Results.Errors;

/// <summary>Coin on its way into the till.</summary>
/// <remarks>
/// Opened with the lines already on it, because <see cref="LineItems" /> cannot be empty
/// and a purchase with nothing on it is not a state the shop can be in.
/// </remarks>
public sealed class Purchase
{
    private readonly List<LineItem> _lines = [];

    /// <remarks>
    /// For rehydration. Something outside this project reads a purchase back from
    /// wherever it was kept and fills the properties in; it cannot call
    /// <see cref="Open" />, because that opens one the shop has never had. Private, so
    /// nothing here can reach it either.
    /// </remarks>
    private Purchase()
    { }

    private Purchase(PurchaseId id, PatronId patron, LineItems items)
    {
        Id = id;
        Patron = patron;
        _lines.AddRange(items.All);
    }

    /// <summary>Which purchase this is.</summary>
    public PurchaseId Id { get; private init; }

    /// <summary>Who is buying.</summary>
    public PatronId Patron { get; private init; }

    /// <summary>Everything on it.</summary>
    public IReadOnlyList<LineItem> Lines => _lines;

    /// <summary>What the whole purchase comes to.</summary>
    /// <remarks>
    /// The agreed price where a line was haggled over and the asking price times the
    /// quantity where it was not, so a purchase nobody haggled over totals what the shop
    /// asked.
    /// </remarks>
    public Coin Total
    {
        get
        {
            Coin running = Coin.Nothing;

            foreach (LineItem line in _lines)
            {
                running += line.Due;
            }

            return running;
        }
    }

    /// <summary>Whether the coin has moved.</summary>
    internal bool Settled { get; private set; }

    /// <summary>Opens a purchase on a set of lines.</summary>
    /// <param name="id">Which purchase this becomes.</param>
    /// <param name="patron">Who is buying.</param>
    /// <param name="items">What they are buying.</param>
    /// <returns>An open purchase nobody has settled.</returns>
    /// <remarks>
    /// Not a <c>Result</c>. The only way this could fail was an empty set of lines, and
    /// <see cref="LineItems.Of" /> already refused that.
    /// </remarks>
    public static Purchase Open(PurchaseId id, PatronId patron, LineItems items) =>
        new(id, patron, items);

    /// <summary>Takes an offer for one line of the purchase.</summary>
    /// <param name="subject">Which line is being haggled over.</param>
    /// <param name="offer">What the patron said they would pay for it.</param>
    /// <returns>
    /// What was agreed, <see cref="OrderingError.PurchaseAlreadySettled" /> when the coin
    /// has already moved, <see cref="OrderingError.NotOnThisPurchase" /> when the
    /// purchase carries no such line, or <see cref="OrderingError.OfferBelowFloor" />
    /// when the offer is under what the shop will take.
    /// </returns>
    public Result<AgreedPrice, Error> Agree(LineItemSubject subject, Offer offer)
    {
        if (Settled)
        {
            return Result.Err<AgreedPrice, Error>(
                OrderingErrorCatalog.Errors.PurchaseAlreadySettled(
                    $"purchase {Id} settled before {subject} was haggled over"));
        }

        LineItem? line = _lines.Find(item => item.Subject == subject);

        return line is null
            ? Result.Err<AgreedPrice, Error>(
                OrderingErrorCatalog.Errors.NotOnThisPurchase(
                    $"purchase {Id} carries no line for {subject}"))
            : line.Agree(offer);
    }

    /// <summary>Takes the coin a patron put down and closes the purchase.</summary>
    /// <param name="tendered">What was put on the counter.</param>
    /// <param name="clock">Reads the moment the shop took it.</param>
    /// <returns>
    /// The receipt, <see cref="OrderingError.PurchaseAlreadySettled" /> when the coin has
    /// already moved, or <see cref="OrderingError.InsufficientCoin" /> when what was
    /// tendered falls short of the total.
    /// </returns>
    /// <remarks>
    /// <para>
    /// The receipt records the total rather than what was tendered. A patron who puts
    /// down more than the purchase comes to is owed change, and the books record what the
    /// shop took.
    /// </para>
    /// <para>
    /// The clock is a parameter rather than <c>DateTimeOffset.UtcNow</c>. An aggregate
    /// that reads the ambient clock cannot be tested for what it writes on the receipt.
    /// <c>TimeProvider</c> is the framework's own answer to that, so the tests pass a
    /// <c>FakeTimeProvider</c> and the host passes <c>TimeProvider.System</c>.
    /// </para>
    /// </remarks>
    public Result<Receipt, Error> Settle(Coin tendered, TimeProvider clock)
    {
        ArgumentNullException.ThrowIfNull(clock);

        if (Settled)
        {
            return Result.Err<Receipt, Error>(
                OrderingErrorCatalog.Errors.PurchaseAlreadySettled(
                    $"purchase {Id} has already been settled"));
        }

        Coin total = Total;

        if (!tendered.IsAtLeast(total))
        {
            return Result.Err<Receipt, Error>(
                OrderingErrorCatalog.Errors.InsufficientCoin(
                    $"{tendered} tendered against a total of {total}"));
        }

        Settled = true;

        return Result.Ok<Receipt, Error>(
            new Receipt(ReceiptId.New(), Patron, total, clock.GetUtcNow()));
    }
}
