namespace Vagrant.Host.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Waystone.Monads.Results;
using Waystone.Monads.Results.Errors;

/// <summary>Where the shop's SQLite file lives and how it comes into being.</summary>
internal static class ShopDatabase
{
    /// <summary>The configuration key naming the SQLite file.</summary>
    public const string ConnectionName = "Shop";

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
    /// preserve — a reader deletes the file and runs again — and a migration per context
    /// would be four sets of generated files teaching nothing about the library.
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
