namespace Waystone.Monads.Results.Errors;

using System;
using Configs;
using Shouldly;
using Xunit;

[Collection(GlobalMonadOptionsCollection.Name)]
public sealed class ErrorCodeTests
{
    [Fact]
    public void GivenErrorCode_WhenToString_ThenStringShouldBeValue()
    {
        var sut = new ErrorCode("bob");
        sut.ToString().ShouldBe("bob");
    }

    [Fact]
    public void GivenString_WhenConvertingToErrorCode_ThenReturnExpectedCode()
    {
        const string code = "bob";
        ErrorCode sut = code;
        sut.Value.ShouldBe(code);
        sut.ToString().ShouldBe(code);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void
        GivenNullOrWhiteSpaceValue_WhenCreatingCode_ThenUseDefaultCode(
            string? value)
    {
        var sut = new ErrorCode(value!);
        sut.Value.ShouldBe("Unspecified");
        sut.ToString().ShouldBe("Unspecified");
    }


    [Fact]
    public void
        GivenException_WhenCreatingErrorCode_ThenReturnExpectedCode()
    {
        ErrorCode result = ErrorCode.FromException(new TestException());
        result.Value.ShouldBe("Test");
        result.ToString().ShouldBe("Test");
    }

    [Fact]
    public void GivenErrorCode_WhenConvertingToString_ThenReturnItsValue()
    {
        var code = new ErrorCode("bob");

        string converted = code;

        converted.ShouldBe("bob");
    }

    /// <summary>
    /// The conversion is implicit, so this fires on a line that names no cast. It
    /// throws rather than dereferencing null, which is what it used to do.
    /// </summary>
    [Fact]
    public void GivenNullErrorCode_WhenConvertingToString_ThenThrow()
    {
        ErrorCode? code = null;

        Should.Throw<ArgumentNullException>(() =>
        {
            string _ = code!;
        }).ParamName.ShouldBe("value");
    }

    private class TestException : Exception
    { }
}
