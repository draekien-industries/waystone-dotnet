namespace Vagrant.Staffing;

using Waystone.Monads.Options;
using Waystone.Monads.Results;
using Waystone.Monads.Results.Errors;

/// <summary>One of the shop's clerks, serving one patron at a time.</summary>
/// <remarks>
/// <para>
/// The aggregate root, rather than a roster holding every clerk. Two patrons claiming
/// two different clerks change two rows and do not contend; a roster aggregate would
/// serialise every claim on one row, and the contention would be something the model
/// invented rather than something the shop has.
/// </para>
/// <para>
/// Pumat Sol has three simulacra who mostly run the shop while he enchants in the back
/// room, so the shop has four of these. Three of them answer to the same name.
/// </para>
/// </remarks>
public sealed class Clerk
{
    /// <remarks>
    /// For rehydration. Something outside this project reads a clerk back from wherever
    /// they were kept and fills the properties in; it cannot call <see cref="Hired" />,
    /// because that takes on a clerk the shop has never had. Private, so nothing here
    /// can reach it either.
    /// </remarks>
    private Clerk()
    { }

    private Clerk(ClerkId id, string name)
    {
        Id = id;
        Name = name;
    }

    /// <summary>Which clerk this is.</summary>
    public ClerkId Id { get; private init; }

    /// <summary>What they answer to.</summary>
    /// <remarks>
    /// Not unique, and not an identity. Every simulacrum introduces itself as
    /// "Enchanter Pumat Sol", so three of the shop's four clerks share this value.
    /// </remarks>
    public string Name { get; private init; } = string.Empty;

    /// <summary>What they are holding, if anything.</summary>
    public Option<Assignment> Engagement =>
        Engaged
            ? Option.Some(Holding)
            : Option.None<Assignment>();

    /// <summary>What they are holding, whether or not they are holding it.</summary>
    internal Assignment Holding { get; private set; }

    /// <summary>Whether they are holding anything.</summary>
    /// <remarks>
    /// The concurrency token as well as the state. Two requests that both read a free
    /// clerk produce one <c>UPDATE ... WHERE Engaged = 0</c> that matches a row and one
    /// that matches none, so the second is a conflict SQLite reports rather than a race
    /// nobody notices.
    /// </remarks>
    internal bool Engaged { get; private set; }

    /// <summary>Takes a clerk on.</summary>
    /// <param name="id">Which clerk this becomes.</param>
    /// <param name="name">What they answer to.</param>
    /// <returns>A clerk holding nothing.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="name" /> is null, empty or whitespace.
    /// </exception>
    public static Clerk Hired(ClerkId id, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new Clerk(id, name.Trim());
    }

    /// <summary>Gives the clerk something to do.</summary>
    /// <param name="errand">The work.</param>
    /// <param name="clock">Reads the moment they took it.</param>
    /// <returns>
    /// What they are now holding, or
    /// <see cref="StaffingError.ClerkAlreadyEngaged" /> when they were already holding
    /// something.
    /// </returns>
    /// <remarks>
    /// A <c>Result</c> rather than a <c>bool</c>, because the caller trying every free
    /// clerk in turn has to tell "this one was taken" from "this one refused" — and
    /// because the reason reaches a patron when no clerk is left to try.
    /// </remarks>
    public Result<Assignment, Error> Take(Errand errand, TimeProvider clock)
    {
        ArgumentNullException.ThrowIfNull(clock);

        if (Engaged)
        {
            return Result.Err<Assignment, Error>(
                StaffingErrorCatalog.Errors.ClerkAlreadyEngaged(
                    $"clerk {Id} is already holding {Holding.Errand.Subject}"));
        }

        Holding = new Assignment(Id, errand, clock.GetUtcNow());
        Engaged = true;

        return Result.Ok<Assignment, Error>(Holding);
    }

    /// <summary>Lets the clerk go.</summary>
    /// <remarks>
    /// Not a <c>Result</c>. Releasing a clerk who is holding nothing is the state the
    /// caller wanted, and there is nobody to tell that it was already so.
    /// </remarks>
    public void Release()
    {
        Engaged = false;
    }
}
