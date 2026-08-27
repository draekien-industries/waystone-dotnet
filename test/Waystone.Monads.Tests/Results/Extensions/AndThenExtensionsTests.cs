namespace Waystone.Monads.Results.Extensions;

using JetBrains.Annotations;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

[TestSubject(typeof(AndThenExtensions))]
public sealed class AndThenExtensionsTests
{
    private static Task<Result<int, string>> OkTask(int value) =>
        Task.FromResult(Result.Ok<int, string>(value));

    private static ValueTask<Result<int, string>> OkValueTask(int value) =>
        new ValueTask<Result<int, string>>(Result.Ok<int, string>(value));

    [Fact]
    public async Task GivenOkTask_WhenAndThenAsyncReturnsOk_ThenReturnThatOk()
    {
        Result<int, string> result = await OkTask(10)
           .AndThenAsync(
                _ => new ValueTask<Result<int, string>>(
                    Result.Ok<int, string>(20)));

        result.ShouldBeOkValue(20);
    }

    [Fact]
    public async Task
        GivenOkTask_WhenAndThenAsyncReturnsErr_ThenReturnThatErr()
    {
        Result<int, string> result = await OkTask(10)
           .AndThenAsync(
                _ => new ValueTask<Result<int, string>>(
                    Result.Err<int, string>("Error occurred")));

        result.ShouldBeErrValue("Error occurred");
    }

    [Fact]
    public async Task
        GivenOkTask_WhenAndThenAsyncWithASyncDelegateReturningOk_ThenReturnThatOk()
    {
        Result<int, string> result =
            await OkTask(10).AndThenAsync(_ => Result.Ok<int, string>(30));

        result.ShouldBeOkValue(30);
    }

    [Fact]
    public async Task
        GivenOkTask_WhenAndThenAsyncWithASyncDelegateReturningErr_ThenReturnThatErr()
    {
        Result<int, string> result = await OkTask(10)
           .AndThenAsync(_ => Result.Err<int, string>("Sync error"));

        result.ShouldBeErrValue("Sync error");
    }

    [Fact]
    public async Task
        GivenOkValueTask_WhenAndThenAsyncReturnsOk_ThenReturnThatOk()
    {
        Result<int, string> result = await OkValueTask(10)
           .AndThenAsync(
                _ => new ValueTask<Result<int, string>>(
                    Result.Ok<int, string>(40)));

        result.ShouldBeOkValue(40);
    }

    [Fact]
    public async Task
        GivenOkValueTask_WhenAndThenAsyncReturnsErr_ThenReturnThatErr()
    {
        Result<int, string> result = await OkValueTask(10)
           .AndThenAsync(
                _ => new ValueTask<Result<int, string>>(
                    Result.Err<int, string>("ValueTask error")));

        result.ShouldBeErrValue("ValueTask error");
    }

    [Fact]
    public async Task
        GivenOkValueTask_WhenAndThenAsyncWithASyncDelegateReturningOk_ThenReturnThatOk()
    {
        Result<int, string> result =
            await OkValueTask(10)
               .AndThenAsync(_ => Result.Ok<int, string>(50));

        result.ShouldBeOkValue(50);
    }

    [Fact]
    public async Task
        GivenOkValueTask_WhenAndThenAsyncWithASyncDelegateReturningErr_ThenReturnThatErr()
    {
        Result<int, string> result = await OkValueTask(10)
           .AndThenAsync(_ => Result.Err<int, string>("Sync ValueTask error"));

        result.ShouldBeErrValue("Sync ValueTask error");
    }
}
