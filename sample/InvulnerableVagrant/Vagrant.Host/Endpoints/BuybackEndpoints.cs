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
                async (
                        OpenBuybackRequest body,
                        IBuybackBook book,
                        CancellationToken ct) =>
                    await OpenBuybackSchema
                         .Instance
                         .Parse(body)
                         .Match((book, ct), TakeInAsync, Refusal.Rejected)
                         .ConfigureAwait(false))
           .WithTags("Ordering")
           .WithSummary("Offers a patron coin for something they are selling back.")
           .Produces<BuybackResponse>(StatusCodes.Status201Created)
           .ProducesValidationProblem();

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
                })
           .WithTags("Ordering")
           .WithSummary("Reads a buyback back.")
           .Produces<BuybackResponse>(StatusCodes.Status200OK)
           .ProducesProblem(StatusCodes.Status404NotFound);

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
                })
           .WithTags("Ordering")
           .WithSummary("Pays a patron what the shop offered them.")
           .WithDescription(
                "No request body, and the absence is the point. What the shop offered "
              + "and what its till holds are both the shop's to know, so a 402 "
              + "carrying vagrant.ordering.till_cannot_cover is an answer a patron "
              + "cannot have asked for.")
           .Produces<ReceiptResponse>(StatusCodes.Status200OK)
           .ProducesProblem(StatusCodes.Status402PaymentRequired)
           .ProducesProblem(StatusCodes.Status404NotFound)
           .ProducesProblem(StatusCodes.Status409Conflict);
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
