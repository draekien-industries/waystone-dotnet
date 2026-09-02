namespace Waystone.Monads.Schemas.SourceGenerators;

using Shouldly;
using Xunit;

/// <summary>
/// The type exists so a record holding an array can be cached by the incremental
/// pipeline. Nothing about that is visible from the generator's output — a pipeline
/// that never hits its cache produces exactly the same source — so it is tested
/// directly or not at all.
/// </summary>
public sealed class EquatableArrayTests
{
    [Fact]
    public void TwoArraysOverTheSameValuesAreEqual() =>
        new EquatableArray<string>(["one", "two"])
           .ShouldBe(new EquatableArray<string>(["one", "two"]));

    [Fact]
    public void TwoArraysOfDifferentLengthsDiffer() =>
        new EquatableArray<string>(["one"])
           .ShouldNotBe(new EquatableArray<string>(["one", "two"]));

    [Fact]
    public void TwoArraysDifferingAtOnePositionDiffer() =>
        new EquatableArray<string>(["one", "two"])
           .ShouldNotBe(new EquatableArray<string>(["one", "three"]));

    /// <summary>
    /// The pipeline compares through <c>object</c>, so the untyped overload is the
    /// one that actually runs rather than a formality.
    /// </summary>
    [Fact]
    public void AnArrayIsNotEqualToSomethingElseEntirely() =>
        new EquatableArray<string>(["one"]).Equals("one").ShouldBeFalse();

    [Fact]
    public void EqualArraysHashAlike() =>
        new EquatableArray<string>(["one", "two"])
           .GetHashCode()
           .ShouldBe(
                new EquatableArray<string>(["one", "two"]).GetHashCode());

    [Fact]
    public void DifferentArraysHashApart() =>
        new EquatableArray<string>(["one"])
           .GetHashCode()
           .ShouldNotBe(new EquatableArray<string>(["two"]).GetHashCode());

    [Fact]
    public void AnEmptyArrayIsEqualToAnother() =>
        new EquatableArray<int>([]).ShouldBe(new EquatableArray<int>([]));

    [Fact]
    public void TheLengthIsTheUnderlyingLength() =>
        new EquatableArray<int>([1, 2, 3]).Length.ShouldBe(3);
}
