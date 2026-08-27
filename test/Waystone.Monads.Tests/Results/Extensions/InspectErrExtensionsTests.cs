namespace Waystone.Monads.Results.Extensions;

using JetBrains.Annotations;
using Monads.Extensions;
using NSubstitute;
using Shouldly;
using System;
using System.Threading.Tasks;
using Xunit;

[TestSubject(typeof(ResultExtensions))]
public sealed class InspectErrExtensionsTests
{
    [Fact]
    public async Task GivenErrTask_WhenInspectErrAsync_ThenInvokeTheAction()
    {
        var action = Substitute.For<Action<string>>();

        await Task.FromResult(Result.Err<int, string>("error"))
           .InspectErrAsync(action);

        action.Received(1).Invoke("error");
    }

    [Fact]
    public async Task
        GivenOkTask_WhenInspectErrAsync_ThenDoNotInvokeTheAction()
    {
        var action = Substitute.For<Action<string>>();

        await Task.FromResult(Result.Ok<int, string>(1))
           .InspectErrAsync(action);

        action.DidNotReceiveWithAnyArgs().Invoke(default!);
    }

    [Fact]
    public async Task
        GivenErrValueTask_WhenInspectErrAsync_ThenInvokeTheAction()
    {
        var action = Substitute.For<Action<string>>();

        await new ValueTask<Result<int, string>>(
                Result.Err<int, string>("error"))
           .InspectErrAsync(action);

        action.Received(1).Invoke("error");
    }

    [Fact]
    public async Task GivenOk_WhenInspectErrAsync_ThenSkipTheAsyncDelegate()
    {
        var inspect = Substitute.For<Func<string, Task>>();

        Result<int, string> result = await Result.Ok<int, string>(10)
           .InspectErrAsync(inspect);

        result.ShouldBeOkValue(10);
        await inspect.DidNotReceive().Invoke(Arg.Any<string>());
    }

    [Fact]
    public async Task GivenErr_WhenInspectErrAsync_ThenAwaitTheDelegate()
    {
        var inspect = Substitute.For<Func<string, Task>>();

        Result<int, string> result = await Result.Err<int, string>("Error")
           .InspectErrAsync(inspect);

        result.ShouldBeErrValue("Error");
        await inspect.Received(1).Invoke("Error");
    }

    [Fact]
    public async Task
        GivenOkTask_WhenInspectErrAsyncWithAnAsyncDelegate_ThenSkipIt()
    {
        var inspect = Substitute.For<Func<string, Task>>();

        Result<int, string> result =
            await Task.FromResult(Result.Ok<int, string>(20))
               .InspectErrAsync(inspect);

        result.ShouldBeOkValue(20);
        await inspect.DidNotReceive().Invoke(Arg.Any<string>());
    }

    [Fact]
    public async Task
        GivenErrTask_WhenInspectErrAsyncWithAnAsyncDelegate_ThenAwaitIt()
    {
        var inspect = Substitute.For<Func<string, Task>>();

        Result<int, string> result =
            await Task.FromResult(Result.Err<int, string>("Async Error"))
               .InspectErrAsync(inspect);

        result.ShouldBeErrValue("Async Error");
        await inspect.Received(1).Invoke("Async Error");
    }

    [Fact]
    public async Task
        GivenOkValueTask_WhenInspectErrAsyncWithAnAsyncDelegate_ThenSkipIt()
    {
        var inspect = Substitute.For<Func<string, Task>>();

        Result<int, string> result =
            await new ValueTask<Result<int, string>>(
                Result.Ok<int, string>(30)).InspectErrAsync(inspect);

        result.ShouldBeOkValue(30);
        await inspect.DidNotReceive().Invoke(Arg.Any<string>());
    }

    [Fact]
    public async Task
        GivenErrValueTask_WhenInspectErrAsyncWithAnAsyncDelegate_ThenAwaitIt()
    {
        var inspect = Substitute.For<Func<string, Task>>();

        Result<int, string> result =
            await new ValueTask<Result<int, string>>(
                    Result.Err<int, string>("Async ValueTask Error"))
               .InspectErrAsync(inspect);

        result.ShouldBeErrValue("Async ValueTask Error");
        await inspect.Received(1).Invoke("Async ValueTask Error");
    }
}
