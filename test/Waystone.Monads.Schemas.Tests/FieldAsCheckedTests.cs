namespace Waystone.Monads.Schemas;

using System;
using Shouldly;
using Waystone.Monads.Options;
using Xunit;

/// <summary>
/// <c>Field.AsChecked</c>, the deliberate form of handing a value-producing field
/// to <c>Refine</c>. The value goes; the rules and the path a caller reads stay.
/// </summary>
public sealed class FieldAsCheckedTests
{
    [Fact]
    public void GivenAPassingField_WhenChecking_ThenYieldChecked()
    {
        string? patronEmail = "ada@example.com";

        Outcome<Checked> outcome = Evaluate(
            Schema.Required(patronEmail, new PassThrough<string>())
                  .AsChecked());

        outcome.Value.ShouldBe(Checked.Instance);
        outcome.Violations.ShouldBeEmpty();
    }

    [Fact]
    public void GivenAFailingField_WhenChecking_ThenKeepItsViolations()
    {
        string? patronEmail = "ada@example.com";

        Evaluate(
                Schema.Required(patronEmail, new Rejects<string>())
                      .AsChecked())
           .Violations.Count.ShouldBe(1);
    }

    /// <summary>
    /// The property the reporter of the gap was blocked on: <c>Schema.Extend</c>
    /// files at the empty root path, so a per-field validation response cannot say
    /// which field failed. This keeps the field's own segment.
    /// </summary>
    [Fact]
    public void GivenAFailingField_WhenChecking_ThenReportAtItsOwnPath()
    {
        string? patronEmail = "ada@example.com";

        Evaluate(
                Schema.Required(patronEmail, new Rejects<string>())
                      .AsChecked())
           .Violations[0].Path.ToString()
           .ShouldBe("patronEmail");
    }

    [Fact]
    public void GivenAnAbsentRequiredField_WhenChecking_ThenStillReportIt()
    {
        string? patronEmail = null;

        Evaluate(
                Schema.Required(patronEmail, new PassThrough<string>())
                      .AsChecked())
           .Violations[0].Path.ToString()
           .ShouldBe("patronEmail");
    }

    /// <summary>
    /// A refinement that fails keeps its value, so the inner outcome has both a
    /// value and violations. Anything but a failure here would let a violated
    /// field pass as checked.
    /// </summary>
    [Fact]
    public void GivenARefinedField_WhenChecking_ThenStillFail()
    {
        string? patronEmail = "ada@example.com";

        Outcome<Checked> outcome = Evaluate(
            Schema.Required(patronEmail, new RefinesAndKeeps<string>())
                  .AsChecked());

        outcome.HasValue.ShouldBeFalse();
        outcome.Violations.Count.ShouldBe(1);
    }

    [Fact]
    public void GivenAnOptionalField_WhenChecking_ThenReportAtItsOwnPath()
    {
        int? partySize = 4;

        Evaluate(Schema.Optional(partySize, new Rejects<int>()).AsChecked())
           .Violations[0].Path.ToString()
           .ShouldBe("partySize");
    }

    [Fact]
    public void GivenAnAbsentOptionalField_WhenChecking_ThenPass() =>
        Evaluate(
                Schema.Optional((int?)null, new Rejects<int>()).AsChecked())
           .Violations.ShouldBeEmpty();

    [Fact]
    public void GivenAListField_WhenChecking_ThenKeepTheIndexInThePath() =>
        Evaluate(
                Schema.Required(
                           new[] { "ada" },
                           Schema.List(new Rejects<string>()))
                      .Named("cc")
                      .AsChecked())
           .Violations[0].Path.ToString()
           .ShouldBe("cc[0]");

    [Fact]
    public void GivenACheckedField_WhenNaming_ThenReportUnderTheNewName()
    {
        string? patronEmail = "ada@example.com";

        Evaluate(
                Schema.Required(patronEmail, new Rejects<string>())
                      .AsChecked()
                      .Named("patron"))
           .Violations[0].Path.ToString()
           .ShouldBe("patron");
    }

    [Fact]
    public void GivenANamedField_WhenChecking_ThenReportUnderTheNewName()
    {
        string? patronEmail = "ada@example.com";

        Evaluate(
                Schema.Required(patronEmail, new Rejects<string>())
                      .Named("patron")
                      .AsChecked())
           .Violations[0].Path.ToString()
           .ShouldBe("patron");
    }

    /// <summary>
    /// The point of the adapter: a field set spends its arity on the fields that
    /// build the result, and the rest go to <c>Refine</c> at their own paths.
    /// </summary>
    [Fact]
    public void GivenAMixedParse_WhenRefiningACheckedField_ThenGatherBoth()
    {
        FieldAccumulator accumulator = FieldAccumulator.Start();
        string? patronName = "Ada";
        string? patronEmail = "ada@example.com";

        accumulator.Take(Schema.Required(patronName, new PassThrough<string>()))
                   .ShouldBe("Ada");

        accumulator.Refine(
            Schema.Required(patronEmail, new Rejects<string>()).AsChecked());

        accumulator.HasViolations.ShouldBeTrue();

        accumulator.Failed<string>()
                   .UnwrapErr()
                   .Violations[0]
                   .Path.ToString()
                   .ShouldBe("patronEmail");
    }

    [Fact]
    public void GivenNoField_WhenChecking_ThenThrow() =>
        Should.Throw<ArgumentNullException>(
            () => ((Field<string>)null!).AsChecked());

    private static Outcome<T> Evaluate<T>(Field<T> field) where T : notnull =>
        field.EvaluateValue(ParseContext.Root);
}
