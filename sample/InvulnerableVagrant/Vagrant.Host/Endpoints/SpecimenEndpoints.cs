namespace Vagrant.Host.Endpoints;

using Vagrant.Appraisal;
using Vagrant.Host.Contracts;
using Waystone.Monads.Options;
using Waystone.Monads.Schemas;

/// <summary>What a patron can leave with the shop to have identified.</summary>
internal static class SpecimenEndpoints
{
    /// <summary>Maps the two ways to deal with an unidentified item.</summary>
    /// <param name="routes">Where to map them.</param>
    public static void MapSpecimens(this IEndpointRouteBuilder routes)
    {
        ArgumentNullException.ThrowIfNull(routes);

        // The schema is the only route from a body to a Specimen, so there is no second
        // validation step and nothing downstream that accepts an unparsed request. The
        // Result it returns is unwrapped into a status here and never reaches the wire.
        routes.MapPost(
            "/specimens",
            async (
                    HandInSpecimenRequest body,
                    ISpecimenShelf shelf,
                    CancellationToken ct) =>
                await HandInSpecimenSchema
                     .Instance
                     .Parse(body)
                     .Match((shelf, ct), ShelveAsync, Rejected)
                     .ConfigureAwait(false));

        // Two monads answering two questions. The Result says whether the request was a
        // check at all; the Option says whether the shop holds the item. A specimen that
        // was read and gave nothing up is neither — it is a 200 whose enchantment is
        // null.
        routes.MapPost(
            "/specimens/{id:guid}/identify",
            async (
                    Guid id,
                    IdentifySpecimenRequest body,
                    ISpecimenShelf shelf,
                    CancellationToken ct) =>
                await IdentifySpecimenSchema
                     .Instance
                     .Parse(body)
                     .Match((id, shelf, ct), ReadAsync, Rejected)
                     .ConfigureAwait(false));
    }

    private static async Task<IResult> ShelveAsync(
        Specimen specimen,
        (ISpecimenShelf Shelf, CancellationToken Ct) state)
    {
        await state.Shelf.AddAsync(specimen, state.Ct).ConfigureAwait(false);

        return Results.Created(
            $"/specimens/{specimen.Id}",
            SpecimenResponse.From(specimen));
    }

    private static async Task<IResult> ReadAsync(
        ArcanaCheck check,
        (Guid Id, ISpecimenShelf Shelf, CancellationToken Ct) state)
    {
        Option<Specimen> read = await state.Shelf
                                           .IdentifyAsync(
                                                new SpecimenId(state.Id),
                                                check,
                                                state.Ct)
                                           .ConfigureAwait(false);

        return read.Match(
            specimen => Results.Ok(SpecimenResponse.From(specimen)),
            () => Results.NotFound());
    }

    private static Task<IResult> Rejected<TState>(SchemaViolation violation, TState state) =>
        Task.FromResult<IResult>(
            Results.ValidationProblem(violation.ToDictionary()));
}
