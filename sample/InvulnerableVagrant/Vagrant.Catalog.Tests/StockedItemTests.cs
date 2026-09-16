namespace Vagrant.Catalog.Tests;

using Shouldly;
using Vagrant.SharedKernel;
using Waystone.Monads.Results.Errors;
using Xunit;

public sealed class StockedItemTests
{
    [Fact]
    public void Stock_keeps_what_it_was_given()
    {
        StockedItemId id = StockedItemId.New();

        StockedItem item = StockedItem.Stock(id, "Potion of Healing", Band(), 12);

        item.Id.ShouldBe(id);
        item.Name.ShouldBe("Potion of Healing");
        item.Band.ShouldBe(Band());
        item.OnHand.ShouldBe(12u);
    }

    [Fact]
    public void Stock_trims_the_name()
    {
        StockedItem item = Stocked("  Potion of Healing  ", 1);

        item.Name.ShouldBe("Potion of Healing");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Stock_refuses_a_blank_name(string name)
    {
        Should.Throw<ArgumentException>(
            () => StockedItem.Stock(StockedItemId.New(), name, Band(), 1));
    }

    [Fact]
    public void Stock_allows_a_line_the_shop_has_sold_out_of()
    {
        Stocked("Potion of Healing", 0).OnHand.ShouldBe(0u);
    }

    [Fact]
    public void Withdraw_takes_the_quantity_off_the_shelf()
    {
        StockedItem item = Stocked("Potion of Healing", 12);

        Withdrawal taken = item.Withdraw(4).ShouldBeOk();

        taken.Quantity.ShouldBe(4u);
        taken.Item.ShouldBe(item.Id);
        item.OnHand.ShouldBe(8u);
    }

    [Fact]
    public void Withdraw_carries_the_price_the_item_was_taken_at()
    {
        StockedItem item = Stocked("Potion of Healing", 12);

        item.Withdraw(1).ShouldBeOk().Band.ShouldBe(Band());
    }

    [Fact]
    public void Withdraw_of_everything_on_hand_leaves_the_shelf_empty()
    {
        StockedItem item = Stocked("Potion of Healing", 12);

        item.Withdraw(12).ShouldBeOk();

        item.OnHand.ShouldBe(0u);
    }

    [Fact]
    public void Withdraw_of_one_more_than_is_there_is_refused()
    {
        StockedItem item = Stocked("Potion of Healing", 12);

        Error error = item.Withdraw(13).ShouldBeErr();

        error.Code.Value.ShouldBe("vagrant.catalog.not_enough_on_hand");
    }

    [Fact]
    public void A_refused_withdrawal_leaves_the_shelf_alone()
    {
        StockedItem item = Stocked("Potion of Healing", 12);

        item.Withdraw(13).ShouldBeErr();

        item.OnHand.ShouldBe(12u);
    }

    [Fact]
    public void Withdraw_of_nothing_is_a_caller_that_has_not_decided()
    {
        StockedItem item = Stocked("Potion of Healing", 12);

        Should.Throw<ArgumentOutOfRangeException>(() => item.Withdraw(0));
    }

    private static StockedItem Stocked(string name, uint onHand) =>
        StockedItem.Stock(StockedItemId.New(), name, Band(), onHand);

    private static PriceBand Band() =>
        PriceBand.Between(Coin.FromGold(50), Coin.FromGold(45)).ShouldBeOk();
}
