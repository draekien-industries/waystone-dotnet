namespace Vagrant.Host.Endpoints;

using Vagrant.Host.Contracts;
using Vagrant.Staffing;

/// <summary>Who is behind the counter, and what they are busy with.</summary>
internal static class ClerkEndpoints
{
    /// <summary>Maps the one way to see the counter.</summary>
    /// <param name="routes">Where to map it.</param>
    public static void MapClerks(this IEndpointRouteBuilder routes)
    {
        ArgumentNullException.ThrowIfNull(routes);

        // No Result to unwrap. Asking who is behind the counter cannot be refused, so
        // the roster returns a list and the endpoint returns a 200 — an empty shop is an
        // empty array, not a 404.
        routes.MapGet(
                "/clerks",
                async (IClerkRoster roster, CancellationToken ct) =>
                {
                    IReadOnlyList<Clerk> onDuty =
                        await roster.OnDutyAsync(ct).ConfigureAwait(false);

                    return Results.Ok(onDuty.Select(ClerkResponse.From).ToList());
                })
           .WithTags("Staffing")
           .WithSummary("Reads who is behind the counter.")
           .WithDescription(
                "One 200 and no refusal. Three of the four rows come back named "
              + "\"Pumat Sol\" because the simulacra share a name, and holding is "
              + "null for a clerk who is free.")
           .Produces<List<ClerkResponse>>(StatusCodes.Status200OK);
    }
}
