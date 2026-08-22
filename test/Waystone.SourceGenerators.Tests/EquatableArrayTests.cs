namespace Waystone.SourceGenerators;

using Shouldly;
using Waystone.SourceGenerators.AwaitedReceivers;
using Xunit;

/// <summary>
/// The incremental pipeline compares its cached values through
/// <see cref="System.IEquatable{T}" />, so the object overrides are exercised here
/// rather than through the generator.
/// </summary>
public sealed class EquatableArrayTests
{
    [Fact]
    public void EqualsMatchesElementwise()
    {
        var left = new EquatableArray<string>(["a", "b"]);
        var right = new EquatableArray<string>(["a", "b"]);

        left.Equals(right).ShouldBeTrue();
    }

    [Fact]
    public void EqualsRejectsADifferentLength()
    {
        var left = new EquatableArray<string>(["a"]);
        var right = new EquatableArray<string>(["a", "b"]);

        left.Equals(right).ShouldBeFalse();
    }

    [Fact]
    public void EqualsRejectsADifferentElement()
    {
        var left = new EquatableArray<string>(["a", "b"]);
        var right = new EquatableArray<string>(["a", "c"]);

        left.Equals(right).ShouldBeFalse();
    }

    [Fact]
    public void EqualsObjectMatchesTheSameShape()
    {
        var left = new EquatableArray<string>(["a"]);
        object right = new EquatableArray<string>(["a"]);

        left.Equals(right).ShouldBeTrue();
    }

    [Fact]
    public void EqualsObjectRejectsAnotherType()
    {
        var left = new EquatableArray<string>(["a"]);

        left.Equals("a").ShouldBeFalse();
    }

    [Fact]
    public void HashCodeAgreesWithEquality()
    {
        var left = new EquatableArray<string>(["a", "b"]);
        var right = new EquatableArray<string>(["a", "b"]);

        left.GetHashCode().ShouldBe(right.GetHashCode());
    }

    [Fact]
    public void HashCodeDistinguishesOrder()
    {
        var left = new EquatableArray<string>(["a", "b"]);
        var right = new EquatableArray<string>(["b", "a"]);

        left.GetHashCode().ShouldNotBe(right.GetHashCode());
    }

    [Fact]
    public void HashCodeOfAnEmptyArrayIsItsLength()
    {
        new EquatableArray<string>([]).GetHashCode().ShouldBe(0);
    }
}
