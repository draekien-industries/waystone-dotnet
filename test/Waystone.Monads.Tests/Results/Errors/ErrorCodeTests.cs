namespace Waystone.Monads.Results.Errors;

using System;
using Shouldly;
using Xunit;

public sealed class ErrorCodeTests
{
    [Fact]
    public void GivenErrorCode_WhenToString_ThenStringShouldBeValue()
    {
        var sut = new ErrorCode("bob");
        sut.ToString().ShouldBe("bob");
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
    public void GivenEnum_WhenCreatingErrorCode_ThenReturnExpectedCode()
    {
        ErrorCode result = ErrorCode.FromEnum(TestErrorCodes.TestValue);
        result.Value.ShouldBe("TestErrorCodes.TestValue");
        result.ToString().ShouldBe("TestErrorCodes.TestValue");
    }

    [Fact]
    public void
        GivenException_WhenCreatingErrorCode_ThenReturnExpectedCode()
    {
        ErrorCode result = ErrorCode.FromException(new TestException());
        result.Value.ShouldBe("Test");
        result.ToString().ShouldBe("Test");
    }

    private enum TestErrorCodes
    {
        TestValue,
    }

    private class TestException : Exception
    { }
}
