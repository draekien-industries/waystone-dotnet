namespace Vagrant.Ordering;

using Vagrant.SharedKernel;
using Waystone.Monads.Options;
using Waystone.Monads.Results;
using Waystone.Monads.Results.Errors;

/// <summary>The shop's record of what it has bought back.</summary>
/// <remarks>
/// Declared here and implemented in <c>Vagrant.Host</c>. Separate from
/// <see cref="IPurchaseBook" /> for the same reason <see cref="Buyback" /> is separate
/// from <see cref="Purchase" />: the two answer different questions and share only a
/// receipt.
/// </remarks>
public interface IBuybackBook
{
    /// <summary>Looks a buyback up by its identifier.</summary>
    /// <param name="id">Which buyback to look for.</param>
    /// <param name="ct">Cancels the work.</param>
    /// <returns>The buyback, or <c>None</c> when the book holds no such buyback.</returns>
    Task<Option<Buyback>> FindAsync(BuybackId id, CancellationToken ct);

    /// <summary>Writes an opened buyback down.</summary>
    /// <param name="buyback">The buyback.</param>
    /// <param name="ct">Cancels the work.</param>
    /// <returns>A task that completes once the book has it.</returns>
    Task AddAsync(Buyback buyback, CancellationToken ct);

    /// <summary>Pays the patron out of the till and closes the buyback.</summary>
    /// <param name="id">Which buyback.</param>
    /// <param name="fromTill">What the till holds.</param>
    /// <param name="ct">Cancels the work.</param>
    /// <returns>
    /// The receipt, or whichever of <see cref="OrderingError" />'s refusals
    /// <see cref="Buyback.Settle" /> gave.
    /// </returns>
    Task<Result<Receipt, Error>> SettleAsync(
        BuybackId id,
        Coin fromTill,
        CancellationToken ct);
}
