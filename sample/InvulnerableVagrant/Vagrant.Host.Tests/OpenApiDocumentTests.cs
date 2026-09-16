namespace Vagrant.Host.Tests;

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Shouldly;
using Xunit;

// The document is generated from the running host, so these assertions fail the moment
// the wiring in Program.cs stops matching the converters beside it. Nothing else notices:
// a schema that contradicts the wire still serves every request correctly.
//
// A shared shop. Reading the document claims no clerk.
public sealed class OpenApiDocumentTests : IClassFixture<ShopFixture>, IDisposable
{
    private readonly HttpClient _client;

    public OpenApiDocumentTests(ShopFixture shop)
    {
        ArgumentNullException.ThrowIfNull(shop);

        _client = shop.CreateClient();
    }

    [Fact]
    public async Task The_shop_publishes_a_document_under_its_own_name()
    {
        JsonElement document = await DocumentAsync();

        document.GetProperty("info").GetProperty("title").GetString()
                .ShouldBe("The Invulnerable Vagrant");
    }

    [Fact]
    public async Task The_reference_page_is_served()
    {
        using HttpResponseMessage reference = await _client.GetAsync(
            new Uri("/scalar/v1", UriKind.Relative),
            TestContext.Current.CancellationToken);

        reference.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    // The assertion the rest of this file exists for. OptionJsonConverter writes a Some as
    // the payload and a None as null, and without OptionSchemaTransformer the document
    // describes neither: Option<Enchantment> is a converted type, so the schema exporter
    // finds no properties to read off it and publishes an empty schema.
    [Fact]
    public async Task An_option_is_described_as_the_payload_it_holds_or_null()
    {
        JsonElement enchantment = await PropertyAsync("SpecimenResponse", "enchantment");

        // A case is its $ref where it has one and its type where it does not: the payload
        // is a component and the null is inline. Inline rather than a named helper, which
        // WM3001 rejects for declaring a string? return.
        string?[] cases =
        [
            .. enchantment.GetProperty("anyOf")
                          .EnumerateArray()
                          .Select(
                               static option =>
                                   option.TryGetProperty(
                                       "$ref",
                                       out JsonElement reference)
                                       ? reference.GetString()
                                       : option.GetProperty("type").GetString()),
        ];

        cases.ShouldBe(["#/components/schemas/Enchantment", "null"]);
    }

    [Fact]
    public async Task An_optional_field_keeps_the_summary_written_on_it()
    {
        JsonElement holding = await PropertyAsync("ClerkResponse", "holding");

        holding.GetProperty("description").GetString()
               .ShouldBe("What they are working on, if anything.");
    }

    // CreateSchemaReferenceId inlines an option instead of registering it. Left to the
    // default, the document grows an OptionOfGuid component and publishes the name of a
    // type no client ever sees in a body.
    [Fact]
    public async Task No_component_is_named_after_an_option()
    {
        JsonElement schemas =
            (await DocumentAsync()).GetProperty("components").GetProperty("schemas");

        schemas.EnumerateObject()
               .Select(static schema => schema.Name)
               .ShouldNotContain(
                    static name =>
                        name.StartsWith("Option", StringComparison.Ordinal));
    }

    // Refusal.From writes the code into ProblemDetails.Extensions, which the serializer
    // flattens into the body and the exporter does not describe.
    [Fact]
    public async Task A_refusal_names_the_code_it_carries()
    {
        JsonElement code = await PropertyAsync("ProblemDetails", "code");

        code.GetProperty("description").GetString()
            .ShouldNotBeNull()
            .ShouldContain("vagrant.ordering.offer_below_floor");
    }

    // Every handler returns IResult, so the document says nothing about a response the
    // endpoint has not declared. The 503 is the one the whole Staffing context exists to
    // produce, and it would be the first to go missing.
    [Theory]
    [InlineData("/specimens/{id}/identify", "503")]
    [InlineData("/purchases/{id}/settle", "402")]
    [InlineData("/buybacks/{id}/settle", "409")]
    public async Task An_endpoint_declares_the_status_it_refuses_with(
        string route,
        string status)
    {
        JsonElement responses = (await DocumentAsync())
                               .GetProperty("paths")
                               .GetProperty(route)
                               .GetProperty("post")
                               .GetProperty("responses");

        responses.TryGetProperty(status, out _).ShouldBeTrue();
    }

    [Theory]
    [InlineData("/items", "Catalog")]
    [InlineData("/specimens", "Appraisal")]
    [InlineData("/clerks", "Staffing")]
    public async Task A_route_is_tagged_with_the_context_it_belongs_to(
        string route,
        string context)
    {
        JsonElement operations =
            (await DocumentAsync()).GetProperty("paths").GetProperty(route);

        JsonElement operation = operations.EnumerateObject().First().Value;

        operation.GetProperty("tags")
                 .EnumerateArray()
                 .Select(static tag => tag.GetString())
                 .ShouldContain(context);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _client.Dispose();
    }

    private async Task<JsonElement> PropertyAsync(string schema, string property) =>
        (await DocumentAsync())
       .GetProperty("components")
       .GetProperty("schemas")
       .GetProperty(schema)
       .GetProperty("properties")
       .GetProperty(property);

    private async Task<JsonElement> DocumentAsync() =>
        await _client.GetFromJsonAsync<JsonElement>(
            new Uri("/openapi/v1.json", UriKind.Relative),
            TestContext.Current.CancellationToken);
}
