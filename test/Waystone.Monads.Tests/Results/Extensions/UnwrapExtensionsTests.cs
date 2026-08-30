namespace Waystone.Monads.Results.Extensions;

using JetBrains.Annotations;
using Shouldly;
using System.Threading.Tasks;
using Waystone.Monads.Exceptions;
using Xunit;

[TestSubject(typeof(ResultExtensions))]
public sealed class UnwrapExtensionsTests
{
    private static Task<Result<int, string>> OkTask(int value) =>
        Task.FromResult(Result.Ok<int, string>(value));

    private static Task<Result<int, string>> ErrTask(string error) =>
        Task.FromResult(Result.Err<int, string>(error));

    private static ValueTask<Result<int, string>> OkValueTask(int value) =>
        new ValueTask<Result<int, string>>(Result.Ok<int, string>(value));

    private static ValueTask<Result<int, string>> ErrValueTask(string error) =>
        new ValueTask<Result<int, string>>(Result.Err<int, string>(error));

    [Fact]
    public async Task GivenOkTask_WhenUnwrapAsync_ThenReturnTheValue()
    {
        int result = await OkTask(10).UnwrapAsync();

        result.ShouldBe(10);
    }

    [Fact]
    public async Task GivenOkValueTask_WhenUnwrapAsync_ThenReturnTheValue()
    {
        int result = await OkValueTask(20).UnwrapAsync();

        result.ShouldBe(20);
    }

    [Fact]
    public async Task GivenErrTask_WhenUnwrapAsync_ThenThrow()
    {
        await Should.ThrowAsync<UnwrapException>(
            async () => await ErrTask("Error occurred").UnwrapAsync());
    }

    [Fact]
    public async Task GivenErrValueTask_WhenUnwrapAsync_ThenThrow()
    {
        await Should.ThrowAsync<UnwrapException>(
            async () => await ErrValueTask("Error occurred").UnwrapAsync());
    }

    [Fact]
    public async Task GivenErrTask_WhenUnwrapErrAsync_ThenReturnTheError()
    {
        string result = await ErrTask("Error occurred").UnwrapErrAsync();

        result.ShouldBe("Error occurred");
    }

    [Fact]
    public async Task GivenErrValueTask_WhenUnwrapErrAsync_ThenReturnTheError()
    {
        string result = await ErrValueTask("Critical Error").UnwrapErrAsync();

        result.ShouldBe("Critical Error");
    }

    [Fact]
    public async Task GivenOkTask_WhenUnwrapErrAsync_ThenThrow()
    {
        await Should.ThrowAsync<UnwrapException>(
            async () => await OkTask(10).UnwrapErrAsync());
    }

    [Fact]
    public async Task GivenOkValueTask_WhenUnwrapErrAsync_ThenThrow()
    {
        await Should.ThrowAsync<UnwrapException>(
            async () => await OkValueTask(10).UnwrapErrAsync());
    }

    [Fact]
    public async Task GivenOkTask_WhenUnwrapOrAsync_ThenIgnoreTheDefault()
    {
        int result = await OkTask(10).UnwrapOrAsync(99);

        result.ShouldBe(10);
    }

    [Fact]
    public async Task GivenErrTask_WhenUnwrapOrAsync_ThenReturnTheDefault()
    {
        int result = await ErrTask("Error occurred").UnwrapOrAsync(99);

        result.ShouldBe(99);
    }

    [Fact]
    public async Task GivenOkValueTask_WhenUnwrapOrAsync_ThenIgnoreTheDefault()
    {
        int result = await OkValueTask(30).UnwrapOrAsync(99);

        result.ShouldBe(30);
    }

    [Fact]
    public async Task GivenErrValueTask_WhenUnwrapOrAsync_ThenReturnTheDefault()
    {
        int result = await ErrValueTask("Fatal Error").UnwrapOrAsync(99);

        result.ShouldBe(99);
    }

    [Fact]
    public async Task GivenOkTask_WhenUnwrapOrDefaultAsync_ThenReturnTheValue()
    {
        int result = await OkTask(10).UnwrapOrDefaultAsync();

        result.ShouldBe(10);
    }

    [Fact]
    public async Task
        GivenErrTask_WhenUnwrapOrDefaultAsync_ThenReturnTheTypeDefault()
    {
        int result = await ErrTask("Error occurred").UnwrapOrDefaultAsync();

        result.ShouldBe(0);
    }

    [Fact]
    public async Task
        GivenOkValueTask_WhenUnwrapOrDefaultAsync_ThenReturnTheValue()
    {
        int result = await OkValueTask(40).UnwrapOrDefaultAsync();

        result.ShouldBe(40);
    }

    [Fact]
    public async Task
        GivenErrValueTask_WhenUnwrapOrDefaultAsync_ThenReturnTheTypeDefault()
    {
        int result = await ErrValueTask("Severe Error").UnwrapOrDefaultAsync();

        result.ShouldBe(0);
    }
}
