namespace Vagrant.Host.OpenApi;

using System.Text.Json.Serialization.Metadata;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Waystone.Monads.Options;

/// <summary>Describes an <c>Option&lt;T&gt;</c> as the payload it holds, or null.</summary>
/// <remarks>
/// <para>
/// Without this the document contradicts the wire. <c>OptionJsonConverter</c> writes a
/// some option as the payload and a none option as <c>null</c>, but the schema exporter
/// reads properties off a type and a converted type has none to read — so
/// <c>SpecimenResponse.Enchantment</c> arrives in the document as an empty
/// <c>OptionOfEnchantment</c> schema, describing neither the enchantment nor the null.
/// </para>
/// <para>
/// One <c>anyOf</c> for every payload type, rather than copying the payload's schema onto
/// the option's field by field. A copy has to name each keyword it carries over, so the
/// first payload to arrive with a constraint nobody listed loses it silently —
/// <c>Option&lt;IReadOnlyList&lt;ItemWantedRequest&gt;&gt;</c> already carries an
/// <c>items</c> with a reference nested inside it.
/// </para>
/// </remarks>
internal sealed class OptionSchemaTransformer : IOpenApiSchemaTransformer
{
    /// <summary>
    /// Names the component a schema is registered under, and answers none for an option so
    /// that the document inlines it instead.
    /// </summary>
    /// <remarks>
    /// An option is not part of the shop's wire contract. Left to the default, the
    /// document grows an <c>OptionOfGuid</c> component and every optional field points at
    /// it, publishing the name of a CLR type a client never sees in a body.
    /// </remarks>
    /// <param name="jsonTypeInfo">The type the document is about to describe.</param>
    /// <returns>
    /// The component name, or none to inline the schema at every field that uses it.
    /// </returns>
    public static Option<string> ReferenceId(JsonTypeInfo jsonTypeInfo)
    {
        ArgumentNullException.ThrowIfNull(jsonTypeInfo);

        return IsOption(jsonTypeInfo.Type)
            ? Option.None<string>()
            : Option.FromNullable(
                  OpenApiOptions.CreateDefaultSchemaReferenceId(jsonTypeInfo));
    }

    /// <inheritdoc />
    public async Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(schema);
        ArgumentNullException.ThrowIfNull(context);

        Type option = context.JsonTypeInfo.Type;

        if (!IsOption(option)) return;

        IOpenApiSchema payload = await context
                                      .GetOrCreateSchemaAsync(
                                           option.GetGenericArguments()[0],
                                           parameterDescription: null,
                                           cancellationToken)
                                      .ConfigureAwait(false);

        schema.AnyOf = [payload, new OpenApiSchema { Type = JsonSchemaType.Null }];
    }

    private static bool IsOption(Type type) =>
        type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Option<>);
}
