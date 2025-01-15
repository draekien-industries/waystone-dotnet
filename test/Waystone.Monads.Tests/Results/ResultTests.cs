namespace Waystone.Monads.Results
{
    using System;
    using System.Threading.Tasks;
    using FluentAssertions;
    using JetBrains.Annotations;
    using NSubstitute;
    using Xunit;

    [TestSubject(typeof(Result))]
    public sealed class ResultTests
    {
        [Fact]
        public void GivenFactoryThatSucceeds_WhenBindingFactory_ThenReturnOk()
        {
            var callback = Substitute.For<Func<Exception, string>>();
            Result<int, string>
                result = Result.Bind(() => 1, callback);
            result.Should().Be(Result.Ok<int, string>(1));
            callback.DidNotReceive().Invoke(Arg.Any<Exception>());
        }

        [Fact]
        public void GivenFactoryThatFails_WhenBindingFactory_ThenReturnError()
        {
            var callback = Substitute.For<Func<Exception, string>>();
            callback.Invoke(Arg.Any<Exception>()).Returns("error");
            Result<int, string> result = Result.Bind(
                () =>
                {
                    throw new Exception();
#pragma warning disable CS0162 // Unreachable code detected
                    return 1;
#pragma warning restore CS0162 // Unreachable code detected
                },
                callback);
            result.Should().Be(Result.Err<int, string>("error"));
            callback.Received(1).Invoke(Arg.Any<Exception>());
        }

        [Fact]
        public async Task
            GivenAsyncFactoryThatSucceeds_WhenBindingFactory_ThenReturnOk()
        {
            var callback = Substitute.For<Func<Exception, string>>();
            Result<int, string> result = await Result.BindAsync(
                () => Task.FromResult(1),
                callback);
            result.Should().Be(Result.Ok<int, string>(1));
            callback.DidNotReceive().Invoke(Arg.Any<Exception>());
        }

        [Fact]
        public async Task
            GivenAsyncFactoryThatFails_WhenBindingFactory_ThenReturnError()
        {
            var callback = Substitute.For<Func<Exception, string>>();
            callback.Invoke(Arg.Any<Exception>()).Returns("error");
            Result<int, string> result = await Result.BindAsync(
                () =>
                {
                    throw new Exception();
#pragma warning disable CS0162 // Unreachable code detected
                    return Task.FromResult(1);
#pragma warning restore CS0162 // Unreachable code detected
                },
                callback);
            result.Should().Be(Result.Err<int, string>("error"));
            callback.Received(1).Invoke(Arg.Any<Exception>());
        }

        [Fact]
        public void WhenImplicitlyCreatingResult_ThenReturnExpected()
        {
            Result<int, string> ok = 1;
            Result<int, string> err = "error";

            ok.Should().Be(Result.Ok<int, string>(1));
            err.Should().Be(Result.Err<int, string>("error"));
        }
    }
}
