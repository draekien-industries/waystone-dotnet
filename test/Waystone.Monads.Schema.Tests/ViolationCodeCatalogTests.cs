namespace Waystone.Monads.Schemas;

using Shouldly;
using Waystone.Monads.Results.Errors;
using Xunit;

/// <summary>
/// The codes reach consumers, who branch on them. Nothing in the build fails when
/// the format attribute or a member name changes, so these pin the strings.
/// </summary>
public sealed class ViolationCodeCatalogTests
{
    [Theory]
    [InlineData(ViolationCode.Incomplete, "schema_violation.incomplete")]
    [InlineData(ViolationCode.Malformed, "schema_violation.malformed")]
    [InlineData(ViolationCode.NotAllowed, "schema_violation.not-allowed")]
    [InlineData(ViolationCode.OutOfRange, "schema_violation.out-of-range")]
    [InlineData(ViolationCode.Mismatched, "schema_violation.mismatched")]
    [InlineData(ViolationCode.Duplicate, "schema_violation.duplicate")]
    [InlineData(ViolationCode.Conflicting, "schema_violation.conflicting")]
    public void GivenAViolationCode_WhenReadingItsCode_ThenUseTheKebabCasedPrefixedName(
        ViolationCode code,
        string expected)
    {
        code.ToErrorCode().Value.ShouldBe(expected);
        code.ToErrorCodeName().ShouldBe(expected);
    }

    [Fact]
    public void GivenTheCatalog_WhenReadingACode_ThenItMatchesTheExtension()
    {
        ViolationCodeCatalog.Codes.Duplicate.ShouldBe(
            ViolationCode.Duplicate.ToErrorCode());
    }

    [Fact]
    public void GivenTheCatalog_WhenBuildingAnError_ThenCarryTheCodeAndMessage()
    {
        Error error = ViolationCodeCatalog.Errors.Mismatched("Wrong shape.");

        error.Code.Value.ShouldBe("schema_violation.mismatched");
        error.Message.ShouldBe("Wrong shape.");
    }

    [Fact]
    public void GivenTheAggregateCode_WhenComparedToAMemberCode_ThenTheyShareAPrefix()
    {
        ViolationCodeCatalog.Codes.Incomplete.Value.ShouldStartWith(
            SchemaViolation.ErrorCodeName);
    }
}
