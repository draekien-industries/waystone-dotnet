namespace Waystone.Monads.Options;

using System;
using JetBrains.Annotations;
using Shouldly;
using Xunit;

[TestSubject(typeof(Option<>))]
public sealed class PatternMatchingTests
{
    [Fact]
    public void GivenASome_WhenMatchingAPositionalPattern_ThenBindTheValue()
    {
        Option<int> option = Option.Some(42);

        if (option is Some<int>(var value))
        {
            value.ShouldBe(42);
        }
        else
        {
            Assert.Fail("The positional pattern did not match a Some.");
        }
    }

    [Fact]
    public void GivenANone_WhenMatchingASomePositionalPattern_ThenDoNotMatch()
    {
        Option<int> option = Option.None<int>();

        (option is Some<int>(_)).ShouldBeFalse();
        (option is None<int>).ShouldBeTrue();
    }

    [Fact]
    public void GivenBothCases_WhenSwitchingOverThePair_ThenEachArmIsReached()
    {
        Describe(Option.Some(42)).ShouldBe("some 42");
        Describe(Option.None<int>()).ShouldBe("none");
    }

    [Fact]
    public void GivenBothCases_WhenSwitchingAsAStatement_ThenNoArmIsNeededForNeither()
    {
        DescribeByStatement(Option.Some(42)).ShouldBe("some 42");
        DescribeByStatement(Option.None<int>()).ShouldBe("none");
    }

    [Fact]
    public void GivenASome_WhenTryingToUnwrap_ThenReturnTrueAndTheValue()
    {
        Option<int> option = Option.Some(42);

        option.TryUnwrap(out var value).ShouldBeTrue();
        value.ShouldBe(42);
    }

    [Fact]
    public void GivenANone_WhenTryingToUnwrap_ThenReturnFalseAndTheDefault()
    {
        Option<int> option = Option.None<int>();

        option.TryUnwrap(out var value).ShouldBeFalse();
        value.ShouldBe(0);
    }

    [Fact]
    public void
        GivenANoneOfAReferenceType_WhenTryingToUnwrap_ThenReturnFalseAndNull()
    {
        Option<string> option = Option.None<string>();

        option.TryUnwrap(out var value).ShouldBeFalse();
        value.ShouldBeNull();
    }

    [Fact]
    public void GivenASome_WhenTryUnwrapSucceeds_ThenTheValueIsNotNullable()
    {
        Option<string> option = Option.Some("abc");

        if (!option.TryUnwrap(out var value))
        {
            Assert.Fail("TryUnwrap did not succeed for a Some.");

            return;
        }

        value.Length.ShouldBe(3);
    }

    private static string Describe(Option<int> option) =>
        option switch
        {
            Some<int>(var value) => $"some {value}",
            None<int> => "none",
            _ => throw new InvalidOperationException(
                "Option has no third case."),
        };

    private static string DescribeByStatement(Option<int> option)
    {
        switch (option)
        {
            case Some<int>(var value):
                return $"some {value}";
            case None<int>:
                return "none";
        }

        throw new InvalidOperationException("Option has no third case.");
    }
}
