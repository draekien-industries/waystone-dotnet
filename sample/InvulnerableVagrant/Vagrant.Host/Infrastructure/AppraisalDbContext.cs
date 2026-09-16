namespace Vagrant.Host.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Vagrant.Appraisal;

/// <summary>The items the shop has been asked to examine, as rows.</summary>
/// <remarks>
/// The enchantment a specimen carries is stored whether or not the shop has read it,
/// and a boolean column records whether it has. <see cref="Specimen.Enchantment" />
/// computes the <c>Option</c> from the two, so nothing is stored in a nullable shape
/// that would have to be translated back.
/// </remarks>
internal sealed class AppraisalDbContext(DbContextOptions<AppraisalDbContext> options)
    : VagrantDbContext(options)
{
    /// <summary>Every item on the examination shelf.</summary>
    public DbSet<Specimen> Specimens => Set<Specimen>();

    /// <inheritdoc />
    protected override void ConfigureConventions(ModelConfigurationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Properties<SpecimenId>().HaveConversion<SpecimenIdConverter>();

        base.ConfigureConventions(builder);
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Entity<Specimen>(
            specimen =>
            {
                specimen.ToTable("Specimens");
                specimen.HasKey(s => s.Id);
                specimen.Property(s => s.Description).IsRequired();

                // Columns on this table rather than tables of their own. Neither an
                // aura nor an enchantment has identity, and neither is ever read
                // without the specimen it belongs to.
                specimen.ComplexProperty(
                    s => s.Aura,
                    aura => aura.Property(a => a.Obscurity).HasColumnName("Obscurity"));

                specimen.ComplexProperty(
                    s => s.Carries,
                    carries =>
                    {
                        carries.Property(c => c.Name)
                               .HasColumnName("EnchantmentName")
                               .IsRequired();
                        carries.Property(c => c.Effect)
                               .HasColumnName("EnchantmentEffect")
                               .IsRequired();
                    });
            });

        base.OnModelCreating(builder);
    }
}
