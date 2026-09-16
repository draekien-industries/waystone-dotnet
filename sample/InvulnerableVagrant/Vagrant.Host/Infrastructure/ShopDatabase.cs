namespace Vagrant.Host.Infrastructure;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Waystone.Monads.Results;
using Waystone.Monads.Results.Errors;

/// <summary>Where the shop's SQLite files live and how they come into being.</summary>
internal static class ShopDatabase
{
    /// <summary>The configuration key naming the SQLite file.</summary>
    public const string ConnectionName = "Shop";

    /// <summary>Where the shop keeps its files when nothing says otherwise.</summary>
    /// <remarks>
    /// Beside the host, which is what the README tells a reader to expect. A missing
    /// connection string is not a misconfiguration to report — it is the ordinary case
    /// of running the sample as it ships.
    /// </remarks>
    public const string Default = "Data Source=invulnerable-vagrant.db";

    /// <summary>Names the file one bounded context keeps its tables in.</summary>
    /// <param name="connectionString">What the <c>Shop</c> connection string says.</param>
    /// <param name="context">Which bounded context is asking.</param>
    /// <returns>A connection string naming that context's own file.</returns>
    /// <remarks>
    /// One file per context, derived from the one configured name:
    /// <c>invulnerable-vagrant.db</c> becomes <c>invulnerable-vagrant-catalog.db</c> and
    /// <c>invulnerable-vagrant-appraisal.db</c>.
    ///
    /// Not a preference. <c>EnsureCreated</c> creates a schema only when the database
    /// does not exist, so on a shared file the first context creates its tables and
    /// every later one finds a database already there and creates nothing — which
    /// surfaces as <c>SQLite Error 1: 'no such table: Specimens'</c> on the first
    /// request, not at startup.
    ///
    /// Separate files also make the boundary physical. A join across two contexts is now
    /// impossible in SQL as well as in the project graph.
    /// </remarks>
    public static string For(string connectionString, string context)
    {
        SqliteConnectionStringBuilder shop = new(connectionString);

        string directory = Path.GetDirectoryName(shop.DataSource) ?? string.Empty;
        string name = Path.GetFileNameWithoutExtension(shop.DataSource);
        string extension = Path.GetExtension(shop.DataSource);

        shop.DataSource = Path.Combine(directory, $"{name}-{context}{extension}");

        return shop.ConnectionString;
    }

    /// <summary>
    /// Creates the schema for every registered context, and seeds a shop that has
    /// never been opened before.
    /// </summary>
    /// <param name="services">The application's service provider.</param>
    /// <param name="ct">Cancels the work.</param>
    /// <returns>
    /// How many lines of stock were seeded, or the reason the shop cannot open. The
    /// caller is <c>Program</c>, which is the first place with anything to do about it.
    /// </returns>
    /// <remarks>
    /// <c>EnsureCreated</c> rather than a migration. The shop has no schema history to
    /// preserve — a reader deletes the files and runs again — and a migration per context
    /// would be four sets of generated files teaching nothing about the library. It works
    /// per context only because <see cref="For" /> gives each one its own file.
    /// </remarks>
    public static async Task<Result<int, Error>> OpenAsync(
        IServiceProvider services,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(services);

        await using AsyncServiceScope scope = services.CreateAsyncScope();

        foreach (VagrantDbContext context in
                 scope.ServiceProvider.GetServices<VagrantDbContext>())
        {
            await context.Database.EnsureCreatedAsync(ct).ConfigureAwait(false);
        }

        return await CatalogSeed
                    .StockTheShelvesAsync(
                         scope.ServiceProvider.GetRequiredService<CatalogDbContext>(),
                         ct)
                    .ConfigureAwait(false);
    }
}
