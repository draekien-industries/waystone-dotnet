namespace Waystone.Monads.Schemas;

using System;
using System.Text.RegularExpressions;
using Shouldly;
using Xunit;

public sealed class SchemaTextRuleTests
{
    private static readonly ParseContext At = ParseContext.Root.At("name");

    [Fact]
    public void GivenPaddedText_WhenTrimming_ThenRemoveTheWhitespace()
    {
        Schema.Text.Trim().Evaluate("  alice  ", At).Value.ShouldBe("alice");
    }

    [Fact]
    public void GivenOnlyWhitespace_WhenTrimmingBeforeRequiringText_ThenReportIt()
    {
        Schema.Text.Trim()
              .NotEmpty()
              .Evaluate("   ", At)
              .Violations.ShouldHaveSingleItem()
              .Message.ShouldBe("Expected name not to be empty.");
    }

    [Fact]
    public void GivenOnlyWhitespace_WhenRequiringTextBeforeTrimming_ThenAcceptIt()
    {
        Outcome<string> outcome =
            Schema.Text.NotEmpty().Trim().Evaluate("   ", At);

        outcome.Violations.ShouldBeEmpty();
        outcome.Value.ShouldBeEmpty();
    }

    [Fact]
    public void GivenNonEmptyText_WhenRequiringText_ThenReportNothing()
    {
        Schema.Text.NotEmpty().Evaluate("a", At).Violations.ShouldBeEmpty();
    }

    [Fact]
    public void GivenEmptyText_WhenRequiringText_ThenReportOutOfRange()
    {
        Schema.Text.NotEmpty()
              .Evaluate(string.Empty, At)
              .Violations.ShouldHaveSingleItem()
              .Code.ShouldBe(ViolationCodeCatalog.Codes.OutOfRange);
    }

    [Theory]
    [InlineData("ab", 1)]
    [InlineData("ab", 2)]
    public void GivenTextAtOrAboveTheMinimum_WhenBounding_ThenAcceptIt(
        string value,
        int length)
    {
        Schema.Text.MinLength(length)
              .Evaluate(value, At)
              .Violations.ShouldBeEmpty();
    }

    [Fact]
    public void GivenTextBelowTheMinimum_WhenBounding_ThenNameTheLength()
    {
        Schema.Text.MinLength(3)
              .Evaluate("ab", At)
              .Violations.ShouldHaveSingleItem()
              .Message.ShouldBe("Expected name to be at least 3 characters.");
    }

    [Theory]
    [InlineData("ab", 3)]
    [InlineData("ab", 2)]
    public void GivenTextAtOrBelowTheMaximum_WhenBounding_ThenAcceptIt(
        string value,
        int length)
    {
        Schema.Text.MaxLength(length)
              .Evaluate(value, At)
              .Violations.ShouldBeEmpty();
    }

    [Fact]
    public void GivenTextAboveTheMaximum_WhenBounding_ThenNameTheLength()
    {
        Schema.Text.MaxLength(1)
              .Evaluate("ab", At)
              .Violations.ShouldHaveSingleItem()
              .Message.ShouldBe("Expected name to be at most 1 characters.");
    }

    [Fact]
    public void GivenTextOutsideBothBounds_WhenBounding_ThenReportBoth()
    {
        Schema.Text.MinLength(5)
              .MaxLength(1)
              .Evaluate("abc", At)
              .Violations.Count.ShouldBe(2);
    }

    [Fact]
    public void GivenMatchingText_WhenMatchingAPattern_ThenReportNothing()
    {
        Schema.Text.Matches("^a+$")
              .Evaluate("aaa", At)
              .Violations.ShouldBeEmpty();
    }

    [Fact]
    public void GivenUnmatchedText_WhenMatchingAPattern_ThenNameThePattern()
    {
        Violation violation = Schema.Text.Matches("^a+$")
                                   .Evaluate("bbb", At)
                                   .Violations.ShouldHaveSingleItem();

        violation.Code.ShouldBe(ViolationCodeCatalog.Codes.Mismatched);
        violation.Message.ShouldBe("Expected name to match ^a+$, but got bbb.");
    }

    [Fact]
    public void GivenAnExpressionOfYourOwn_WhenMatching_ThenUseIt()
    {
        var pattern = new Regex("^A+$", RegexOptions.IgnoreCase);

        Schema.Text.Matches(pattern)
              .Evaluate("aaa", At)
              .Violations.ShouldBeEmpty();

        Schema.Text.Matches(pattern)
              .Evaluate("b", At)
              .Violations.ShouldHaveSingleItem();
    }

    [Fact]
    public void GivenASensitiveSchema_WhenTextFailsAPattern_ThenRedactTheValue()
    {
        Schema.Text.Matches("^a+$")
              .Sensitive()
              .Evaluate("hunter2", At)
              .Violations.ShouldHaveSingleItem()
              .Message.ShouldBe("Expected name to match ^a+$, but got ***.");
    }

    [Fact]
    public void GivenNoSchema_WhenAddingATextRule_ThenThrow()
    {
        Should.Throw<ArgumentNullException>(
                   () => ((Schema<string, string>)null!).Trim())
              .ParamName.ShouldBe("schema");

        Should.Throw<ArgumentNullException>(
                   () => ((Schema<string, string>)null!).NotEmpty())
              .ParamName.ShouldBe("schema");

        Should.Throw<ArgumentNullException>(
                   () => ((Schema<string, string>)null!).MinLength(1))
              .ParamName.ShouldBe("schema");

        Should.Throw<ArgumentNullException>(
                   () => ((Schema<string, string>)null!).MaxLength(1))
              .ParamName.ShouldBe("schema");

        Should.Throw<ArgumentNullException>(
                   () => ((Schema<string, string>)null!).Matches("a"))
              .ParamName.ShouldBe("schema");
    }

    [Fact]
    public void GivenNoPattern_WhenMatching_ThenThrow()
    {
        Should.Throw<ArgumentNullException>(
                   () => Schema.Text.Matches((string)null!))
              .ParamName.ShouldBe("pattern");

        Should.Throw<ArgumentNullException>(
                   () => Schema.Text.Matches((Regex)null!))
              .ParamName.ShouldBe("pattern");
    }
}
