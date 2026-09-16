namespace Vagrant.Ordering;

using Waystone.Monads.Results;
using Waystone.Monads.Results.Errors;

/// <summary>What a purchase is for. Never empty, never the same thing twice.</summary>
/// <remarks>
/// The invariants live here rather than in <see cref="Purchase.Open" />, so a purchase
/// has no empty-purchase failure to report and no caller has to remember to check.
/// <see cref="Of" /> is the one place a bad set can be presented, and the one place it is
/// refused.
/// </remarks>
public readonly record struct LineItems
{
    private readonly IReadOnlyList<LineItem>? _all;

    private LineItems(IReadOnlyList<LineItem> all)
    {
        _all = all;
    }

    /// <summary>Every line, in the order they were presented.</summary>
    public IReadOnlyList<LineItem> All => _all ?? [];

    /// <summary>Gathers lines into a set a purchase can be opened on.</summary>
    /// <param name="items">The lines presented.</param>
    /// <returns>
    /// The set, <see cref="OrderingError.NoLineItems" /> when nothing was presented, or
    /// <see cref="OrderingError.DuplicateLine" /> when two lines name the same thing.
    /// </returns>
    /// <remarks>
    /// Three potions are one line of three, not three lines of one. Two lines naming the
    /// same subject would make <see cref="Purchase.Agree" /> haggle over whichever it
    /// found first and leave the other at the asking price, so the set that would allow
    /// it cannot be built.
    /// </remarks>
    public static Result<LineItems, Error> Of(IEnumerable<LineItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        List<LineItem> all = [.. items];

        if (all.Count is 0)
        {
            return Result.Err<LineItems, Error>(
                OrderingErrorCatalog.Errors.NoLineItems(
                    "a purchase must carry at least one line"));
        }

        HashSet<LineItemSubject> subjects = [.. all.Select(line => line.Subject)];

        return subjects.Count == all.Count
            ? Result.Ok<LineItems, Error>(new LineItems(all))
            : Result.Err<LineItems, Error>(
                OrderingErrorCatalog.Errors.DuplicateLine(
                    "a purchase carries one line per thing bought"));
    }
}
