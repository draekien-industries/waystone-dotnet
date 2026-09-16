namespace Vagrant.Host.Endpoints;

using Vagrant.Catalog;
using Vagrant.Host.Contracts;
using Waystone.Monads.Options;

/// <summary>What a patron can read off the shelves.</summary>
internal static class ItemEndpoints
{
    /// <summary>Maps the two ways to read the shop's stock.</summary>
    /// <param name="routes">Where to map them.</param>
    public static void MapItems(this IEndpointRouteBuilder routes)
    {
        ArgumentNullException.ThrowIfNull(routes);

        routes.MapGet(
            "/items",
            async (IStockLedger ledger, CancellationToken ct) =>
            {
                IReadOnlyList<StockedItem> onDisplay =
                    await ledger.OnDisplayAsync(ct).ConfigureAwait(false);

                return Results.Ok(onDisplay.Select(StockedItemResponse.From).ToList());
            });

        // Match, not IsSome and Unwrap. The Option is the whole answer here: an
        // identifier the shop has never held has one thing to say about it, and 404
        // says it without an error code a client would have nothing to do with.
        routes.MapGet(
            "/items/{id:guid}",
            async (Guid id, IStockLedger ledger, CancellationToken ct) =>
            {
                Option<StockedItem> found = await ledger
                                                 .FindAsync(new StockedItemId(id), ct)
                                                 .ConfigureAwait(false);

                return found.Match(
                    item => Results.Ok(StockedItemResponse.From(item)),
                    () => Results.NotFound());
            });
    }
}
