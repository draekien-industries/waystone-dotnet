namespace Waystone.Monads.Schemas;

using System;
using System.Globalization;
using Shouldly;
using Xunit;

public sealed class SchemaTextTemporalConversionTests
{
    private static readonly ParseContext At = ParseContext.Root.At("dueAt");

    private static readonly CultureInfo Australian = CultureInfo.GetCultureInfo("en-AU");

    [Fact]
    public void GivenAnOffset_WhenConvertingToTimestamp_ThenKeepIt()
    {
        Outcome<DateTimeOffset> outcome = Schema.Text
                                                .ToTimestamp()
                                                .Evaluate("2026-10-01T09:00:00+10:00", At);

        outcome.Value.ShouldBe(
            new DateTimeOffset(2026, 10, 1, 9, 0, 0, TimeSpan.FromHours(10)));
    }

    [Fact]
    public void GivenUtc_WhenConvertingToTimestamp_ThenUseAZeroOffset()
    {
        Outcome<DateTimeOffset> outcome = Schema.Text
                                                .ToTimestamp()
                                                .Evaluate("2026-10-01T09:00:00Z", At);

        outcome.Value.ShouldBe(
            new DateTimeOffset(2026, 10, 1, 9, 0, 0, TimeSpan.Zero));
    }

    [Fact]
    public void GivenNoOffset_WhenConvertingToTimestamp_ThenUseTheLocalTimeZone()
    {
        var local = new DateTime(2026, 10, 1, 9, 0, 0);

        Outcome<DateTimeOffset> outcome = Schema.Text
                                                .ToTimestamp()
                                                .Evaluate("2026-10-01T09:00:00", At);

        outcome.Value.ShouldBe(
            new DateTimeOffset(local, TimeZoneInfo.Local.GetUtcOffset(local)));
    }

    [Fact]
    public void GivenNoTime_WhenConvertingToTimestamp_ThenUseMidnight()
    {
        Outcome<DateTimeOffset> outcome = Schema.Text
                                                .ToTimestamp()
                                                .Evaluate("2026-10-01+10:00", At);

        outcome.Value.ShouldBe(
            new DateTimeOffset(2026, 10, 1, 0, 0, 0, TimeSpan.FromHours(10)));
    }

    [Fact]
    public void GivenANumericDateInTheInvariantCulture_WhenConvertingToTimestamp_ThenReadTheMonthFirst()
    {
        Outcome<DateTimeOffset> outcome = Schema.Text
                                                .ToTimestamp()
                                                .Evaluate("01/10/2026 +00:00", At);

        outcome.Value.ShouldBe(
            new DateTimeOffset(2026, 1, 10, 0, 0, 0, TimeSpan.Zero));
    }

    [Fact]
    public void GivenAProvider_WhenConvertingToTimestamp_ThenReadItsDateOrder()
    {
        Outcome<DateTimeOffset> outcome = Schema.Text
                                                .ToTimestamp(Australian)
                                                .Evaluate("01/10/2026 +00:00", At);

        outcome.Value.ShouldBe(
            new DateTimeOffset(2026, 10, 1, 0, 0, 0, TimeSpan.Zero));
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("")]
    [InlineData("2026-13-01")]
    public void GivenTextThatIsNotATimestamp_WhenConverting_ThenReportItMalformed(
        string text)
    {
        Outcome<DateTimeOffset> outcome = Schema.Text.ToTimestamp().Evaluate(text, At);

        outcome.HasValue.ShouldBeFalse();

        Violation violation = outcome.Violations.ShouldHaveSingleItem();
        violation.Code.ShouldBe(ViolationCodeCatalog.Codes.Malformed);
        violation.Message.ShouldBe(
            $"Expected dueAt to be a date and time, but got {text}.");
    }

    [Fact]
    public void GivenANullArgument_WhenConvertingToTimestamp_ThenThrow()
    {
        Should.Throw<ArgumentNullException>(() => Schema.Text.ToTimestamp(null!))
              .ParamName.ShouldBe("provider");

        Should.Throw<ArgumentNullException>(
                   () => ((Schema<string, string>)null!).ToTimestamp())
              .ParamName.ShouldBe("schema");
    }

#if NET8_0_OR_GREATER
    [Fact]
    public void GivenAnIsoDate_WhenConvertingToDate_ThenProduceIt()
    {
        Outcome<DateOnly> outcome = Schema.Text.ToDate().Evaluate("2026-10-01", At);

        outcome.Value.ShouldBe(new DateOnly(2026, 10, 1));
    }

    [Fact]
    public void GivenANumericDateInTheInvariantCulture_WhenConvertingToDate_ThenReadTheMonthFirst()
    {
        Outcome<DateOnly> outcome = Schema.Text.ToDate().Evaluate("01/10/2026", At);

        outcome.Value.ShouldBe(new DateOnly(2026, 1, 10));
    }

    [Fact]
    public void GivenAProvider_WhenConvertingToDate_ThenReadItsDateOrder()
    {
        Outcome<DateOnly> outcome = Schema.Text
                                          .ToDate(Australian)
                                          .Evaluate("01/10/2026", At);

        outcome.Value.ShouldBe(new DateOnly(2026, 10, 1));
    }

    [Fact]
    public void GivenAnIsoTimeOfDay_WhenConvertingToDate_ThenDiscardIt()
    {
        Outcome<DateOnly> outcome = Schema.Text
                                          .ToDate()
                                          .Evaluate("2026-10-01T09:00:00", At);

        outcome.Violations.ShouldBeEmpty();
        outcome.Value.ShouldBe(new DateOnly(2026, 10, 1));
    }

    [Theory]
    [InlineData("2026-10-01T09:00:00+10:00")]
    [InlineData("abc")]
    [InlineData("")]
    public void GivenTextThatIsNotADate_WhenConverting_ThenReportItMalformed(
        string text)
    {
        Outcome<DateOnly> outcome = Schema.Text.ToDate().Evaluate(text, At);

        outcome.Violations.ShouldHaveSingleItem()
               .Message.ShouldBe($"Expected dueAt to be a date, but got {text}.");
    }

    [Fact]
    public void GivenANullArgument_WhenConvertingToDate_ThenThrow()
    {
        Should.Throw<ArgumentNullException>(() => Schema.Text.ToDate(null!))
              .ParamName.ShouldBe("provider");

        Should.Throw<ArgumentNullException>(
                   () => ((Schema<string, string>)null!).ToDate())
              .ParamName.ShouldBe("schema");
    }
#endif
}
