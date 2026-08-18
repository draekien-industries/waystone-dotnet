namespace Waystone.Monads.Results.Extensions;

using JetBrains.Annotations;
using Monads.Extensions;
using NSubstitute;
using System;
using System.Threading.Tasks;
using Xunit;

[TestSubject(typeof(InspectExtensions))]
public sealed class InspectExtensionsTests
{
    [Fact]
    public async Task GivenOkTask_WhenInspectAsync_ThenInvokeTheAction()
    {
        var action = Substitute.For<Action<int>>();

        await Task.FromResult(Result.Ok<int, string>(1))
           .InspectAsync(action);

        action.Received(1).Invoke(1);
    }

    [Fact]
    public async Task GivenErrTask_WhenInspectAsync_ThenDoNotInvokeTheAction()
    {
        var action = Substitute.For<Action<int>>();

        await Task.FromResult(Result.Err<int, string>("error"))
           .InspectAsync(action);

        action.DidNotReceiveWithAnyArgs().Invoke(default);
    }

    [Fact]
    public async Task GivenOkValueTask_WhenInspectAsync_ThenInvokeTheAction()
    {
        var action = Substitute.For<Action<int>>();

        await new ValueTask<Result<int, string>>(Result.Ok<int, string>(1))
           .InspectAsync(action);

        action.Received(1).Invoke(1);
    }

    [Fact]
    public async Task
        GivenOkValueTask_WhenInspectAsyncWithATaskAction_ThenAwaitIt()
    {
        var action = Substitute.For<Func<int, Task>>();

        await new ValueTask<Result<int, string>>(Result.Ok<int, string>(1))
           .InspectAsync(action);

        await action.Received(1).Invoke(1);
    }
}
