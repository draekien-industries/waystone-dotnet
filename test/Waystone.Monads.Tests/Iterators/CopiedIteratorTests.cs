namespace Waystone.Monads.Iterators;

using System.Collections.Generic;
using Extensions;
using Shouldly;
using Xunit;

public sealed class CopiedIteratorTests
{
    [Fact]
    public void
        Given_SourceSequence_When_SourceIsModified_Then_ClonedSequenceRemainsUnchanged()
    {
        // Arrange
        var source = new List<int> { 1, 2, 3 };
        IEnumerable<int> cloned = source.IntoIter().Copied().Collect();

        // Act
        source.Add(4);

        // Assert
        cloned.ShouldBe([1, 2, 3]);
    }

    [Fact]
    public void Given_EmptySequence_When_Cloning_Then_ReturnsEmptySequence()
    {
        // Arrange
        var source = new List<int>();

        // Act
        IEnumerable<int> result = source.IntoIter().Copied().Collect();

        // Assert
        result.ShouldBeEmpty();
    }
}
