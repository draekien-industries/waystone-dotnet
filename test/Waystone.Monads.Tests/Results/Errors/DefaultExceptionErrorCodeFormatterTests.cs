namespace Waystone.Monads.Results.Errors;

using System;
using Shouldly;
using Xunit;

public sealed class DefaultExceptionErrorCodeFormatterTests
{
    [Fact]
    public void
        GivenTestException_WhenFormatting_ThenReturnsFormattedString()
    {
        // Given
        var sut = new DefaultExceptionErrorCodeFormatter<TestException>();
        var value = new TestException();

        string result = sut.Format(value);

        result.ShouldBe("Err.Test");
    }

    [Fact]
    public void GivenException_WhenFormatting_ThenReturnsFormattedString()
    {
        // Given
        var sut = new DefaultExceptionErrorCodeFormatter<Exception>();
        var value = new Exception();

        string result = sut.Format(value);

        result.ShouldBe("Err.Exception");
    }

    [Fact]
    public void
        GivenExceptionWithNoSuffix_WhenFormatting_ThenReturnsFormattedString()
    {
        // Given
        var sut =
            new DefaultExceptionErrorCodeFormatter<ExceptionWithNoSuffix>();
        var value = new ExceptionWithNoSuffix();
        string result = sut.Format(value);
        result.ShouldBe("Err.ExceptionWithNoSuffix");
    }

    private class TestException : Exception
    { }

    private class ExceptionWithNoSuffix : Exception
    { }
}
