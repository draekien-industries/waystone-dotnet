namespace Vagrant.Catalog.Tests;

using Shouldly;
using Vagrant.SharedKernel;
using Waystone.Monads.Results;
using Waystone.Monads.Results.Errors;
using Xunit;

// The prices here are Pumat's: a healing potion at 50gp with a floor of 45gp, and a
// bag of holding at 4000gp he will not discount at all.
public sealed class PriceBandTests
{
    [Fact]
    public void Between_keeps_both_prices_as_they_were_given()
    {
        PriceBand band = PriceBand
                        .Between(Coin.FromGold(50), Coin.FromGold(45))
                        .ShouldBeOk();

        band.AskingPrice.ShouldBe(Coin.FromGold(50));
        band.FloorPrice.ShouldBe(Coin.FromGold(45));
    }

    [Fact]
    public void Between_allows_a_floor_equal_to_the_asking_price()
    {
        Result<PriceBand, Error> band =
            PriceBand.Between(Coin.FromGold(4000), Coin.FromGold(4000));

        band.ShouldBeOk();
    }

    [Fact]
    public void Between_refuses_a_floor_one_copper_above_the_asking_price()
    {
        Coin asking = Coin.FromGold(50);
        Coin floor = asking + Coin.From(0, 0, 0, 1);

        Error error = PriceBand.Between(asking, floor).ShouldBeErr();

        error.Code.Value.ShouldBe("vagrant.catalog.floor_above_asking");
    }

    [Fact]
    public void Admits_an_offer_that_reaches_the_floor_exactly()
    {
        PriceBand band = Band();

        band.Admits(Coin.FromGold(45)).ShouldBeTrue();
    }

    [Fact]
    public void Admits_nothing_one_copper_short_of_the_floor()
    {
        PriceBand band = Band();

        band.Admits(Coin.From(0, 44, 9, 9)).ShouldBeFalse();
    }

    [Fact]
    public void Admits_an_offer_above_the_asking_price()
    {
        PriceBand band = Band();

        band.Admits(Coin.FromGold(60)).ShouldBeTrue();
    }

    [Fact]
    public void ToString_writes_one_price_when_the_shop_will_not_discount()
    {
        PriceBand fixedPrice = PriceBand
                              .Between(Coin.FromGold(4000), Coin.FromGold(4000))
                              .ShouldBeOk();

        fixedPrice.ToString().ShouldBe("400pp");
    }

    [Fact]
    public void ToString_writes_both_prices_when_there_is_room_to_haggle()
    {
        Band().ToString().ShouldBe("5pp (floor 4pp 5gp)");
    }

    private static PriceBand Band() =>
        PriceBand.Between(Coin.FromGold(50), Coin.FromGold(45)).ShouldBeOk();
}
