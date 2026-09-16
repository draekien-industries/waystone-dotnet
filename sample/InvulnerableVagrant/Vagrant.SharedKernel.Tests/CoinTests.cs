namespace Vagrant.SharedKernel.Tests;

using Shouldly;
using Waystone.Monads.Options;
using Xunit;

// Every expected value here comes from the Dwendalian rate table, not from running
// Coin: 1pp = 10gp, 1gp = 10sp, 1sp = 10cp, so one gold is 100 copper and one
// platinum is 1000.
public sealed class CoinTests
{
    [Fact]
    public void From_sums_the_four_denominations_into_copper()
    {
        Coin coin = Coin.From(45, 1, 0, 3);

        coin.InCopper().ShouldBe((45 * 1000) + (1 * 100) + 3);
    }

    [Fact]
    public void From_accepts_components_that_are_not_reduced()
    {
        Coin twelveSilver = Coin.From(0, 0, 12, 0);
        Coin oneGoldTwoSilver = Coin.From(0, 1, 2, 0);

        twelveSilver.ShouldBe(oneGoldTwoSilver);
    }

    [Fact]
    public void FromGold_is_a_hundred_copper_to_the_gold()
    {
        Coin.FromGold(450).InCopper().ShouldBe(45_000);
    }

    [Theory]
    [InlineData(-1, 0, 0, 0)]
    [InlineData(0, -1, 0, 0)]
    [InlineData(0, 0, -1, 0)]
    [InlineData(0, 0, 0, -1)]
    public void From_rejects_a_negative_component(
        int platinum,
        int gold,
        int silver,
        int copper)
    {
        Should.Throw<ArgumentOutOfRangeException>(
            () => Coin.From(platinum, gold, silver, copper));
    }

    [Fact]
    public void FromGold_rejects_a_negative_amount()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => Coin.FromGold(-1));
    }

    [Fact]
    public void Nothing_is_zero_copper()
    {
        Coin.Nothing.InCopper().ShouldBe(0);
        Coin.Nothing.IsNothing.ShouldBeTrue();
    }

    [Fact]
    public void An_amount_with_coin_in_it_is_not_nothing()
    {
        Coin.From(0, 0, 0, 1).IsNothing.ShouldBeFalse();
    }

    [Fact]
    public void Addition_totals_both_amounts()
    {
        Coin sum = Coin.FromGold(4) + Coin.From(0, 0, 5, 0);

        sum.InCopper().ShouldBe(450);
    }

    [Fact]
    public void Multiplication_repeats_an_amount()
    {
        Coin line = Coin.FromGold(45) * 3;

        line.InCopper().ShouldBe(13_500);
    }

    [Fact]
    public void Multiplying_by_zero_leaves_nothing()
    {
        (Coin.FromGold(45) * 0).ShouldBe(Coin.Nothing);
    }

    [Fact]
    public void Multiplication_rejects_a_negative_quantity()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => Coin.FromGold(45) * -1);
    }

    [Fact]
    public void Less_leaves_the_difference()
    {
        Option<Coin> left = Coin.FromGold(450).Less(Coin.FromGold(400));

        left.ShouldBeSomeValue(Coin.FromGold(50));
    }

    [Fact]
    public void Less_of_an_equal_amount_leaves_nothing_rather_than_none()
    {
        Option<Coin> left = Coin.FromGold(450).Less(Coin.FromGold(450));

        left.ShouldBeSomeValue(Coin.Nothing);
    }

    [Fact]
    public void Less_of_a_larger_amount_is_none()
    {
        Coin.FromGold(400).Less(Coin.FromGold(450)).ShouldBeNone();
    }

    [Fact]
    public void An_amount_is_at_least_itself()
    {
        Coin.FromGold(450).IsAtLeast(Coin.FromGold(450)).ShouldBeTrue();
    }

    [Fact]
    public void A_larger_amount_is_at_least_a_smaller_one()
    {
        Coin.FromGold(450).IsAtLeast(Coin.FromGold(449)).ShouldBeTrue();
    }

    [Fact]
    public void A_smaller_amount_is_not_at_least_a_larger_one()
    {
        Coin.FromGold(449).IsAtLeast(Coin.FromGold(450)).ShouldBeFalse();
    }

    [Fact]
    public void CompareTo_orders_by_what_the_amount_is_worth()
    {
        Coin.FromGold(1).CompareTo(Coin.FromGold(2)).ShouldBeLessThan(0);
        Coin.FromGold(2).CompareTo(Coin.FromGold(1)).ShouldBeGreaterThan(0);
        Coin.FromGold(1).CompareTo(Coin.FromGold(1)).ShouldBe(0);
    }

    [Fact]
    public void Equal_amounts_are_neither_less_than_nor_greater_than()
    {
        Coin left = Coin.FromGold(450);
        Coin right = Coin.From(0, 450, 0, 0);

        (left < right).ShouldBeFalse();
        (left > right).ShouldBeFalse();
        (left <= right).ShouldBeTrue();
        (left >= right).ShouldBeTrue();
    }

    [Fact]
    public void A_smaller_amount_compares_below_a_larger_one()
    {
        Coin small = Coin.From(0, 0, 0, 99);
        Coin large = Coin.FromGold(1);

        (small < large).ShouldBeTrue();
        (small <= large).ShouldBeTrue();
        (small > large).ShouldBeFalse();
        (small >= large).ShouldBeFalse();
    }

    [Fact]
    public void FromCopper_reverses_InCopper()
    {
        Coin original = Coin.From(45, 1, 0, 3);

        Coin.FromCopper(original.InCopper()).ShouldBe(original);
    }

    [Fact]
    public void FromCopper_rejects_a_negative_total()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => Coin.FromCopper(-1));
    }

    [Fact]
    public void ToString_writes_the_largest_denominations_first_and_skips_the_empty_ones()
    {
        Coin.From(45, 1, 0, 3).ToString().ShouldBe("45pp 1gp 3cp");
    }

    [Fact]
    public void ToString_reduces_components_that_were_given_unreduced()
    {
        Coin.From(0, 0, 12, 0).ToString().ShouldBe("1gp 2sp");
    }

    [Fact]
    public void ToString_writes_a_single_denomination_alone()
    {
        Coin.From(0, 0, 0, 5).ToString().ShouldBe("5cp");
    }

    [Fact]
    public void ToString_of_nothing_still_names_a_price()
    {
        Coin.Nothing.ToString().ShouldBe("0gp");
    }
}
