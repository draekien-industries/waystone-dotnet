namespace Vagrant.Host.Endpoints;

using Vagrant.Appraisal;
using Vagrant.Host.Contracts;
using Vagrant.Host.Translation;
using Vagrant.Staffing;
using Waystone.Monads.Options;
using Waystone.Monads.Results;
using Waystone.Monads.Results.Errors;
using Waystone.Monads.Schemas;

/// <summary>What a patron can leave with the shop to have identified.</summary>
internal static class SpecimenEndpoints
{
    /// <summary>Maps the three ways to deal with an unidentified item.</summary>
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
                    IClerkRoster roster,
                    CancellationToken ct) =>
                await IdentifySpecimenSchema
                     .Instance
                     .Parse(body)
                     .Match((id, shelf, roster, ct), ReadAsync, Rejected)
                     .ConfigureAwait(false));

        // Collecting is what frees the clerk. A shop of four cannot read a fifth item
        // until a patron comes back for one of the four, which is why POST /identify can
        // answer 503 at all.
        routes.MapPost(
            "/specimens/{id:guid}/collect",
            async (
                    Guid id,
                    ISpecimenShelf shelf,
                    IClerkRoster roster,
                    CancellationToken ct) =>
                await CollectAsync(id, shelf, roster, ct).ConfigureAwait(false));
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
        (Guid Id, ISpecimenShelf Shelf, IClerkRoster Roster, CancellationToken Ct) state)
    {
        Examination work = new(
            new SpecimenId(state.Id),
            check,
            state.Shelf,
            state.Roster,
            state.Ct);

        // The shelf is asked before a clerk is called over. A claim made first would have
        // to be given back when the shop turns out not to hold the item, and a
        // compensating release is a second rule about the same clerk.
        Option<Specimen> held = await work
                                    .Shelf
                                    .FindAsync(work.Id, work.Ct)
                                    .ConfigureAwait(false);

        if (held.IsNone) return Results.NotFound();

        Result<Assignment, Error> claimed = await work
                                                 .Roster
                                                 .ClaimAsync(
                                                      ClerkErrands.For(work.Id),
                                                      work.Ct)
                                                 .ConfigureAwait(false);

        return await claimed
                    .Match(work, ExamineAsync, RefusedAsync)
                    .ConfigureAwait(false);
    }

    private static async Task<IResult> ExamineAsync(
        Assignment assignment,
        Examination work)
    {
        Option<Specimen> read = await work
                                    .Shelf
                                    .IdentifyAsync(work.Id, work.Check, work.Ct)
                                    .ConfigureAwait(false);

        return read.Match(
            static specimen => Results.Ok(SpecimenResponse.From(specimen)),
            static () => Results.NotFound());
    }

    private static Task<IResult> RefusedAsync(Error error, Examination work) =>
        Task.FromResult(Refusal.From(error));

    private static async Task<IResult> CollectAsync(
        Guid id,
        ISpecimenShelf shelf,
        IClerkRoster roster,
        CancellationToken ct)
    {
        Option<Assignment> released = await roster
                                           .ReleaseAsync(new ErrandSubject(id), ct)
                                           .ConfigureAwait(false);

        if (released.IsNone) return Results.NotFound();

        Option<Specimen> collected = await shelf
                                          .FindAsync(new SpecimenId(id), ct)
                                          .ConfigureAwait(false);

        return collected.Match(
            static specimen => Results.Ok(SpecimenResponse.From(specimen)),
            static () => Results.NotFound());
    }

    private static Task<IResult> Rejected<TState>(SchemaViolation violation, TState state) =>
        Task.FromResult<IResult>(
            Results.ValidationProblem(violation.ToDictionary()));

    /// <remarks>
    /// A named state rather than a tuple. The claim carries four values through a
    /// <c>Match</c> and out the other side, and the tuple that would hold them nests
    /// inside the tuple the parse is already carrying.
    /// </remarks>
    private readonly record struct Examination(
        SpecimenId Id,
        ArcanaCheck Check,
        ISpecimenShelf Shelf,
        IClerkRoster Roster,
        CancellationToken Ct);
}
