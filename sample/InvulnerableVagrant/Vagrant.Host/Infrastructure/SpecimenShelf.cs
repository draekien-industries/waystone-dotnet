namespace Vagrant.Host.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Vagrant.Appraisal;
using Waystone.Monads.Options;

/// <summary>The examination shelf, over SQLite.</summary>
/// <remarks>
/// The one place in the sample that knows a specimen is a row. It decides nothing about
/// what an examination reveals — that is <see cref="Specimen.Identify" />'s answer, and
/// this type only persists it.
/// </remarks>
internal sealed class SpecimenShelf(AppraisalDbContext db) : ISpecimenShelf
{
    /// <inheritdoc />
    public async Task<Option<Specimen>> FindAsync(SpecimenId id, CancellationToken ct) =>
        Option.FromNullable(
            await db.Specimens
                    .AsNoTracking()
                    .FirstOrDefaultAsync(specimen => specimen.Id == id, ct)
                    .ConfigureAwait(false));

    /// <inheritdoc />
    public async Task AddAsync(Specimen specimen, CancellationToken ct)
    {
        await db.Specimens.AddAsync(specimen, ct).ConfigureAwait(false);
        await db.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<Option<Specimen>> IdentifyAsync(
        SpecimenId id,
        ArcanaCheck check,
        CancellationToken ct)
    {
        Specimen? specimen = await db.Specimens
                                     .FirstOrDefaultAsync(s => s.Id == id, ct)
                                     .ConfigureAwait(false);

        if (specimen is null) return Option.None<Specimen>();

        specimen.Identify(check);

        await db.SaveChangesAsync(ct).ConfigureAwait(false);

        return Option.Some(specimen);
    }
}
