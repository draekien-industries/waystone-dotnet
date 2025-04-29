namespace Waystone.Monads.Results.Errors
{
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

            result.ShouldBe("TestException");
        }

        private class TestException : Exception
        { }
    }
}
