namespace Waystone.Monads.Results.Errors;

using System;
using Configs;
using Shouldly;
using Xunit;

[Collection(GlobalMonadOptionsCollection.Name)]
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

    /// <summary>
    /// A blank message is repaired, but a null code is not. Consumers branch on the
    /// code, so there is no fallback that would be correct, and the alternative was
    /// an Error that renders as "[] message".
    /// </summary>
    [Fact]
    public void GivenNullErrorCode_WhenCreatingError_ThenThrow()
    {
        Should.Throw<ArgumentNullException>(() => new Error(null!, "message"))
              .ParamName.ShouldBe("code");
    }
}
