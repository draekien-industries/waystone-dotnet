namespace Waystone.Monads.Options;

using JetBrains.Annotations;
using Shouldly;
using Xunit;

/// <remarks>
/// The singleton changes reference identity and nothing else. These pin the
/// "nothing else" half: a rule that made two <c>None</c> values stop being
/// equal, or hash differently, would break correct programs, where the
/// identity change cannot.
/// </remarks>
[TestSubject(typeof(None<>))]
public sealed class NoneSingletonTests
{
    [Fact]
    public void TwoNonesOfTheSameTypeAreTheSameInstance() =>
        ReferenceEquals(Option.None<int>(), Option.None<int>())
           .ShouldBeTrue();

    [Fact]
    public void TwoNonesOfDifferentTypesAreDifferentInstances() =>
        ReferenceEquals(Option.None<int>(), Option.None<string>())
           .ShouldBeFalse();

    [Fact]
    public void TwoNonesAreEqual() =>
        Option.None<int>().ShouldBe(Option.None<int>());

    [Fact]
    public void TwoNonesHashTheSame() =>
        Option.None<int>().GetHashCode()
           .ShouldBe(Option.None<int>().GetHashCode());

    [Fact]
    public void ANoneEqualsAnIndependentlyConstructedOne()
    {
        Option<int> constructed = new None<int>();

        constructed.ShouldBe(Option.None<int>());
        constructed.GetHashCode().ShouldBe(Option.None<int>().GetHashCode());
    }

    [Fact]
    public void TheSingletonIsNotEqualToASome() =>
        Option.None<int>().ShouldNotBe(Option.Some(0));

    /// <remarks>
    /// A record's <c>with</c> goes through the compiler-generated clone rather
    /// than the factory, so it hands back a second instance. That is harmless —
    /// there is no state to diverge — but it means the singleton is the
    /// factory's guarantee, not the type's.
    /// </remarks>
    [Fact]
    public void AWithExpressionStillProducesAnEqualNone()
    {
        var original = (None<int>)Option.None<int>();
        var copied = original with { };

        copied.ShouldBe(original);
        copied.IsNone.ShouldBeTrue();
    }

    [Fact]
    public void TheSingletonSurvivesConversionRoundTrips()
    {
        Option<int> converted = Option.FromNullable<int>(null);

        converted.ShouldBe(Option.None<int>());
        converted.IsNone.ShouldBeTrue();
    }

    [Fact]
    public void AFilteredOutSomeIsTheSingleton() =>
        ReferenceEquals(
                Option.Some(1).Filter(static value => value < 0),
                Option.None<int>())
           .ShouldBeTrue();
}
