namespace Waystone.Monads.Results;

using System;
using System.Threading.Tasks;
using Configs;
using Errors;
using Fixtures;
using JetBrains.Annotations;
using NSubstitute;
using Shouldly;
using Waystone.Monads.Extensions.Logging.Configs;
using Xunit;

[TestSubject(typeof(Result))]
public sealed class ResultTests
{
    [Fact]
    public void GivenFactoryThatSucceeds_WhenBindingFactory_ThenReturnOk()
    {
        var callback = Substitute.For<Func<Exception, string>>();
        Result<int, string>
            result = Result.Try(() => 1, callback);
        result.ShouldBe(Result.Ok<int, string>(1));
        callback.DidNotReceive().Invoke(Arg.Any<Exception>());
    }

    [Fact]
    public void GivenFactoryThatFails_WhenBindingFactory_ThenReturnError()
    {
        var callback = Substitute.For<Func<Exception, string>>();
        callback.Invoke(Arg.Any<Exception>()).Returns("error");
        Result<int, string> result = Result.Try(
            () =>
            {
                throw new Exception();
#pragma warning disable CS0162 // Unreachable code detected
                return 1;
#pragma warning restore CS0162 // Unreachable code detected
            },
            callback);
        result.ShouldBe(Result.Err<int, string>("error"));
        callback.Received(1).Invoke(Arg.Any<Exception>());
    }

    [Fact]
    public async Task
        GivenAsyncFactoryThatSucceeds_WhenBindingFactory_ThenReturnOk()
    {
        var callback = Substitute.For<Func<Exception, string>>();
        Result<int, string> result = await Result.TryAsync(
            () => Task.FromResult(1),
            callback);
        result.ShouldBe(Result.Ok<int, string>(1));
        callback.DidNotReceive().Invoke(Arg.Any<Exception>());
    }

    [Fact]
    public async Task
        GivenAsyncFactoryThatFails_WhenBindingFactory_ThenReturnError()
    {
        var callback = Substitute.For<Func<Exception, string>>();
        callback.Invoke(Arg.Any<Exception>()).Returns("error");
        Result<int, string> result = await Result.TryAsync(
            () =>
            {
                throw new Exception();
#pragma warning disable CS0162 // Unreachable code detected
                return Task.FromResult(1);
#pragma warning restore CS0162 // Unreachable code detected
            },
            callback);
        result.ShouldBe(Result.Err<int, string>("error"));
        callback.Received(1).Invoke(Arg.Any<Exception>());
    }

    [Fact]
    public void GivenFactoryReturningNull_WhenBindingFactory_ThenReturnError()
    {
        var logger = Substitute.For<Action<Exception, CallerInfo>>();
        var callback = Substitute.For<Func<Exception, string>>();
        callback.Invoke(Arg.Any<Exception>()).Returns("error");

        using (MonadOptions.BeginScope(
                   options => options.UseLogger(
                       new HandledExceptionProbe(logger))))
        {
            Result<string, string> result =
                Result.Try(() => default(string)!, callback);

            result.ShouldBe(Result.Err<string, string>("error"));

            callback.Received(1)
               .Invoke(
                    Arg.Is<ArgumentNullException>(
                        ex => ex.ParamName == "factory"));

            logger.DidNotReceive()
               .Invoke(Arg.Any<Exception>(), Arg.Any<CallerInfo>());
        }
    }

    [Fact]
    public async Task
        GivenAsyncFactoryReturningNull_WhenBindingFactory_ThenReturnError()
    {
        var logger = Substitute.For<Action<Exception, CallerInfo>>();
        var callback = Substitute.For<Func<Exception, string>>();
        callback.Invoke(Arg.Any<Exception>()).Returns("error");

        using (MonadOptions.BeginScope(
                   options => options.UseLogger(
                       new HandledExceptionProbe(logger))))
        {
            Result<string, string> result = await Result.TryAsync(
                () => Task.FromResult(default(string)!),
                callback);

            result.ShouldBe(Result.Err<string, string>("error"));

            callback.Received(1)
               .Invoke(
                    Arg.Is<ArgumentNullException>(
                        ex => ex.ParamName == "asyncFactory"));

            logger.DidNotReceive()
               .Invoke(Arg.Any<Exception>(), Arg.Any<CallerInfo>());
        }
    }

    [Fact]
    public void GivenNull_WhenCreatingOk_ThenThrow()
    {
        Should.Throw<ArgumentNullException>(
                () => Result.Ok<string, string>(default!))
           .ParamName.ShouldBe("value");

        Should.Throw<ArgumentNullException>(
                () => Result.Ok<string>(default!))
           .ParamName.ShouldBe("value");
    }

    [Fact]
    public void GivenNull_WhenCreatingErr_ThenThrow()
    {
        Should.Throw<ArgumentNullException>(
                () => Result.Err<string, string>(default!))
           .ParamName.ShouldBe("value");

        Should.Throw<ArgumentNullException>(
                () => Result.Err<string>(default(Error)!))
           .ParamName.ShouldBe("value");
    }

    [Fact]
    public void GivenNull_WhenBinding_ThenReturnErr()
    {
        var callback = Substitute.For<Func<Exception, int>>();
        callback.Invoke(Arg.Any<Exception>()).Returns(1);

        Result.Try(() => default(string)!, callback).ShouldBeErr();
    }

    [Fact]
    public void GivenTheDefaultOfAValueType_WhenCreatingOk_ThenReturnOk()
    {
        Result<int, string> zero = Result.Ok<int, string>(0);

        zero.ShouldBeOk();
        zero.ShouldBeOkValue(0);
    }
}
