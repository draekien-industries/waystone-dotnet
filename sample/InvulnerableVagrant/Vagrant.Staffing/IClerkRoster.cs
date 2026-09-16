namespace Vagrant.Staffing;

using Waystone.Monads.Options;
using Waystone.Monads.Results;
using Waystone.Monads.Results.Errors;

/// <summary>Who is behind the counter, and who is free.</summary>
/// <remarks>
/// Declared here and implemented in <c>Vagrant.Host</c>. This project references no
/// persistence library, so a caller inside the context cannot reach past the roster to a
/// database, and the context's tests need none.
/// </remarks>
public interface IClerkRoster
{
    /// <summary>Reads every clerk the shop has.</summary>
    /// <param name="ct">Cancels the work.</param>
    /// <returns>Every clerk, in no particular order.</returns>
    Task<IReadOnlyList<Clerk>> OnDutyAsync(CancellationToken ct);

    /// <summary>Finds a free clerk and gives them the errand.</summary>
    /// <param name="errand">The work.</param>
    /// <param name="ct">Cancels the work.</param>
    /// <returns>
    /// What a clerk is now holding, or <see cref="StaffingError.NoClerkFree" /> when
    /// every clerk in the shop is already holding something.
    /// </returns>
    /// <remarks>
    /// <para>
    /// One call. It finds a free clerk, gives them the errand, saves, and on a lost race
    /// tries the next free clerk. A caller learns that no clerk was free — never that a
    /// row it had never heard of was stale.
    /// </para>
    /// <para>
    /// The conflict is the point of this context, and it is a real one. Two requests
    /// reading the same free clerk both write, and the implementation converts the
    /// database's refusal into another attempt rather than letting it out.
    /// </para>
    /// </remarks>
    Task<Result<Assignment, Error>> ClaimAsync(Errand errand, CancellationToken ct);

    /// <summary>Lets go of whoever is holding a piece of work.</summary>
    /// <param name="subject">What the work was about.</param>
    /// <param name="ct">Cancels the work.</param>
    /// <returns>
    /// What was released, or <c>None</c> when no clerk was holding it.
    /// </returns>
    /// <remarks>
    /// <c>Option</c> rather than <c>Result</c>: nobody is owed an explanation for work
    /// no clerk has. Takes the subject rather than the <see cref="Assignment" /> the
    /// claim returned, because a patron coming back to collect an item names the item.
    /// </remarks>
    Task<Option<Assignment>> ReleaseAsync(ErrandSubject subject, CancellationToken ct);
}
