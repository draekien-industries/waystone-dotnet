namespace Vagrant.Host.OpenApi;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

/// <summary>Adds the code a refusal carries to the problem document's schema.</summary>
/// <remarks>
/// <c>Refusal.From</c> writes the code into <c>ProblemDetails.Extensions</c>, which the
/// serializer flattens into the body and the schema exporter does not describe — so the
/// one member a client is meant to branch on is the one member the document leaves out.
/// <para>
/// Optional rather than required. Every refusal the shop makes carries a code, but
/// <c>UseStatusCodePages</c> renders a bodiless 404 as a problem document too, and that
/// one has no code to carry.
/// </para>
/// </remarks>
internal sealed class RefusalSchemaTransformer : IOpenApiSchemaTransformer
{
    /// <inheritdoc />
    public Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(schema);
        ArgumentNullException.ThrowIfNull(context);

        if (context.JsonTypeInfo.Type != typeof(ProblemDetails))
        {
            return Task.CompletedTask;
        }

        schema.Properties ??=
            new Dictionary<string, IOpenApiSchema>(StringComparer.Ordinal);

        schema.Properties["code"] = new OpenApiSchema
        {
            Type = JsonSchemaType.String | JsonSchemaType.Null,
            Description =
                "The code the shop refused under, such as "
              + "vagrant.ordering.offer_below_floor. Absent on a problem document the "
              + "shop did not write itself.",
        };

        return Task.CompletedTask;
    }
}
