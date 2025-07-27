namespace Waystone.Monads.Iterators;

using System.Collections.Generic;
using Options;
using Shouldly;
using Xunit;

public sealed class IteratorTests
{
    [Fact]
    public void Next_WithNonEmptyCollection_ReturnsItemsSequentially()
    {
        // Arrange
        var source = new List<string> { "first", "second", "third" };
        var iterator = new Iterator<string>(source);

        // Act & Assert
        Option<string> result = iterator.Next();
        result.IsSome.ShouldBeTrue();
        result.Unwrap().ShouldBe("first");
        result = iterator.Next();
        result.IsSome.ShouldBeTrue();
        result.Unwrap().ShouldBe("second");
        result = iterator.Next();
        result.IsSome.ShouldBeTrue();
        result.Unwrap().ShouldBe("third");
        result = iterator.Next();
        result.IsNone.ShouldBeTrue();
    }

    [Fact]
    public void GetEnumerator_WithNonEmptyCollection_EnumeratesAllItems()
    {
        // Arrange
        var source = new List<string> { "first", "second", "third" };
        var iterator = new Iterator<string>(source);
        var result = new List<string>();

        // Act
        foreach (Option<string> item in iterator)
        {
            item.IsSome.ShouldBeTrue();
            result.Add(item.Unwrap());
        }

        // Assert
        result.ShouldBe(source);
    }

    [Fact]
    public void SizeHint_WithNonEmptyCollection_ReturnsCorrectBounds()
    {
        // Arrange
        var source = new List<string> { "first", "second", "third" };
        var iterator = new Iterator<string>(source);

        // Act
        (int lower, Option<int> upper) = iterator.SizeHint();

        // Assert
        lower.ShouldBe(3);
        upper.IsSome.ShouldBeTrue();
        upper.Unwrap().ShouldBe(3);
    }

    [Fact]
    public void SizeHint_WithEmptyCollection_ReturnsZeroLowerAndNoneUpper()
    {
        // Arrange
        var source = new List<string>();
        var iterator = new Iterator<string>(source);

        // Act
        (int lower, Option<int> upper) = iterator.SizeHint();

        // Assert
        lower.ShouldBe(0);
        upper.IsNone.ShouldBeTrue();
    }

    [Fact]
    public void Dispose_AfterDisposal_NextReturnsNone()
    {
        // Arrange
        var source = new List<string> { "first", "second", "third" };
        var iterator = new Iterator<string>(source);

        // Act
        iterator.Dispose();

        // Assert
        iterator.Next().IsNone.ShouldBeTrue();
    }

    [Fact]
    public void Collect_WithNonEmptyCollection_ReturnsExpectedCollection()
    {
        // Arrange
        var source = new List<string> { "first", "second", "third" };
        var iterator = new Iterator<string>(source);

        // Act
        IEnumerable<string> result = iterator.Collect();

        // Assert
        result.ShouldBe(source);
    }

    [Fact]
    public void Collect_WithEmptyCollection_ReturnsEmptyCollection()
    {
        // Arrange
        var source = new List<string>();
        var iterator = new Iterator<string>(source);

        // Act
        IEnumerable<string> result = iterator.Collect();

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public void Collect_AfterNextInvoked_ReturnsExpectedCollection()
    {
        // Arrange
        var source = new List<string> { "first", "second", "third" };
        var iterator = new Iterator<string>(source);

        // Act
        Option<string> next = iterator.Next();
        next.IsSome.ShouldBeTrue();
        next.Unwrap().ShouldBe("first");

        IEnumerable<string> result = iterator.Collect();
        result.ShouldBe(new List<string> { "second", "third" });
    }
}
