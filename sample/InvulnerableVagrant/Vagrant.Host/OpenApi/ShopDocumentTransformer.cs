namespace Vagrant.Host.OpenApi;

using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

/// <summary>Titles the document and describes the tag each route is grouped under.</summary>
/// <remarks>
/// One tag per bounded context, plus <c>Shop</c> for the root route, which belongs to
/// none of them. The reference then groups routes the way the project graph divides the
/// contexts, rather than by path. A route that reads two contexts is tagged with the one
/// it answers for: <c>POST /purchases</c> takes stock off the Catalog's shelf and is
/// tagged <c>Ordering</c>, because what it opens is a purchase.
/// </remarks>
internal sealed class ShopDocumentTransformer : IOpenApiDocumentTransformer
{
    /// <inheritdoc />
    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(document);

        document.Info = new OpenApiInfo
        {
            Title = "The Invulnerable Vagrant",
            Version = "v1",
            // ASCII only. No file in this sample carries a byte order mark, so the
            // compiler reads one as Windows-1252 and an em dash reaches the document as
            // three bytes of mojibake. Comments are safe; a string literal is not.
            Description =
                "Pumat Sol's magic item shop in Zadash. Every refusal is a problem "
              + "document carrying a `code` member, such as "
              + "`vagrant.ordering.offer_below_floor`, so a client branches on the code "
              + "rather than on the message.",
        };

        // A HashSet because Tags is an ISet, which no collection expression constructs.
        document.Tags = new HashSet<OpenApiTag>
        {
            new()
            {
                Name = "Shop",
                Description = "Which shop this is.",
            },
            new()
            {
                Name = "Catalog",
                Description = "What the shop holds and what it asks for it.",
            },
            new()
            {
                Name = "Appraisal",
                Description = "What an unidentified item turns out to be.",
            },
            new()
            {
                Name = "Ordering",
                Description = "Coin moving in either direction.",
            },
            new()
            {
                Name = "Staffing",
                Description = "Which clerk is free.",
            },
        };

        return Task.CompletedTask;
    }
}
