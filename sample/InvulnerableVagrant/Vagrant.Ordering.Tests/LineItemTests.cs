namespace Vagrant.Ordering.Tests;

using Shouldly;
using Vagrant.SharedKernel;
using Waystone.Monads.Results.Errors;
using Xunit;

// Asking and Floor are per unit, so a line of three at 50gp asking and 45gp floor is due
// 150gp and will not go below 135gp. The expectations below are that arithmetic, not a
// reading of what the code returned.
public sealed class LineItemTests
{
    private static readonly LineItemSubject Potion = new(Guid.CreateVersion7());

    [Fact]
    public void A_line_nobody_haggled_over_is_due_the_asking_price_times_the_quantity()
    {
        Line(3).Due.ShouldBe(Coin.FromGold(150));
    }

    [Fact]
    public void A_line_nobody_haggled_over_has_no_agreed_price()
    {
        Line(3).Agreed.ShouldBeNone();
    }

    [Fact]
    public void An_offer_at_the_floor_for_the_whole_line_is_taken()
    {
        LineItem line = Line(3);

        line.Agree(new Offer(Coin.FromGold(135))).ShouldBeOk();

        line.Due.ShouldBe(Coin.FromGold(135));
    }

    [Fact]
    public void An_offer_a_copper_under_the_floor_is_refused()
    {
        Error error = Line(3)
                     .Agree(new Offer(Coin.From(0, 134, 9, 9)))
                     .ShouldBeErr();

        error.Code.Value.ShouldBe("vagrant.ordering.offer_below_floor");
    }

    [Fact]
    public void A_refused_offer_leaves_the_line_at_the_asking_price()
    {
        LineItem line = Line(3);

        line.Agree(new Offer(Coin.FromGold(100))).ShouldBeErr();

        line.Agreed.ShouldBeNone();
        line.Due.ShouldBe(Coin.FromGold(150));
    }

    [Fact]
    public void The_floor_is_per_unit_so_a_bigger_line_has_a_higher_floor()
    {
        Line(1).Agree(new Offer(Coin.FromGold(45))).ShouldBeOk();
        Line(2).Agree(new Offer(Coin.FromGold(45))).ShouldBeErr();
    }

    [Fact]
    public void Haggling_again_replaces_the_last_figure()
    {
        LineItem line = Line(3);

        line.Agree(new Offer(Coin.FromGold(150))).ShouldBeOk();
        line.Agree(new Offer(Coin.FromGold(140))).ShouldBeOk();

        line.Due.ShouldBe(Coin.FromGold(140));
    }

    [Fact]
    public void An_agreed_price_equal_to_the_asking_price_is_not_the_same_as_no_agreement()
    {
        LineItem line = Line(3);

        line.Agree(new Offer(Coin.FromGold(150))).ShouldBeOk();

        line.Agreed.ShouldBeSomeValue(new AgreedPrice(Coin.FromGold(150)));
    }

    [Fact]
    public void A_line_for_none_of_something_is_refused()
    {
        Should.Throw<ArgumentOutOfRangeException>(
            () => LineItem.For(Potion, 0, Coin.FromGold(50), Coin.FromGold(45)));
    }

    private static LineItem Line(uint quantity) =>
        LineItem.For(Potion, quantity, Coin.FromGold(50), Coin.FromGold(45));
}
