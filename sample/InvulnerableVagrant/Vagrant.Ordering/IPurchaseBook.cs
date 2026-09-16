namespace Vagrant.Ordering;

using Vagrant.SharedKernel;
using Waystone.Monads.Options;
using Waystone.Monads.Results;
using Waystone.Monads.Results.Errors;

/// <summary>The shop's record of what it has sold and what it is selling.</summary>
/// <remarks>
/// Declared here and implemented in <c>Vagrant.Host</c>. This project references no
/// persistence library, so a caller inside the context cannot reach past the book to a
/// database, and the context's tests need none.
/// </remarks>
public interface IPurchaseBook
{
    /// <summary>Looks a purchase up by its identifier.</summary>
    /// <param name="id">Which purchase to look for.</param>
    /// <param name="ct">Cancels the work.</param>
    /// <returns>The purchase, or <c>None</c> when the book holds no such purchase.</returns>
    /// <remarks>
    /// <c>Option</c> rather than <c>Result</c>: nobody is owed an explanation for an
    /// identifier the shop has never written down.
    /// </remarks>
    Task<Option<Purchase>> FindAsync(PurchaseId id, CancellationToken ct);

    /// <summary>Writes an opened purchase down.</summary>
    /// <param name="purchase">The purchase.</param>
    /// <param name="ct">Cancels the work.</param>
    /// <returns>A task that completes once the book has it.</returns>
    Task AddAsync(Purchase purchase, CancellationToken ct);

    /// <summary>Records what a patron and the shop settled on for one line.</summary>
    /// <param name="id">Which purchase.</param>
    /// <param name="subject">Which line on it.</param>
    /// <param name="offer">What the patron said they would pay.</param>
    /// <param name="ct">Cancels the work.</param>
    /// <returns>
    /// What was agreed, or whichever of <see cref="OrderingError" />'s refusals
    /// <see cref="Purchase.Agree" /> gave.
    /// </returns>
    /// <remarks>
    /// One call rather than a <c>FindAsync</c> the caller mutates and saves. Splitting it
    /// would put the save on the caller and let a price be agreed without the agreement
    /// being kept.
    /// </remarks>
    Task<Result<AgreedPrice, Error>> AgreeAsync(
        PurchaseId id,
        LineItemSubject subject,
        Offer offer,
        CancellationToken ct);

    /// <summary>Takes the patron's coin and closes the purchase.</summary>
    /// <param name="id">Which purchase.</param>
    /// <param name="tendered">What was put on the counter.</param>
    /// <param name="ct">Cancels the work.</param>
    /// <returns>
    /// The receipt, or whichever of <see cref="OrderingError" />'s refusals
    /// <see cref="Purchase.Settle" /> gave.
    /// </returns>
    /// <remarks>
    /// No moment on the signature. <see cref="Purchase.Settle" /> takes a
    /// <c>TimeProvider</c>, and the implementation holds the one the host registered, so a
    /// caller cannot name the hour the coin moved.
    /// </remarks>
    Task<Result<Receipt, Error>> SettleAsync(
        PurchaseId id,
        Coin tendered,
        CancellationToken ct);
}
