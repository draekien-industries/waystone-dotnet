namespace Waystone.Monads.Schemas;

using System;
using Shouldly;
using Waystone.Monads.Results.Errors;
using Xunit;

public sealed class ViolationTests
{
    [Fact]
    public void GivenPathCodeAndMessage_WhenConstructing_ThenExposeAllThree()
    {
        ViolationPath path = ViolationPath.Root.Append("sku");
        ErrorCode code = ViolationCodeCatalog.Codes.Incomplete;

        var sut = new Violation(path, code, "Expected sku.");

        sut.Path.ShouldBe(path);
        sut.Code.ShouldBe(code);
        sut.Message.ShouldBe("Expected sku.");
    }

    [Fact]
    public void GivenADomainCode_WhenConstructing_ThenKeepItRatherThanMapIt()
    {
        var sut = new Violation(
            ViolationPath.Root,
            new ErrorCode("order.line_count_exceeded"),
            "Too many lines.");

        sut.Code.Value.ShouldBe("order.line_count_exceeded");
    }

    [Fact]
    public void GivenNullPath_WhenConstructing_ThenThrow() =>
        Should.Throw<ArgumentNullException>(
            () => new Violation(
                null!,
                ViolationCodeCatalog.Codes.Malformed,
                "x"));

    [Fact]
    public void GivenNullCode_WhenConstructing_ThenThrow() =>
        Should.Throw<ArgumentNullException>(
            () => new Violation(ViolationPath.Root, null!, "x"));

    [Fact]
    public void GivenTwoViolationsWithTheSameParts_WhenComparing_ThenTheyAreEqual()
    {
        var left = new Violation(
            ViolationPath.Root.Append("sku"),
            ViolationCodeCatalog.Codes.Incomplete,
            "Expected sku.");

        var right = new Violation(
            ViolationPath.Root.Append("sku"),
            ViolationCodeCatalog.Codes.Incomplete,
            "Expected sku.");

        left.ShouldBe(right);
        left.GetHashCode().ShouldBe(right.GetHashCode());
    }

    [Fact]
    public void GivenDifferentPaths_WhenComparing_ThenTheyAreNotEqual()
    {
        var left = new Violation(
            ViolationPath.Root.Append("sku"),
            ViolationCodeCatalog.Codes.Incomplete,
            "Expected sku.");

        var right = new Violation(
            ViolationPath.Root.Append("name"),
            ViolationCodeCatalog.Codes.Incomplete,
            "Expected sku.");

        left.ShouldNotBe(right);
    }
}
