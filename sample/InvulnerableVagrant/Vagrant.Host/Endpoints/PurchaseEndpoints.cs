namespace Vagrant.Host.Endpoints;

using Vagrant.Catalog;
using Vagrant.Host.Contracts;
using Vagrant.Host.Translation;
using Vagrant.Ordering;
using Vagrant.SharedKernel;
using Waystone.Monads.Options;
using Waystone.Monads.Results;
using Waystone.Monads.Results.Errors;

/// <summary>What a patron can do at the counter with coin going in.</summary>
/// <remarks>
/// Each route is named for what the patron is trying to do, not for the row it changes.
/// There is no <c>PATCH /purchases/{id}</c> carrying a settled flag or an agreed price,
/// because those are outcomes the shop decides and not fields a client may set — the
/// body of an offer says what the patron will pay, and the answer comes back.
/// </remarks>
internal static class PurchaseEndpoints
{
    /// <summary>Maps the four ways to deal with a purchase.</summary>
    /// <param name="routes">Where to map them.</param>
    public static void MapPurchases(this IEndpointRouteBuilder routes)
    {
        ArgumentNullException.ThrowIfNull(routes);

        RouteGroupBuilder ordering = routes.MapGroup(string.Empty).WithTags("Ordering");

        // The body names shelf labels and quantities and no prices at all. What each
        // line costs is read off the Catalog withdrawal, so a client cannot state what
        // it intends to be charged.
        ordering.MapPost(
                "/purchases",
                async (
                        OpenPurchaseRequest body,
                        IStockLedger ledger,
                        IPurchaseBook book,
                        CancellationToken ct) =>
                    await OpenPurchaseSchema
                         .Instance
                         .Parse(body)
                         .Match((ledger, book, ct), OpenAsync, Refusal.Rejected)
                         .ConfigureAwait(false))
           .WithSummary("Opens a purchase over what a patron has brought to the counter.")
           .WithDescription(
                "The body names shelf labels and quantities and carries no prices. "
              + "What each line costs is read off the Catalog withdrawal, so the 404 "
              + "and the 409 here are the Catalog's refusals carried out unchanged. A "
              + "400 is either the field list shown below or a refusal code, depending "
              + "on whether the body parsed at all.")
           .Produces<PurchaseResponse>(StatusCodes.Status201Created)
           .ProducesValidationProblem()
           .ProducesProblem(StatusCodes.Status404NotFound)
           .ProducesProblem(StatusCodes.Status409Conflict);

        ordering.MapGet(
                "/purchases/{id:guid}",
                async (Guid id, IPurchaseBook book, CancellationToken ct) =>
                {
                    Option<Purchase> found = await book
                                                  .FindAsync(new PurchaseId(id), ct)
                                                  .ConfigureAwait(false);

                    return found.Match(
                        purchase => Results.Ok(PurchaseResponse.From(purchase)),
                        () => Results.NotFound());
                })
           .WithSummary("Reads a purchase back, line by line.")
           .WithDescription(
                "The agreedPrice on each line is the strongest case in the shop for "
              + "putting an Option on the wire: null against \"13pp 5gp\" is the "
              + "difference between a line nobody haggled over and one agreed at its "
              + "asking price, which a nullable decimal would render identically.")
           .Produces<PurchaseResponse>(StatusCodes.Status200OK)
           .ProducesProblem(StatusCodes.Status404NotFound);

        ordering.MapPost(
                "/purchases/{id:guid}/offer",
                async (
                        Guid id,
                        MakeOfferRequest body,
                        IPurchaseBook book,
                        CancellationToken ct) =>
                    await MakeOfferSchema
                         .Instance
                         .Parse(body)
                         .Match((id, book, ct), HaggleAsync, Refusal.Rejected)
                         .ConfigureAwait(false))
           .WithSummary("Offers a price for one line of a purchase.")
           .WithDescription(
                "Answers with the agreement rather than the whole purchase. A 409 "
              + "carries vagrant.ordering.offer_below_floor and the floor the shop "
              + "will not go below.")
           .Produces<AgreementResponse>(StatusCodes.Status200OK)
           .ProducesValidationProblem()
           .ProducesProblem(StatusCodes.Status404NotFound)
           .ProducesProblem(StatusCodes.Status409Conflict);

        ordering.MapPost(
                "/purchases/{id:guid}/settle",
                async (
                        Guid id,
                        SettlePurchaseRequest body,
                        IPurchaseBook book,
                        CancellationToken ct) =>
                    await SettlePurchaseSchema
                         .Instance
                         .Parse(body)
                         .Match((id, book, ct), SettleAsync, Refusal.Rejected)
                         .ConfigureAwait(false))
           .WithSummary("Pays for a purchase with coin the patron tenders.")
           .WithDescription(
                "The body says what is being handed over and nothing about the "
              + "outcome. Coin short of the total is a 402 carrying "
              + "vagrant.ordering.insufficient_coin, and a purchase already paid for "
              + "is a 409.")
           .Produces<ReceiptResponse>(StatusCodes.Status200OK)
           .ProducesValidationProblem()
           .ProducesProblem(StatusCodes.Status402PaymentRequired)
           .ProducesProblem(StatusCodes.Status404NotFound)
           .ProducesProblem(StatusCodes.Status409Conflict);
    }

    /// <remarks>
    /// Two contexts in one request, which is why this is in the host and not in either
    /// of them. Catalog takes the stock off the shelf and says what it was asking;
    /// <see cref="PurchaseLines" /> reads that as lines; Ordering opens a purchase on
    /// them. A failure anywhere in the chain is the Catalog's or the Ordering context's
    /// own refusal, carried out unchanged.
    /// </remarks>
    private static async Task<IResult> OpenAsync(
        OpenPurchase intent,
        (IStockLedger Ledger, IPurchaseBook Book, CancellationToken Ct) state)
    {
        Result<IReadOnlyList<Withdrawal>, Error> withdrawn =
            await state.Ledger
                       .WithdrawAsync(intent.Items, state.Ct)
                       .ConfigureAwait(false);

        Result<Purchase, Error> opened =
            withdrawn
               .AndThen(PurchaseLines.From)
               .Map(
                    intent.Patron,
                    static (lines, patron) =>
                        Purchase.Open(PurchaseId.New(), patron, lines));

        return await opened
                    .Match(state, ShelveAsync, RefusedAsync)
                    .ConfigureAwait(false);
    }

    private static async Task<IResult> ShelveAsync(
        Purchase purchase,
        (IStockLedger Ledger, IPurchaseBook Book, CancellationToken Ct) state)
    {
        await state.Book.AddAsync(purchase, state.Ct).ConfigureAwait(false);

        return Results.Created(
            $"/purchases/{purchase.Id}",
            PurchaseResponse.From(purchase));
    }

    /// <remarks>
    /// Answers with the agreement rather than the purchase. Whether the shop will take
    /// four hundred for the potions is the question, and reading the whole purchase back
    /// to answer it would be a second query nobody asked for.
    /// </remarks>
    private static async Task<IResult> HaggleAsync(
        MakeOffer haggle,
        (Guid Id, IPurchaseBook Book, CancellationToken Ct) state)
    {
        Result<AgreedPrice, Error> agreed =
            await state.Book
                       .AgreeAsync(
                            new PurchaseId(state.Id),
                            haggle.Subject,
                            haggle.Offer,
                            state.Ct)
                       .ConfigureAwait(false);

        return agreed.Match(
            haggle.Subject,
            static (price, subject) => Results.Ok(
                new AgreementResponse(subject.Value, price.Settled.ToString())),
            static (error, _) => Refusal.From(error));
    }

    private static async Task<IResult> SettleAsync(
        Coin tendered,
        (Guid Id, IPurchaseBook Book, CancellationToken Ct) state)
    {
        Result<Receipt, Error> settled =
            await state.Book
                       .SettleAsync(new PurchaseId(state.Id), tendered, state.Ct)
                       .ConfigureAwait(false);

        return settled.Match(
            static receipt => Results.Ok(ReceiptResponse.From(receipt)),
            static error => Refusal.From(error));
    }

    private static Task<IResult> RefusedAsync<TState>(Error error, TState state) =>
        Task.FromResult(Refusal.From(error));
}
