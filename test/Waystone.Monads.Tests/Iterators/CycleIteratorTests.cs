namespace Waystone.Monads.Iterators;

using System;
using System.Collections.Generic;
using Extensions;
using Options;
using Shouldly;
using Xunit;

public sealed class CycleIteratorTests
{
    [Fact]
    public void Given_Iterator_When_Cycling_Then_RepeatsElements()
    {
        // Given
        var source = new Iterator<TestCloneable>(
            new[] { new TestCloneable("A"), new TestCloneable("B") });
        CycleIterator<TestCloneable> cycle = source.Cycle();

        // When
        var results = new List<string>();
        for (var i = 0; i < 5; i++)
        {
            Option<TestCloneable> item = cycle.Next();
            results.Add(item.Unwrap().Value);
        }

        // Then
        results.ShouldBe(new[] { "A", "B", "A", "B", "A" });
    }

    [Fact]
    public void Given_EmptyIterator_When_Cycling_Then_ReturnsNone()
    {
        // Given
        var source = new Iterator<TestCloneable>(Array.Empty<TestCloneable>());
        CycleIterator<TestCloneable> cycle = source.Cycle();

        // When
        Option<TestCloneable> result = cycle.Next();

        // Then
        result.IsNone.ShouldBeTrue();
    }

    [Fact]
    public void
        Given_SingleElementIterator_When_Cycling_Then_RepeatsSingleElement()
    {
        // Given
        var source =
            new Iterator<TestCloneable>(new[] { new TestCloneable("A") });
        CycleIterator<TestCloneable> cycle = source.Cycle();

        // When
        var results = new List<string>();
        for (var i = 0; i < 3; i++)
        {
            Option<TestCloneable> item = cycle.Next();
            results.Add(item.Unwrap().Value);
        }

        // Then
        results.ShouldBe(new[] { "A", "A", "A" });
    }

    private sealed class TestCloneable : ICloneable
    {
        public TestCloneable(string value)
        {
            Value = value;
        }

        public string Value { get; }

        public object Clone() => new TestCloneable(Value);
    }
}
