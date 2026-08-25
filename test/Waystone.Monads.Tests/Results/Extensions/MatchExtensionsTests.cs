namespace Waystone.Monads.Results.Extensions;

using JetBrains.Annotations;
using Monads.Extensions;
using NSubstitute;
using Shouldly;
using System;
using System.Threading.Tasks;
using Xunit;

/// <remarks>
/// The awaited-receiver overloads here were hand-written until DRA-108 put the
/// family on the generator, and nothing covered them at the time. These are the
/// regression cases for that conversion: every shape the hand-written blocks
/// declared, plus the state overloads the generator picked up from the core
/// member.
/// </remarks>
[TestSubject(typeof(MatchExtensions))]
public sealed class MatchExtensionsTests
{
    [Fact]
    public async Task GivenOkTask_WhenMatchAsyncWithFuncs_ThenInvokeOnOk()
    {
        int result = await Task.FromResult(Result.Ok<int, string>(1))
           .MatchAsync(value => value + 1, _ => 0);

        result.ShouldBe(2);
    }

    [Fact]
    public async Task GivenErrTask_WhenMatchAsyncWithFuncs_ThenInvokeOnErr()
    {
        int result = await Task.FromResult(Result.Err<int, string>("error"))
           .MatchAsync(value => value + 1, error => error.Length);

        result.ShouldBe(5);
    }

    [Fact]
    public async Task GivenOkTask_WhenMatchAsyncWithActions_ThenInvokeOnOk()
    {
        var onOk = Substitute.For<Action<int>>();
        var onErr = Substitute.For<Action<string>>();

        await Task.FromResult(Result.Ok<int, string>(1))
           .MatchAsync(onOk, onErr);

        onOk.Received().Invoke(1);
        onErr.DidNotReceiveWithAnyArgs().Invoke(default!);
    }

    [Fact]
    public async Task GivenErrValueTask_WhenMatchAsyncWithActions_ThenInvokeOnErr()
    {
        var onOk = Substitute.For<Action<int>>();
        var onErr = Substitute.For<Action<string>>();

        await new ValueTask<Result<int, string>>(
                Result.Err<int, string>("error"))
           .MatchAsync(onOk, onErr);

        onErr.Received().Invoke("error");
        onOk.DidNotReceiveWithAnyArgs().Invoke(default);
    }

    [Fact]
    public async Task GivenOkTask_AndState_WhenMatchAsync_ThenInvokeOnOk()
    {
        int result = await Task.FromResult(Result.Ok<int, string>(1))
           .MatchAsync(
                10,
                static (value, state) => value + state,
                static (string _, int state) => state * 100);

        result.ShouldBe(11);
    }

    [Fact]
    public async Task GivenErrValueTask_AndState_WhenMatchAsync_ThenInvokeOnErr()
    {
        int result = await new ValueTask<Result<int, string>>(
                Result.Err<int, string>("error"))
           .MatchAsync(
                10,
                static (value, state) => value + state,
                static (string _, int state) => state * 100);

        result.ShouldBe(1000);
    }

    [Fact]
    public async Task
        GivenOkTask_AndState_WhenMatchAsyncWithActions_ThenInvokeOnOk()
    {
        var onOk = Substitute.For<Action<int, int>>();
        var onErr = Substitute.For<Action<string, int>>();

        await Task.FromResult(Result.Ok<int, string>(1))
           .MatchAsync(10, onOk, onErr);

        onOk.Received().Invoke(1, 10);
        onErr.DidNotReceiveWithAnyArgs().Invoke(default!, default);
    }

    [Fact]
    public async Task
        GivenOkTask_WhenMatchAsyncWithAsyncOnOk_ThenAwaitTheBranch()
    {
        int result = await Task.FromResult(Result.Ok<int, string>(1))
           .MatchAsync(
                value => Task.FromResult(value + 1),
                _ => Task.FromResult(0));

        result.ShouldBe(2);
    }

    [Fact]
    public async Task
        GivenErrTask_WhenMatchAsyncWithMixedBranches_ThenAwaitTheErrBranch()
    {
        var onOk = Substitute.For<Action<int>>();

        await Task.FromResult(Result.Err<int, string>("error"))
           .MatchAsync(onOk, error => Task.FromResult(error.Length));

        onOk.DidNotReceiveWithAnyArgs().Invoke(default);
    }
}
