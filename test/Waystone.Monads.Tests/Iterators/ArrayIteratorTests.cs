namespace Waystone.Monads.Iterators;

using Shouldly;
using Waystone.Monads.Extensions;
using Waystone.Monads.Iterators.Extensions;
using Waystone.Monads.Options;
using Waystone.Monads.Primitives;
using Xunit;

public sealed class ArrayIteratorTests
{
    [Fact]
    public void GivenArrayIterator_WhenInvokingNext_ThenReturnSome()
    {
        // Arrange
        var items = new[] { 1, 2, 3, 4, 5 };
        var iter = items.IntoIter();

        // Act + Assert
        iter.Next().ShouldBeSomeValue(1);
        iter.Next().ShouldBeSomeValue(2);
        iter.Next().ShouldBeSomeValue(3);
        iter.Next().ShouldBeSomeValue(4);
        iter.Next().ShouldBeSomeValue(5);
        iter.Next().ShouldBeNone();
    }

    [Fact]
    public void GivenArrayIterator_WhenInvokingSizeHint_ThenReturnExpected()
    {
        // Arrange
        var items = new[] { 1, 2, 3, 4, 5 };
        var iter = items.IntoIter();

        // Act + Assert
        iter.SizeHint().ShouldBe(((PosInt)5, Option.Some<PosInt>(5)));

        iter.Next(); // Move to first item
        iter.SizeHint().ShouldBe(((PosInt)4, Option.Some<PosInt>(4)));

        iter.Next(); // Move to second item
        iter.SizeHint().ShouldBe(((PosInt)3, Option.Some<PosInt>(3)));

        iter.Next(); // Move to third item
        iter.SizeHint().ShouldBe(((PosInt)2, Option.Some<PosInt>(2)));

        iter.Next(); // Move to fourth item
        iter.SizeHint().ShouldBe(((PosInt)1, Option.Some<PosInt>(1)));

        iter.Next(); // Move to fifth item
        iter.SizeHint().ShouldBe(((PosInt)0, Option.None<PosInt>()));
    }
}
