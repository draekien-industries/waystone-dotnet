namespace Vagrant.Ordering;

using Vagrant.SharedKernel;
using Waystone.Monads.Results;
using Waystone.Monads.Results.Errors;

/// <summary>Coin on its way out of the till.</summary>
/// <remarks>
/// Not a <see cref="Purchase" /> with the sign flipped, and deliberately not unified with
/// one. They share a receipt and nothing else: a purchase is opened on lines the shop
/// already holds and priced from its own shelf labels, and a buyback is opened on one
/// description of something the shop has never seen, at a figure Pumat named.
/// </remarks>
public sealed class Buyback
{
    /// <remarks>
    /// For rehydration. Something outside this project reads a buyback back from wherever
    /// it was kept and fills the properties in; it cannot call <see cref="Open" />,
    /// because that opens one the shop has never had. Private, so nothing here can reach
    /// it either.
    /// </remarks>
    private Buyback()
    { }

    private Buyback(BuybackId id, PatronId patron, string description, Coin offered)
    {
        Id = id;
        Patron = patron;
        Description = description;
        Offered = offered;
    }

    /// <summary>Which buyback this is.</summary>
    public BuybackId Id { get; private init; }

    /// <summary>Who is selling.</summary>
    public PatronId Patron { get; private init; }

    /// <summary>What they brought in.</summary>
    public string Description { get; private init; } = string.Empty;

    /// <summary>What Pumat said he would give for it.</summary>
    public Coin Offered { get; private init; }

    /// <summary>Whether the coin has moved.</summary>
    internal bool Settled { get; private set; }

    /// <summary>Opens a buyback on something a patron brought in.</summary>
    /// <param name="id">Which buyback this becomes.</param>
    /// <param name="patron">Who is selling.</param>
    /// <param name="description">What they brought.</param>
    /// <param name="offered">What the shop offered for it.</param>
    /// <returns>An open buyback nobody has settled.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="description" /> is null, empty or whitespace.
    /// </exception>
    public static Buyback Open(
        BuybackId id,
        PatronId patron,
        string description,
        Coin offered)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        return new Buyback(id, patron, description.Trim(), offered);
    }

    /// <summary>Pays the patron out of the till.</summary>
    /// <param name="fromTill">What the till holds.</param>
    /// <param name="clock">Reads the moment the shop reached for it.</param>
    /// <returns>
    /// The receipt, <see cref="OrderingError.BuybackAlreadySettled" /> when the coin has
    /// already moved, or <see cref="OrderingError.TillCannotCover" /> when the till holds
    /// less than was offered.
    /// </returns>
    /// <remarks>
    /// A shop that cannot cover what it offered owes the patron that reason, which is why
    /// this is a <c>Result</c> rather than a silent partial payment.
    ///
    /// The clock is a parameter for the same reason it is on
    /// <see cref="Purchase.Settle" />: an aggregate that reads the ambient clock cannot be
    /// tested for what it writes on the receipt.
    /// </remarks>
    public Result<Receipt, Error> Settle(Coin fromTill, TimeProvider clock)
    {
        ArgumentNullException.ThrowIfNull(clock);

        if (Settled)
        {
            return Result.Err<Receipt, Error>(
                OrderingErrorCatalog.Errors.BuybackAlreadySettled(
                    $"buyback {Id} has already been settled"));
        }

        if (!fromTill.IsAtLeast(Offered))
        {
            return Result.Err<Receipt, Error>(
                OrderingErrorCatalog.Errors.TillCannotCover(
                    $"{Offered} offered against a till holding {fromTill}"));
        }

        Settled = true;

        return Result.Ok<Receipt, Error>(
            new Receipt(ReceiptId.New(), Patron, Offered, clock.GetUtcNow()));
    }
}
