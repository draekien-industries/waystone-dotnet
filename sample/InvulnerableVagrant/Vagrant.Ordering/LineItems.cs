namespace Vagrant.Ordering;

using Waystone.Monads.Results;
using Waystone.Monads.Results.Errors;

/// <summary>What a purchase is for. Never empty.</summary>
/// <remarks>
/// The invariant lives here rather than in <see cref="Purchase.Open" />, so a purchase
/// has no empty-purchase failure to report and no caller has to remember to check.
/// <see cref="Of" /> is the one place an empty set can be presented, and the one place it
/// is refused.
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
    /// The set, or <see cref="OrderingError.NoLineItems" /> when nothing was presented.
    /// </returns>
    public static Result<LineItems, Error> Of(IEnumerable<LineItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        List<LineItem> all = [.. items];

        return all.Count is 0
            ? Result.Err<LineItems, Error>(
                OrderingErrorCatalog.Errors.NoLineItems(
                    "a purchase must carry at least one line"))
            : Result.Ok<LineItems, Error>(new LineItems(all));
    }
}
