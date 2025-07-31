namespace Waystone.Monads.Iterators;

using System.Collections.Generic;
using System.Linq;
using Options;
using Shouldly;
using Xunit;

public sealed class EnumerateIteratorTests
{
    [Fact]
    public void
        Given_NonEmptySequence_When_Enumerate_Then_ShouldPairWithIndices()
    {
        // Given
        var sequence = new List<string> { "a", "b", "c" };
        Iterator<string> iterator = sequence.IntoIter();

        // When
        List<(int Index, string Value)> result =
            iterator.Enumerate().Collect().ToList();

        // Then
        result.Count.ShouldBe(3);
        result[0].ShouldBe((0, "a"));
        result[1].ShouldBe((1, "b"));
        result[2].ShouldBe((2, "c"));
    }

    [Fact]
    public void
        Given_EmptySequence_When_Enumerate_Then_ShouldReturnEmptyCollection()
    {
        // Given
        var sequence = new List<string>();
        Iterator<string> iterator = sequence.IntoIter();

        // When
        List<(int Index, string Value)> result =
            iterator.Enumerate().Collect().ToList();

        // Then
        result.Count.ShouldBe(0);
    }

    [Fact]
    public void
        Given_EnumerateIterator_When_Next_Then_ShouldReturnItemsWithIndices()
    {
        // Given
        var sequence = new List<int> { 10, 20, 30 };
        var enumerateIterator = new EnumerateIterator<int>(sequence);

        // When
        Option<(int Index, int Value)> first = enumerateIterator.Next();
        Option<(int Index, int Value)> second = enumerateIterator.Next();
        Option<(int Index, int Value)> third = enumerateIterator.Next();
        Option<(int Index, int Value)> fourth = enumerateIterator.Next();

        // Then
        first.IsSome.ShouldBeTrue();
        first.Unwrap().ShouldBe((0, 10));

        second.IsSome.ShouldBeTrue();
        second.Unwrap().ShouldBe((1, 20));

        third.IsSome.ShouldBeTrue();
        third.Unwrap().ShouldBe((2, 30));

        fourth.IsNone.ShouldBeTrue();
    }

    [Fact]
    public void Given_EnumerateIterator_When_Disposed_Then_ShouldNotEnumerate()
    {
        // Given
        var sequence = new List<string> { "a", "b", "c" };
        var enumerateIterator = new EnumerateIterator<string>(sequence);

        // When
        enumerateIterator.Dispose();
        List<(int Index, string Value)> result =
            enumerateIterator.Collect().ToList();

        // Then
        result.Count.ShouldBe(0);
    }

    [Fact]
    public void
        Given_EnumerateIterator_When_SizeHint_Then_ShouldReturnCorrectSize()
    {
        // Given
        var sequence = new List<int> { 1, 2, 3, 4, 5 };
        var enumerateIterator = new EnumerateIterator<int>(sequence);

        // When
        (int Lower, Option<int> Upper) result = enumerateIterator.SizeHint();

        // Then
        result.Lower.ShouldBe(5);
        result.Upper.IsSome.ShouldBeTrue();
        result.Upper.Unwrap().ShouldBe(5);

        // When consuming an element
        enumerateIterator.Next();
        result = enumerateIterator.SizeHint();

        // Then
        result.Lower.ShouldBe(4);
        result.Upper.IsSome.ShouldBeTrue();
        result.Upper.Unwrap().ShouldBe(4);
    }

    [Fact]
    public void
        Given_EnumerateIterator_When_UsingMoveNext_Then_ShouldIterateCorrectly()
    {
        // Given
        var sequence = new List<string> { "a", "b", "c" };
        var enumerateIterator = new EnumerateIterator<string>(sequence);

        // When & Then
        enumerateIterator.MoveNext().ShouldBeTrue();
        enumerateIterator.Current.IsSome.ShouldBeTrue();
        enumerateIterator.Current.Unwrap().ShouldBe((0, "a"));

        enumerateIterator.MoveNext().ShouldBeTrue();
        enumerateIterator.Current.IsSome.ShouldBeTrue();
        enumerateIterator.Current.Unwrap().ShouldBe((1, "b"));

        enumerateIterator.MoveNext().ShouldBeTrue();
        enumerateIterator.Current.IsSome.ShouldBeTrue();
        enumerateIterator.Current.Unwrap().ShouldBe((2, "c"));

        enumerateIterator.MoveNext().ShouldBeFalse();
        enumerateIterator.Current.IsNone.ShouldBeTrue();
    }

    [Fact]
    public void
        Given_EnumerateIterator_When_FilteringWithAll_Then_ShouldEvaluateCorrectly()
    {
        // Given
        var sequence = new List<int> { 1, 2, 3, 4 };
        var enumerateIterator = new EnumerateIterator<int>(sequence);

        // When & Then
        // Check if all indices are less than the values
        enumerateIterator.All(pair => pair.Index < pair.Value).ShouldBeTrue();

        // Reset and check another condition
        enumerateIterator = new EnumerateIterator<int>(sequence);
        // Check if all indices are equal to the values (should be false)
        enumerateIterator.All(pair => pair.Index == pair.Value).ShouldBeFalse();
    }

    [Fact]
    public void
        Given_EnumerateIterator_When_CheckingAny_Then_ShouldEvaluateCorrectly()
    {
        // Given
        var sequence = new List<int> { 1, 2, 3, 4 };
        var enumerateIterator = new EnumerateIterator<int>(sequence);

        // When & Then
        // Check if any pair has index equal to the value
        enumerateIterator.Any(pair => pair.Index == pair.Value - 1)
                         .ShouldBeTrue(); // true for (1,1)

        // Reset and check another condition
        enumerateIterator = new EnumerateIterator<int>(sequence);
        // Check if any index is greater than its value (should be false)
        enumerateIterator.Any(pair => pair.Index > pair.Value).ShouldBeFalse();
    }

    [Fact]
    public void
        Given_EnumerateIterator_When_CountingElements_Then_ShouldReturnCorrectCount()
    {
        // Given
        var sequence = new List<string> { "a", "b", "c", "d" };
        var enumerateIterator = new EnumerateIterator<string>(sequence);

        // When
        int count = enumerateIterator.Count();

        // Then
        count.ShouldBe(4);
    }
}
