namespace Waystone.Monads.Iterators;

using System;
using Shouldly;
using Waystone.Monads.Extensions;
using Waystone.Monads.Iterators.Extensions;
using Waystone.Monads.Iterators.Primitives;
using Waystone.Monads.Options;
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
}