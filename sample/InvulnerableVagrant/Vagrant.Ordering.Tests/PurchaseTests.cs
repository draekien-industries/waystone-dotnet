namespace Vagrant.Ordering.Tests;

using Microsoft.Extensions.Time.Testing;
using Shouldly;
using Vagrant.SharedKernel;
using Waystone.Monads.Results.Errors;
using Xunit;

// Two lines: three potions at 50gp asking / 45gp floor, and one driftglobe at 750gp
// asking / 700gp floor. Untouched that totals 150 + 750 = 900gp.
public sealed class PurchaseTests
{
    private static readonly LineItemSubject Potion = new(Guid.CreateVersion7());
    private static readonly LineItemSubject Driftglobe = new(Guid.CreateVersion7());
    private static readonly DateTimeOffset Noon =
        new(2026, 9, 16, 12, 0, 0, TimeSpan.Zero);
    private static readonly FakeTimeProvider Clock = new(Noon);

    [Fact]
    public void A_purchase_nobody_haggled_over_totals_what_the_shop_asked()
    {
        Open().Total.ShouldBe(Coin.FromGold(900));
    }

    [Fact]
    public void An_agreed_line_totals_what_was_agreed()
    {
        Purchase purchase = Open();

        purchase.Agree(Potion, new Offer(Coin.FromGold(140))).ShouldBeOk();

        purchase.Total.ShouldBe(Coin.FromGold(890));
    }

    [Fact]
    public void Haggling_over_a_line_the_purchase_does_not_carry_says_so()
    {
        Error error = Open()
                     .Agree(new LineItemSubject(Guid.CreateVersion7()), Offer(1))
                     .ShouldBeErr();

        error.Code.Value.ShouldBe("vagrant.ordering.not_on_this_purchase");
    }

    [Fact]
    public void A_purchase_cannot_be_opened_on_nothing()
    {
        Error error = LineItems.Of([]).ShouldBeErr();

        error.Code.Value.ShouldBe("vagrant.ordering.no_line_items");
    }

    [Fact]
    public void A_purchase_cannot_carry_the_same_thing_twice()
    {
        Error error = LineItems.Of([Potions(1), Potions(2)]).ShouldBeErr();

        error.Code.Value.ShouldBe("vagrant.ordering.duplicate_line");
    }

    [Fact]
    public void Two_lines_for_different_things_are_not_a_duplicate()
    {
        LineItems lines = LineItems
                         .Of(
                              [
                                  Potions(1),
                                  LineItem.For(
                                      Driftglobe,
                                      1,
                                      Coin.FromGold(750),
                                      Coin.FromGold(700)),
                              ])
                         .ShouldBeOk();

        lines.All.Count.ShouldBe(2);
    }

    [Fact]
    public void Coin_short_of_the_total_is_refused()
    {
        Error error = Open()
                     .Settle(Coin.From(0, 899, 9, 9), Clock)
                     .ShouldBeErr();

        error.Code.Value.ShouldBe("vagrant.ordering.insufficient_coin");
    }

    [Fact]
    public void Coin_exactly_covering_the_total_settles_it()
    {
        Receipt receipt = Open().Settle(Coin.FromGold(900), Clock).ShouldBeOk();

        receipt.Moved.ShouldBe(Coin.FromGold(900));
        receipt.At.ShouldBe(Noon);
    }

    [Fact]
    public void The_receipt_records_the_total_rather_than_what_was_put_down()
    {
        Receipt receipt = Open()
                         .Settle(Coin.FromGold(1000), Clock)
                         .ShouldBeOk();

        receipt.Moved.ShouldBe(Coin.FromGold(900));
    }

    [Fact]
    public void A_settled_purchase_cannot_be_settled_again()
    {
        Purchase purchase = Open();
        purchase.Settle(Coin.FromGold(900), Clock).ShouldBeOk();

        Error error = purchase.Settle(Coin.FromGold(900), Clock).ShouldBeErr();

        error.Code.Value.ShouldBe("vagrant.ordering.purchase_already_settled");
    }

    [Fact]
    public void A_settled_purchase_cannot_be_haggled_over()
    {
        Purchase purchase = Open();
        purchase.Settle(Coin.FromGold(900), Clock).ShouldBeOk();

        Error error = purchase.Agree(Potion, Offer(140)).ShouldBeErr();

        error.Code.Value.ShouldBe("vagrant.ordering.purchase_already_settled");
    }

    [Fact]
    public void A_refused_settlement_leaves_the_purchase_open()
    {
        Purchase purchase = Open();
        purchase.Settle(Coin.FromGold(1), Clock).ShouldBeErr();

        purchase.Settle(Coin.FromGold(900), Clock).ShouldBeOk();
    }

    [Fact]
    public void Haggling_below_the_floor_leaves_the_total_alone()
    {
        Purchase purchase = Open();

        purchase.Agree(Potion, Offer(100)).ShouldBeErr();

        purchase.Total.ShouldBe(Coin.FromGold(900));
    }

    [Fact]
    public void A_purchase_keeps_the_patron_it_was_opened_for()
    {
        PatronId patron = PatronId.New();
        LineItems lines = LineItems.Of([Potions(3)]).ShouldBeOk();

        Purchase purchase = Purchase.Open(PurchaseId.New(), patron, lines);

        purchase.Patron.ShouldBe(patron);
        purchase.Lines.Count.ShouldBe(1);
    }

    private static Offer Offer(uint gold) => new(Coin.FromGold(gold));

    private static LineItem Potions(uint quantity) =>
        LineItem.For(Potion, quantity, Coin.FromGold(50), Coin.FromGold(45));

    private static Purchase Open()
    {
        LineItems lines = LineItems
                         .Of(
                              [
                                  Potions(3),
                                  LineItem.For(
                                      Driftglobe,
                                      1,
                                      Coin.FromGold(750),
                                      Coin.FromGold(700)),
                              ])
                         .ShouldBeOk();

        return Purchase.Open(PurchaseId.New(), PatronId.New(), lines);
    }
}
