namespace Vagrant.Host.Endpoints;

using Vagrant.Catalog;
using Vagrant.Ordering;
using Waystone.Monads.Results.Errors;
using Waystone.Monads.Schemas;

/// <summary>Where a <c>Result</c> stops being a monad and becomes a status code.</summary>
/// <remarks>
/// <para>
/// One table rather than a decision at each endpoint. The same refusal has to mean the
/// same status wherever it surfaces, and a code added to a context without a row here
/// falls to 500 — which is the right answer for a reason nobody has decided how to
/// report.
/// </para>
/// <para>
/// The generated code travels in the problem document. A client can branch on
/// <c>vagrant.ordering.offer_below_floor</c> without parsing the message, which is what
/// the codes are for.
/// </para>
/// </remarks>
internal static class Refusal
{
    private static readonly Dictionary<ErrorCode, int> Statuses = new()
    {
        [OrderingErrorCatalog.Codes.NoLineItems] = StatusCodes.Status400BadRequest,
        [OrderingErrorCatalog.Codes.DuplicateLine] = StatusCodes.Status400BadRequest,
        [OrderingErrorCatalog.Codes.InsufficientCoin] =
            StatusCodes.Status402PaymentRequired,
        [OrderingErrorCatalog.Codes.TillCannotCover] =
            StatusCodes.Status402PaymentRequired,
        [CatalogErrorCatalog.Codes.NotStocked] = StatusCodes.Status404NotFound,
        [OrderingErrorCatalog.Codes.NoSuchPurchase] = StatusCodes.Status404NotFound,
        [OrderingErrorCatalog.Codes.NoSuchBuyback] = StatusCodes.Status404NotFound,
        [OrderingErrorCatalog.Codes.NotOnThisPurchase] = StatusCodes.Status404NotFound,
        [CatalogErrorCatalog.Codes.NotEnoughOnHand] = StatusCodes.Status409Conflict,
        [OrderingErrorCatalog.Codes.OfferBelowFloor] = StatusCodes.Status409Conflict,
        [OrderingErrorCatalog.Codes.PurchaseAlreadySettled] =
            StatusCodes.Status409Conflict,
        [OrderingErrorCatalog.Codes.BuybackAlreadySettled] =
            StatusCodes.Status409Conflict,
    };

    /// <summary>Renders a refusal the shop made as a problem document.</summary>
    /// <param name="error">Why the shop would not do it.</param>
    /// <returns>The status the code maps to, carrying the code and the message.</returns>
    public static IResult From(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);

        return Results.Problem(
            detail: error.Message,
            statusCode: Statuses.TryGetValue(error.Code, out int status)
                ? status
                : StatusCodes.Status500InternalServerError,
            extensions: new Dictionary<string, object?> { ["code"] = error.Code.Value });
    }

    /// <summary>Renders a body the shop could not read at all.</summary>
    /// <typeparam name="TState">Whatever the parse was carrying. Unused.</typeparam>
    /// <param name="violation">Every field that failed, with its path.</param>
    /// <param name="state">Unused.</param>
    /// <returns>A 400 listing the fields.</returns>
    /// <remarks>
    /// Takes a state parameter it ignores so that it can be passed as a method group to
    /// the stateful <c>Match</c>, which is what the other branch needs.
    /// </remarks>
    public static Task<IResult> Rejected<TState>(
        SchemaViolation violation,
        TState state)
    {
        ArgumentNullException.ThrowIfNull(violation);

        return Task.FromResult<IResult>(
            Results.ValidationProblem(violation.ToDictionary()));
    }
}
