namespace Waystone.Monads.SourceGenerators;

using Shouldly;
using Waystone.Monads.SourceGenerators.ErrorCodes;
using Xunit;

public sealed class ErrorCodeFormatTests
{
    [Theory]
    [InlineData("{enum}.{member}", "OrderError", "NotFound", "OrderError.NotFound")]
    [InlineData("{member}", "OrderError", "NotFound", "NotFound")]
    [InlineData("order.{member}", "OrderError", "NotFound", "order.NotFound")]
    [InlineData("{enum}/{member}", "OrderError", "NotFound", "OrderError/NotFound")]
    [InlineData("{{{member}}}", "OrderError", "NotFound", "{NotFound}")]
    public void AppliesLiteralsAndPlaceholders(
        string format,
        string enumName,
        string memberName,
        string expected)
    {
        ErrorCodeFormat.TryParse(format, out ErrorCodeFormat? parsed, out string? error)
                       .ShouldBeTrue(error);

        parsed!.Apply(enumName, memberName).ShouldBe(expected);
    }

    [Theory]
    [InlineData("kebab", "NotFound", "not-found")]
    [InlineData("kebab", "HTTPNotFound", "http-not-found")]
    [InlineData("kebab", "Error404", "error-404")]
    [InlineData("kebab", "Already_Shipped", "already-shipped")]
    [InlineData("kebab", "Payment", "payment")]
    [InlineData("snake", "NotFound", "not_found")]
    [InlineData("snake", "HTTPNotFound", "http_not_found")]
    [InlineData("lower", "NotFound", "notfound")]
    [InlineData("upper", "NotFound", "NOTFOUND")]
    public void AppliesTheCasing(string casing, string memberName, string expected)
    {
        ErrorCodeFormat.TryParse(
                            "{member:" + casing + "}",
                            out ErrorCodeFormat? parsed,
                            out string? error)
                       .ShouldBeTrue(error);

        parsed!.Apply("OrderError", memberName).ShouldBe(expected);
    }

    [Fact]
    public void CasesTheEnumAndMemberIndependently()
    {
        ErrorCodeFormat.TryParse(
            "{enum:upper}.{member:kebab}",
            out ErrorCodeFormat? parsed,
            out _);

        parsed!.Apply("OrderError", "NotFound").ShouldBe("ORDERERROR.not-found");
    }

    [Theory]
    [InlineData("", "the format is empty")]
    [InlineData("{member", "is not closed")]
    [InlineData("a}b{member}", "closes nothing")]
    [InlineData("{code}", "is not a placeholder")]
    [InlineData("{member:pascal}", "is not a casing")]
    public void RejectsAnUnusableFormat(string format, string expected)
    {
        ErrorCodeFormat.TryParse(format, out ErrorCodeFormat? parsed, out string? error)
                       .ShouldBeFalse();

        parsed.ShouldBeNull();
        error.ShouldNotBeNull().ShouldContain(expected);
    }

    [Theory]
    [InlineData("{enum}.{member}", true)]
    [InlineData("{member:kebab}", true)]
    [InlineData("{enum}", false)]
    [InlineData("literal", false)]
    public void KnowsWhetherTheMemberIsUsed(string format, bool expected)
    {
        ErrorCodeFormat.TryParse(format, out ErrorCodeFormat? parsed, out _);

        parsed!.UsesMember.ShouldBe(expected);
    }

    /// <summary>
    /// The undeclared-value expression folds every part that does not depend on the
    /// member into a literal, so the emitted code is a concatenation of constants
    /// around one <c>ToString()</c>.
    /// </summary>
    [Theory]
    [InlineData("{enum}.{member}", "\"OrderError.\" + value.ToString()")]
    [InlineData("{member}", "value.ToString()")]
    [InlineData("{enum:kebab}/{member}-x", "\"order-error/\" + value.ToString() + \"-x\"")]
    public void RendersTheUndeclaredValueExpression(string format, string expected)
    {
        ErrorCodeFormat.TryParse(format, out ErrorCodeFormat? parsed, out _);

        parsed!.ApplyToUndeclared("OrderError", "value.ToString()")
               .ShouldBe(expected);
    }

    /// <summary>
    /// All four casings are the identity on digits, which is why the member's casing
    /// is dropped from the undeclared-value expression rather than emitted as a
    /// runtime call.
    /// </summary>
    [Theory]
    [InlineData("kebab")]
    [InlineData("snake")]
    [InlineData("lower")]
    [InlineData("upper")]
    public void EveryCasingIsTheIdentityOnDigits(string casing)
    {
        ErrorCodeFormat.TryParse(
            "{member:" + casing + "}",
            out ErrorCodeFormat? parsed,
            out _);

        parsed!.Apply("OrderError", "99").ShouldBe("99");
    }
}
