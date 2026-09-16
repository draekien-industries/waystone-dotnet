namespace Vagrant.Host.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Vagrant.Catalog;

/// <summary>The shop's stock, as rows.</summary>
/// <remarks>
/// <see cref="StockedItem" /> is mapped as it stands — a private constructor, a private
/// setter on the count, and no property EF asked for. Nothing in
/// <c>Vagrant.Catalog</c> mentions EF Core, so the mapping is stated here or not at all.
/// </remarks>
internal sealed class CatalogDbContext(DbContextOptions<CatalogDbContext> options)
    : VagrantDbContext(options)
{
    /// <summary>Every line of stock the shop holds.</summary>
    public DbSet<StockedItem> StockedItems => Set<StockedItem>();

    /// <inheritdoc />
    protected override void ConfigureConventions(ModelConfigurationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Properties<StockedItemId>().HaveConversion<StockedItemIdConverter>();

        base.ConfigureConventions(builder);
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Entity<StockedItem>(
            item =>
            {
                item.ToTable("StockedItems");
                item.HasKey(i => i.Id);
                item.Property(i => i.Name).IsRequired();

                // Two columns on this table rather than a table of its own. A price
                // band has no identity and is never read without the item it prices.
                item.ComplexProperty(
                    i => i.Band,
                    band =>
                    {
                        band.Property(b => b.AskingPrice).HasColumnName("AskingPrice");
                        band.Property(b => b.FloorPrice).HasColumnName("FloorPrice");
                    });
            });

        base.OnModelCreating(builder);
    }
}
