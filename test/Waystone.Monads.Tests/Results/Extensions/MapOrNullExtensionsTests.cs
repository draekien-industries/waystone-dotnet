namespace Waystone.Monads.Results.Extensions;

using JetBrains.Annotations;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

[TestSubject(typeof(ResultExtensions))]
public sealed class MapOrNullExtensionsTests
{
    [Fact]
    public void GivenOk_WhenMapOrNull_ThenReturnTheMappedValue() =>
        Result.Ok<int, string>(1).MapOrNull(value => value + 1).ShouldBe(2);

    [Fact]
    public void GivenErr_WhenMapOrNull_ThenReturnNull() =>
        Result.Err<int, string>("failed")
           .MapOrNull(value => value + 1)
           .ShouldBeNull();

    [Fact]
    public void GivenOk_WhenMapOrNullToTheDefault_ThenReturnTheDefault() =>
        Result.Ok<int, string>(1).MapOrNull(_ => 0).ShouldBe(0);

    [Fact]
    public async Task GivenOk_WhenMapOrNullAsync_ThenReturnTheMappedValue()
    {
        int? value = await Result.Ok<int, string>(1)
           .MapOrNullAsync(ok => Task.FromResult(ok + 1));

        value.ShouldBe(2);
    }

    [Fact]
    public async Task GivenErr_WhenMapOrNullAsync_ThenReturnNull()
    {
        int? value = await Result.Err<int, string>("failed")
           .MapOrNullAsync(ok => Task.FromResult(ok + 1));

        value.ShouldBeNull();
    }

    [Fact]
    public async Task
        GivenOkTask_WhenMapOrNullAsyncWithASyncMap_ThenReturnTheMappedValue()
    {
        int? value = await Task.FromResult(Result.Ok<int, string>(1))
           .MapOrNullAsync(ok => ok + 1);

        value.ShouldBe(2);
    }

    [Fact]
    public async Task GivenErrTask_WhenMapOrNullAsyncWithASyncMap_ThenReturnNull()
    {
        int? value = await Task.FromResult(Result.Err<int, string>("failed"))
           .MapOrNullAsync(ok => ok + 1);

        value.ShouldBeNull();
    }

    [Fact]
    public async Task
        GivenOkTask_WhenMapOrNullAsyncWithAnAsyncMap_ThenReturnTheMappedValue()
    {
        int? value = await Task.FromResult(Result.Ok<int, string>(1))
           .MapOrNullAsync(ok => Task.FromResult(ok + 1));

        value.ShouldBe(2);
    }

    [Fact]
    public async Task
        GivenErrTask_WhenMapOrNullAsyncWithAnAsyncMap_ThenReturnNull()
    {
        int? value = await Task.FromResult(Result.Err<int, string>("failed"))
           .MapOrNullAsync(ok => Task.FromResult(ok + 1));

        value.ShouldBeNull();
    }

    [Fact]
    public async Task
        GivenOkValueTask_WhenMapOrNullAsyncWithASyncMap_ThenReturnTheMappedValue()
    {
        int? value =
            await new ValueTask<Result<int, string>>(
                    Result.Ok<int, string>(1))
               .MapOrNullAsync(ok => ok + 1);

        value.ShouldBe(2);
    }

    [Fact]
    public async Task
        GivenErrValueTask_WhenMapOrNullAsyncWithASyncMap_ThenReturnNull()
    {
        int? value =
            await new ValueTask<Result<int, string>>(
                    Result.Err<int, string>("failed"))
               .MapOrNullAsync(ok => ok + 1);

        value.ShouldBeNull();
    }

    [Fact]
    public async Task
        GivenOkValueTask_WhenMapOrNullAsyncWithAnAsyncMap_ThenReturnTheMappedValue()
    {
        int? value =
            await new ValueTask<Result<int, string>>(
                    Result.Ok<int, string>(1))
               .MapOrNullAsync(ok => Task.FromResult(ok + 1));

        value.ShouldBe(2);
    }

    [Fact]
    public async Task
        GivenErrValueTask_WhenMapOrNullAsyncWithAnAsyncMap_ThenReturnNull()
    {
        int? value =
            await new ValueTask<Result<int, string>>(
                    Result.Err<int, string>("failed"))
               .MapOrNullAsync(ok => Task.FromResult(ok + 1));

        value.ShouldBeNull();
    }
}
