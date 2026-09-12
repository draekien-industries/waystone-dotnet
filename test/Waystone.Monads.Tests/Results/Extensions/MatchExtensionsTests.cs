namespace Waystone.Monads.Results.Extensions;

using JetBrains.Annotations;
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
[TestSubject(typeof(ResultExtensions))]
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

    [Fact]
    public async Task
        GivenOkTask_WhenMatchAsyncWithAsyncOnOkAndAsyncOnErr_ThenInvokeOnOk()
    {
        var onOk = Substitute.For<Func<int, Task>>();
        var onErr = Substitute.For<Func<string, Task>>();

        await Task.FromResult(Result.Ok<int, string>(10))
           .MatchAsync(onOk, onErr);

        await onOk.Received(1).Invoke(10);
        await onErr.DidNotReceive().Invoke(Arg.Any<string>());
    }

    [Fact]
    public async Task
        GivenErrTask_WhenMatchAsyncWithAsyncOnOkAndAsyncOnErr_ThenInvokeOnErr()
    {
        var onOk = Substitute.For<Func<int, Task>>();
        var onErr = Substitute.For<Func<string, Task>>();

        await Task.FromResult(Result.Err<int, string>("Error"))
           .MatchAsync(onOk, onErr);

        await onErr.Received(1).Invoke("Error");
        await onOk.DidNotReceive().Invoke(Arg.Any<int>());
    }

    [Fact]
    public async Task
        GivenOkTask_WhenMatchAsyncWithAsyncOnOkAndSyncOnErr_ThenInvokeOnOk()
    {
        var onOk = Substitute.For<Func<int, Task>>();
        var onErr = Substitute.For<Action<string>>();

        await Task.FromResult(Result.Ok<int, string>(10))
           .MatchAsync(onOk, onErr);

        await onOk.Received(1).Invoke(10);
        onErr.DidNotReceive().Invoke(Arg.Any<string>());
    }

    [Fact]
    public async Task
        GivenErrTask_WhenMatchAsyncWithAsyncOnOkAndSyncOnErr_ThenInvokeOnErr()
    {
        var onOk = Substitute.For<Func<int, Task>>();
        var onErr = Substitute.For<Action<string>>();

        await Task.FromResult(Result.Err<int, string>("Error"))
           .MatchAsync(onOk, onErr);

        onErr.Received(1).Invoke("Error");
        await onOk.DidNotReceive().Invoke(Arg.Any<int>());
    }

    [Fact]
    public async Task
        GivenOkTask_WhenMatchAsyncWithSyncOnOkAndAsyncOnErr_ThenInvokeOnOk()
    {
        var onOk = Substitute.For<Action<int>>();
        var onErr = Substitute.For<Func<string, Task>>();

        await Task.FromResult(Result.Ok<int, string>(10))
           .MatchAsync(onOk, onErr);

        onOk.Received(1).Invoke(10);
        await onErr.DidNotReceive().Invoke(Arg.Any<string>());
    }

    [Fact]
    public async Task
        GivenErrTask_WhenMatchAsyncWithSyncOnOkAndAsyncOnErr_ThenInvokeOnErr()
    {
        var onOk = Substitute.For<Action<int>>();
        var onErr = Substitute.For<Func<string, Task>>();

        await Task.FromResult(Result.Err<int, string>("Error"))
           .MatchAsync(onOk, onErr);

        await onErr.Received(1).Invoke("Error");
        onOk.DidNotReceive().Invoke(Arg.Any<int>());
    }

    [Fact]
    public async Task
        GivenOkTask_WhenMatchAsyncWithSyncOnOkAndSyncOnErr_ThenInvokeOnOk()
    {
        var onOk = Substitute.For<Action<int>>();
        var onErr = Substitute.For<Action<string>>();

        await Task.FromResult(Result.Ok<int, string>(10))
           .MatchAsync(onOk, onErr);

        onOk.Received(1).Invoke(10);
        onErr.DidNotReceive().Invoke(Arg.Any<string>());
    }

    [Fact]
    public async Task
        GivenErrTask_WhenMatchAsyncWithSyncOnOkAndSyncOnErr_ThenInvokeOnErr()
    {
        var onOk = Substitute.For<Action<int>>();
        var onErr = Substitute.For<Action<string>>();

        await Task.FromResult(Result.Err<int, string>("Error"))
           .MatchAsync(onOk, onErr);

        onErr.Received(1).Invoke("Error");
        onOk.DidNotReceive().Invoke(Arg.Any<int>());
    }

    [Fact]
    public async Task
        GivenOkValueTask_WhenMatchAsyncWithAsyncOnOkAndAsyncOnErr_ThenInvokeOnOk()
    {
        var onOk = Substitute.For<Func<int, Task>>();
        var onErr = Substitute.For<Func<string, Task>>();

        await new ValueTask<Result<int, string>>(Result.Ok<int, string>(10))
           .MatchAsync(onOk, onErr);

        await onOk.Received(1).Invoke(10);
        await onErr.DidNotReceive().Invoke(Arg.Any<string>());
    }

    [Fact]
    public async Task
        GivenErrValueTask_WhenMatchAsyncWithAsyncOnOkAndAsyncOnErr_ThenInvokeOnErr()
    {
        var onOk = Substitute.For<Func<int, Task>>();
        var onErr = Substitute.For<Func<string, Task>>();

        await new ValueTask<Result<int, string>>(Result.Err<int, string>("Error"))
           .MatchAsync(onOk, onErr);

        await onErr.Received(1).Invoke("Error");
        await onOk.DidNotReceive().Invoke(Arg.Any<int>());
    }

    [Fact]
    public async Task
        GivenOkValueTask_WhenMatchAsyncWithAsyncOnOkAndSyncOnErr_ThenInvokeOnOk()
    {
        var onOk = Substitute.For<Func<int, Task>>();
        var onErr = Substitute.For<Action<string>>();

        await new ValueTask<Result<int, string>>(Result.Ok<int, string>(10))
           .MatchAsync(onOk, onErr);

        await onOk.Received(1).Invoke(10);
        onErr.DidNotReceive().Invoke(Arg.Any<string>());
    }

    [Fact]
    public async Task
        GivenErrValueTask_WhenMatchAsyncWithAsyncOnOkAndSyncOnErr_ThenInvokeOnErr()
    {
        var onOk = Substitute.For<Func<int, Task>>();
        var onErr = Substitute.For<Action<string>>();

        await new ValueTask<Result<int, string>>(Result.Err<int, string>("Error"))
           .MatchAsync(onOk, onErr);

        onErr.Received(1).Invoke("Error");
        await onOk.DidNotReceive().Invoke(Arg.Any<int>());
    }

    [Fact]
    public async Task
        GivenOkValueTask_WhenMatchAsyncWithSyncOnOkAndAsyncOnErr_ThenInvokeOnOk()
    {
        var onOk = Substitute.For<Action<int>>();
        var onErr = Substitute.For<Func<string, Task>>();

        await new ValueTask<Result<int, string>>(Result.Ok<int, string>(10))
           .MatchAsync(onOk, onErr);

        onOk.Received(1).Invoke(10);
        await onErr.DidNotReceive().Invoke(Arg.Any<string>());
    }

    [Fact]
    public async Task
        GivenErrValueTask_WhenMatchAsyncWithSyncOnOkAndAsyncOnErr_ThenInvokeOnErr()
    {
        var onOk = Substitute.For<Action<int>>();
        var onErr = Substitute.For<Func<string, Task>>();

        await new ValueTask<Result<int, string>>(Result.Err<int, string>("Error"))
           .MatchAsync(onOk, onErr);

        await onErr.Received(1).Invoke("Error");
        onOk.DidNotReceive().Invoke(Arg.Any<int>());
    }

    [Fact]
    public async Task
        GivenOkValueTask_WhenMatchAsyncWithSyncOnOkAndSyncOnErr_ThenInvokeOnOk()
    {
        var onOk = Substitute.For<Action<int>>();
        var onErr = Substitute.For<Action<string>>();

        await new ValueTask<Result<int, string>>(Result.Ok<int, string>(10))
           .MatchAsync(onOk, onErr);

        onOk.Received(1).Invoke(10);
        onErr.DidNotReceive().Invoke(Arg.Any<string>());
    }

    [Fact]
    public async Task
        GivenErrValueTask_WhenMatchAsyncWithSyncOnOkAndSyncOnErr_ThenInvokeOnErr()
    {
        var onOk = Substitute.For<Action<int>>();
        var onErr = Substitute.For<Action<string>>();

        await new ValueTask<Result<int, string>>(Result.Err<int, string>("Error"))
           .MatchAsync(onOk, onErr);

        onErr.Received(1).Invoke("Error");
        onOk.DidNotReceive().Invoke(Arg.Any<int>());
    }

    /// <remarks>
    /// The mixed value-returning shapes arrived in DRA-211, where
    /// <see cref="Options.Option{T}" /> had carried both since 7.0.0 and
    /// <see cref="Result{TOk,TErr}" /> only the both-asynchronous one — the
    /// asymmetry that made the grid worth squaring. The branch written
    /// synchronously is asserted through
    /// <see cref="ValueTask{TResult}.IsCompleted" /> before the result is read,
    /// which is what distinguishes it from the both-asynchronous overload rather
    /// than merely agreeing with it.
    /// </remarks>
    [Fact]
    public async Task GivenOk_WhenMatchAsyncWithAsyncOnOk_ThenAwaitOnOk()
    {
        int matched = await Result.Ok<int, string>(2)
           .MatchAsync(value => Task.FromResult(value + 1), error => error.Length);

        matched.ShouldBe(3);
    }

    [Fact]
    public async Task GivenErr_WhenMatchAsyncWithAsyncOnOk_ThenRunOnErr()
    {
        ValueTask<int> matched = Result.Err<int, string>("boom")
           .MatchAsync(value => Task.FromResult(value + 1), error => error.Length);

        matched.IsCompleted.ShouldBeTrue();
        (await matched).ShouldBe(4);
    }

    [Fact]
    public async Task GivenOk_WhenMatchAsyncWithAsyncOnErr_ThenRunOnOk()
    {
        ValueTask<int> matched = Result.Ok<int, string>(2)
           .MatchAsync(value => value + 1, error => Task.FromResult(error.Length));

        matched.IsCompleted.ShouldBeTrue();
        (await matched).ShouldBe(3);
    }

    [Fact]
    public async Task GivenErr_WhenMatchAsyncWithAsyncOnErr_ThenAwaitOnErr()
    {
        int matched = await Result.Err<int, string>("boom")
           .MatchAsync(value => value + 1, error => Task.FromResult(error.Length));

        matched.ShouldBe(4);
    }

    [Fact]
    public async Task GivenOkTask_WhenMatchAsyncWithAsyncOnOk_ThenAwaitOnOk()
    {
        int matched = await Task.FromResult(Result.Ok<int, string>(2))
           .MatchAsync(value => Task.FromResult(value + 1), error => error.Length);

        matched.ShouldBe(3);
    }

    [Fact]
    public async Task GivenErrValueTask_WhenMatchAsyncWithAsyncOnOk_ThenRunOnErr()
    {
        int matched =
            await new ValueTask<Result<int, string>>(
                    Result.Err<int, string>("boom"))
               .MatchAsync(
                    value => Task.FromResult(value + 1),
                    error => error.Length);

        matched.ShouldBe(4);
    }

    [Fact]
    public async Task GivenOkValueTask_WhenMatchAsyncWithAsyncOnErr_ThenRunOnOk()
    {
        int matched =
            await new ValueTask<Result<int, string>>(Result.Ok<int, string>(2))
               .MatchAsync(
                    value => value + 1,
                    error => Task.FromResult(error.Length));

        matched.ShouldBe(3);
    }

    [Fact]
    public async Task GivenErrTask_WhenMatchAsyncWithAsyncOnErr_ThenAwaitOnErr()
    {
        int matched = await Task.FromResult(Result.Err<int, string>("boom"))
           .MatchAsync(value => value + 1, error => Task.FromResult(error.Length));

        matched.ShouldBe(4);
    }
}
