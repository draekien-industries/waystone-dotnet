namespace Waystone.Monads.Iterators;

using System;
using System.Collections.Generic;
using Shouldly;
using Xunit;

public sealed class MapIteratorTests
{
    [Fact]
    public void
        Given_IntegerSequence_When_MappingToString_Then_ReturnsStringSequence()
    {
        // Arrange
        var source = new[] { 1, 2, 3 };

        // Act
        IEnumerable<string> result =
            source.IntoIter().Map(x => x.ToString()).Collect();

        // Assert
        result.ShouldBe(["1", "2", "3"]);
    }

    [Fact]
    public void
        Given_StringSequence_When_MappingToLength_Then_ReturnsLengthSequence()
    {
        // Arrange
        var source = new[] { "a", "bb", "ccc" };

        // Act
        IEnumerable<int> result =
            source.IntoIter().Map(s => s.Length).Collect();

        // Assert
        result.ShouldBe([1, 2, 3]);
    }

    [Fact]
    public void Given_EmptySequence_When_Mapping_Then_ReturnsEmptySequence()
    {
        // Arrange
        int[] source = [];

        // Act
        IEnumerable<string> result =
            source.IntoIter().Map(x => x.ToString()).Collect();

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public void
        Given_NullMapper_When_Constructing_Then_ThrowsArgumentNullException()
    {
        // Arrange
        var source = new[] { 1, 2, 3 };
        Func<int, string> mapper = null!;

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => source.IntoIter()
                                               .Map(mapper));
    }
}
