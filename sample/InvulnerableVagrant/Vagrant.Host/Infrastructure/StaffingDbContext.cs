namespace Vagrant.Host.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Vagrant.Staffing;

/// <summary>Who is behind the counter, as rows.</summary>
/// <remarks>
/// The one context in the shop with a concurrency token. <see cref="Clerk.Engaged" /> is
/// both the state and the token, so claiming a clerk is an
/// <c>UPDATE ... WHERE Engaged = 0</c> that either matches a row or does not — and the
/// second of two requests reading the same free clerk is told so.
/// </remarks>
internal sealed class StaffingDbContext(DbContextOptions<StaffingDbContext> options)
    : VagrantDbContext(options)
{
    /// <summary>Every clerk the shop has.</summary>
    public DbSet<Clerk> Clerks => Set<Clerk>();

    /// <inheritdoc />
    protected override void ConfigureConventions(ModelConfigurationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Properties<ClerkId>().HaveConversion<ClerkIdConverter>();
        builder.Properties<Errand>().HaveConversion<ErrandConverter>();

        base.ConfigureConventions(builder);
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Entity<Clerk>(
            clerk =>
            {
                clerk.ToTable("Clerks");
                clerk.HasKey(c => c.Id);
                clerk.Property(c => c.Name).IsRequired();

                // EF maps public properties by convention and nothing else, so these
                // internal ones have to be named here or they are silently not stored.
                clerk.Property(c => c.Engaged).IsConcurrencyToken();

                // Columns on this table rather than a table of its own. An assignment
                // has no identity and is never read without the clerk holding it.
                clerk.ComplexProperty(
                    c => c.Holding,
                    holding =>
                    {
                        holding.Property(h => h.Clerk).HasColumnName("HoldingClerk");
                        holding.Property(h => h.Since).HasColumnName("HoldingSince");
                        holding.Property(h => h.Errand).HasColumnName("HoldingErrand");
                    });

                clerk.Ignore(c => c.Engagement);
            });

        base.OnModelCreating(builder);
    }
}
