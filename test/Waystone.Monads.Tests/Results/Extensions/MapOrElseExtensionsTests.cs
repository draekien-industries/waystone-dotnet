namespace Waystone.Monads.Results.Extensions;

using JetBrains.Annotations;
using Shouldly;
using System;
using System.Threading.Tasks;
using Xunit;

[TestSubject(typeof(ResultExtensions))]
public sealed class MapOrElseExtensionsTests
{
    private static readonly Func<int, Task<string>> AsyncMap =
        value => Task.FromResult(value.ToString());

    private static readonly Func<int, string> SyncMap =
        value => value.ToString();

    private static Func<string, Task<string>> AsyncFactory(string fallback) =>
        _ => Task.FromResult(fallback);

    private static Func<string, string> SyncFactory(string fallback) =>
        _ => fallback;

    private static Result<int, string> Ok(int value) =>
        Result.Ok<int, string>(value);

    private static Result<int, string> Err(string error) =>
        Result.Err<int, string>(error);

    [Fact]
    public async Task GivenOk_WhenMapOrElseAsync_ThenReturnTheMappedValue()
    {
        string result =
            await Ok(10).MapOrElseAsync(AsyncFactory("Missing"), AsyncMap);

        result.ShouldBe("10");
    }

    [Fact]
    public async Task GivenErr_WhenMapOrElseAsync_ThenReturnTheFactoryValue()
    {
        string result = await Err("Error occurred")
           .MapOrElseAsync(AsyncFactory("Error handled"), AsyncMap);

        result.ShouldBe("Error handled");
    }

    [Fact]
    public async Task GivenOkTask_WhenMapOrElseAsync_ThenReturnTheMappedValue()
    {
        string result = await Task.FromResult(Ok(10))
           .MapOrElseAsync(AsyncFactory("Not Found"), AsyncMap);

        result.ShouldBe("10");
    }

    [Fact]
    public async Task
        GivenOkTask_WhenMapOrElseAsyncWithASyncFactory_ThenReturnTheMappedValue()
    {
        string result = await Task.FromResult(Ok(10))
           .MapOrElseAsync(SyncFactory("Not Found"), AsyncMap);

        result.ShouldBe("10");
    }

    [Fact]
    public async Task
        GivenOkTask_WhenMapOrElseAsyncWithASyncMap_ThenReturnTheMappedValue()
    {
        string result = await Task.FromResult(Ok(10))
           .MapOrElseAsync(AsyncFactory("Not Found"), SyncMap);

        result.ShouldBe("10");
    }

    [Fact]
    public async Task
        GivenOkTask_WhenMapOrElseAsyncWithSyncBranches_ThenReturnTheMappedValue()
    {
        string result = await Task.FromResult(Ok(10))
           .MapOrElseAsync(SyncFactory("Not Found"), SyncMap);

        result.ShouldBe("10");
    }

    [Fact]
    public async Task
        GivenErrTask_WhenMapOrElseAsync_ThenReturnTheFactoryValue()
    {
        string result = await Task.FromResult(Err("Error occurred"))
           .MapOrElseAsync(AsyncFactory("Error handled"), AsyncMap);

        result.ShouldBe("Error handled");
    }

    [Fact]
    public async Task
        GivenErrTask_WhenMapOrElseAsyncWithASyncFactory_ThenReturnTheFactoryValue()
    {
        string result = await Task.FromResult(Err("Error occurred"))
           .MapOrElseAsync(SyncFactory("Error handled"), AsyncMap);

        result.ShouldBe("Error handled");
    }

    [Fact]
    public async Task
        GivenErrTask_WhenMapOrElseAsyncWithASyncMap_ThenReturnTheFactoryValue()
    {
        string result = await Task.FromResult(Err("Error occurred"))
           .MapOrElseAsync(AsyncFactory("Error handled"), SyncMap);

        result.ShouldBe("Error handled");
    }

    [Fact]
    public async Task
        GivenErrTask_WhenMapOrElseAsyncWithSyncBranches_ThenReturnTheFactoryValue()
    {
        string result = await Task.FromResult(Err("Error occurred"))
           .MapOrElseAsync(SyncFactory("Error handled"), SyncMap);

        result.ShouldBe("Error handled");
    }

    [Fact]
    public async Task
        GivenOkValueTask_WhenMapOrElseAsync_ThenReturnTheMappedValue()
    {
        string result = await new ValueTask<Result<int, string>>(Ok(20))
           .MapOrElseAsync(AsyncFactory("Not Available"), AsyncMap);

        result.ShouldBe("20");
    }

    [Fact]
    public async Task
        GivenErrValueTask_WhenMapOrElseAsync_ThenReturnTheFactoryValue()
    {
        string result =
            await new ValueTask<Result<int, string>>(Err("Critical Error"))
               .MapOrElseAsync(AsyncFactory("Recovered"), AsyncMap);

        result.ShouldBe("Recovered");
    }

    [Fact]
    public async Task
        GivenOkValueTask_WhenMapOrElseAsyncWithSyncBranches_ThenReturnTheMappedValue()
    {
        string result = await new ValueTask<Result<int, string>>(Ok(30))
           .MapOrElseAsync(SyncFactory("Unavailable"), SyncMap);

        result.ShouldBe("30");
    }

    [Fact]
    public async Task
        GivenErrValueTask_WhenMapOrElseAsyncWithSyncBranches_ThenReturnTheFactoryValue()
    {
        string result =
            await new ValueTask<Result<int, string>>(Err("Fatal Error"))
               .MapOrElseAsync(SyncFactory("Resolved"), SyncMap);

        result.ShouldBe("Resolved");
    }

    [Fact]
    public async Task
        GivenOkValueTask_WhenMapOrElseAsyncWithASyncMap_ThenReturnTheMappedValue()
    {
        string result = await new ValueTask<Result<int, string>>(Ok(40))
           .MapOrElseAsync(AsyncFactory("Not Present"), SyncMap);

        result.ShouldBe("40");
    }

    [Fact]
    public async Task
        GivenErrValueTask_WhenMapOrElseAsyncWithASyncMap_ThenReturnTheFactoryValue()
    {
        string result =
            await new ValueTask<Result<int, string>>(Err("Severe Error"))
               .MapOrElseAsync(AsyncFactory("Fixed"), SyncMap);

        result.ShouldBe("Fixed");
    }

    [Fact]
    public async Task
        GivenOkValueTask_WhenMapOrElseAsyncWithASyncFactory_ThenReturnTheMappedValue()
    {
        string result = await new ValueTask<Result<int, string>>(Ok(50))
           .MapOrElseAsync(SyncFactory("Not Here"), AsyncMap);

        result.ShouldBe("50");
    }

    [Fact]
    public async Task
        GivenErrValueTask_WhenMapOrElseAsyncWithASyncFactory_ThenReturnTheFactoryValue()
    {
        string result =
            await new ValueTask<Result<int, string>>(Err("Major Error"))
               .MapOrElseAsync(SyncFactory("Handled"), AsyncMap);

        result.ShouldBe("Handled");
    }
}
