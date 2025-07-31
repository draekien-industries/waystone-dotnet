namespace Waystone.Monads.Iterators;

using System.Collections.Generic;
using System.Linq;
using Options;
using Shouldly;
using Xunit;

public sealed class FilterIteratorTests
{
    [Fact]
    public void
        Given_IntegerSequence_When_FilteringEvens_Then_ReturnsOnlyEvenNumbers()
    {
        // Given
        var source = new List<int> { 1, 2, 3, 4, 5, 6 };
        Iterator<int> iterator = source.IntoIter();

        // When
        List<int> result = iterator.Filter(x => x % 2 == 0).Collect().ToList();

        // Then
        result.Count.ShouldBe(3);
        result.ShouldBe(new[] { 2, 4, 6 });
    }

    [Fact]
    public void
        Given_StringSequence_When_FilteringByLength_Then_ReturnsMatchingStrings()
    {
        // Given
        var source = new List<string> { "a", "bb", "ccc", "dddd", "eeeee" };
        Iterator<string> iterator = source.IntoIter();

        // When
        List<string> result =
            iterator.Filter(s => s.Length > 2).Collect().ToList();

        // Then
        result.Count.ShouldBe(3);
        result.ShouldBe(new[] { "ccc", "dddd", "eeeee" });
    }

    [Fact]
    public void Given_EmptySequence_When_Filtering_Then_ReturnsEmptySequence()
    {
        // Given
        var source = new List<int>();
        Iterator<int> iterator = source.IntoIter();

        // When
        List<int> result = iterator.Filter(x => x > 0).Collect().ToList();

        // Then
        result.Count.ShouldBe(0);
        result.ShouldBeEmpty();
    }

    [Fact]
    public void Given_Sequence_When_FilteringAll_Then_ReturnsEmptySequence()
    {
        // Given
        var source = new List<int> { 1, 2, 3, 4, 5 };
        Iterator<int> iterator = source.IntoIter();

        // When
        List<int> result = iterator.Filter(x => x > 10).Collect().ToList();

        // Then
        result.Count.ShouldBe(0);
        result.ShouldBeEmpty();
    }

    [Fact]
    public void Given_Sequence_When_FilteringNone_Then_ReturnsAllElements()
    {
        // Given
        var source = new List<int> { 1, 2, 3, 4, 5 };
        Iterator<int> iterator = source.IntoIter();

        // When
        List<int> result = iterator.Filter(x => true).Collect().ToList();

        // Then
        result.Count.ShouldBe(5);
        result.ShouldBe(new[] { 1, 2, 3, 4, 5 });
    }

    [Fact]
    public void
        Given_FilterIterator_When_Next_Then_ReturnsFilteredItemsSequentially()
    {
        // Given
        var source = new List<int> { 1, 2, 3, 4, 5, 6 };
        var filterIterator = new FilterIterator<int>(source, x => x % 2 == 0);

        // When
        Option<int> first = filterIterator.Next();
        Option<int> second = filterIterator.Next();
        Option<int> third = filterIterator.Next();
        Option<int> fourth = filterIterator.Next();

        // Then
        first.IsSome.ShouldBeTrue();
        first.Unwrap().ShouldBe(2);

        second.IsSome.ShouldBeTrue();
        second.Unwrap().ShouldBe(4);

        third.IsSome.ShouldBeTrue();
        third.Unwrap().ShouldBe(6);

        fourth.IsNone.ShouldBeTrue();
    }

    [Fact]
    public void Given_FilterIterator_When_Disposed_Then_ShouldNotEnumerate()
    {
        // Given
        var source = new List<int> { 1, 2, 3, 4, 5, 6 };
        var filterIterator = new FilterIterator<int>(source, x => x % 2 == 0);

        // When
        filterIterator.Dispose();
        List<int> result = filterIterator.Collect().ToList();

        // Then
        result.Count.ShouldBe(0);
        result.ShouldBeEmpty();
    }

    [Fact]
    public void Given_FilterIterator_When_SizeHint_Then_ReturnsEstimatedSize()
    {
        // Given
        var source = new List<int> { 1, 2, 3, 4, 5, 6 };
        var filterIterator = new FilterIterator<int>(source, x => x % 2 == 0);

        // When
        (int Lower, Option<int> Upper) result = filterIterator.SizeHint();

        // Then
        // Lower bound should be at most the original count
        result.Lower.ShouldBeLessThanOrEqualTo(6);
        result.Upper.IsSome.ShouldBeTrue();
        result.Upper.Unwrap().ShouldBeLessThanOrEqualTo(6);
    }

    [Fact]
    public void
        Given_FilterIterator_When_UsingMoveNext_Then_ShouldIterateCorrectly()
    {
        // Given
        var source = new List<int> { 1, 2, 3, 4, 5 };
        var filterIterator = new FilterIterator<int>(source, x => x % 2 == 0);

        // When & Then
        filterIterator.MoveNext().ShouldBeTrue();
        filterIterator.Current.IsSome.ShouldBeTrue();
        filterIterator.Current.Unwrap().ShouldBe(2);

        filterIterator.MoveNext().ShouldBeTrue();
        filterIterator.Current.IsSome.ShouldBeTrue();
        filterIterator.Current.Unwrap().ShouldBe(4);

        filterIterator.MoveNext().ShouldBeFalse();
        filterIterator.Current.IsNone.ShouldBeTrue();
    }

    [Fact]
    public void
        Given_FilterIterator_When_CheckingAll_Then_ShouldEvaluateCorrectly()
    {
        // Given
        var source = new List<int> { 1, 2, 3, 4, 5, 6 };
        var filterIterator = new FilterIterator<int>(source, x => x % 2 == 0);

        // When & Then
        filterIterator.All(x => x % 2 == 0).ShouldBeTrue();

        // Reset with a different condition
        filterIterator = new FilterIterator<int>(source, x => x > 1);
        filterIterator.All(x => x % 2 == 0).ShouldBeFalse();
    }

    [Fact]
    public void
        Given_FilterIterator_When_CheckingAny_Then_ShouldEvaluateCorrectly()
    {
        // Given
        var source = new List<int> { 1, 2, 3, 4, 5 };
        var filterIterator = new FilterIterator<int>(source, x => x > 3);

        // When & Then
        filterIterator.Any(x => x % 2 == 0).ShouldBeTrue(); // 4 is even

        // Reset and test another condition
        filterIterator = new FilterIterator<int>(source, x => x < 2);
        filterIterator.Any(x => x > 5).ShouldBeFalse(); // no values > 5
    }

    [Fact]
    public void
        Given_FilterIterator_When_CountingElements_Then_ShouldReturnCorrectCount()
    {
        // Given
        var source = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8 };
        var filterIterator = new FilterIterator<int>(source, x => x % 2 == 0);

        // When
        int count = filterIterator.Count();

        // Then
        count.ShouldBe(4); // There are 4 even numbers
    }

    [Fact]
    public void
        Given_FilterIterator_When_ChainedWithAnother_Then_ShouldCombineFiltered()
    {
        // Given
        var source = new List<int> { 1, 2, 3, 4, 5 };
        var other = new List<int> { 6, 7, 8, 9, 10 };

        // Filter to get even numbers from first source
        var filterIterator = new FilterIterator<int>(source, x => x % 2 == 0);

        // When
        // Chain with second source and filter for numbers > 5
        List<int> result = filterIterator.Chain(other)
                                         .Filter(x => x > 5)
                                         .Collect()
                                         .ToList();

        // Then
        result.Count.ShouldBe(other.Count);
        result.ShouldBe(other);
    }

    [Fact]
    public void Given_NestedFilters_When_Collecting_Then_AppliesBothFilters()
    {
        // Given
        var source = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        Iterator<int> iterator = source.IntoIter();

        // When
        // Filter for even numbers and then for numbers greater than 5
        List<int> result = iterator
                          .Filter(x => x % 2 == 0) // 2, 4, 6, 8, 10
                          .Filter(x => x > 5)      // 6, 8, 10
                          .Collect()
                          .ToList();

        // Then
        result.Count.ShouldBe(3);
        result.ShouldBe(new[] { 6, 8, 10 });
    }

    [Fact]
    public void Given_FilteredIterator_When_Mapped_Then_AppliesFilterThenMap()
    {
        // Given
        var source = new List<int> { 1, 2, 3, 4, 5 };
        Iterator<int> iterator = source.IntoIter();

        // When
        // Filter for even numbers, then map to strings
        List<string> result = iterator
                             .Filter(x => x % 2 == 0)
                             .Map(x => $"Number {x}")
                             .Collect()
                             .ToList();

        // Then
        result.Count.ShouldBe(2);
        result.ShouldBe(new[] { "Number 2", "Number 4" });
    }
}
