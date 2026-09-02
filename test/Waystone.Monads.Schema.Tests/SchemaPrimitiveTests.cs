namespace Waystone.Monads.Schemas;

using System;
using Shouldly;
using Xunit;

public sealed class SchemaPrimitiveTests
{
    private static readonly ParseContext At = ParseContext.Root.At("status");

    internal enum Status
    {
        Draft,
        Sent,
    }

    [Fact]
    public void GivenAnyValue_WhenUsingAPrimitive_ThenProduceItUnchanged()
    {
        Schema.Text.Evaluate("alice", At).Value.ShouldBe("alice");
        Schema.Bool.Evaluate(true, At).Value.ShouldBe(true);
        Schema.Number.Int32.Evaluate(7, At).Value.ShouldBe(7);
        Schema.Number.Int64.Evaluate(7L, At).Value.ShouldBe(7L);
        Schema.Number.Decimal.Evaluate(7.5m, At).Value.ShouldBe(7.5m);
        Schema.Number.Double.Evaluate(7.5d, At).Value.ShouldBe(7.5d);
    }

    [Fact]
    public void GivenAnyValue_WhenUsingAPrimitive_ThenReportNothing()
    {
        Schema.Text.Evaluate(string.Empty, At).Violations.ShouldBeEmpty();
        Schema.Id.Evaluate(Guid.Empty, At).Violations.ShouldBeEmpty();

        Schema.Timestamp.Evaluate(DateTimeOffset.MinValue, At)
              .Violations.ShouldBeEmpty();
    }

    [Fact]
    public void GivenTheSameType_WhenAskingForASchema_ThenReuseOneInstance()
    {
        Schema.For<string>().ShouldBeSameAs(Schema.Text);
        Schema.For<bool>().ShouldBeSameAs(Schema.Bool);
        Schema.For<Guid>().ShouldBeSameAs(Schema.Id);
        Schema.For<DateTimeOffset>().ShouldBeSameAs(Schema.Timestamp);
        Schema.For<int>().ShouldBeSameAs(Schema.Number.Int32);
        Schema.For<long>().ShouldBeSameAs(Schema.Number.Int64);
        Schema.For<decimal>().ShouldBeSameAs(Schema.Number.Decimal);
        Schema.For<double>().ShouldBeSameAs(Schema.Number.Double);
        Schema.Enum<Status>().ShouldBeSameAs(Schema.Enum<Status>());
    }

    [Fact]
    public void GivenADeclaredMember_WhenParsingAnEnumeration_ThenAcceptIt()
    {
        Outcome<Status> outcome = Schema.Enum<Status>().Evaluate(Status.Sent, At);

        outcome.Violations.ShouldBeEmpty();
        outcome.Value.ShouldBe(Status.Sent);
    }

    [Fact]
    public void GivenAValueOutsideTheEnumeration_WhenParsing_ThenReportIt()
    {
        Outcome<Status> outcome =
            Schema.Enum<Status>().Evaluate((Status)97, At);

        Violation violation = outcome.Violations.ShouldHaveSingleItem();
        violation.Code.ShouldBe(ViolationCodeCatalog.Codes.Mismatched);
        violation.Path.ToString().ShouldBe("status");

        violation.Message.ShouldBe(
            "Expected status to be a recognised value, but got 97.");
    }

    [Fact]
    public void GivenAnUndefinedMember_WhenParsingAnEnumeration_ThenKeepTheValue()
    {
        Schema.Enum<Status>().Evaluate((Status)97, At).HasValue.ShouldBeTrue();
    }

#if NET8_0_OR_GREATER
    [Fact]
    public void GivenADate_WhenUsingTheDateSchema_ThenProduceItUnchanged()
    {
        var day = new DateOnly(2026, 9, 2);

        Schema.Date.Evaluate(day, At).Value.ShouldBe(day);
        Schema.For<DateOnly>().ShouldBeSameAs(Schema.Date);
    }
#endif
}
