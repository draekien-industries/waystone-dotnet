namespace Vagrant.Appraisal.Tests;

using Shouldly;
using Xunit;

public sealed class AuraTests
{
    [Fact]
    public void A_check_above_the_obscurity_reads_it()
    {
        new Aura(15).YieldsTo(new ArcanaCheck(16)).ShouldBeTrue();
    }

    [Fact]
    public void A_check_equal_to_the_obscurity_reads_it()
    {
        new Aura(15).YieldsTo(new ArcanaCheck(15)).ShouldBeTrue();
    }

    [Fact]
    public void A_check_one_below_the_obscurity_does_not()
    {
        new Aura(15).YieldsTo(new ArcanaCheck(14)).ShouldBeFalse();
    }

    [Fact]
    public void An_unobscured_aura_yields_to_a_check_of_zero()
    {
        new Aura(0).YieldsTo(new ArcanaCheck(0)).ShouldBeTrue();
    }

    [Fact]
    public void A_negative_check_reads_nothing_at_all()
    {
        new Aura(0).YieldsTo(new ArcanaCheck(-1)).ShouldBeFalse();
    }

    [Fact]
    public void The_most_obscure_aura_yields_to_nothing_an_examiner_can_roll()
    {
        new Aura(uint.MaxValue).YieldsTo(new ArcanaCheck(int.MaxValue)).ShouldBeFalse();
    }
}
