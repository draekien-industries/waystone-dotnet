namespace Waystone.Monads.Iterators;

using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using Shouldly;
using Xunit;

public sealed class ClonedTests
{
    [Fact]
    public void Given_StringSequence_When_Cloning_Then_ReturnsClonedStrings()
    {
        // Arrange
        var source = new[] { "test1", "test2", "test3" };

        // Act
        IEnumerable<string> result = source.IntoIter().Cloned().Collect();

        // Assert
        result.ShouldBe(source);
        result.ShouldNotBeSameAs(source);
    }

    [Fact]
    public void Given_CloneableObjects_When_Cloning_Then_ReturnsClonedObjects()
    {
        // Arrange
        var source = new[]
        {
            new CloneableObject { Value = 1 },
            new CloneableObject { Value = 2 },
        };

        // Act
        IEnumerable<CloneableObject> result =
            source.IntoIter().Cloned().Collect();

        // Assert
        result.ShouldNotBeSameAs(source);
        CloneableObject[] resultArray = result.ToArray();
        resultArray[0].Value.ShouldBe(1);
        resultArray[1].Value.ShouldBe(2);
        resultArray[0].ShouldNotBeSameAs(source[0]);
        resultArray[1].ShouldNotBeSameAs(source[1]);
    }

    [Fact]
    public void Given_EmptySequence_When_Cloning_Then_ReturnsEmptySequence()
    {
        // Arrange
        string[] source = [];

        // Act
        IEnumerable<string> result = source.IntoIter().Cloned().Collect();

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public void Given_NullSource_When_Cloning_Then_ThrowsArgumentNullException()
    {
        // Arrange
        IEnumerable<string> source = null!;

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => source.IntoIter().Cloned());
    }

    private class CloneableObject : ICloneable
    {
        public int Value { get; set; }

        public object Clone() => new CloneableObject { Value = Value };
    }
}
