namespace Vagrant.Appraisal;

using Waystone.Monads.Options;

/// <summary>Where the shop keeps items it has been asked to examine.</summary>
/// <remarks>
/// Declared here and implemented in <c>Vagrant.Host</c>. This project references no
/// persistence library, so a caller inside the context cannot reach past the shelf to a
/// database, and the context's tests need none.
///
/// Every member returns an <c>Option</c> or nothing. Appraisal declares no error codes:
/// the only question it can be asked and fail to answer is about an identifier it has
/// never seen, and <c>None</c> answers that.
/// </remarks>
public interface ISpecimenShelf
{
    /// <summary>Looks a specimen up by its identifier.</summary>
    /// <param name="id">Which specimen to look for.</param>
    /// <param name="ct">Cancels the work.</param>
    /// <returns>The specimen, or <c>None</c> when the shop holds no such item.</returns>
    Task<Option<Specimen>> FindAsync(SpecimenId id, CancellationToken ct);

    /// <summary>Puts an item on the shelf to be examined.</summary>
    /// <param name="specimen">What the patron handed in.</param>
    /// <param name="ct">Cancels the work.</param>
    /// <returns>A task that completes once the shop has taken the item.</returns>
    Task AddAsync(Specimen specimen, CancellationToken ct);

    /// <summary>Examines a specimen and records whatever the shop learned.</summary>
    /// <param name="id">Which specimen to read.</param>
    /// <param name="check">How well the examination went.</param>
    /// <param name="ct">Cancels the work.</param>
    /// <returns>
    /// The specimen as it stands after the examination, or <c>None</c> when the shop
    /// holds no such item. A specimen that could not be read comes back with its
    /// <see cref="Specimen.Enchantment" /> still <c>None</c>, which is an answer rather
    /// than a failure.
    /// </returns>
    /// <remarks>
    /// One call rather than a <c>FindAsync</c> the caller mutates and saves. Splitting
    /// it would put the save on the caller and let a specimen be read without the
    /// reading being kept.
    /// </remarks>
    Task<Option<Specimen>> IdentifyAsync(
        SpecimenId id,
        ArcanaCheck check,
        CancellationToken ct);
}
