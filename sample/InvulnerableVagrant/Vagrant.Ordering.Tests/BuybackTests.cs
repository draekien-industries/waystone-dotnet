namespace Vagrant.Ordering.Tests;

using Microsoft.Extensions.Time.Testing;
using Shouldly;
using Vagrant.SharedKernel;
using Waystone.Monads.Results.Errors;
using Xunit;

// Pumat offers 200gp for a wand nobody can identify. The till either covers that or it
// does not; the figures below are that comparison, not a reading of what the code did.
public sealed class BuybackTests
{
    private static readonly DateTimeOffset Noon =
        new(2026, 9, 16, 12, 0, 0, TimeSpan.Zero);
    private static readonly FakeTimeProvider Clock = new(Noon);

    [Fact]
    public void A_buyback_keeps_what_it_was_opened_on()
    {
        PatronId patron = PatronId.New();

        Buyback buyback = Buyback.Open(
            BuybackId.New(),
            patron,
            "a wand of some kind",
            Coin.FromGold(200));

        buyback.Patron.ShouldBe(patron);
        buyback.Description.ShouldBe("a wand of some kind");
        buyback.Offered.ShouldBe(Coin.FromGold(200));
    }

    [Fact]
    public void Open_trims_the_description()
    {
        Open("  a wand of some kind  ").Description.ShouldBe("a wand of some kind");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Open_refuses_a_blank_description(string description)
    {
        Should.Throw<ArgumentException>(
            () => Buyback.Open(
                BuybackId.New(),
                PatronId.New(),
                description,
                Coin.FromGold(200)));
    }

    [Fact]
    public void A_till_holding_exactly_the_offer_covers_it()
    {
        Receipt receipt = Open().Settle(Coin.FromGold(200), Clock).ShouldBeOk();

        receipt.Moved.ShouldBe(Coin.FromGold(200));
        receipt.At.ShouldBe(Noon);
    }

    [Fact]
    public void A_till_a_copper_short_cannot_cover_it()
    {
        Error error = Open()
                     .Settle(Coin.From(0, 199, 9, 9), Clock)
                     .ShouldBeErr();

        error.Code.Value.ShouldBe("vagrant.ordering.till_cannot_cover");
    }

    [Fact]
    public void The_receipt_records_the_offer_rather_than_what_the_till_held()
    {
        Receipt receipt = Open().Settle(Coin.FromGold(5000), Clock).ShouldBeOk();

        receipt.Moved.ShouldBe(Coin.FromGold(200));
    }

    [Fact]
    public void A_settled_buyback_cannot_be_settled_again()
    {
        Buyback buyback = Open();
        buyback.Settle(Coin.FromGold(200), Clock).ShouldBeOk();

        Error error = buyback.Settle(Coin.FromGold(200), Clock).ShouldBeErr();

        error.Code.Value.ShouldBe("vagrant.ordering.buyback_already_settled");
    }

    [Fact]
    public void A_till_that_could_not_cover_it_can_cover_it_later()
    {
        Buyback buyback = Open();
        buyback.Settle(Coin.FromGold(1), Clock).ShouldBeErr();

        buyback.Settle(Coin.FromGold(200), Clock).ShouldBeOk();
    }

    private static Buyback Open(string description = "a wand of some kind") =>
        Buyback.Open(
            BuybackId.New(),
            PatronId.New(),
            description,
            Coin.FromGold(200));
}
