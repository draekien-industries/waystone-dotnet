namespace Waystone.Monads.Schemas;

using System;
using Shouldly;
using Xunit;

/// <summary>
/// The text rules that match a shape or a literal, as opposed to the length and
/// expression rules in <c>SchemaTextRuleTests</c>. The email subset itself is
/// pinned in <c>EmailAddressTests</c>; these cases only prove the rule reports
/// what the scan decided.
/// </summary>
public sealed class SchemaTextFormatRuleTests
{
    private static readonly ParseContext At = ParseContext.Root.At("contact");

    [Fact]
    public void GivenAnAddress_WhenRequiringAnEmail_ThenReportNothing() =>
        Schema.Text.Email()
              .Evaluate("ada@example.com", At)
              .Violations.ShouldBeEmpty();

    [Fact]
    public void GivenSomethingElse_WhenRequiringAnEmail_ThenReportMalformed()
    {
        Violation violation = Schema.Text.Email()
                                    .Evaluate("ada", At)
                                    .Violations.ShouldHaveSingleItem();

        violation.Message.ShouldBe(
            "Expected contact to be an email address, but got ada.");

        violation.Code.ShouldBe(ViolationCodeCatalog.Codes.Malformed);
    }

    [Theory]
    [InlineData("https://example.com", true)]
    [InlineData("javascript:alert(1)", true)]
    [InlineData("/quests/3", false)]
    [InlineData("not a url", false)]
    public void GivenAValue_WhenRequiringAnyUrl_ThenJudgeItAbsoluteOrNot(
        string value,
        bool accepted) =>
        Schema.Text.Url().Evaluate(value, At).Violations.Count.ShouldBe(
            accepted ? 0 : 1);

    [Theory]
    [InlineData("https://example.com", true)]
    [InlineData("HTTPS://example.com", true)]
    [InlineData("http://example.com", true)]
    [InlineData("javascript:alert(1)", false)]
    [InlineData("file:///etc/passwd", false)]
    [InlineData("not a url", false)]
    public void GivenAValue_WhenRestrictingTheScheme_ThenAcceptOnlyThose(
        string value,
        bool accepted) =>
        Schema.Text.Url("https", "http")
              .Evaluate(value, At)
              .Violations.Count.ShouldBe(accepted ? 0 : 1);

    [Fact]
    public void GivenARejectedScheme_WhenRestricting_ThenNameTheOnesAllowed() =>
        Schema.Text.Url("https", "http")
              .Evaluate("file:///etc/passwd", At)
              .Violations[0].Message
              .ShouldBe(
                   "Expected contact to be a https or http URL, but got file:///etc/passwd.");

    /// <summary>
    /// An empty list accepts nothing rather than quietly becoming the
    /// unrestricted rule, which is the reading that would be a security hole.
    /// </summary>
    [Fact]
    public void GivenNoSchemes_WhenRestricting_ThenAcceptNothing() =>
        Schema.Text.Url()
              .Url([])
              .Evaluate("https://example.com", At)
              .Violations.ShouldHaveSingleItem();

    [Theory]
    [InlineData("gold", true)]
    [InlineData("silver", true)]
    [InlineData("GOLD", false)]
    [InlineData("bronze", false)]
    public void GivenAValue_WhenRestrictingToASet_ThenCompareOrdinally(
        string value,
        bool accepted) =>
        Schema.Text.OneOf("gold", "silver")
              .Evaluate(value, At)
              .Violations.Count.ShouldBe(accepted ? 0 : 1);

    [Theory]
    [InlineData("GOLD", true)]
    [InlineData("bronze", false)]
    public void GivenAValue_WhenRestrictingWithAComparison_ThenUseIt(
        string value,
        bool accepted) =>
        Schema.Text.OneOf(StringComparison.OrdinalIgnoreCase, "gold", "silver")
              .Evaluate(value, At)
              .Violations.Count.ShouldBe(accepted ? 0 : 1);

    [Fact]
    public void GivenAValueOutsideTheSet_WhenRestricting_ThenNameTheSet()
    {
        Violation violation = Schema.Text.OneOf("gold", "silver")
                                    .Evaluate("bronze", At)
                                    .Violations.ShouldHaveSingleItem();

        violation.Message.ShouldBe(
            "Expected contact to be one of gold, silver, but got bronze.");

        violation.Code.ShouldBe(ViolationCodeCatalog.Codes.NotAllowed);
    }

    [Theory]
    [InlineData("abcd", true)]
    [InlineData("abc", false)]
    [InlineData("abcde", false)]
    public void GivenAValue_WhenFixingTheLength_ThenAcceptOnlyThatLength(
        string value,
        bool accepted) =>
        Schema.Text.Length(4)
              .Evaluate(value, At)
              .Violations.Count.ShouldBe(accepted ? 0 : 1);

    [Fact]
    public void GivenTheWrongLength_WhenFixingIt_ThenNameTheLength() =>
        Schema.Text.Length(4)
              .Evaluate("abc", At)
              .Violations[0].Message
              .ShouldBe("Expected contact to be exactly 4 characters.");

    [Theory]
    [InlineData("ab", false)]
    [InlineData("abc", true)]
    [InlineData("abcde", true)]
    [InlineData("abcdef", false)]
    public void GivenAValue_WhenBoundingTheLength_ThenIncludeBothEnds(
        string value,
        bool accepted) =>
        Schema.Text.LengthBetween(3, 5)
              .Evaluate(value, At)
              .Violations.Count.ShouldBe(accepted ? 0 : 1);

    [Fact]
    public void GivenALengthOutsideTheRange_WhenBounding_ThenNameBothEnds() =>
        Schema.Text.LengthBetween(3, 5)
              .Evaluate("ab", At)
              .Violations[0].Message
              .ShouldBe("Expected contact to be between 3 and 5 characters.");

    [Fact]
    public void GivenAnInvertedLengthRange_WhenBounding_ThenThrowAtBuildTime() =>
        Should.Throw<ArgumentException>(
                   () => Schema.Text.LengthBetween(5, 3))
              .ParamName.ShouldBe("max");

    [Fact]
    public void GivenASingleLengthRange_WhenBounding_ThenAcceptThatLength() =>
        Schema.Text.LengthBetween(3, 3)
              .Evaluate("abc", At)
              .Violations.ShouldBeEmpty();

    [Theory]
    [InlineData("tag:hero", true)]
    [InlineData("TAG:hero", false)]
    [InlineData("hero", false)]
    public void GivenAValue_WhenRequiringAPrefix_ThenMatchItLiterally(
        string value,
        bool accepted) =>
        Schema.Text.StartsWith("tag:")
              .Evaluate(value, At)
              .Violations.Count.ShouldBe(accepted ? 0 : 1);

    [Fact]
    public void GivenACasedPrefix_WhenGivenAComparison_ThenUseIt() =>
        Schema.Text.StartsWith("tag:", StringComparison.OrdinalIgnoreCase)
              .Evaluate("TAG:hero", At)
              .Violations.ShouldBeEmpty();

    [Fact]
    public void GivenAMissingPrefix_WhenRequiringOne_ThenNameIt()
    {
        Violation violation = Schema.Text.StartsWith("tag:")
                                    .Evaluate("hero", At)
                                    .Violations.ShouldHaveSingleItem();

        violation.Message.ShouldBe(
            "Expected contact to start with tag:, but got hero.");

        violation.Code.ShouldBe(ViolationCodeCatalog.Codes.Malformed);
    }

    [Theory]
    [InlineData("quest.md", true)]
    [InlineData("quest.MD", false)]
    [InlineData("quest.txt", false)]
    public void GivenAValue_WhenRequiringASuffix_ThenMatchItLiterally(
        string value,
        bool accepted) =>
        Schema.Text.EndsWith(".md")
              .Evaluate(value, At)
              .Violations.Count.ShouldBe(accepted ? 0 : 1);

    [Fact]
    public void GivenACasedSuffix_WhenGivenAComparison_ThenUseIt() =>
        Schema.Text.EndsWith(".md", StringComparison.OrdinalIgnoreCase)
              .Evaluate("quest.MD", At)
              .Violations.ShouldBeEmpty();

    [Fact]
    public void GivenAMissingSuffix_WhenRequiringOne_ThenNameIt() =>
        Schema.Text.EndsWith(".md")
              .Evaluate("quest.txt", At)
              .Violations[0].Message
              .ShouldBe("Expected contact to end with .md, but got quest.txt.");

    [Theory]
    [InlineData("a@b", true)]
    [InlineData("ab", false)]
    public void GivenAValue_WhenRequiringALiteral_ThenLookForIt(
        string value,
        bool accepted) =>
        Schema.Text.Contains("@")
              .Evaluate(value, At)
              .Violations.Count.ShouldBe(accepted ? 0 : 1);

    [Fact]
    public void GivenACasedLiteral_WhenGivenAComparison_ThenUseIt() =>
        Schema.Text.Contains("QUEST", StringComparison.OrdinalIgnoreCase)
              .Evaluate("a quest here", At)
              .Violations.ShouldBeEmpty();

    [Fact]
    public void GivenAMissingLiteral_WhenRequiringOne_ThenNameIt() =>
        Schema.Text.Contains("@")
              .Evaluate("ab", At)
              .Violations[0].Message
              .ShouldBe("Expected contact to contain @, but got ab.");

    [Fact]
    public void GivenNoSchemeArray_WhenRestricting_ThenThrow() =>
        Should.Throw<ArgumentNullException>(
                   () => Schema.Text.Url(null!))
              .ParamName.ShouldBe("schemes");

    [Fact]
    public void GivenANullScheme_WhenRestricting_ThenThrow() =>
        Should.Throw<ArgumentNullException>(
                   () => Schema.Text.Url("https", null!))
              .ParamName.ShouldBe("schemes");

    [Fact]
    public void GivenNoAcceptedArray_WhenRestrictingToASet_ThenThrow() =>
        Should.Throw<ArgumentNullException>(
                   () => Schema.Text.OneOf(null!))
              .ParamName.ShouldBe("accepted");

    [Fact]
    public void GivenANullAcceptedValue_WhenRestrictingToASet_ThenThrow() =>
        Should.Throw<ArgumentNullException>(
                   () => Schema.Text.OneOf("gold", null!))
              .ParamName.ShouldBe("accepted");

    [Fact]
    public void GivenNoPrefix_WhenRequiringOne_ThenThrow() =>
        Should.Throw<ArgumentNullException>(
            () => Schema.Text.StartsWith(null!));

    [Fact]
    public void GivenNoSuffix_WhenRequiringOne_ThenThrow() =>
        Should.Throw<ArgumentNullException>(
            () => Schema.Text.EndsWith(null!));

    [Fact]
    public void GivenNoLiteral_WhenRequiringOne_ThenThrow() =>
        Should.Throw<ArgumentNullException>(
            () => Schema.Text.Contains(null!));
}
