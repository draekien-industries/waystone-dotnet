namespace Waystone.Monads.Iterators;

using System;
using System.Linq;
using Shouldly;
using Waystone.Monads.Extensions;
using Waystone.Monads.Iterators.Extensions;
using Waystone.Monads.Options;
using Waystone.Monads.Primitives;
using Xunit;

public sealed class StepByIteratorTests
{
    [Fact]
    public void GivenStepGreaterThanZero_WhenInvokingNext_ThenIterateByStep()
    {
        // Arrange
        var items = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        int step = 3;
        var iter = items.IntoIter().StepBy(step);

        // Act + Assert
        iter.Next().ShouldBeSomeValue(1);
        iter.Next().ShouldBeSomeValue(4);
        iter.Next().ShouldBeSomeValue(7);
        iter.Next().ShouldBeSomeValue(10);
        iter.Next().ShouldBeNone();
    }

    [Fact]
    public void GivenStepIsZero_WhenInvokingStepBy_ThenThrowException()
    {
        // Arrange
        var items = new[] { 1, 2, 3 };
        int step = 0;

        Should.Throw<ArgumentOutOfRangeException>(() => items.IntoIter().StepBy(step));
    }

    [Fact]
    public void GivenStepGreaterThanZero_WhenInvokingSizeHint_ThenReturnExpectedHints()
    {
        // Arrange
        var items = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        int step = 3;
        var iter = items.IntoIter().StepBy(step);

        iter.SizeHint().ShouldBe(((PosInt)4, Option.Some<PosInt>(4)));

        iter.Next();
        iter.SizeHint().ShouldBe(((PosInt)3, Option.Some<PosInt>(3)));

        iter.Next();
        iter.SizeHint().ShouldBe(((PosInt)2, Option.Some<PosInt>(2)));

        iter.Next();
        iter.SizeHint().ShouldBe(((PosInt)1, Option.Some<PosInt>(1)));

        iter.Next();
        iter.SizeHint().ShouldBe(((PosInt)0, Option.None<PosInt>()));
    }

    [Fact]
    public void WhenInvokingNth_ThenReturnExpected()
    {
        // Arrange
        var items = Enumerable.Range(0, 100).Select(i => i + 1);
        int step = 3;
        var iter = items.IntoIter().StepBy(step);

        // Act + Assert
        iter.Nth(0).ShouldBeSomeValue(1);
        iter.Nth(0).ShouldBeSomeValue(4);
        iter.Nth(1).ShouldBeSomeValue(10);
        iter.Nth(6).ShouldBeSomeValue(31);
        iter.Nth(30).ShouldBeNone();
    }

    [Fact]
    public void GivenStepBy_WhenInvokingStepBy_ThenReturnExpected()
    {
        // Arrange
        var items = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        int step = 2;
        var iter = items.IntoIter().StepBy(step);
        iter.Next().ShouldBeSomeValue(1);
        iter.Next().ShouldBeSomeValue(3);

        // Act
        var stepByIter = iter.StepBy(2);

        // Assert
        stepByIter.Next().ShouldBeSomeValue(5);
        stepByIter.Next().ShouldBeSomeValue(9);
        stepByIter.Next().ShouldBeNone();
    }
}
