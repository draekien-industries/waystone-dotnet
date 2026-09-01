namespace Waystone.Monads.Schemas;

using System;
using System.Collections.Generic;
using Shouldly;
using Waystone.Monads.Results;
using Xunit;

public sealed class OutcomeTests
{
    private static IReadOnlyList<Violation> OneViolation() =>
        new[]
        {
            new Violation(
                ViolationPath.Root.Append("email"),
                ViolationCodeCatalog.Codes.Malformed,
                "Not an email."),
        };

    [Fact]
    public void GivenAValue_WhenPassed_ThenCarryItWithNoViolations()
    {
        Outcome<int> outcome = Outcome<int>.Passed(7);

        outcome.HasValue.ShouldBeTrue();
        outcome.Value.ShouldBe(7);
        outcome.Violations.ShouldBeEmpty();
    }

    [Fact]
    public void GivenViolations_WhenFailed_ThenCarryNoValue()
    {
        Outcome<int> outcome = Outcome<int>.Failed(OneViolation());

        outcome.HasValue.ShouldBeFalse();
        outcome.Violations.Count.ShouldBe(1);
    }

    [Fact]
    public void GivenAFailedOutcome_WhenReadingTheValue_ThenThrow()
    {
        Outcome<int> outcome = Outcome<int>.Failed(OneViolation());

        Should.Throw<InvalidOperationException>(() => outcome.Value);
    }

    [Fact]
    public void GivenASurvivingValue_WhenRefined_ThenCarryBoth()
    {
        Outcome<int> outcome = Outcome<int>.Refined(7, OneViolation());

        outcome.HasValue.ShouldBeTrue();
        outcome.Value.ShouldBe(7);
        outcome.Violations.Count.ShouldBe(1);
    }

    [Fact]
    public void GivenNull_WhenFailed_ThenThrow()
    {
        Should.Throw<ArgumentNullException>(() => Outcome<int>.Failed(null!));
    }

    [Fact]
    public void GivenNoViolations_WhenFailed_ThenThrow()
    {
        Should.Throw<ArgumentException>(
            () => Outcome<int>.Failed(Array.Empty<Violation>()));
    }

    [Fact]
    public void GivenNoViolations_WhenRefined_ThenThrow()
    {
        Should.Throw<ArgumentException>(
            () => Outcome<int>.Refined(7, Array.Empty<Violation>()));
    }

    [Fact]
    public void GivenAPassedOutcome_WhenConverting_ThenReturnOk()
    {
        Outcome<int>.Passed(7)
                    .ToResult()
                    .ShouldBe(Result.Ok<int, SchemaViolation>(7));
    }

    [Fact]
    public void GivenAFailedOutcome_WhenConverting_ThenReturnTheViolations()
    {
        Result<int, SchemaViolation> result =
            Outcome<int>.Failed(OneViolation()).ToResult();

        result.IsErr.ShouldBeTrue();
        result.UnwrapErr().Violations.Count.ShouldBe(1);
    }

    [Fact]
    public void GivenARefinedOutcome_WhenConverting_ThenStillFail()
    {
        Outcome<int>.Refined(7, OneViolation()).ToResult().IsErr.ShouldBeTrue();
    }
}
