namespace Vagrant.Host.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Vagrant.SharedKernel;

/// <summary>What every one of the shop's contexts configures the same way.</summary>
/// <remarks>
/// Each bounded context gets its own derived context over the same SQLite file, so a
/// context's tables are its own and nothing joins across a boundary in SQL that the
/// project graph forbids in C#.
/// </remarks>
internal abstract class VagrantDbContext(DbContextOptions options) : DbContext(options)
{
    /// <inheritdoc />
    protected override void ConfigureConventions(ModelConfigurationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Properties<Coin>().HaveConversion<CoinConverter>();
        builder.Properties<PatronId>().HaveConversion<PatronIdConverter>();

        base.ConfigureConventions(builder);
    }
}
