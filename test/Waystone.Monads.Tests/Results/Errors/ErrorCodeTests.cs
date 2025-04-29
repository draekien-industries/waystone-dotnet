namespace Waystone.Monads.Results.Errors
{
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
    }
}
