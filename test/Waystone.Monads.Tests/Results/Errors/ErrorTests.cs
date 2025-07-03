namespace Waystone.Monads.Results.Errors;

using System;
using Shouldly;
using Xunit;

public sealed class ErrorTests
{
    [Fact]
    public void
        GivenErrorCode_AndMessage_WhenConvertingToString_ThenReturnExpectedFormat()
    {
        var code = new ErrorCode("abc");
        var error = new Error(code, "message");
        error.Code.ShouldBe(code);
        error.Message.ShouldBe("message");
        error.ToString().ShouldBe("[abc] message");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void
        GivenNullOrWhiteSpaceMessage_WhenCreatingError_ThenUseDefaultMessage(
            string? message)
    {
        var code = new ErrorCode("abc");
        var error = new Error(code, message!);
        error.ToString().ShouldBe("[abc] An unexpected error occurred.");
    }

    [Fact]
    public void
        GivenException_WhenCreatingError_ThenReturnExpectedError()
    {
        Error error = Error.FromException(
            new InvalidOperationException("Something went wrong"));
        error.Code.ShouldBe(new ErrorCode("InvalidOperation"));
        error.Message.ShouldBe("Something went wrong");
    }
}
