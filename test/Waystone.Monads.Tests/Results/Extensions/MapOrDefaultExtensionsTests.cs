namespace Waystone.Monads.Results.Extensions;

using JetBrains.Annotations;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

[TestSubject(typeof(MapOrDefaultExtensions))]
public sealed class MapOrDefaultExtensionsTests
{
    [Fact]
    public async Task GivenOk_WhenMapOrDefaultAsync_ThenReturnTheMappedValue()
    {
        Result<int, string> ok = Result.Ok<int, string>(1);

        int result =
            await ok.MapOrDefaultAsync(value => Task.FromResult(value + 1));

        result.ShouldBe(2);
    }

    [Fact]
    public async Task GivenErr_WhenMapOrDefaultAsync_ThenReturnTheDefault()
    {
        Result<int, string> err = Result.Err<int, string>("error");

        int result =
            await err.MapOrDefaultAsync(value => Task.FromResult(value + 1));

        result.ShouldBe(0);
    }

    [Fact]
    public async Task
        GivenOkTask_WhenMapOrDefaultAsyncWithASyncMap_ThenReturnTheMappedValue()
    {
        int result = await Task.FromResult(Result.Ok<int, string>(1))
           .MapOrDefaultAsync(value => value + 1);

        result.ShouldBe(2);
    }

    [Fact]
    public async Task
        GivenErrTask_WhenMapOrDefaultAsyncWithASyncMap_ThenReturnTheDefault()
    {
        int result = await Task.FromResult(Result.Err<int, string>("error"))
           .MapOrDefaultAsync(value => value + 1);

        result.ShouldBe(0);
    }

    [Fact]
    public async Task
        GivenOkTask_WhenMapOrDefaultAsyncWithAnAsyncMap_ThenReturnTheMappedValue()
    {
        int result = await Task.FromResult(Result.Ok<int, string>(1))
           .MapOrDefaultAsync(value => Task.FromResult(value + 1));

        result.ShouldBe(2);
    }

    [Fact]
    public async Task
        GivenErrTask_WhenMapOrDefaultAsyncWithAnAsyncMap_ThenReturnTheDefault()
    {
        int result = await Task.FromResult(Result.Err<int, string>("error"))
           .MapOrDefaultAsync(value => Task.FromResult(value + 1));

        result.ShouldBe(0);
    }

    [Fact]
    public async Task
        GivenOkValueTask_WhenMapOrDefaultAsyncWithASyncMap_ThenReturnTheMappedValue()
    {
        int result =
            await new ValueTask<Result<int, string>>(
                    Result.Ok<int, string>(1))
               .MapOrDefaultAsync(value => value + 1);

        result.ShouldBe(2);
    }

    [Fact]
    public async Task
        GivenErrValueTask_WhenMapOrDefaultAsyncWithASyncMap_ThenReturnTheDefault()
    {
        int result =
            await new ValueTask<Result<int, string>>(
                    Result.Err<int, string>("error"))
               .MapOrDefaultAsync(value => value + 1);

        result.ShouldBe(0);
    }

    [Fact]
    public async Task
        GivenOkValueTask_WhenMapOrDefaultAsyncWithAnAsyncMap_ThenReturnTheMappedValue()
    {
        int result =
            await new ValueTask<Result<int, string>>(
                    Result.Ok<int, string>(1))
               .MapOrDefaultAsync(value => Task.FromResult(value + 1));

        result.ShouldBe(2);
    }

    [Fact]
    public async Task
        GivenErrValueTask_WhenMapOrDefaultAsyncWithAnAsyncMap_ThenReturnTheDefault()
    {
        int result =
            await new ValueTask<Result<int, string>>(
                    Result.Err<int, string>("error"))
               .MapOrDefaultAsync(value => Task.FromResult(value + 1));

        result.ShouldBe(0);
    }

    [Fact]
    public async Task
        GivenErr_WhenMapOrDefaultAsyncToAReferenceType_ThenReturnNull()
    {
        Result<int, string> err = Result.Err<int, string>("error");

        string? result = await err.MapOrDefaultAsync(
            value => Task.FromResult(value.ToString()));

        result.ShouldBeNull();
    }
}
