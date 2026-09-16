namespace Vagrant.Host.Endpoints;

using Vagrant.Host.Contracts;
using Vagrant.Host.Infrastructure;
using Vagrant.Ordering;
using Waystone.Monads.Options;
using Waystone.Monads.Results;
using Waystone.Monads.Results.Errors;

/// <summary>What a patron can do at the counter with coin going out.</summary>
internal static class BuybackEndpoints
{
    /// <summary>Maps the three ways to deal with a buyback.</summary>
    /// <param name="routes">Where to map them.</param>
    public static void MapBuybacks(this IEndpointRouteBuilder routes)
    {
        ArgumentNullException.ThrowIfNull(routes);

        routes.MapPost(
            "/buybacks",
            async (OpenBuybackRequest body, IBuybackBook book, CancellationToken ct) =>
                await OpenBuybackSchema
                     .Instance
                     .Parse(body)
                     .Match((book, ct), TakeInAsync, Refusal.Rejected)
                     .ConfigureAwait(false));

        routes.MapGet(
            "/buybacks/{id:guid}",
            async (Guid id, IBuybackBook book, CancellationToken ct) =>
            {
                Option<Buyback> found = await book
                                             .FindAsync(new BuybackId(id), ct)
                                             .ConfigureAwait(false);

                return found.Match(
                    buyback => Results.Ok(BuybackResponse.From(buyback)),
                    () => Results.NotFound());
            });

        // No body. The shop already knows what it offered and what its till holds, and a
        // patron cannot be asked either — so there is nothing left for the request to
        // say beyond which buyback is being paid out.
        routes.MapPost(
            "/buybacks/{id:guid}/settle",
            async (Guid id, IBuybackBook book, CancellationToken ct) =>
            {
                Result<Receipt, Error> settled =
                    await book
                         .SettleAsync(new BuybackId(id), ShopTill.Holding, ct)
                         .ConfigureAwait(false);

                return settled.Match(
                    static receipt => Results.Ok(ReceiptResponse.From(receipt)),
                    static error => Refusal.From(error));
            });
    }

    private static async Task<IResult> TakeInAsync(
        Buyback buyback,
        (IBuybackBook Book, CancellationToken Ct) state)
    {
        await state.Book.AddAsync(buyback, state.Ct).ConfigureAwait(false);

        return Results.Created(
            $"/buybacks/{buyback.Id}",
            BuybackResponse.From(buyback));
    }
}
