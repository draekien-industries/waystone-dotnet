namespace Waystone.Monads.Results.Extensions;

using JetBrains.Annotations;
using Monads.Extensions;
using NSubstitute;
using System;
using System.Threading.Tasks;
using Xunit;

[TestSubject(typeof(InspectErrExtensions))]
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

        action.DidNotReceiveWithAnyArgs().Invoke(default);
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
}
