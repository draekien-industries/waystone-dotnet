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

    [Fact]
    public void Given_EmptyCollection_When_Count_Then_ReturnsZero()
    {
        // Given
        var iterator = new Iterator<string>([]);

        // When
        int result = iterator.Count();

        // Then
        result.ShouldBe(0);
    }

    [Fact]
    public void Given_NonEmptyCollection_When_Count_Then_ReturnsCorrectCount()
    {
        // Given
        var source = new List<string> { "first", "second", "third" };
        var iterator = new Iterator<string>(source);

        // When
        int result = iterator.Count();

        // Then
        result.ShouldBe(3);
    }

    [Fact]
    public void
        Given_NonEmptyCollection_When_CountAfterNext_Then_ReturnsRemainingCount()
    {
        // Given
        var source = new List<string> { "first", "second", "third" };
        var iterator = new Iterator<string>(source);
        iterator.Next();

        // When
        int result = iterator.Count();

        // Then
        result.ShouldBe(2);
    }

    [Fact]
    public void Given_DisposedIterator_When_Count_Then_ReturnsZero()
    {
        // Given
        var source = new List<string> { "first", "second", "third" };
        var iterator = new Iterator<string>(source);
        iterator.Dispose();

        // When
        int result = iterator.Count();

        // Then
        result.ShouldBe(0);
    }

    [Fact]
    public void Given_TwoIdenticalSequences_When_Eq_Then_ReturnsTrue()
    {
        // Given
        var source = new List<int> { 1, 2, 3 };
        var other = new List<int> { 1, 2, 3 };
        var iterator = new Iterator<int>(source);

        // When
        bool result = iterator.Eq(other);

        // Then
        result.ShouldBeTrue();
    }

    [Fact]
    public void Given_TwoDifferentSequences_When_Eq_Then_ReturnsFalse()
    {
        // Given
        var source = new List<int> { 1, 2, 3 };
        var other = new List<int> { 1, 2, 4 };
        var iterator = new Iterator<int>(source);

        // When
        bool result = iterator.Eq(other);

        // Then
        result.ShouldBeFalse();
    }

    [Fact]
    public void Given_SequencesWithDifferentLengths_When_Eq_Then_ReturnsFalse()
    {
        // Given
        var source = new List<string> { "a", "b", "c" };
        var other = new List<string> { "a", "b" };
        var iterator = new Iterator<string>(source);

        // When
        bool result = iterator.Eq(other);

        // Then
        result.ShouldBeFalse();
    }

    [Fact]
    public void Given_EmptySequences_When_Eq_Then_ReturnsTrue()
    {
        // Given
        var source = new List<int>();
        var other = new List<int>();
        var iterator = new Iterator<int>(source);

        // When
        bool result = iterator.Eq(other);

        // Then
        result.ShouldBeTrue();
    }

    [Fact]
    public void Given_PartiallyConsumedIterator_When_Eq_Then_ComparesOriginalSource()
    {
        // Given
        var source = new List<int> { 1, 2, 3 };
        var other = new List<int> { 1, 2, 3 };
        var iterator = new Iterator<int>(source);

        // Consume one item
        iterator.Next();

        // When
        bool result = iterator.Eq(other);

        // Then
        // Should still compare the original source, not just remaining items
        result.ShouldBeTrue();
    }

    [Fact]
    public void Given_TwoIdenticalIterators_When_EqWithIterator_Then_ReturnsTrue()
    {
        // Given
        var source = new List<int> { 1, 2, 3 };
        var otherSource = new List<int> { 1, 2, 3 };
        var iterator = new Iterator<int>(source);
        var otherIterator = new Iterator<int>(otherSource);

        // When
        bool result = iterator.Eq(otherIterator);

        // Then
        result.ShouldBeTrue();
    }

    [Fact]
    public void Given_TwoDifferentIterators_When_EqWithIterator_Then_ReturnsFalse()
    {
        // Given
        var source = new List<int> { 1, 2, 3 };
        var otherSource = new List<int> { 1, 2, 4 };
        var iterator = new Iterator<int>(source);
        var otherIterator = new Iterator<int>(otherSource);

        // When
        bool result = iterator.Eq(otherIterator);

        // Then
        result.ShouldBeFalse();
    }

    [Fact]
    public void Given_IteratorsWithDifferentLengths_When_EqWithIterator_Then_ReturnsFalse()
    {
        // Given
        var source = new List<string> { "a", "b", "c" };
        var otherSource = new List<string> { "a", "b" };
        var iterator = new Iterator<string>(source);
        var otherIterator = new Iterator<string>(otherSource);

        // When
        bool result = iterator.Eq(otherIterator);

        // Then
        result.ShouldBeFalse();
    }

    [Fact]
    public void Given_EmptyIterators_When_EqWithIterator_Then_ReturnsTrue()
    {
        // Given
        var source = new List<int>();
        var otherSource = new List<int>();
        var iterator = new Iterator<int>(source);
        var otherIterator = new Iterator<int>(otherSource);

        // When
        bool result = iterator.Eq(otherIterator);

        // Then
        result.ShouldBeTrue();
    }

    [Fact]
    public void Given_PartiallyConsumedIterators_When_EqWithIterator_Then_ComparesOriginalSources()
    {
        // Given
        var source = new List<int> { 1, 2, 3 };
        var otherSource = new List<int> { 1, 2, 3 };
        var iterator = new Iterator<int>(source);
        var otherIterator = new Iterator<int>(otherSource);

        // Consume one item from first iterator
        iterator.Next();

        // Consume two items from second iterator
        otherIterator.Next();
        otherIterator.Next();

        // When
        bool result = iterator.Eq(otherIterator);

        // Then
        // Should compare the original sources, not just remaining items
        result.ShouldBeTrue();
    }

    [Fact]
    public void Given_DisposedIterator_When_EqWithIterator_Then_StillComparesOriginalSources()
    {
        // Given
        var source = new List<int> { 1, 2, 3 };
        var otherSource = new List<int> { 1, 2, 3 };
        var iterator = new Iterator<int>(source);
        var otherIterator = new Iterator<int>(otherSource);

        // Dispose the first iterator
        iterator.Dispose();

        // When
        bool result = iterator.Eq(otherIterator);

        // Then
        // Should still compare the original sources
        result.ShouldBeTrue();
    }
}
