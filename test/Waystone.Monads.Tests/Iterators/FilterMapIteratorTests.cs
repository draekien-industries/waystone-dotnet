namespace Waystone.Monads.Iterators;

using System;
using System.Collections.Generic;
using System.Linq;
using Options;
using Shouldly;
using Xunit;

public sealed class FilterMapIteratorTests
{
    [Fact]
    public void Given_IntegerSequence_When_FilterMapToEvenStrings_Then_ReturnsOnlyEvenNumberStrings()
    {
        // Given
        var source = new List<int> { 1, 2, 3, 4, 5, 6 };
        Iterator<int> iterator = source.IntoIter();

        // When
        List<string> result = iterator
            .FilterMap(x => x % 2 == 0 ? Option.Some($"Even: {x}") : Option.None<string>())
            .Collect()
            .ToList();

        // Then
        result.Count.ShouldBe(3);
        result.ShouldBe(new[] { "Even: 2", "Even: 4", "Even: 6" });
    }

    [Fact]
    public void Given_StringSequence_When_FilterMapByLength_Then_ReturnsProcessedStrings()
    {
        // Given
        var source = new List<string> { "a", "bb", "ccc", "dddd", "eeeee" };
        Iterator<string> iterator = source.IntoIter();

        // When
        List<string> result = iterator
            .FilterMap(s => s.Length > 2 ? Option.Some(s.ToUpper()) : Option.None<string>())
            .Collect()
            .ToList();

        // Then
        result.Count.ShouldBe(3);
        result.ShouldBe(new[] { "CCC", "DDDD", "EEEEE" });
    }

    [Fact]
    public void Given_MixedValues_When_FilterMapToNumbers_Then_ReturnsParsedNumbers()
    {
        // Given
        var source = new List<string> { "123", "not a number", "456", "789", "invalid" };
        Iterator<string> iterator = source.IntoIter();

        // When
        List<int> result = iterator
            .FilterMap(s => int.TryParse(s, out int value) ? Option.Some(value) : Option.None<int>())
            .Collect()
            .ToList();

        // Then
        result.Count.ShouldBe(3);
        result.ShouldBe(new[] { 123, 456, 789 });
    }

    [Fact]
    public void Given_EmptySequence_When_FilterMapping_Then_ReturnsEmptySequence()
    {
        // Given
        var source = new List<int>();
        Iterator<int> iterator = source.IntoIter();

        // When
        List<string> result = iterator
            .FilterMap(x => Option.Some(x.ToString()))
            .Collect()
            .ToList();

        // Then
        result.Count.ShouldBe(0);
        result.ShouldBeEmpty();
    }

    [Fact]
    public void Given_Sequence_When_FilterMappingToAllNone_Then_ReturnsEmptySequence()
    {
        // Given
        var source = new List<int> { 1, 2, 3, 4, 5 };
        Iterator<int> iterator = source.IntoIter();

        // When
        List<string> result = iterator
            .FilterMap(_ => Option.None<string>())
            .Collect()
            .ToList();

        // Then
        result.Count.ShouldBe(0);
        result.ShouldBeEmpty();
    }

    [Fact]
    public void Given_Sequence_When_FilterMappingToAllSome_Then_ReturnsAllMappedElements()
    {
        // Given
        var source = new List<int> { 1, 2, 3 };
        Iterator<int> iterator = source.IntoIter();

        // When
        List<string> result = iterator
            .FilterMap(x => Option.Some($"Item {x}"))
            .Collect()
            .ToList();

        // Then
        result.Count.ShouldBe(3);
        result.ShouldBe(new[] { "Item 1", "Item 2", "Item 3" });
    }

    [Fact]
    public void Given_FilterMapIterator_When_Next_Then_ReturnsFilterMappedItemsSequentially()
    {
        // Given
        var source = new List<int> { 1, 2, 3, 4, 5, 6 };
        var filterMapIterator = new FilterMapIterator<int, string>(
            source,
            x => x % 2 == 0 ? Option.Some($"Even: {x}") : Option.None<string>()
        );

        // When
        Option<string> first = filterMapIterator.Next();
        Option<string> second = filterMapIterator.Next();
        Option<string> third = filterMapIterator.Next();
        Option<string> fourth = filterMapIterator.Next();

        // Then
        first.IsSome.ShouldBeTrue();
        first.Unwrap().ShouldBe("Even: 2");

        second.IsSome.ShouldBeTrue();
        second.Unwrap().ShouldBe("Even: 4");

        third.IsSome.ShouldBeTrue();
        third.Unwrap().ShouldBe("Even: 6");

        fourth.IsNone.ShouldBeTrue();
    }

    [Fact]
    public void Given_FilterMapIterator_When_Disposed_Then_ShouldNotEnumerate()
    {
        // Given
        var source = new List<int> { 1, 2, 3, 4, 5, 6 };
        var filterMapIterator = new FilterMapIterator<int, string>(
            source,
            x => x % 2 == 0 ? Option.Some($"Even: {x}") : Option.None<string>()
        );

        // When
        filterMapIterator.Dispose();
        List<string> result = filterMapIterator.Collect().ToList();

        // Then
        result.Count.ShouldBe(0);
        result.ShouldBeEmpty();
    }

    [Fact]
    public void Given_FilterMapIterator_When_SizeHint_Then_ReturnsEstimatedSize()
    {
        // Given
        var source = new List<int> { 1, 2, 3, 4, 5, 6 };
        var filterMapIterator = new FilterMapIterator<int, string>(
            source,
            x => x % 2 == 0 ? Option.Some($"Even: {x}") : Option.None<string>()
        );

        // When
        (int Lower, Option<int> Upper) result = filterMapIterator.SizeHint();

        // Then
        // Size hint should reflect the source size, as filtering happens during iteration
        result.Lower.ShouldBeLessThanOrEqualTo(6);
        result.Upper.IsSome.ShouldBeTrue();
        result.Upper.Unwrap().ShouldBeLessThanOrEqualTo(6);
    }

    [Fact]
    public void Given_FilterMapIterator_When_UsingMoveNext_Then_ShouldIterateCorrectly()
    {
        // Given
        var source = new List<int> { 1, 2, 3, 4, 5 };
        var filterMapIterator = new FilterMapIterator<int, string>(
            source,
            x => x % 2 == 0 ? Option.Some($"Even: {x}") : Option.None<string>()
        );

        // When & Then
        filterMapIterator.MoveNext().ShouldBeTrue();
        filterMapIterator.Current.IsSome.ShouldBeTrue();
        filterMapIterator.Current.Unwrap().ShouldBe("Even: 2");

        filterMapIterator.MoveNext().ShouldBeTrue();
        filterMapIterator.Current.IsSome.ShouldBeTrue();
        filterMapIterator.Current.Unwrap().ShouldBe("Even: 4");

        filterMapIterator.MoveNext().ShouldBeFalse();
        filterMapIterator.Current.IsNone.ShouldBeTrue();
    }

    [Fact]
    public void Given_FilterMapIterator_When_CheckingAll_Then_ShouldEvaluateCorrectly()
    {
        // Given
        var source = new List<int> { 1, 2, 3, 4, 5, 6 };
        var filterMapIterator = new FilterMapIterator<int, string>(
            source,
            x => x % 2 == 0 ? Option.Some($"Even: {x}") : Option.None<string>()
        );

        // When & Then
        filterMapIterator.All(x => x.StartsWith("Even:")).ShouldBeTrue();

        // Reset with a different condition
        filterMapIterator = new FilterMapIterator<int, string>(
            source,
            x => Option.Some(x.ToString())
        );
        filterMapIterator.All(x => x.Length > 1).ShouldBeFalse(); // "1" has length 1
    }

    [Fact]
    public void Given_FilterMapIterator_When_CheckingAny_Then_ShouldEvaluateCorrectly()
    {
        // Given
        var source = new List<int> { 1, 2, 3, 4, 5 };
        var filterMapIterator = new FilterMapIterator<int, int>(
            source,
            x => x > 3 ? Option.Some(x * 2) : Option.None<int>()
        );

        // When & Then
        filterMapIterator.Any(x => x > 8).ShouldBeTrue(); // 5*2 = 10 > 8

        // Reset and test another condition
        filterMapIterator = new FilterMapIterator<int, int>(
            source,
            x => x < 3 ? Option.Some(x) : Option.None<int>()
        );
        filterMapIterator.Any(x => x > 5).ShouldBeFalse(); // no values > 5
    }

    [Fact]
    public void Given_FilterMapIterator_When_CountingElements_Then_ShouldReturnCorrectCount()
    {
        // Given
        var source = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8 };
        var filterMapIterator = new FilterMapIterator<int, string>(
            source,
            x => x % 2 == 0 ? Option.Some($"Even: {x}") : Option.None<string>()
        );

        // When
        int count = filterMapIterator.Count();

        // Then
        count.ShouldBe(4); // There are 4 even numbers
    }

    [Fact]
    public void Given_FilterMapIterator_When_ChainedWithFilterMap_Then_ShouldCombineOperations()
    {
        // Given
        var source = new List<string> { "1", "2", "3", "4", "abc" };
        Iterator<string> iterator = source.IntoIter();

        // When
        // First parse strings to integers, filtering out non-parsable ones
        // Then convert even numbers to descriptive strings
        List<string> result = iterator
            .FilterMap(s => int.TryParse(s, out int value) ? Option.Some(value) : Option.None<int>())
            .FilterMap(x => x % 2 == 0 ? Option.Some($"Even number: {x}") : Option.None<string>())
            .Collect()
            .ToList();

        // Then
        result.Count.ShouldBe(2);
        result.ShouldBe(new[] { "Even number: 2", "Even number: 4" });
    }

    [Fact]
    public void Given_FilterMapAndRegularMap_When_Combined_Then_AppliesOperationsInOrder()
    {
        // Given
        var source = new List<string> { "1", "2", "3", "4", "abc" };
        Iterator<string> iterator = source.IntoIter();

        // When
        // Parse strings to integers, then multiply all by 10
        List<int> result = iterator
            .FilterMap(s => int.TryParse(s, out int value) ? Option.Some(value) : Option.None<int>())
            .Map(x => x * 10)
            .Collect()
            .ToList();

        // Then
        result.Count.ShouldBe(4);
        result.ShouldBe(new[] { 10, 20, 30, 40 });
    }

    [Fact]
    public void Given_NullMapper_When_Constructing_Then_ThrowsArgumentNullException()
    {
        // Arrange
        var source = new[] { 1, 2, 3 };
        Func<int, Option<string>> mapper = null!;

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => source.IntoIter().FilterMap(mapper));
    }
}
