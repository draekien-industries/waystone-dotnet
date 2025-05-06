namespace Waystone.Monads.Results.Errors
{
    using Shouldly;
    using Xunit;

    public sealed class DefaultEnumErrorCodeFormatterTests
    {
        [Fact]
        public void
            GivenEnumValue_WhenFormatting_ThenReturnsFormattedString()
        {
            // Given
            var sut = new DefaultEnumErrorCodeFormatter<TestErrorCodes>();
            const TestErrorCodes value = TestErrorCodes.TestValue;

            // When
            string result = sut.Format(value);

            // Then
            result.ShouldBe("TestErrorCodes.TestValue");
        }

        private enum TestErrorCodes
        {
            TestValue,
        }
    }
}
