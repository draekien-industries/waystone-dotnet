namespace Waystone.Monads.Schemas;

using System;
using Shouldly;
using Waystone.Monads.Options;
using Xunit;

/// <summary>
/// <c>Field.Named</c>, which is where a path a caller reads should be set. The
/// schema-level <c>Schema.Named</c> is the other half and belongs to a schema
/// parsed on its own; <c>SchemaReportingTests</c> covers that one.
/// </summary>
public sealed class FieldNameTests
{
    [Fact]
    public void GivenARequiredField_WhenNaming_ThenReportUnderTheNewName()
    {
        string? patronEmail = null;

        Evaluate(
                Schema.Required(patronEmail, new PassThrough<string>())
                      .Named("patron"))
           .Violations[0].Path.ToString()
           .ShouldBe("patron");
    }

    [Fact]
    public void GivenARequiredField_WhenNaming_ThenKeepItsAbsenceMessage()
    {
        string? patronEmail = null;

        Evaluate(
                Schema.Required(
                           patronEmail,
                           new PassThrough<string>(),
                           "We need {Path}.")
                      .Named("patron"))
           .Violations[0].Message.ShouldBe("We need patron.");
    }

    [Fact]
    public void GivenARequiredField_WhenNaming_ThenStillParseTheValue()
    {
        string? patronEmail = "ada@example.com";

        Outcome<string> outcome = Evaluate(
            Schema.Required(patronEmail, new PassThrough<string>())
                  .Named("patron"));

        outcome.Value.ShouldBe("ada@example.com");
        outcome.Violations.ShouldBeEmpty();
    }

    [Fact]
    public void GivenAnOptionalField_WhenNaming_ThenReportUnderTheNewName()
    {
        int? partySize = 4;

        Evaluate(
                Schema.Optional(partySize, new Rejects<int>())
                      .Named("adventurers"))
           .Violations[0].Path.ToString()
           .ShouldBe("adventurers");
    }

    [Fact]
    public void GivenAnAbsentOptionalField_WhenNaming_ThenStillYieldNone()
    {
        int? partySize = null;

        Evaluate(
                Schema.Optional(partySize, new PassThrough<int>())
                      .Named("adventurers"))
           .Value.IsNone.ShouldBeTrue();
    }

    [Fact]
    public void GivenAForbiddenField_WhenNaming_ThenReportUnderTheNewName()
    {
        string? legacyId = "MMXIV";

        Evaluate(
                Schema.Forbidden(legacyId, "Do not send {Path}.")
                      .Named("identifier"))
           .Violations[0].Message.ShouldBe("Do not send identifier.");
    }

    [Fact]
    public void GivenAnAbsentForbiddenField_WhenNaming_ThenStillPass()
    {
        string? legacyId = null;

        Evaluate(
                Schema.Forbidden(legacyId, "Do not send {Path}.")
                      .Named("identifier"))
           .Violations.ShouldBeEmpty();
    }

    /// <summary>
    /// An extension reports at the subject's own path, so it has no segment to
    /// replace. Naming one nests instead, which is how a cross-field rule gets
    /// somewhere to be reported.
    /// </summary>
    [Fact]
    public void GivenAnExtension_WhenNaming_ThenNestRatherThanReplace() =>
        Evaluate(
                Schema.Extend("anything", new Rejects<string>())
                      .Named("dates"))
           .Violations[0].Path.ToString()
           .ShouldBe("dates");

    [Fact]
    public void GivenAnUnnamedExtension_WhenEvaluating_ThenReportAtTheRoot() =>
        Evaluate(Schema.Extend("anything", new Rejects<string>()))
           .Violations[0].Path.IsRoot.ShouldBeTrue();

    [Fact]
    public void GivenAPassingExtension_WhenNaming_ThenReportNothing() =>
        Evaluate(
                Schema.Extend("anything", new PassThrough<string>())
                      .Named("dates"))
           .Violations.ShouldBeEmpty();

    [Fact]
    public void GivenNoField_WhenNaming_ThenThrow() =>
        Should.Throw<ArgumentNullException>(
            () => ((Field<string>)null!).Named("patron"));

    [Fact]
    public void GivenNoName_WhenNaming_ThenThrow() =>
        Should.Throw<ArgumentNullException>(
            () => Schema.Required("a", new PassThrough<string>()).Named(null!));

    private static Outcome<T> Evaluate<T>(Field<T> field) where T : notnull =>
        field.EvaluateValue(ParseContext.Root);
}
