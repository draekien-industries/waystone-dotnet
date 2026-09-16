namespace Vagrant.Host.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Vagrant.Ordering;
using Vagrant.SharedKernel;
using Waystone.Monads.Options;
using Waystone.Monads.Results;
using Waystone.Monads.Results.Errors;

/// <summary>The purchase book, over SQLite.</summary>
/// <remarks>
/// <para>
/// The one place in the sample that knows a purchase is rows. It decides nothing about
/// what haggling or settling is allowed to do — those are <see cref="Purchase" />'s
/// answers, and this type only persists them.
/// </para>
/// <para>
/// The clock arrives here rather than on <see cref="IPurchaseBook.SettleAsync" />. A
/// caller that could name the moment could write any hour it liked on a receipt, and
/// nothing above this line has a legitimate reason to.
/// </para>
/// </remarks>
internal sealed class PurchaseBook(OrderingDbContext db, TimeProvider clock)
    : IPurchaseBook
{
    /// <inheritdoc />
    public async Task<Option<Purchase>> FindAsync(
        PurchaseId id,
        CancellationToken ct) =>
        Option.FromNullable(
            await db.Purchases
                    .AsNoTracking()
                    .FirstOrDefaultAsync(purchase => purchase.Id == id, ct)
                    .ConfigureAwait(false));

    /// <inheritdoc />
    public async Task AddAsync(Purchase purchase, CancellationToken ct)
    {
        await db.Purchases.AddAsync(purchase, ct).ConfigureAwait(false);
        await db.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<Result<AgreedPrice, Error>> AgreeAsync(
        PurchaseId id,
        LineItemSubject subject,
        Offer offer,
        CancellationToken ct)
    {
        Purchase? purchase = await db.Purchases
                                     .FirstOrDefaultAsync(entry => entry.Id == id, ct)
                                     .ConfigureAwait(false);

        if (purchase is null) return Result.Err<AgreedPrice, Error>(NoSuch(id));

        Result<AgreedPrice, Error> agreed = purchase.Agree(subject, offer);

        if (agreed.IsOk) await db.SaveChangesAsync(ct).ConfigureAwait(false);

        return agreed;
    }

    /// <inheritdoc />
    public async Task<Result<Receipt, Error>> SettleAsync(
        PurchaseId id,
        Coin tendered,
        CancellationToken ct)
    {
        Purchase? purchase = await db.Purchases
                                     .FirstOrDefaultAsync(entry => entry.Id == id, ct)
                                     .ConfigureAwait(false);

        if (purchase is null) return Result.Err<Receipt, Error>(NoSuch(id));

        Result<Receipt, Error> settled = purchase.Settle(tendered, clock);

        if (settled.IsOk) await db.SaveChangesAsync(ct).ConfigureAwait(false);

        return settled;
    }

    private static Error NoSuch(PurchaseId id) =>
        OrderingErrorCatalog.Errors.NoSuchPurchase(
            $"the shop has written down no purchase under {id}");
}
