namespace Waystone.Monads.Schemas;

using System;
using System.Globalization;
using Shouldly;
using Xunit;

public sealed class SchemaTextNumberConversionTests
{
    private static readonly ParseContext At = ParseContext.Root.At("quantity");

    private static readonly NumberFormatInfo CommaDecimals = new()
    {
        NumberDecimalSeparator = ",",
        NumberGroupSeparator = ".",
    };

    [Theory]
    [InlineData("42", 42)]
    [InlineData("-7", -7)]
    [InlineData("+5", 5)]
    [InlineData("  42  ", 42)]
    [InlineData("2147483647", int.MaxValue)]
    [InlineData("-2147483648", int.MinValue)]
    public void GivenAnIntegerInRange_WhenConvertingToInt32_ThenProduceIt(
        string text,
        int expected)
    {
        Outcome<int> outcome = Schema.Text.ToInt32().Evaluate(text, At);

        outcome.Violations.ShouldBeEmpty();
        outcome.Value.ShouldBe(expected);
    }

    [Theory]
    [InlineData("1.0")]
    [InlineData("1,000")]
    [InlineData("1e3")]
    [InlineData("2147483648")]
    [InlineData("abc")]
    [InlineData("")]
    public void GivenTextThatIsNotAnInt32_WhenConverting_ThenReportItMalformed(
        string text)
    {
        Outcome<int> outcome = Schema.Text.ToInt32().Evaluate(text, At);

        outcome.HasValue.ShouldBeFalse();

        Violation violation = outcome.Violations.ShouldHaveSingleItem();
        violation.Code.ShouldBe(ViolationCodeCatalog.Codes.Malformed);
        violation.Message.ShouldBe(
            $"Expected quantity to be a whole number from -2147483648 to 2147483647, but got {text}.");
    }

    [Fact]
    public void GivenAProvider_WhenConvertingToInt32_ThenStillRejectAThousandsSeparator()
    {
        Outcome<int> outcome = Schema.Text
                                     .ToInt32(CommaDecimals)
                                     .Evaluate("1.000", At);

        outcome.HasValue.ShouldBeFalse();
    }

    [Theory]
    [InlineData("9223372036854775807", long.MaxValue)]
    [InlineData("-9223372036854775808", long.MinValue)]
    [InlineData(" 12 ", 12L)]
    public void GivenAnIntegerInRange_WhenConvertingToInt64_ThenProduceIt(
        string text,
        long expected)
    {
        Outcome<long> outcome = Schema.Text.ToInt64().Evaluate(text, At);

        outcome.Violations.ShouldBeEmpty();
        outcome.Value.ShouldBe(expected);
    }

    [Theory]
    [InlineData("9223372036854775808")]
    [InlineData("1.5")]
    [InlineData("1,000")]
    public void GivenTextThatIsNotAnInt64_WhenConverting_ThenReportItMalformed(
        string text)
    {
        Outcome<long> outcome = Schema.Text.ToInt64().Evaluate(text, At);

        outcome.Violations.ShouldHaveSingleItem()
               .Message.ShouldBe(
                    $"Expected quantity to be a whole number from -9223372036854775808 to 9223372036854775807, but got {text}.");
    }

    [Fact]
    public void GivenAProvider_WhenConvertingToInt64_ThenUseIt()
    {
        Outcome<long> outcome = Schema.Text
                                      .ToInt64(CommaDecimals)
                                      .Evaluate("-12", At);

        outcome.Value.ShouldBe(-12L);
    }

    [Theory]
    [InlineData("1.5", "1.5")]
    [InlineData("1,234.5", "1234.5")]
    [InlineData("1,5", "15")]
    [InlineData("-0.25", "-0.25")]
    [InlineData(" 3 ", "3")]
    public void GivenANumberInTheInvariantCulture_WhenConvertingToDecimal_ThenProduceIt(
        string text,
        string expected)
    {
        Outcome<decimal> outcome = Schema.Text.ToDecimal().Evaluate(text, At);

        outcome.Violations.ShouldBeEmpty();
        outcome.Value.ShouldBe(decimal.Parse(expected, CultureInfo.InvariantCulture));
    }

    [Theory]
    [InlineData("$5")]
    [InlineData("1e3")]
    [InlineData("abc")]
    public void GivenTextThatIsNotADecimal_WhenConverting_ThenReportItMalformed(
        string text)
    {
        Outcome<decimal> outcome = Schema.Text.ToDecimal().Evaluate(text, At);

        outcome.Violations.ShouldHaveSingleItem()
               .Message.ShouldBe(
                    $"Expected quantity to be a decimal number, but got {text}.");
    }

    [Theory]
    [InlineData("1,5", "1.5")]
    [InlineData("1.234,5", "1234.5")]
    public void GivenAProvider_WhenConvertingToDecimal_ThenReadItsSeparators(
        string text,
        string expected)
    {
        Outcome<decimal> outcome = Schema.Text
                                         .ToDecimal(CommaDecimals)
                                         .Evaluate(text, At);

        outcome.Value.ShouldBe(decimal.Parse(expected, CultureInfo.InvariantCulture));
    }

    [Theory]
    [InlineData("1e3", 1000d)]
    [InlineData("1,000.5", 1000.5d)]
    [InlineData("-2.5", -2.5d)]
    [InlineData("Infinity", double.PositiveInfinity)]
    public void GivenANumberInTheInvariantCulture_WhenConvertingToDouble_ThenProduceIt(
        string text,
        double expected)
    {
        Outcome<double> outcome = Schema.Text.ToDouble().Evaluate(text, At);

        outcome.Violations.ShouldBeEmpty();
        outcome.Value.ShouldBe(expected);
    }

    [Fact]
    public void GivenNaN_WhenConvertingToDouble_ThenAcceptIt()
    {
        Outcome<double> outcome = Schema.Text.ToDouble().Evaluate("NaN", At);

        double.IsNaN(outcome.Value).ShouldBeTrue();
    }

    [Theory]
    [InlineData("$5")]
    [InlineData("abc")]
    [InlineData("")]
    public void GivenTextThatIsNotADouble_WhenConverting_ThenReportItMalformed(
        string text)
    {
        Outcome<double> outcome = Schema.Text.ToDouble().Evaluate(text, At);

        outcome.Violations.ShouldHaveSingleItem()
               .Message.ShouldBe(
                    $"Expected quantity to be a number, but got {text}.");
    }

    [Fact]
    public void GivenAProvider_WhenConvertingToDouble_ThenReadItsSeparators()
    {
        Outcome<double> outcome = Schema.Text
                                        .ToDouble(CommaDecimals)
                                        .Evaluate("1.000,5", At);

        outcome.Value.ShouldBe(1000.5d);
    }

    [Fact]
    public void GivenANullProvider_WhenConverting_ThenThrow()
    {
        Should.Throw<ArgumentNullException>(() => Schema.Text.ToInt32(null!))
              .ParamName.ShouldBe("provider");

        Should.Throw<ArgumentNullException>(() => Schema.Text.ToInt64(null!))
              .ParamName.ShouldBe("provider");

        Should.Throw<ArgumentNullException>(() => Schema.Text.ToDecimal(null!))
              .ParamName.ShouldBe("provider");

        Should.Throw<ArgumentNullException>(() => Schema.Text.ToDouble(null!))
              .ParamName.ShouldBe("provider");
    }

    [Fact]
    public void GivenANullSchema_WhenConverting_ThenThrow()
    {
        var schema = (Schema<string, string>)null!;

        Should.Throw<ArgumentNullException>(() => schema.ToInt32())
              .ParamName.ShouldBe("schema");

        Should.Throw<ArgumentNullException>(() => schema.ToInt64())
              .ParamName.ShouldBe("schema");

        Should.Throw<ArgumentNullException>(() => schema.ToDecimal())
              .ParamName.ShouldBe("schema");

        Should.Throw<ArgumentNullException>(() => schema.ToDouble())
              .ParamName.ShouldBe("schema");
    }
}
