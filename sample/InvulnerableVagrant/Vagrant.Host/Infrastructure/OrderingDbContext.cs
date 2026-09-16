namespace Vagrant.Host.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Vagrant.Ordering;

/// <summary>Coin moving in either direction, as rows.</summary>
/// <remarks>
/// <para>
/// A purchase owns its lines. They have no identity a patron could name, they are never
/// read without the purchase they belong to, and nothing outside the aggregate is
/// allowed to reach one — so EF keeps them in a table with a key it made up, and
/// <see cref="Purchase.Lines" /> is the only way in.
/// </para>
/// <para>
/// No receipts table. A receipt is what a patron walks away with; what the shop keeps is
/// the purchase or the buyback and the fact that it is settled.
/// </para>
/// </remarks>
internal sealed class OrderingDbContext(DbContextOptions<OrderingDbContext> options)
    : VagrantDbContext(options)
{
    /// <summary>Everything the shop is selling or has sold.</summary>
    public DbSet<Purchase> Purchases => Set<Purchase>();

    /// <summary>Everything the shop has agreed to take in.</summary>
    public DbSet<Buyback> Buybacks => Set<Buyback>();

    /// <inheritdoc />
    protected override void ConfigureConventions(ModelConfigurationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Properties<PurchaseId>().HaveConversion<PurchaseIdConverter>();
        builder.Properties<BuybackId>().HaveConversion<BuybackIdConverter>();
        builder.Properties<LineItemSubject>()
               .HaveConversion<LineItemSubjectConverter>();
        builder.Properties<AgreedPrice>().HaveConversion<AgreedPriceConverter>();

        base.ConfigureConventions(builder);
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Entity<Purchase>(
            purchase =>
            {
                purchase.ToTable("Purchases");
                purchase.HasKey(p => p.Id);

                // EF maps public properties by convention and nothing else, so an
                // internal flag has to be named here or it is silently not stored — and
                // a settled purchase reads back as open.
                purchase.Property(p => p.Settled);

                // Computed from the lines every time it is read, so there is nothing to
                // store and nothing that can disagree with them.
                purchase.Ignore(p => p.Total);

                purchase.OwnsMany(
                    p => p.Lines,
                    line =>
                    {
                        line.ToTable("PurchaseLines");
                        line.WithOwner();

                        // The owning purchase and the thing bought, rather than the
                        // surrogate EF would invent. A purchase carries one line per
                        // subject — LineItems.Of refuses a set that does not — so the
                        // pair already identifies a line, and the key says so.
                        line.HasKey("PurchaseId", nameof(LineItem.Subject));

                        line.Ignore(l => l.Agreed);
                        line.Ignore(l => l.Due);

                        // The agreed price and the flag saying one was reached are
                        // stored separately, and LineItem.Agreed computes the Option
                        // from the two. A nullable column would have to be translated
                        // back into one on every read.
                        line.Property(l => l.Settled).HasColumnName("AgreedPrice");
                        line.Property(l => l.Haggled);
                    });

                purchase.Navigation(p => p.Lines)
                        .UsePropertyAccessMode(PropertyAccessMode.Field);
            });

        builder.Entity<Buyback>(
            buyback =>
            {
                buyback.ToTable("Buybacks");
                buyback.HasKey(b => b.Id);
                buyback.Property(b => b.Description).IsRequired();
                buyback.Property(b => b.Settled);
            });

        base.OnModelCreating(builder);
    }
}
