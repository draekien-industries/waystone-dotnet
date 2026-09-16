namespace Vagrant.Host.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Vagrant.Staffing;

/// <summary>Who is behind the counter when the shop opens for the first time.</summary>
/// <remarks>
/// <para>
/// Four, and three of them answer to "Pumat Sol" — the simulacra Pumat keeps to serve a
/// counter he cannot serve alone. <c>Clerk</c> has no unique constraint on
/// <c>Name</c> for that reason, and the roster tells them apart by <see cref="ClerkId" />.
/// </para>
/// <para>
/// No <c>Result</c> here, unlike <see cref="CatalogSeed" />. A price band can be
/// nonsense — a floor above an asking price — so seeding one returns a failure somebody
/// has to act on. A name literal cannot be, so this returns how many were hired and
/// nothing else.
/// </para>
/// </remarks>
internal static class ClerkSeed
{
    private static readonly string[] Names =
    [
        "Pumat Prime",
        "Pumat Sol",
        "Pumat Sol",
        "Pumat Sol",
    ];

    /// <summary>Opens the counter, unless it is already staffed.</summary>
    /// <param name="db">The roster's rows.</param>
    /// <param name="ct">Cancels the work.</param>
    /// <returns>How many clerks were hired — zero when the shop was already staffed.</returns>
    public static async Task<int> OpenTheCounterAsync(
        StaffingDbContext db,
        CancellationToken ct)
    {
        if (await db.Clerks.AnyAsync(ct).ConfigureAwait(false)) return 0;

        foreach (string name in Names)
        {
            await db.Clerks
                    .AddAsync(Clerk.Hired(ClerkId.New(), name), ct)
                    .ConfigureAwait(false);
        }

        await db.SaveChangesAsync(ct).ConfigureAwait(false);

        return Names.Length;
    }
}
