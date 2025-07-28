namespace Waystone.Monads.Iterators;

using System.Collections.Generic;
using System.Linq;
using Options;
using Shouldly;
using Xunit;

public sealed class ChainIteratorTests
{
    [Fact]
    public void Given_TwoSequences_When_ChainingThem_Then_ShouldCombineInOrder()
    {
        // Given
        var first = new List<int> { 1, 2, 3 };
        var second = new List<int> { 4, 5, 6 };
        Iterator<int> iterator = first.IntoIter();

        // When
        List<int> result = iterator.Chain(second).Collect().ToList();

        // Then
        result.Count.ShouldBe(6);
        result.ShouldBe(new[] { 1, 2, 3, 4, 5, 6 });
    }

    [Fact]
    public void
        Given_NonEmptyAndEmptySequence_When_ChainingThem_Then_ShouldReturnOriginalSequence()
    {
        // Given
        var first = new List<int> { 1, 2, 3 };
        var second = new List<int>();
        Iterator<int> iterator = first.IntoIter();

        // When
        List<int> result = iterator.Chain(second).Collect().ToList();

        // Then
        result.Count.ShouldBe(3);
        result.ShouldBe(new[] { 1, 2, 3 });
    }

    [Fact]
    public void
        Given_EmptyAndNonEmptySequence_When_ChainingThem_Then_ShouldReturnSecondSequence()
    {
        // Given
        var first = new List<int>();
        var second = new List<int> { 1, 2, 3 };
        Iterator<int> iterator = first.IntoIter();

        // When
        List<int> result = iterator.Chain(second).Collect().ToList();

        // Then
        result.Count.ShouldBe(3);
        result.ShouldBe(new[] { 1, 2, 3 });
    }

    [Fact]
    public void Given_Chain_When_Disposed_Then_ShouldNotEnumerate()
    {
        // Given
        var first = new List<int> { 1, 2 };
        var second = new List<int> { 3, 4 };
        Iterator<int> iterator = first.IntoIter();

        // When
        ChainIterator<int> chainIterator = iterator.Chain(second);
        chainIterator.Dispose();
        List<int> result = chainIterator.Collect().ToList();

        // Then
        result.Count.ShouldBe(0);
    }

    [Fact]
    public void
        Given_AllEvenNumbers_When_CheckingAllElements_Then_ShouldReturnTrue()
    {
        // Given
        var first = new List<int> { 2, 4 };
        var second = new List<int> { 6, 8 };
        Iterator<int> iterator = first.IntoIter();

        // When
        bool result = iterator.Chain(second).All(x => x % 2 == 0);

        // Then
        result.ShouldBeTrue();
    }

    [Fact]
    public void
        Given_AllOddNumbers_When_CheckingForEvenNumbers_Then_ShouldReturnFalse()
    {
        // Given
        var first = new List<int> { 1, 3 };
        var second = new List<int> { 5, 7 };
        Iterator<int> iterator = first.IntoIter();

        // When
        bool result = iterator.Chain(second).All(x => x % 2 == 0);

        // Then
        result.ShouldBeFalse();
    }

    [Fact]
    public void
        Given_MixedNumbers_When_CheckingForEvenNumbers_Then_ShouldReturnFalse()
    {
        // Given
        var first = new List<int> { 2, 3 };
        var second = new List<int> { 4, 5 };
        Iterator<int> iterator = first.IntoIter();

        // When
        bool result = iterator.Chain(second).All(x => x % 2 == 0);

        // Then
        result.ShouldBeFalse();
    }

    [Fact]
    public void Given_DisposedChain_When_CheckingAll_Then_ShouldReturnTrue()
    {
        // Given
        var first = new List<int> { 1, 2 };
        var second = new List<int> { 3, 4 };
        Iterator<int> iterator = first.IntoIter();
        ChainIterator<int> chainIterator = iterator.Chain(second);

        // When
        chainIterator.Dispose();
        bool result = chainIterator.All(x => x > 0);

        // Then
        result.ShouldBeTrue();
    }

    [Fact]
    public void
        Given_NonEmptyCollection_When_SizeHint_Then_ShouldReturnSizeOfCombinedChain()
    {
        var first = new List<int> { 1, 2, 3 };
        var second = new List<int> { 3, 4 };

        Iterator<int> iterator = first.IntoIter();
        ChainIterator<int> chainIterator = iterator.Chain(second);

        (int Lower, Option<int> Upper) result = chainIterator.SizeHint();

        result.Lower.ShouldBe(5);
        result.Upper.IsSome.ShouldBeTrue();
        result.Upper.Unwrap().ShouldBe(5);

        chainIterator.Next();

        result = chainIterator.SizeHint();
        result.Lower.ShouldBe(4);
        result.Upper.IsSome.ShouldBeTrue();
        result.Upper.Unwrap().ShouldBe(4);
    }
}
