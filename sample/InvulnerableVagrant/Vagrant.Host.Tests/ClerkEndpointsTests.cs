namespace Vagrant.Host.Tests;

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Shouldly;
using Xunit;

// The roster comes from ClerkSeed: four clerks, of whom three answer to "Pumat Sol".
// That is the wiki's count of Pumat's simulacra, not a number chosen to make a test
// pass, and holding is null for a free clerk because OptionJsonConverter<T> writes a
// None as null.
//
// A shop per test, for the same reason SpecimenEndpointsTests has one: a clerk stays
// engaged until the item is collected, so a shared shop would carry one test's claims
// into the next.
public sealed class ClerkEndpointsTests : IAsyncDisposable
{
    private readonly ShopFixture _shop = new();
    private readonly HttpClient _client;

    public ClerkEndpointsTests()
    {
        _client = _shop.CreateClient();
    }

    [Fact]
    public async Task The_counter_is_staffed_by_four()
    {
        (await OnDutyAsync()).GetArrayLength().ShouldBe(4);
    }

    [Fact]
    public async Task Three_of_the_four_answer_to_the_same_name()
    {
        JsonElement onDuty = await OnDutyAsync();

        onDuty.EnumerateArray()
              .Count(clerk => clerk.GetProperty("name").GetString() == "Pumat Sol")
              .ShouldBe(3);
    }

    [Fact]
    public async Task A_shared_name_is_not_a_shared_clerk()
    {
        JsonElement onDuty = await OnDutyAsync();

        onDuty.EnumerateArray()
              .Select(clerk => clerk.GetProperty("id").GetGuid())
              .Distinct()
              .Count()
              .ShouldBe(4);
    }

    [Fact]
    public async Task A_clerk_holding_nothing_is_a_null_rather_than_an_empty_guid()
    {
        JsonElement onDuty = await OnDutyAsync();

        onDuty.EnumerateArray()
              .ShouldAllBe(clerk =>
                  clerk.GetProperty("holding").ValueKind == JsonValueKind.Null);
    }

    [Fact]
    public async Task A_clerk_reading_an_item_says_which_item()
    {
        Guid cloak = await HandInAsync();

        await IdentifyAsync(cloak);

        JsonElement onDuty = await OnDutyAsync();

        onDuty.EnumerateArray()
              .Count(clerk => clerk.GetProperty("holding").ValueKind
                           != JsonValueKind.Null)
              .ShouldBe(1);

        onDuty.EnumerateArray()
              .Single(clerk => clerk.GetProperty("holding").ValueKind
                            != JsonValueKind.Null)
              .GetProperty("holding")
              .GetGuid()
              .ShouldBe(cloak);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        _client.Dispose();

        await _shop.DisposeAsync().ConfigureAwait(false);
    }

    private async Task<JsonElement> OnDutyAsync()
    {
        using HttpResponseMessage response = await _client.GetAsync(
            new Uri("/clerks", UriKind.Relative),
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        return await response.Content.ReadFromJsonAsync<JsonElement>(
            TestContext.Current.CancellationToken);
    }

    private async Task<Guid> HandInAsync()
    {
        using HttpResponseMessage response = await _client.PostAsJsonAsync(
            new Uri("/specimens", UriKind.Relative),
            new
            {
                patron = Guid.CreateVersion7(),
                description = "a grey cloak, well worn",
                obscurity = 15,
                enchantmentName = "Cloak of Elvenkind",
                enchantmentEffect = "you are harder to see",
            },
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);

        JsonElement specimen = await response.Content.ReadFromJsonAsync<JsonElement>(
            TestContext.Current.CancellationToken);

        return specimen.GetProperty("id").GetGuid();
    }

    private async Task IdentifyAsync(Guid id)
    {
        using HttpResponseMessage response = await _client.PostAsJsonAsync(
            new Uri($"/specimens/{id}/identify", UriKind.Relative),
            new { check = 15 },
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
