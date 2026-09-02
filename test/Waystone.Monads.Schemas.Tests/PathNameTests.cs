namespace Waystone.Monads.Schemas;

using Shouldly;
using Xunit;

public sealed class PathNameTests
{
    [Theory]
    [InlineData("subject.Email", "email")]
    [InlineData("dto.Address.Line1", "line1")]
    [InlineData("Email", "email")]
    [InlineData("email", "email")]
    [InlineData("subject.email", "email")]
    [InlineData("GetEmail()", "getEmail()")]
    public void GivenAnArgumentExpression_WhenDerivingAName_ThenTakeTheLastSegmentInCamelCase(
        string expression,
        string expected)
    {
        PathName.From(expression).ShouldBe(expected);
    }

    [Fact]
    public void GivenNoExpression_WhenDerivingAName_ThenFallBack()
    {
        PathName.From(null).ShouldBe(PathName.Fallback);
    }

    [Fact]
    public void GivenAnExpressionEndingInADot_WhenDerivingAName_ThenFallBack()
    {
        PathName.From("subject.").ShouldBe(PathName.Fallback);
    }

    [Fact]
    public void GivenAnEmptyExpression_WhenDerivingAName_ThenFallBack()
    {
        PathName.From(string.Empty).ShouldBe(PathName.Fallback);
    }
}
