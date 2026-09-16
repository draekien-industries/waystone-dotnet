namespace Vagrant.Host.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Vagrant.Catalog;
using Waystone.Monads.Options;
using Waystone.Monads.Results;
using Waystone.Monads.Results.Errors;
using Waystone.Monads.Results.Extensions;

/// <summary>The stock ledger, over SQLite.</summary>
/// <remarks>
/// The one place in the sample that knows the shop's stock is rows in a table. It
/// decides nothing about what a withdrawal is allowed to do — that is
/// <see cref="StockedItem.Withdraw" />'s answer, and this type only persists it.
/// </remarks>
internal sealed class StockLedger(CatalogDbContext db) : IStockLedger
{
    /// <inheritdoc />
    public async Task<Option<StockedItem>> FindAsync(
        StockedItemId id,
        CancellationToken ct) =>
        Option.FromNullable(
            await db.StockedItems
                    .AsNoTracking()
                    .FirstOrDefaultAsync(item => item.Id == id, ct)
                    .ConfigureAwait(false));

    /// <inheritdoc />
    public async Task<IReadOnlyList<StockedItem>> OnDisplayAsync(CancellationToken ct) =>
        await db.StockedItems
                .AsNoTracking()
                .OrderBy(item => item.Name)
                .ToListAsync(ct)
                .ConfigureAwait(false);

    /// <inheritdoc />
    /// <remarks>
    /// All or nothing without a transaction. <see cref="StockedItem.Withdraw" /> changes
    /// a tracked entity and nothing else, so the counts reach SQLite only at
    /// <c>SaveChangesAsync</c> — which runs once, after every line has succeeded.
    /// Returning early leaves the changes in a scoped context that is discarded with the
    /// request.
    /// </remarks>
    public async Task<Result<IReadOnlyList<Withdrawal>, Error>> WithdrawAsync(
        IReadOnlyList<Wanted> wanted,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(wanted);

        List<Result<Withdrawal, Error>> taken = new(wanted.Count);

        foreach (Wanted want in wanted)
        {
            taken.Add(await TakeAsync(want, ct).ConfigureAwait(false));
        }

        Result<IReadOnlyList<Withdrawal>, Error> withdrawn = taken.Collect();

        if (withdrawn.IsErr) return withdrawn;

        await db.SaveChangesAsync(ct).ConfigureAwait(false);

        return withdrawn;
    }

    /// <remarks>
    /// Asking for the same line twice is not a special case. EF hands back the entity it
    /// is already tracking, so the second call sees the count the first one left.
    /// </remarks>
    private async Task<Result<Withdrawal, Error>> TakeAsync(
        Wanted want,
        CancellationToken ct)
    {
        StockedItem? item =
            await db.StockedItems
                    .FirstOrDefaultAsync(stocked => stocked.Id == want.Item, ct)
                    .ConfigureAwait(false);

        return item is null
            ? Result.Err<Withdrawal, Error>(
                CatalogErrorCatalog.Errors.NotStocked(
                    $"the shop holds no line of stock under {want.Item}"))
            : item.Withdraw(want.Quantity);
    }
}
