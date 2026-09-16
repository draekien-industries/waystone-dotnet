namespace Vagrant.Host.Tests;

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Shouldly;
using Xunit;

// The till holds two thousand gold. Every figure below is a comparison against that —
// two hundred is covered, three thousand is not — rather than a reading of what the
// endpoints returned.
public sealed class BuybackEndpointsTests : IClassFixture<ShopFixture>, IDisposable
{
    private readonly HttpClient _client;

    public BuybackEndpointsTests(ShopFixture shop)
    {
        ArgumentNullException.ThrowIfNull(shop);

        _client = shop.CreateClient();
    }

    [Fact]
    public async Task Taking_something_in_answers_with_where_it_now_lives()
    {
        using HttpResponseMessage response = await PostOpenAsync(gold: 200);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);

        JsonElement buyback = await BodyAsync(response);
        response.Headers.Location!.ToString()
                .ShouldBe($"/buybacks/{buyback.GetProperty("id").GetGuid()}");
    }

    [Fact]
    public async Task A_buyback_keeps_what_it_was_opened_on()
    {
        JsonElement buyback = await OpenAsync(gold: 200);

        buyback.GetProperty("description").GetString().ShouldBe("a wand of some kind");
        buyback.GetProperty("offered").GetString().ShouldBe("20pp");
    }

    [Fact]
    public async Task A_blank_description_is_refused()
    {
        using HttpResponseMessage response = await PostOpenAsync(200, description: "  ");

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    // Nothing in the body. The shop knows what it offered and what its till holds, and a
    // patron cannot be asked either.
    [Fact]
    public async Task Paying_a_patron_out_takes_no_body_at_all()
    {
        Guid buyback = await OpenedAsync(gold: 200);

        using HttpResponseMessage response = await PostSettleAsync(buyback);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await BodyAsync(response)).GetProperty("moved").GetString().ShouldBe("20pp");
    }

    [Fact]
    public async Task A_figure_the_till_cannot_cover_is_a_402()
    {
        Guid buyback = await OpenedAsync(gold: 3000);

        using HttpResponseMessage response = await PostSettleAsync(buyback);

        response.StatusCode.ShouldBe(HttpStatusCode.PaymentRequired);
        (await CodeAsync(response)).ShouldBe("vagrant.ordering.till_cannot_cover");
    }

    [Fact]
    public async Task A_figure_exactly_the_size_of_the_till_is_covered()
    {
        Guid buyback = await OpenedAsync(gold: 2000);

        using HttpResponseMessage response = await PostSettleAsync(buyback);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task A_settled_buyback_cannot_be_settled_again()
    {
        Guid buyback = await OpenedAsync(gold: 200);

        using (await PostSettleAsync(buyback))
        {
            using HttpResponseMessage again = await PostSettleAsync(buyback);

            again.StatusCode.ShouldBe(HttpStatusCode.Conflict);
            (await CodeAsync(again)).ShouldBe("vagrant.ordering.buyback_already_settled");
        }
    }

    [Fact]
    public async Task A_buyback_the_shop_never_opened_cannot_be_settled()
    {
        using HttpResponseMessage response =
            await PostSettleAsync(Guid.CreateVersion7());

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await CodeAsync(response)).ShouldBe("vagrant.ordering.no_such_buyback");
    }

    [Fact]
    public async Task A_buyback_the_shop_never_opened_reads_back_as_nothing()
    {
        using HttpResponseMessage response = await _client.GetAsync(
            new Uri($"/buybacks/{Guid.CreateVersion7()}", UriKind.Relative),
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task A_buyback_reads_back_with_what_the_shop_offered()
    {
        Guid buyback = await OpenedAsync(gold: 750);

        using HttpResponseMessage response = await _client.GetAsync(
            new Uri($"/buybacks/{buyback}", UriKind.Relative),
            TestContext.Current.CancellationToken);

        (await BodyAsync(response)).GetProperty("offered").GetString()
                                   .ShouldBe("75pp");
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _client.Dispose();
    }

    private async Task<Guid> OpenedAsync(int gold) =>
        (await OpenAsync(gold)).GetProperty("id").GetGuid();

    private async Task<JsonElement> OpenAsync(int gold)
    {
        using HttpResponseMessage response = await PostOpenAsync(gold);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);

        return await BodyAsync(response);
    }

    private Task<HttpResponseMessage> PostOpenAsync(
        int gold,
        string description = "a wand of some kind") =>
        _client.PostAsJsonAsync(
            new Uri("/buybacks", UriKind.Relative),
            new
            {
                patron = Guid.CreateVersion7(),
                description,
                offered = new { gold },
            },
            TestContext.Current.CancellationToken);

    private Task<HttpResponseMessage> PostSettleAsync(Guid buyback) =>
        _client.PostAsync(
            new Uri($"/buybacks/{buyback}/settle", UriKind.Relative),
            content: null,
            TestContext.Current.CancellationToken);

    private static async Task<string?> CodeAsync(HttpResponseMessage response) =>
        (await BodyAsync(response)).GetProperty("code").GetString();

    private static async Task<JsonElement> BodyAsync(HttpResponseMessage response) =>
        await response.Content.ReadFromJsonAsync<JsonElement>(
            TestContext.Current.CancellationToken);
}
