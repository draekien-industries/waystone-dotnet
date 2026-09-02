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

    [Flags]
    public enum Permissions
    {
        None = 0,
        Read = 1,
        Write = 2,
        Delete = 4,
    }

    public enum Ranks
    {
        None = 0,
        Low = 1,
        High = 2,
        Top = 4,
    }

    [Flags]
    public enum Scopes
    {
        Read = 1,
        Write = 2,
    }

    [Flags]
    public enum Signed
    {
        None = 0,
        Everything = -1,
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

    /// <summary>
    /// A combined value is what a <c>[Flags]</c> enum exists to express, and it is
    /// never a declared member, so checking membership alone rejected input that
    /// was always legal.
    /// </summary>
    [Theory]
    [InlineData(Permissions.None, true)]
    [InlineData(Permissions.Read, true)]
    [InlineData(Permissions.Read | Permissions.Write, true)]
    [InlineData(Permissions.Read | Permissions.Write | Permissions.Delete, true)]
    [InlineData((Permissions)8, false)]
    [InlineData((Permissions)9, false)]
    [InlineData((Permissions)99, false)]
    public void GivenAFlagsEnumeration_WhenParsing_ThenAcceptDeclaredBitsOnly(
        Permissions value,
        bool accepted) =>
        Schema.Enum<Permissions>()
              .Evaluate(value, At)
              .Violations.Count.ShouldBe(accepted ? 0 : 1);

    /// <summary>
    /// Zero is made of no bits, so the bit test accepts it from every flags enum.
    /// It is also what a deserialiser produces for a field the payload left out, so
    /// accepting it where no member declares it certifies a value nobody sent.
    /// </summary>
    [Fact]
    public void GivenAFlagsEnumerationWithNoZeroMember_WhenParsingZero_ThenRejectIt()
    {
        Schema.Enum<Scopes>().Evaluate(default, At).Violations
              .ShouldHaveSingleItem();

        Schema.Enum<Permissions>()
              .Evaluate(default, At)
              .Violations.ShouldBeEmpty();
    }

    /// <summary>
    /// The attribute is the whole switch. <c>Ranks</c> declares the same numbers
    /// as <c>Permissions</c> and carries no <c>[Flags]</c>, so the value 3 is a
    /// combination there is no member for — which for a non-flags enum is a real
    /// mistake rather than a legal value.
    /// </summary>
    [Fact]
    public void GivenANonFlagsEnumeration_WhenParsingACombination_ThenRejectIt()
    {
        Schema.Enum<Ranks>().Evaluate((Ranks)3, At).Violations
              .ShouldHaveSingleItem();

        Schema.Enum<Permissions>()
              .Evaluate((Permissions)3, At)
              .Violations.ShouldBeEmpty();
    }

    /// <summary>
    /// A signed enum may declare a negative member, whose bit pattern does not fit
    /// an unsigned conversion. Reading it through <c>long</c> is what stops that
    /// throwing out of the parse.
    /// </summary>
    [Fact]
    public void GivenANegativeFlagsMember_WhenParsing_ThenAcceptItRatherThanThrow()
    {
        Schema.Enum<Signed>()
              .Evaluate(Signed.Everything, At)
              .Violations.ShouldBeEmpty();

        Schema.Enum<Signed>().Evaluate(Signed.None, At).Violations.ShouldBeEmpty();
    }
}
