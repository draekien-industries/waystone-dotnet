namespace Vagrant.Catalog;

using Waystone.Monads.Options;
using Waystone.Monads.Results;
using Waystone.Monads.Results.Errors;

/// <summary>The shop's record of what it holds.</summary>
/// <remarks>
/// Declared here and implemented in <c>Vagrant.Host</c>. This project references no
/// persistence library, so a caller inside the context cannot reach past the ledger to
/// a database, and the context's tests need none.
/// </remarks>
public interface IStockLedger
{
    /// <summary>Looks a line of stock up by its identifier.</summary>
    /// <param name="id">Which line to look for.</param>
    /// <param name="ct">Cancels the work.</param>
    /// <returns>
    /// The line, or <c>None</c> when the shop holds no such line.
    /// </returns>
    /// <remarks>
    /// <c>Option</c> rather than <c>Result</c>: nobody is owed an explanation for an
    /// identifier the shop has never seen. There is one thing to say about it and
    /// <c>None</c> says it.
    /// </remarks>
    Task<Option<StockedItem>> FindAsync(StockedItemId id, CancellationToken ct);

    /// <summary>Reads everything the shop currently has out for sale.</summary>
    /// <param name="ct">Cancels the work.</param>
    /// <returns>
    /// Every line of stock, in no particular order. Empty when the shelves are bare.
    /// </returns>
    /// <remarks>
    /// Not an <c>Option</c>. An empty shop is an empty list, and wrapping it would make
    /// a caller unpack two layers to reach the same nothing.
    /// </remarks>
    Task<IReadOnlyList<StockedItem>> OnDisplayAsync(CancellationToken ct);

    /// <summary>Takes stock off the shelf and records that it has gone.</summary>
    /// <param name="id">Which line to take from.</param>
    /// <param name="quantity">How many to take.</param>
    /// <param name="ct">Cancels the work.</param>
    /// <returns>
    /// What came off the shelf, <see cref="CatalogError.NotStocked" /> when the shop
    /// holds no such line, or <see cref="CatalogError.NotEnoughOnHand" /> when too few
    /// are there.
    /// </returns>
    /// <remarks>
    /// <para>
    /// One call, not a reservation the caller has to commit. Splitting it would hand
    /// back a count that is already stale and make the conflict the caller's problem.
    /// </para>
    /// <para>
    /// Absence is a <c>Result</c> here and an <c>Option</c> on
    /// <see cref="FindAsync" />, because the question differs. Asked what it holds, the
    /// shop answers nothing. Asked to supply four of something it has never stocked, it
    /// owes the patron the reason it cannot.
    /// </para>
    /// </remarks>
    Task<Result<Withdrawal, Error>> WithdrawAsync(
        StockedItemId id,
        uint quantity,
        CancellationToken ct);
}
