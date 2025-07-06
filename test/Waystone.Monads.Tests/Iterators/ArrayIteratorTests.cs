namespace Waystone.Monads.Iterators;

using Waystone.Monads.Extensions;
using Waystone.Monads.Iterators.Extensions;
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
}