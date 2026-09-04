namespace Waystone.Monads.Schemas;

using System;
using Shouldly;
using Waystone.Monads.Options;
using Waystone.Monads.Results;
using Xunit;

public sealed class FieldAccumulatorTests
{
    [Fact]
    public void GivenAPassingField_WhenTakingIt_ThenYieldItsValue()
    {
        FieldAccumulator accumulator = FieldAccumulator.Start();

        accumulator.Take(Schema.Required("abc", new PassThrough<string>()))
                   .ShouldBe("abc");

        accumulator.HasViolations.ShouldBeFalse();
    }

    [Fact]
    public void GivenAFailingField_WhenTakingIt_ThenYieldNothing()
    {
        FieldAccumulator accumulator = FieldAccumulator.Start();

        accumulator.Take(Schema.Required("abc", new Rejects<string>()))
                   .ShouldBeNull();

        accumulator.HasViolations.ShouldBeTrue();
    }

    /// <summary>
    /// A refinement that fails leaves the value intact so the rest of its chain
    /// still runs, which is why a value coming back says nothing about whether the
    /// parse will succeed.
    /// </summary>
    [Fact]
    public void GivenARefinedField_WhenTakingIt_ThenYieldTheValueAndStillFail()
    {
        FieldAccumulator accumulator = FieldAccumulator.Start();

        accumulator.Take(Schema.Required("abc", new RefinesAndKeeps<string>()))
                   .ShouldBe("abc");

        accumulator.HasViolations.ShouldBeTrue();
    }

    [Fact]
    public void GivenNothingAtAll_WhenAsked_ThenReportNoViolations() =>
        FieldAccumulator.Start().HasViolations.ShouldBeFalse();

    /// <summary>
    /// The promise the whole package rests on: every field runs, whatever the ones
    /// before it did, so a caller gets every problem with their input at once.
    /// </summary>
    [Fact]
    public void GivenSeveralFailingFields_WhenFailing_ThenReportAllOfThem()
    {
        var email = "abc";
        var name = "def";

        FieldAccumulator accumulator = FieldAccumulator.Start();

        accumulator.Take(Schema.Required(email, new Rejects<string>()));
        accumulator.Take(Schema.Required(name, new Rejects<string>()));

        SchemaViolation violation = accumulator.Failed<string>().UnwrapErr();

        violation.Violations.Count.ShouldBe(2);
        violation.Violations[0].Message.ShouldBe("Rejected email: got abc.");
        violation.Violations[1].Message.ShouldBe("Rejected name: got def.");
    }

    [Fact]
    public void GivenAGatingField_WhenRefining_ThenReportItsViolation()
    {
        FieldAccumulator accumulator = FieldAccumulator.Start();

        accumulator.Refine(
            Schema.Forbidden(Option.Some("set"), "Do not send {Path}."));

        accumulator.Failed<string>()
                   .UnwrapErr()
                   .Violations.Count.ShouldBe(1);
    }

    [Fact]
    public void GivenASatisfiedGatingField_WhenRefining_ThenReportNothing()
    {
        FieldAccumulator accumulator = FieldAccumulator.Start();

        accumulator.Refine(
            Schema.Forbidden(Option.None<string>(), "Do not send {Path}."));

        accumulator.HasViolations.ShouldBeFalse();
    }

    /// <summary>
    /// The violations are snapshotted, so an accumulator used again cannot change a
    /// result already handed back.
    /// </summary>
    [Fact]
    public void GivenAFailedParse_WhenRefiningAgain_ThenLeaveTheResultAlone()
    {
        FieldAccumulator accumulator = FieldAccumulator.Start();

        accumulator.Refine(
            Schema.Forbidden(Option.Some("set"), "Do not send {Path}."));

        Result<string, SchemaViolation> result = accumulator.Failed<string>();

        accumulator.Refine(
            Schema.Forbidden(Option.Some("other"), "Do not send {Path}."));

        result.UnwrapErr().Violations.Count.ShouldBe(1);
    }

    /// <summary>
    /// Nothing to describe is a caller who did not check <c>HasViolations</c>, and
    /// a failed result carrying no violation would be a lie the rest of the package
    /// cannot render.
    /// </summary>
    [Fact]
    public void GivenNoViolations_WhenFailing_ThenThrow() =>
        Should.Throw<InvalidOperationException>(
                   () => FieldAccumulator.Start().Failed<string>())
              .Message.ShouldContain("HasViolations");

    [Fact]
    public void GivenNoField_WhenTakingIt_ThenThrow() =>
        Should.Throw<ArgumentNullException>(
            () => FieldAccumulator.Start().Take<string>(null!));

    [Fact]
    public void GivenNoField_WhenRefining_ThenThrow() =>
        Should.Throw<ArgumentNullException>(
            () => FieldAccumulator.Start().Refine(null!));

    [Fact]
    public void GivenAnAccumulator_WhenStartingAnother_ThenShareNothing()
    {
        FieldAccumulator first = FieldAccumulator.Start();

        first.Refine(
            Schema.Forbidden(Option.Some("set"), "Do not send {Path}."));

        FieldAccumulator.Start().HasViolations.ShouldBeFalse();
    }
}
