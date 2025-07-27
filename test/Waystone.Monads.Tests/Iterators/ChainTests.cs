namespace Waystone.Monads.Iterators;

using System.Collections.Generic;
using System.Linq;
using Shouldly;
using Xunit;

public sealed class ChainTests
{
    [Fact]
    public void Chain_WhenChainingTwoSequences_ShouldCombineThemInOrder()
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
        Chain_WhenChainingWithEmptySequence_ShouldReturnOriginalSequence()
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
    public void Chain_WhenChainingEmptyWithNonEmpty_ShouldReturnSecondSequence()
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
    public void Chain_WhenDisposed_ShouldNotEnumerate()
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
}
