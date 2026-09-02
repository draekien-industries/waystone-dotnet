namespace Waystone.Monads.Schemas;

using System;
using System.Collections.Generic;
using Shouldly;
using Waystone.Monads.Options;
using Xunit;

public sealed class FieldConstructorTests
{
    private static Outcome<T> Evaluate<T>(Field<T> field) where T : notnull =>
        field.EvaluateValue(ParseContext.Root);

    [Fact]
    public void GivenAPresentReference_WhenRequired_ThenParseItUnderItsOwnName()
    {
        string? email = "someone@example.com";

        Outcome<string> outcome =
            Evaluate(Schema.Required(email, new PassThrough<string>()));

        outcome.HasValue.ShouldBeTrue();
        outcome.Value.ShouldBe("someone@example.com");
    }

    [Fact]
    public void GivenAnAbsentReference_WhenRequired_ThenReportItIncompleteAtItsPath()
    {
        string? email = null;

        Outcome<string> outcome =
            Evaluate(Schema.Required(email, new PassThrough<string>()));

        outcome.HasValue.ShouldBeFalse();
        outcome.Violations[0].Path.ToString().ShouldBe("email");
        outcome.Violations[0].Code
               .ShouldBe(ViolationCodeCatalog.Codes.Incomplete);
        outcome.Violations[0].Message.ShouldBe("Expected email to be present.");
    }

    [Fact]
    public void GivenAnAbsentValueType_WhenRequired_ThenReportItIncomplete()
    {
        int? total = null;

        Evaluate(Schema.Required(total, new PassThrough<int>()))
           .Violations[0].Path.ToString()
           .ShouldBe("total");
    }

    [Fact]
    public void GivenAPresentValueType_WhenRequired_ThenParseIt()
    {
        int? total = 7;

        Evaluate(Schema.Required(total, new PassThrough<int>())).Value
           .ShouldBe(7);
    }

    [Fact]
    public void GivenAnOption_WhenRequired_ThenParseWhatItHolds()
    {
        Option<string> name = Option.Some("ada");

        Evaluate(Schema.Required(name, new PassThrough<string>())).Value
           .ShouldBe("ada");
    }

    [Fact]
    public void GivenANoneOption_WhenRequired_ThenReportItIncomplete()
    {
        Option<string> name = Option.None<string>();

        Evaluate(Schema.Required(name, new PassThrough<string>())).HasValue
           .ShouldBeFalse();
    }

    [Fact]
    public void GivenAnOverridingMessage_WhenRequiredAndAbsent_ThenUseIt()
    {
        string? email = null;

        Evaluate(
                Schema.Required(
                    email,
                    new PassThrough<string>(),
                    "{Path} is mandatory ({Code})."))
           .Violations[0].Message
           .ShouldBe("email is mandatory (schema_violation.incomplete).");
    }

    [Fact]
    public void GivenANullSchema_WhenRequired_ThenThrow()
    {
        Should.Throw<ArgumentNullException>(
            () => Schema.Required(Option.Some("x"), (Schema<string, string>)null!));
    }

    [Fact]
    public void GivenANullOption_WhenRequired_ThenThrow()
    {
        Should.Throw<ArgumentNullException>(
            () => Schema.Required((Option<string>)null!, new PassThrough<string>()));
    }

    [Fact]
    public void GivenAnAbsentReference_WhenOptional_ThenPassWithNone()
    {
        string? nickname = null;

        Outcome<Option<string>> outcome =
            Evaluate(Schema.Optional(nickname, new PassThrough<string>()));

        outcome.Violations.ShouldBeEmpty();
        outcome.Value.IsNone.ShouldBeTrue();
    }

    [Fact]
    public void GivenAnAbsentValueType_WhenOptional_ThenPassWithNone()
    {
        int? count = null;

        Evaluate(Schema.Optional(count, new PassThrough<int>())).Value.IsNone
           .ShouldBeTrue();
    }

    [Fact]
    public void GivenAPresentValue_WhenOptional_ThenPassWithSome()
    {
        string? nickname = "ada";

        Evaluate(Schema.Optional(nickname, new PassThrough<string>())).Value
           .ShouldBe(Option.Some("ada"));
    }

    [Fact]
    public void GivenAnOption_WhenOptional_ThenParseWhatItHolds()
    {
        Evaluate(
                Schema.Optional(
                    Option.Some("ada"),
                    new PassThrough<string>()))
           .Value.ShouldBe(Option.Some("ada"));
    }

    [Fact]
    public void GivenAPresentValueTypeOption_WhenOptional_ThenParseIt()
    {
        int? count = 3;

        Evaluate(Schema.Optional(count, new PassThrough<int>())).Value
           .ShouldBe(Option.Some(3));
    }

    [Fact]
    public void GivenASchemaThatLosesTheValue_WhenOptional_ThenFail()
    {
        string? nickname = "ada";

        Outcome<Option<string>> outcome =
            Evaluate(Schema.Optional(nickname, new Rejects<string>()));

        outcome.HasValue.ShouldBeFalse();
        outcome.Violations.Count.ShouldBe(1);
    }

    [Fact]
    public void GivenASchemaThatKeepsTheValue_WhenOptional_ThenCarryBoth()
    {
        string? nickname = "ada";

        Outcome<Option<string>> outcome =
            Evaluate(Schema.Optional(nickname, new RefinesAndKeeps<string>()));

        outcome.Value.ShouldBe(Option.Some("ada"));
        outcome.Violations.Count.ShouldBe(1);
    }

    [Fact]
    public void GivenANullSchema_WhenOptional_ThenThrow()
    {
        Should.Throw<ArgumentNullException>(
            () => Schema.Optional(Option.Some("x"), (Schema<string, string>)null!));
    }

    [Fact]
    public void GivenANullOption_WhenOptional_ThenThrow()
    {
        Should.Throw<ArgumentNullException>(
            () => Schema.Optional((Option<string>)null!, new PassThrough<string>()));
    }

    [Fact]
    public void GivenAnAbsentReference_WhenForbidden_ThenPass()
    {
        string? legacy = null;

        Outcome<Checked> outcome =
            Evaluate(Schema.Forbidden(legacy, "Not accepted."));

        outcome.Value.ShouldBe(Checked.Instance);
        outcome.Violations.ShouldBeEmpty();
    }

    [Fact]
    public void GivenAPresentReference_WhenForbidden_ThenReportItNotAllowed()
    {
        string? legacy = "set";

        Outcome<Checked> outcome = Evaluate(
            Schema.Forbidden(legacy, "{Path} is not accepted, got {Received}."));

        outcome.HasValue.ShouldBeFalse();
        outcome.Violations[0].Code
               .ShouldBe(ViolationCodeCatalog.Codes.NotAllowed);
        outcome.Violations[0].Message
               .ShouldBe("legacy is not accepted, got set.");
    }

    [Fact]
    public void GivenAPresentValueType_WhenForbidden_ThenReportItNotAllowed()
    {
        int? legacy = 1;

        Evaluate(Schema.Forbidden(legacy, "No.")).HasValue.ShouldBeFalse();
    }

    [Fact]
    public void GivenAnAbsentValueType_WhenForbidden_ThenPass()
    {
        int? legacy = null;

        Evaluate(Schema.Forbidden(legacy, "No.")).HasValue.ShouldBeTrue();
    }

    [Fact]
    public void GivenAnOption_WhenForbidden_ThenCheckWhatItHolds()
    {
        Evaluate(Schema.Forbidden(Option.Some("x"), "No.")).HasValue
           .ShouldBeFalse();
    }

    [Fact]
    public void GivenANullMessage_WhenForbidden_ThenThrow()
    {
        Should.Throw<ArgumentNullException>(
            () => Schema.Forbidden(Option.Some("x"), null!));
    }

    [Fact]
    public void GivenANullOption_WhenForbidden_ThenThrow()
    {
        Should.Throw<ArgumentNullException>(
            () => Schema.Forbidden((Option<string>)null!, "No."));
    }

    [Fact]
    public void GivenPassingRules_WhenExtending_ThenPass()
    {
        Outcome<Checked> outcome =
            Evaluate(Schema.Extend("subject", new PassThrough<string>()));

        outcome.Value.ShouldBe(Checked.Instance);
        outcome.Violations.ShouldBeEmpty();
    }

    [Fact]
    public void GivenFailingRules_WhenExtending_ThenReportAtTheSubjectsOwnPath()
    {
        Outcome<Checked> outcome =
            Evaluate(Schema.Extend("subject", new Rejects<string>()));

        outcome.HasValue.ShouldBeFalse();
        outcome.Violations[0].Path.IsRoot.ShouldBeTrue();
    }

    [Fact]
    public void GivenRulesThatKeepTheValue_WhenExtending_ThenStillReportTheirViolations()
    {
        Evaluate(Schema.Extend("subject", new RefinesAndKeeps<string>()))
           .Violations.Count.ShouldBe(1);
    }

    [Fact]
    public void GivenNullRules_WhenExtending_ThenThrow()
    {
        Should.Throw<ArgumentNullException>(
            () => Schema.Extend("subject", (Schema<string, string>)null!));
    }

    [Fact]
    public void GivenAField_WhenEvaluatedThroughTheNonGenericBase_ThenYieldOnlyViolations()
    {
        Field field = Schema.Required((string?)null, new PassThrough<string>());

        IReadOnlyList<Violation> violations =
            field.Evaluate(ParseContext.Root);

        violations.Count.ShouldBe(1);
    }
}
