namespace Waystone.Monads.Iterators;

using System.Collections.Generic;
using System.Linq;
using Shouldly;
using Xunit;

public sealed class ChainTests
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
        Chain<int> chain = iterator.Chain(second);
        chain.Dispose();
        List<int> result = chain.Collect().ToList();

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
        Chain<int> chain = iterator.Chain(second);

        // When
        chain.Dispose();
        bool result = chain.All(x => x > 0);

        // Then
        result.ShouldBeTrue();
    }
}
