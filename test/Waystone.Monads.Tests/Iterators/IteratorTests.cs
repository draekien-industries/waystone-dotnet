namespace Waystone.Monads.Iterators;

using System.Collections.Generic;
using Options;
using Shouldly;
using Xunit;

public sealed class IteratorTests
{
    [Fact]
    public void
        Given_NonEmptyCollection_When_Next_Then_ReturnsItemsSequentially()
    {
        // Given
        var source = new List<string> { "first", "second", "third" };
        var iterator = new Iterator<string>(source);

        // When
        Option<string> result = iterator.Next();

        // Then
        result.IsSome.ShouldBeTrue();
        result.Unwrap().ShouldBe("first");

        // When
        result = iterator.Next();

        // Then
        result.IsSome.ShouldBeTrue();
        result.Unwrap().ShouldBe("second");

        // When
        result = iterator.Next();

        // Then
        result.IsSome.ShouldBeTrue();
        result.Unwrap().ShouldBe("third");

        // When
        result = iterator.Next();

        // Then
        result.IsNone.ShouldBeTrue();
    }

    [Fact]
    public void
        Given_NonEmptyCollection_When_GetEnumerator_Then_EnumeratesAllItems()
    {
        // Given
        var source = new List<string> { "first", "second", "third" };
        var iterator = new Iterator<string>(source);
        var result = new List<string>();

        // When
        foreach (Option<string> item in iterator)
        {
            item.IsSome.ShouldBeTrue();
            result.Add(item.Unwrap());
        }

        // Then
        result.ShouldBe(source);
    }

    [Fact]
    public void
        Given_NonEmptyCollection_When_SizeHint_Then_ReturnsCorrectBounds()
    {
        // Given
        var source = new List<string> { "first", "second", "third" };
        var iterator = new Iterator<string>(source);

        // When
        (int lower, Option<int> upper) = iterator.SizeHint();

        // Then
        lower.ShouldBe(3);
        upper.IsSome.ShouldBeTrue();
        upper.Unwrap().ShouldBe(3);
    }

    [Fact]
    public void
        Given_EmptyCollection_When_SizeHint_Then_ReturnsZeroLowerAndNoneUpper()
    {
        // Given
        var iterator = new Iterator<string>([]);

        // When
        (int lower, Option<int> upper) = iterator.SizeHint();

        // Then
        lower.ShouldBe(0);
        upper.IsNone.ShouldBeTrue();
    }

    [Fact]
    public void Given_NonEmptyCollection_When_Dispose_Then_NextReturnsNone()
    {
        // Given
        var source = new List<string> { "first", "second", "third" };
        var iterator = new Iterator<string>(source);

        // When
        iterator.Dispose();

        // Then
        iterator.Next().IsNone.ShouldBeTrue();
    }

    [Fact]
    public void
        Given_NonEmptyCollection_When_Collect_Then_ReturnsExpectedCollection()
    {
        // Given
        var source = new List<string> { "first", "second", "third" };
        var iterator = new Iterator<string>(source);

        // When
        IEnumerable<string> result = iterator.Collect();

        // Then
        result.ShouldBe(source);
    }

    [Fact]
    public void Given_EmptyCollection_When_Collect_Then_ReturnsEmptyCollection()
    {
        // Given
        var iterator = new Iterator<string>([]);

        // When
        IEnumerable<string> result = iterator.Collect();

        // Then
        result.ShouldBeEmpty();
    }

    [Fact]
    public void
        Given_NonEmptyCollection_When_CollectAfterNext_Then_ReturnsRemainingItems()
    {
        // Given
        var source = new List<string> { "first", "second", "third" };
        var iterator = new Iterator<string>(source);

        // When
        Option<string> next = iterator.Next();

        // Then
        next.IsSome.ShouldBeTrue();
        next.Unwrap().ShouldBe("first");

        // When
        IEnumerable<string> result = iterator.Collect();

        // Then
        result.ShouldBe(new List<string> { "second", "third" });
    }

    [Fact]
    public void Given_EmptyCollection_When_All_Then_ReturnsTrue()
    {
        // Given
        var iterator = new Iterator<string>([]);

        // When
        bool result = iterator.All(x => x.StartsWith("test"));

        // Then
        result.ShouldBeTrue();
    }

    [Fact]
    public void
        Given_NonEmptyCollection_When_AllPredicateMatches_Then_ReturnsTrue()
    {
        // Given
        var source = new List<string> { "test1", "test2", "test3" };
        var iterator = new Iterator<string>(source);

        // When
        bool result = iterator.All(x => x.StartsWith("test"));

        // Then
        result.ShouldBeTrue();
    }

    [Fact]
    public void
        Given_NonEmptyCollection_When_AllWithEarlyMismatch_Then_ReturnsFalse()
    {
        // Given
        var source = new List<string> { "test1", "fail", "test3" };
        var iterator = new Iterator<string>(source);

        // When
        bool result = iterator.All(x => x.StartsWith("test"));

        // Then
        result.ShouldBeFalse();
    }

    [Fact]
    public void Given_DisposedIterator_When_All_Then_ReturnsTrue()
    {
        // Given
        var source = new List<string> { "test1", "test2", "test3" };
        var iterator = new Iterator<string>(source);
        iterator.Dispose();

        // When
        bool result = iterator.All(x => x.StartsWith("test"));

        // Then
        result.ShouldBeTrue();
    }

    [Fact]
    public void Given_EmptyCollection_When_Any_Then_ReturnsFalse()
    {
        // Given
        var iterator = new Iterator<string>([]);

        // When
        bool result = iterator.Any(x => x.StartsWith("test"));

        // Then
        result.ShouldBeFalse();
    }

    [Fact]
    public void
        Given_NonEmptyCollection_When_AnyPredicateMatches_Then_ReturnsTrue()
    {
        // Given
        var source = new List<string> { "test1", "test2", "test3" };
        var iterator = new Iterator<string>(source);

        // When
        bool result = iterator.Any(x => x.StartsWith("test"));

        // Then
        result.ShouldBeTrue();
    }

    [Fact]
    public void
        Given_NonEmptyCollection_When_AnyWithEarlyMatch_Then_ReturnsTrue()
    {
        // Given
        var source = new List<string> { "other1", "test2", "other3" };
        var iterator = new Iterator<string>(source);

        // When
        bool result = iterator.Any(x => x.StartsWith("test"));

        // Then
        result.ShouldBeTrue();
    }

    [Fact]
    public void Given_DisposedIterator_When_Any_Then_ReturnsFalse()
    {
        // Given
        var source = new List<string> { "test1", "test2", "test3" };
        var iterator = new Iterator<string>(source);
        iterator.Dispose();

        // When
        bool result = iterator.Any(x => x.StartsWith("test"));

        // Then
        result.ShouldBeFalse();
    }
}
