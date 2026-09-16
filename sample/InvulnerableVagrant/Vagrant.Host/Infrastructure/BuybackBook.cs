namespace Vagrant.Host.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Vagrant.Ordering;
using Vagrant.SharedKernel;
using Waystone.Monads.Options;
using Waystone.Monads.Results;
using Waystone.Monads.Results.Errors;

/// <summary>The buyback book, over SQLite.</summary>
/// <remarks>
/// What the till holds is <see cref="ShopTill" />'s to say, not the caller's, for the
/// same reason the moment is the clock's: a patron cannot be asked how much money the
/// shop has.
/// </remarks>
internal sealed class BuybackBook(OrderingDbContext db, TimeProvider clock)
    : IBuybackBook
{
    /// <inheritdoc />
    public async Task<Option<Buyback>> FindAsync(BuybackId id, CancellationToken ct) =>
        Option.FromNullable(
            await db.Buybacks
                    .AsNoTracking()
                    .FirstOrDefaultAsync(buyback => buyback.Id == id, ct)
                    .ConfigureAwait(false));

    /// <inheritdoc />
    public async Task AddAsync(Buyback buyback, CancellationToken ct)
    {
        await db.Buybacks.AddAsync(buyback, ct).ConfigureAwait(false);
        await db.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<Result<Receipt, Error>> SettleAsync(
        BuybackId id,
        Coin fromTill,
        CancellationToken ct)
    {
        Buyback? buyback = await db.Buybacks
                                   .FirstOrDefaultAsync(entry => entry.Id == id, ct)
                                   .ConfigureAwait(false);

        if (buyback is null)
        {
            return Result.Err<Receipt, Error>(
                OrderingErrorCatalog.Errors.NoSuchBuyback(
                    $"the shop has written down no buyback under {id}"));
        }

        Result<Receipt, Error> settled = buyback.Settle(fromTill, clock);

        if (settled.IsOk) await db.SaveChangesAsync(ct).ConfigureAwait(false);

        return settled;
    }
}
