namespace Waystone.Monads.Iterators;

using Shouldly;
using Waystone.Monads.Extensions;
using Waystone.Monads.Iterators.Extensions;
using Xunit;

public sealed class ChainIteratorTests
{
    [Fact]
    public void GivenTwoIteratorsThatHaveBeenChained_WhenTheFirstIteratorEnds_ThenIterateOverTheSecond()
    {
        var first = new[] { 1, 2, 3, 4 };
        var second = new[] { 5, 6, 7, 8 };

        var firstIter = first.IntoIter();
        var secondIter = second.IntoIter();

        var chained = firstIter.Chain(secondIter);

        chained.Next().ShouldBeSomeValue(1);
        chained.Nth(3).ShouldBeSomeValue(5);
        chained.Nth(3).ShouldBeNone();
    }

    [Fact]
    public void GivenThreeIteratorsThatHaveBeenChanged_ThenIterateOverAllThree()
    {
        var first = new[] { 1 };
        var second = new[] { 2 };
        var third = new[] { 3 };

        var chained = first.IntoIter()
            .Chain(second.IntoIter())
            .Chain(third.IntoIter());

        chained.Next().ShouldBeSomeValue(1);
        chained.Next().ShouldBeSomeValue(2);
        chained.Next().ShouldBeSomeValue(3);
        chained.Next().ShouldBeNone();
    }
}