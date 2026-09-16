namespace Vagrant.Host.Tests;

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Shouldly;
using Xunit;

// The expectations here come from OptionJsonConverter<T>.Write, which writes a Some as
// its value with no wrapper and a None as null, and from Aura.YieldsTo, which reads an
// aura when the check reaches its obscurity. Neither was read off a running shop.
//
// A shop per test rather than the IClassFixture the other endpoint classes use. Reading
// an item claims a clerk who holds it until it is collected, and four clerks do not go
// round a class of tests that each read one — a shared shop would fail whichever test
// happened to run fifth.
public sealed class SpecimenEndpointsTests : IAsyncDisposable
{
    private readonly ShopFixture _shop = new();
    private readonly HttpClient _client;

    public SpecimenEndpointsTests()
    {
        _client = _shop.CreateClient();
    }

    [Fact]
    public async Task An_item_the_shop_has_not_read_comes_back_with_a_null_enchantment()
    {
        JsonElement specimen = await HandInAsync(obscurity: 15);

        specimen.GetProperty("enchantment").ValueKind.ShouldBe(JsonValueKind.Null);
    }

    [Fact]
    public async Task Handing_an_item_in_answers_with_where_it_now_lives()
    {
        using HttpResponseMessage response = await PostHandInAsync(Body(obscurity: 15));

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        response.Headers.Location!.ToString()
                .ShouldBe($"/specimens/{await IdOfAsync(response)}");
    }

    [Fact]
    public async Task A_check_that_meets_the_aura_names_the_enchantment()
    {
        Guid id = await HandInIdAsync(obscurity: 15);

        JsonElement read = await IdentifyAsync(id, check: 15);

        JsonElement enchantment = read.GetProperty("enchantment");
        enchantment.GetProperty("name").GetString().ShouldBe("Cloak of Elvenkind");
        enchantment.GetProperty("effect").GetString().ShouldBe("you are harder to see");
    }

    [Fact]
    public async Task A_check_that_falls_short_is_an_answer_rather_than_an_error()
    {
        Guid id = await HandInIdAsync(obscurity: 15);

        JsonElement read = await IdentifyAsync(id, check: 14);

        read.GetProperty("enchantment").ValueKind.ShouldBe(JsonValueKind.Null);
    }

    [Fact]
    public async Task A_negative_check_reads_nothing_off_even_a_plain_item()
    {
        Guid id = await HandInIdAsync(obscurity: 0);

        JsonElement read = await IdentifyAsync(id, check: -1);

        read.GetProperty("enchantment").ValueKind.ShouldBe(JsonValueKind.Null);
    }

    [Fact]
    public async Task Reading_an_item_the_shop_never_took_in_is_a_404()
    {
        using HttpResponseMessage response = await _client.PostAsJsonAsync(
            new Uri($"/specimens/{Guid.CreateVersion7()}/identify", UriKind.Relative),
            new { check = 20 },
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    // Four clerks, from ClerkSeed: one Pumat Prime and three simulacra. A fifth item read
    // before any of the four is collected has nobody to read it, and 503 is the status
    // that says the request was fine and the shop is momentarily out of hands.
    [Fact]
    public async Task A_fifth_item_read_at_once_finds_every_clerk_busy()
    {
        for (int taken = 0; taken < 4; taken++)
        {
            await IdentifyAsync(await HandInIdAsync(obscurity: 15), check: 15);
        }

        using HttpResponseMessage response = await PostIdentifyAsync(
            await HandInIdAsync(obscurity: 15),
            check: 15);

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
        (await ProblemAsync(response)).GetProperty("code")
                                     .GetString()
                                     .ShouldBe("vagrant.staffing.no_clerk_free");
    }

    [Fact]
    public async Task Collecting_an_item_frees_the_clerk_who_was_holding_it()
    {
        List<Guid> read = [];

        for (int taken = 0; taken < 4; taken++)
        {
            Guid id = await HandInIdAsync(obscurity: 15);
            await IdentifyAsync(id, check: 15);
            read.Add(id);
        }

        await CollectAsync(read[0]);

        using HttpResponseMessage response = await PostIdentifyAsync(
            await HandInIdAsync(obscurity: 15),
            check: 15);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    // Otherwise a patron asking four times about one cloak would empty the shop.
    [Fact]
    public async Task Asking_twice_about_the_same_item_takes_no_second_clerk()
    {
        Guid cloak = await HandInIdAsync(obscurity: 15);

        await IdentifyAsync(cloak, check: 15);
        await IdentifyAsync(cloak, check: 15);

        for (int remaining = 0; remaining < 3; remaining++)
        {
            await IdentifyAsync(await HandInIdAsync(obscurity: 15), check: 15);
        }

        using HttpResponseMessage response = await PostIdentifyAsync(
            await HandInIdAsync(obscurity: 15),
            check: 15);

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task Collecting_an_item_hands_back_what_the_shop_read()
    {
        Guid id = await HandInIdAsync(obscurity: 15);
        await IdentifyAsync(id, check: 15);

        using HttpResponseMessage response = await PostCollectAsync(id);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        JsonElement collected = await response.Content.ReadFromJsonAsync<JsonElement>(
            TestContext.Current.CancellationToken);

        collected.GetProperty("enchantment")
                 .GetProperty("name")
                 .GetString()
                 .ShouldBe("Cloak of Elvenkind");
    }

    [Fact]
    public async Task Collecting_an_item_no_clerk_is_holding_is_a_404()
    {
        Guid id = await HandInIdAsync(obscurity: 15);

        using HttpResponseMessage response = await PostCollectAsync(id);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Collecting_the_same_item_twice_is_a_404_the_second_time()
    {
        Guid id = await HandInIdAsync(obscurity: 15);
        await IdentifyAsync(id, check: 15);
        await CollectAsync(id);

        using HttpResponseMessage response = await PostCollectAsync(id);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    // No clerk is claimed for an item the shop does not hold, so the 404 above leaves all
    // four free rather than one holding an errand about nothing.
    [Fact]
    public async Task An_item_the_shop_never_took_in_costs_no_clerk()
    {
        using (HttpResponseMessage missing =
               await PostIdentifyAsync(Guid.CreateVersion7(), check: 20))
        {
            missing.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        }

        for (int taken = 0; taken < 4; taken++)
        {
            await IdentifyAsync(await HandInIdAsync(obscurity: 15), check: 15);
        }
    }

    [Fact]
    public async Task A_body_missing_the_patron_is_refused_by_name()
    {
        object body = new
        {
            description = "a grey cloak, well worn",
            obscurity = 15,
            enchantmentName = "Cloak of Elvenkind",
            enchantmentEffect = "you are harder to see",
        };

        using HttpResponseMessage response = await PostHandInAsync(body);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await ProblemsAsync(response)).GetProperty("patron")
                                      .GetArrayLength()
                                      .ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task A_negative_obscurity_never_reaches_the_domain()
    {
        using HttpResponseMessage response =
            await PostHandInAsync(Body(obscurity: -1));

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await ProblemsAsync(response)).GetProperty("obscurity")
                                      .GetArrayLength()
                                      .ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task A_description_too_short_to_mean_anything_is_refused()
    {
        using HttpResponseMessage response =
            await PostHandInAsync(Body(obscurity: 15, description: "  a "));

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await ProblemsAsync(response)).GetProperty("description")
                                      .GetArrayLength()
                                      .ShouldBeGreaterThan(0);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        _client.Dispose();

        await _shop.DisposeAsync().ConfigureAwait(false);
    }

    private static object Body(int obscurity, string description = "a grey cloak, well worn") =>
        new
        {
            patron = Guid.CreateVersion7(),
            description,
            obscurity,
            enchantmentName = "Cloak of Elvenkind",
            enchantmentEffect = "you are harder to see",
        };

    private static async Task<JsonElement> ProblemsAsync(HttpResponseMessage response)
    {
        JsonElement problem = await response.Content.ReadFromJsonAsync<JsonElement>(
            TestContext.Current.CancellationToken);

        return problem.GetProperty("errors");
    }

    private static async Task<Guid> IdOfAsync(HttpResponseMessage response)
    {
        JsonElement specimen = await response.Content.ReadFromJsonAsync<JsonElement>(
            TestContext.Current.CancellationToken);

        return specimen.GetProperty("id").GetGuid();
    }

    private Task<HttpResponseMessage> PostHandInAsync(object body) =>
        _client.PostAsJsonAsync(
            new Uri("/specimens", UriKind.Relative),
            body,
            TestContext.Current.CancellationToken);

    private async Task<JsonElement> HandInAsync(int obscurity)
    {
        using HttpResponseMessage response = await PostHandInAsync(Body(obscurity));

        response.StatusCode.ShouldBe(HttpStatusCode.Created);

        return await response.Content.ReadFromJsonAsync<JsonElement>(
            TestContext.Current.CancellationToken);
    }

    private async Task<Guid> HandInIdAsync(int obscurity) =>
        (await HandInAsync(obscurity)).GetProperty("id").GetGuid();

    private Task<HttpResponseMessage> PostIdentifyAsync(Guid id, int check) =>
        _client.PostAsJsonAsync(
            new Uri($"/specimens/{id}/identify", UriKind.Relative),
            new { check },
            TestContext.Current.CancellationToken);

    private Task<HttpResponseMessage> PostCollectAsync(Guid id) =>
        _client.PostAsync(
            new Uri($"/specimens/{id}/collect", UriKind.Relative),
            content: null,
            TestContext.Current.CancellationToken);

    private async Task<JsonElement> IdentifyAsync(Guid id, int check)
    {
        using HttpResponseMessage response = await PostIdentifyAsync(id, check);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        return await response.Content.ReadFromJsonAsync<JsonElement>(
            TestContext.Current.CancellationToken);
    }

    private async Task CollectAsync(Guid id)
    {
        using HttpResponseMessage response = await PostCollectAsync(id);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    private static async Task<JsonElement> ProblemAsync(HttpResponseMessage response) =>
        await response.Content.ReadFromJsonAsync<JsonElement>(
            TestContext.Current.CancellationToken);
}
