namespace Vagrant.Host.Tests;

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Shouldly;
using Xunit;

// The shop seeds a Potion of Healing at 50gp asking and 45gp floor, and a Driftglobe at
// 750gp asking and 700gp floor. Every figure below is arithmetic over those two labels —
// three potions come to 150gp and will not go below 135gp — put through Coin.ToString,
// which writes an amount in the largest denominations it reduces to. So 150gp reads 15pp
// and 135gp reads 13pp 5gp. None of it is a reading of what the endpoints returned.
//
// A shop per test rather than per class, unlike every other endpoint test here. These
// tests buy things, the shelf holds twelve potions, and a suite that shares them fails
// on whichever test happens to run once the twelfth is gone.
public sealed class PurchaseEndpointsTests : IAsyncDisposable
{
    private const string Potion = "Potion of Healing";
    private const string Driftglobe = "Driftglobe";

    private readonly ShopFixture _shop = new();
    private readonly HttpClient _client;

    public PurchaseEndpointsTests()
    {
        _client = _shop.CreateClient();
    }

    [Fact]
    public async Task A_purchase_is_priced_from_the_shelf_label_not_from_the_request()
    {
        JsonElement purchase = await OpenAsync(await LinesAsync((Potion, 3)));

        purchase.GetProperty("total").GetString().ShouldBe("15pp");
        purchase.GetProperty("lines")[0].GetProperty("askingPrice").GetString()
                .ShouldBe("5pp");
    }

    [Fact]
    public async Task A_line_nobody_haggled_over_has_a_null_agreed_price()
    {
        JsonElement purchase = await OpenAsync(await LinesAsync((Potion, 3)));

        purchase.GetProperty("lines")[0].GetProperty("agreedPrice").ValueKind
                .ShouldBe(JsonValueKind.Null);
    }

    [Fact]
    public async Task Opening_a_purchase_answers_with_where_it_now_lives()
    {
        using HttpResponseMessage response =
            await PostOpenAsync(await LinesAsync((Potion, 1)));

        response.StatusCode.ShouldBe(HttpStatusCode.Created);

        JsonElement purchase = await BodyAsync(response);
        response.Headers.Location!.ToString()
                .ShouldBe($"/purchases/{purchase.GetProperty("id").GetGuid()}");
    }

    [Fact]
    public async Task A_purchase_carries_every_line_it_was_opened_on()
    {
        JsonElement purchase =
            await OpenAsync(await LinesAsync((Potion, 2), (Driftglobe, 1)));

        purchase.GetProperty("lines").GetArrayLength().ShouldBe(2);
        purchase.GetProperty("total").GetString().ShouldBe("85pp");
    }

    [Fact]
    public async Task A_line_the_shop_has_never_stocked_is_a_404()
    {
        using HttpResponseMessage response = await PostOpenAsync(
            [new { item = Guid.CreateVersion7(), quantity = 1 }]);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await CodeAsync(response)).ShouldBe("vagrant.catalog.not_stocked");
    }

    [Fact]
    public async Task A_line_the_shop_cannot_supply_is_a_409()
    {
        using HttpResponseMessage response =
            await PostOpenAsync(await LinesAsync((Potion, 999)));

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        (await CodeAsync(response)).ShouldBe("vagrant.catalog.not_enough_on_hand");
    }

    // The point of the all-or-nothing withdrawal: the driftglobe succeeds, the potions
    // do not, and the driftglobe has to still be on the shelf afterwards.
    [Fact]
    public async Task A_refused_line_leaves_the_lines_before_it_on_the_shelf()
    {
        uint before = await OnHandAsync(Driftglobe);

        using HttpResponseMessage response =
            await PostOpenAsync(await LinesAsync((Driftglobe, 1), (Potion, 999)));

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        (await OnHandAsync(Driftglobe)).ShouldBe(before);
    }

    [Fact]
    public async Task A_bought_line_comes_off_the_shelf()
    {
        uint before = await OnHandAsync(Potion);

        await OpenAsync(await LinesAsync((Potion, 2)));

        (await OnHandAsync(Potion)).ShouldBe(before - 2);
    }

    [Fact]
    public async Task Asking_for_none_of_something_never_reaches_the_shelf()
    {
        uint before = await OnHandAsync(Potion);

        using HttpResponseMessage response =
            await PostOpenAsync(await LinesAsync((Potion, 0)));

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await OnHandAsync(Potion)).ShouldBe(before);
    }

    [Fact]
    public async Task A_purchase_with_nothing_on_it_is_refused()
    {
        using HttpResponseMessage response = await PostOpenAsync([]);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    // Three potions are one line of three. Two lines naming the same potion would leave
    // Purchase.Agree haggling over whichever it found first.
    [Fact]
    public async Task The_same_thing_twice_on_one_purchase_is_refused()
    {
        using HttpResponseMessage response =
            await PostOpenAsync(await LinesAsync((Potion, 1), (Potion, 2)));

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await CodeAsync(response)).ShouldBe("vagrant.ordering.duplicate_line");
    }

    [Fact]
    public async Task An_offer_at_the_floor_for_the_whole_line_is_taken()
    {
        (Guid purchase, Guid item) = await OpenedAsync((Potion, 3));

        using HttpResponseMessage response =
            await PostOfferAsync(purchase, item, gold: 135);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await BodyAsync(response)).GetProperty("agreedPrice").GetString()
                                   .ShouldBe("13pp 5gp");
    }

    [Fact]
    public async Task An_offer_a_copper_under_the_floor_is_refused()
    {
        (Guid purchase, Guid item) = await OpenedAsync((Potion, 3));

        using HttpResponseMessage response = await PostOfferAsync(
            purchase,
            item,
            new { gold = 134, silver = 9, copper = 9 });

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        (await CodeAsync(response)).ShouldBe("vagrant.ordering.offer_below_floor");
    }

    [Fact]
    public async Task An_agreed_price_changes_what_the_purchase_comes_to()
    {
        (Guid purchase, Guid item) = await OpenedAsync((Potion, 3));

        using (await PostOfferAsync(purchase, item, gold: 140))
        {
            JsonElement read = await ReadAsync(purchase);

            read.GetProperty("total").GetString().ShouldBe("14pp");
            read.GetProperty("lines")[0].GetProperty("agreedPrice").GetString()
                .ShouldBe("14pp");
        }
    }

    [Fact]
    public async Task Haggling_over_a_line_the_purchase_does_not_carry_is_a_404()
    {
        (Guid purchase, _) = await OpenedAsync((Potion, 1));

        using HttpResponseMessage response =
            await PostOfferAsync(purchase, Guid.CreateVersion7(), gold: 40);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await CodeAsync(response)).ShouldBe("vagrant.ordering.not_on_this_purchase");
    }

    [Fact]
    public async Task Coin_a_copper_short_of_the_total_is_a_402()
    {
        (Guid purchase, _) = await OpenedAsync((Potion, 3));

        using HttpResponseMessage response = await PostSettleAsync(
            purchase,
            new { gold = 149, silver = 9, copper = 9 });

        response.StatusCode.ShouldBe(HttpStatusCode.PaymentRequired);
        (await CodeAsync(response)).ShouldBe("vagrant.ordering.insufficient_coin");
    }

    // 149gp 9sp 10cp is 150gp counted out badly, and the shop takes it. Every
    // denomination left out of the body is none of it rather than a missing field.
    [Fact]
    public async Task Coin_counted_out_in_smaller_pieces_still_covers_the_total()
    {
        (Guid purchase, _) = await OpenedAsync((Potion, 3));

        using HttpResponseMessage response = await PostSettleAsync(
            purchase,
            new { gold = 149, silver = 9, copper = 10 });

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await BodyAsync(response)).GetProperty("moved").GetString().ShouldBe("15pp");
    }

    [Fact]
    public async Task The_receipt_records_the_total_rather_than_what_was_put_down()
    {
        (Guid purchase, _) = await OpenedAsync((Potion, 3));

        using HttpResponseMessage response =
            await PostSettleAsync(purchase, new { gold = 200 });

        (await BodyAsync(response)).GetProperty("moved").GetString().ShouldBe("15pp");
    }

    [Fact]
    public async Task A_settled_purchase_cannot_be_settled_again()
    {
        (Guid purchase, _) = await OpenedAsync((Potion, 1));

        using (await PostSettleAsync(purchase, new { gold = 50 }))
        {
            using HttpResponseMessage again =
                await PostSettleAsync(purchase, new { gold = 50 });

            again.StatusCode.ShouldBe(HttpStatusCode.Conflict);
            (await CodeAsync(again))
               .ShouldBe("vagrant.ordering.purchase_already_settled");
        }
    }

    [Fact]
    public async Task A_purchase_the_shop_never_opened_cannot_be_settled()
    {
        using HttpResponseMessage response =
            await PostSettleAsync(Guid.CreateVersion7(), new { gold = 50 });

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await CodeAsync(response)).ShouldBe("vagrant.ordering.no_such_purchase");
    }

    [Fact]
    public async Task A_purchase_the_shop_never_opened_reads_back_as_nothing()
    {
        using HttpResponseMessage response =
            await _client.GetAsync(
                new Uri($"/purchases/{Guid.CreateVersion7()}", UriKind.Relative),
                TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task A_denomination_below_zero_is_refused()
    {
        (Guid purchase, _) = await OpenedAsync((Potion, 1));

        using HttpResponseMessage response =
            await PostSettleAsync(purchase, new { gold = -1 });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        _client.Dispose();

        await _shop.DisposeAsync().ConfigureAwait(false);
    }

    private async Task<(Guid Purchase, Guid Item)> OpenedAsync(
        params (string Name, int Quantity)[] lines)
    {
        JsonElement purchase = await OpenAsync(await LinesAsync(lines));

        return (purchase.GetProperty("id").GetGuid(),
            purchase.GetProperty("lines")[0].GetProperty("item").GetGuid());
    }

    private async Task<JsonElement> OpenAsync(object[] lines)
    {
        using HttpResponseMessage response = await PostOpenAsync(lines);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);

        return await BodyAsync(response);
    }

    private Task<HttpResponseMessage> PostOpenAsync(object[] lines) =>
        _client.PostAsJsonAsync(
            new Uri("/purchases", UriKind.Relative),
            new { patron = Guid.CreateVersion7(), items = lines },
            TestContext.Current.CancellationToken);

    private Task<HttpResponseMessage> PostOfferAsync(
        Guid purchase,
        Guid item,
        int gold) =>
        PostOfferAsync(purchase, item, new { gold });

    private Task<HttpResponseMessage> PostOfferAsync(
        Guid purchase,
        Guid item,
        object offer) =>
        _client.PostAsJsonAsync(
            new Uri($"/purchases/{purchase}/offer", UriKind.Relative),
            new { item, offer },
            TestContext.Current.CancellationToken);

    private Task<HttpResponseMessage> PostSettleAsync(Guid purchase, object tendered) =>
        _client.PostAsJsonAsync(
            new Uri($"/purchases/{purchase}/settle", UriKind.Relative),
            new { tendered },
            TestContext.Current.CancellationToken);

    private async Task<JsonElement> ReadAsync(Guid purchase)
    {
        using HttpResponseMessage response = await _client.GetAsync(
            new Uri($"/purchases/{purchase}", UriKind.Relative),
            TestContext.Current.CancellationToken);

        return await BodyAsync(response);
    }

    private async Task<object[]> LinesAsync(params (string Name, int Quantity)[] lines)
    {
        List<object> wanted = new(lines.Length);

        foreach ((string name, int quantity) in lines)
        {
            wanted.Add(new { item = await IdOfAsync(name), quantity });
        }

        return [.. wanted];
    }

    private async Task<Guid> IdOfAsync(string name) =>
        (await ShelfAsync(name)).GetProperty("id").GetGuid();

    private async Task<uint> OnHandAsync(string name) =>
        (await ShelfAsync(name)).GetProperty("onHand").GetUInt32();

    private async Task<JsonElement> ShelfAsync(string name)
    {
        using HttpResponseMessage response = await _client.GetAsync(
            new Uri("/items", UriKind.Relative),
            TestContext.Current.CancellationToken);

        JsonElement items = await BodyAsync(response);

        return items.EnumerateArray()
                    .Where(item => item.GetProperty("name").GetString() == name)
                    .ToList()
                    .ShouldHaveSingleItem();
    }

    private static async Task<string?> CodeAsync(HttpResponseMessage response) =>
        (await BodyAsync(response)).GetProperty("code").GetString();

    private static async Task<JsonElement> BodyAsync(HttpResponseMessage response) =>
        await response.Content.ReadFromJsonAsync<JsonElement>(
            TestContext.Current.CancellationToken);
}
