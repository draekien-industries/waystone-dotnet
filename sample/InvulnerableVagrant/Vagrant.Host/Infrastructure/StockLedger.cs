namespace Vagrant.Host.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Vagrant.Catalog;
using Waystone.Monads.Options;
using Waystone.Monads.Results;
using Waystone.Monads.Results.Errors;

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
    public async Task<Result<Withdrawal, Error>> WithdrawAsync(
        StockedItemId id,
        uint quantity,
        CancellationToken ct)
    {
        StockedItem? item = await db.StockedItems
                                    .FirstOrDefaultAsync(stocked => stocked.Id == id, ct)
                                    .ConfigureAwait(false);

        if (item is null)
        {
            return Result.Err<Withdrawal, Error>(
                CatalogErrorCatalog.Errors.NotStocked(
                    $"the shop holds no line of stock under {id}"));
        }

        Result<Withdrawal, Error> withdrawal = item.Withdraw(quantity);

        if (withdrawal.IsOk) await db.SaveChangesAsync(ct).ConfigureAwait(false);

        return withdrawal;
    }
}
