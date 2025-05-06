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
        sut.Value.ShouldBe("Err.Unspecified");
        sut.ToString().ShouldBe("Err.Unspecified");
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
        GivenEnum_AndCustomFormatter_WhenCreatingErrorCode_ThenReturnExpectedCode()
    {
        ErrorCode result = ErrorCode.FromEnum(
            TestErrorCodes.TestValue,
            new TestErrorCodeFormatter());
        result.Value.ShouldBe("ABC.TestValue");
        result.ToString().ShouldBe("ABC.TestValue");
    }

    [Fact]
    public void
        GivenException_WhenCreatingErrorCode_ThenReturnExpectedCode()
    {
        ErrorCode result = ErrorCode.FromException(new TestException());
        result.Value.ShouldBe("Err.Test");
        result.ToString().ShouldBe("Err.Test");
    }

    [Fact]
    public void
        GivenException_AndCustomFormatter_WhenCreatingErrorCode_ThenReturnExpectedCode()
    {
        var formatter = new TestExceptionFormatter();
        ErrorCode result = ErrorCode.FromException(
            new TestException(),
            formatter);
        result.Value.ShouldBe("ABC");
        result.ToString().ShouldBe("ABC");
    }

    private enum TestErrorCodes
    {
        TestValue,
    }

    private class TestErrorCodeFormatter
        : IErrorCodeFormatter<TestErrorCodes>
    {
        /// <inheritdoc />
        public string Format(TestErrorCodes value) => $"ABC.{value}";
    }

    private class TestException : Exception
    { }

    private class TestExceptionFormatter
        : IErrorCodeFormatter<TestException>
    {
        /// <inheritdoc />
        public string Format(TestException value) => "ABC";
    }
}
