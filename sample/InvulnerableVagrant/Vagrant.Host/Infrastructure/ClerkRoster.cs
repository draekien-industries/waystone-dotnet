namespace Vagrant.Host.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Vagrant.Staffing;
using Waystone.Monads.Options;
using Waystone.Monads.Results;
using Waystone.Monads.Results.Errors;

/// <summary>The roster, over SQLite.</summary>
/// <remarks>
/// <para>
/// The only repository in the sample that retries. <c>Engaged</c> is a concurrency
/// token, so a claim is an <c>UPDATE Clerks SET Engaged = 1 WHERE Id = @id AND Engaged
/// = 0</c>: two requests that read the same free clerk both issue it, one matches a row
/// and the other matches none. EF raises <see cref="DbUpdateConcurrencyException" /> for
/// the second, and it is caught here rather than surfaced.
/// </para>
/// <para>
/// A caller never hears the word "concurrency". It hears that a clerk took the errand,
/// or that none was free.
/// </para>
/// </remarks>
internal sealed class ClerkRoster(StaffingDbContext db, TimeProvider clock) : IClerkRoster
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<Clerk>> OnDutyAsync(CancellationToken ct) =>
        await db.Clerks
                .AsNoTracking()
                .OrderBy(clerk => clerk.Id)
                .ToListAsync(ct)
                .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<Result<Assignment, Error>> ClaimAsync(
        Errand errand,
        CancellationToken ct)
    {
        Option<Assignment> taken = await TakenAsync(errand, ct).ConfigureAwait(false);

        return taken.OkOrElse<Errand, Error>(
            errand,
            static waiting => StaffingErrorCatalog.Errors.NoClerkFree(
                $"every clerk is holding something, and {waiting.Subject} is waiting"));
    }

    /// <inheritdoc />
    public async Task<Option<Assignment>> ReleaseAsync(
        ErrandSubject subject,
        CancellationToken ct)
    {
        Errand errand = new(subject);

        Clerk? holder = await db.Clerks
                                .FirstOrDefaultAsync(
                                     clerk => clerk.Engaged
                                           && clerk.Holding.Errand == errand,
                                     ct)
                                .ConfigureAwait(false);

        if (holder is null) return Option.None<Assignment>();

        Option<Assignment> released = holder.Engagement;
        holder.Release();
        await db.SaveChangesAsync(ct).ConfigureAwait(false);

        return released;
    }

    /// <remarks>
    /// A subject already in a clerk's hands is answered with that clerk rather than
    /// given to a second one. A patron who asks twice about the same item is asking
    /// about the same item, and a shop of four clerks that hired one per asking would
    /// run out on the fourth question about one cloak.
    /// </remarks>
    private async Task<Option<Assignment>> TakenAsync(
        Errand errand,
        CancellationToken ct)
    {
        Clerk? already = await db.Clerks
                                 .FirstOrDefaultAsync(
                                      clerk => clerk.Engaged
                                            && clerk.Holding.Errand == errand,
                                      ct)
                                 .ConfigureAwait(false);

        if (already is not null) return already.Engagement;

        List<Clerk> free = await db.Clerks
                                   .Where(clerk => !clerk.Engaged)
                                   .OrderBy(clerk => clerk.Id)
                                   .ToListAsync(ct)
                                   .ConfigureAwait(false);

        foreach (Clerk clerk in free)
        {
            Option<Assignment> taken = clerk.Take(errand, clock).GetOk();

            if (await SavedAsync(ct).ConfigureAwait(false)) return taken;
        }

        return Option.None<Assignment>();
    }

    private async Task<bool> SavedAsync(CancellationToken ct)
    {
        try
        {
            await db.SaveChangesAsync(ct).ConfigureAwait(false);

            return true;
        }
        catch (DbUpdateConcurrencyException conflict)
        {
            foreach (EntityEntry entry in conflict.Entries)
            {
                await entry.ReloadAsync(ct).ConfigureAwait(false);
            }

            return false;
        }
    }
}
