namespace Vagrant.Host.Tests;

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Shouldly;
using Xunit;

// The seven lines and their prices are CatalogSeed's. The expectations below come from
// that table and the Dwendalian rate — 50gp is 5000 copper, which is 5pp — not from
// running the shop and copying what it said.
public sealed class ItemEndpointsTests : IClassFixture<ShopFixture>, IDisposable
{
    private readonly HttpClient _client;

    public ItemEndpointsTests(ShopFixture shop)
    {
        ArgumentNullException.ThrowIfNull(shop);

        _client = shop.CreateClient();
    }

    [Fact]
    public async Task The_shop_stocks_its_shelves_on_first_open()
    {
        JsonElement items = await GetAsync("/items");

        items.GetArrayLength().ShouldBe(7);
    }

    [Fact]
    public async Task Items_are_listed_by_name()
    {
        JsonElement items = await GetAsync("/items");

        string[] names = [.. items.EnumerateArray().Select(Name)];

        names.ShouldBe(
        [
            "Bag of Holding",
            "Cloak of Elvenkind",
            "Driftglobe",
            "Immovable Rod",
            "Potion of Greater Healing",
            "Potion of Healing",
            "Rope of Climbing",
        ]);
    }

    [Fact]
    public async Task An_item_carries_its_asking_price_as_a_shelf_label_writes_it()
    {
        JsonElement potion = await FindAsync("Potion of Healing");

        // 50gp is 5000 copper, and 5000 copper is 5pp.
        potion.GetProperty("askingPrice").GetString().ShouldBe("5pp");
        potion.GetProperty("onHand").GetUInt32().ShouldBe(12u);
    }

    [Fact]
    public async Task An_item_never_carries_the_floor_price()
    {
        JsonElement potion = await FindAsync("Potion of Healing");

        potion.TryGetProperty("floorPrice", out _).ShouldBeFalse();
    }

    [Fact]
    public async Task A_stocked_item_can_be_read_on_its_own()
    {
        JsonElement potion = await FindAsync("Potion of Healing");
        string id = potion.GetProperty("id").GetString()!;

        JsonElement read = await GetAsync($"/items/{id}");

        Name(read).ShouldBe("Potion of Healing");
    }

    [Fact]
    public async Task An_identifier_the_shop_has_never_held_is_not_found()
    {
        using HttpResponseMessage response = await _client.GetAsync(
            new Uri("/items/00000000-0000-0000-0000-000000000000", UriKind.Relative),
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task An_identifier_that_is_not_a_guid_never_reaches_the_ledger()
    {
        using HttpResponseMessage response = await _client.GetAsync(
            new Uri("/items/a-potion-please", UriKind.Relative),
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    /// <inheritdoc />
    public void Dispose() => _client.Dispose();

    private static string Name(JsonElement item) =>
        item.GetProperty("name").GetString()!;

    private async Task<JsonElement> FindAsync(string name)
    {
        JsonElement items = await GetAsync("/items");

        return items.EnumerateArray().Single(item => Name(item) == name);
    }

    private async Task<JsonElement> GetAsync(string path)
    {
        using HttpResponseMessage response = await _client.GetAsync(
            new Uri(path, UriKind.Relative),
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        return await response.Content.ReadFromJsonAsync<JsonElement>(
            TestContext.Current.CancellationToken);
    }
}
