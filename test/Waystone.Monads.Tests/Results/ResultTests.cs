namespace Waystone.Monads.Results;

using System;
using System.Threading.Tasks;
using Configs;
using Errors;
using JetBrains.Annotations;
using NSubstitute;
using Shouldly;
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
                   options => options.UseExceptionLogger(logger)))
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
                   options => options.UseExceptionLogger(logger)))
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
    public void WhenImplicitlyCreatingResult_ThenReturnExpected()
    {
        Result<int, string> ok = 1;
        Result<int, string> err = "error";

        ok.ShouldBe(Result.Ok<int, string>(1));
        err.ShouldBe(Result.Err<int, string>("error"));
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
    public void GivenNull_WhenImplicitlyCreatingResult_ThenThrow()
    {
        Should.Throw<ArgumentNullException>(
            () =>
            {
                Result<string, int> _ = default(string)!;
            });

        Should.Throw<ArgumentNullException>(
            () =>
            {
                Result<int, string> _ = default(string)!;
            });
    }

    [Fact]
    public void GivenNull_WhenBindingAndConverting_ThenBothRejectIt()
    {
        var callback = Substitute.For<Func<Exception, int>>();
        callback.Invoke(Arg.Any<Exception>()).Returns(1);

        Should.Throw<ArgumentNullException>(
            () =>
            {
                Result<string, int> _ = default(string)!;
            });

        Result.Try(() => default(string)!, callback).IsErr.ShouldBeTrue();
    }

    [Fact]
    public void GivenTheDefaultOfAValueType_WhenCreatingOk_ThenReturnOk()
    {
        Result.Ok<int, string>(0).ShouldBe(Result.Ok<int, string>(0));

        Result<int, string> zero = 0;

        zero.IsOk.ShouldBeTrue();
        zero.Unwrap().ShouldBe(0);
    }
}
