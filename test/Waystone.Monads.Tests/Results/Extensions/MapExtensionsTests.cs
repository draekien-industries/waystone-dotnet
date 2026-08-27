namespace Waystone.Monads.Results.Extensions;

using JetBrains.Annotations;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

/// <remarks>
/// DRA-133 converted this family to generated awaited receivers, which is what
/// exposed that its awaited-receiver overloads had never been exercised. The
/// state overload lives in <c>AwaitedStateOverloadTests</c> with the rest of the
/// lifted ones; the two delegate shapes on each receiver are here.
/// </remarks>
[TestSubject(typeof(ResultExtensions))]
public sealed class MapExtensionsTests
{
    [Fact]
    public async Task GivenOkTask_WhenMapAsyncWithASyncMap_ThenMapTheOk()
    {
        Result<int, string> result =
            await Task.FromResult(Result.Ok<int, string>(1))
                      .MapAsync(value => value + 1);

        result.ShouldBeOkValue(2);
    }

    [Fact]
    public async Task GivenErrTask_WhenMapAsyncWithASyncMap_ThenKeepTheErr()
    {
        Result<int, string> result =
            await Task.FromResult(Result.Err<int, string>("failed"))
                      .MapAsync(value => value + 1);

        result.ShouldBeErrValue("failed");
    }

    [Fact]
    public async Task GivenOkTask_WhenMapAsyncWithAnAsyncMap_ThenMapTheOk()
    {
        Result<int, string> result =
            await Task.FromResult(Result.Ok<int, string>(1))
                      .MapAsync(value => Task.FromResult(value + 1));

        result.ShouldBeOkValue(2);
    }

    [Fact]
    public async Task GivenErrTask_WhenMapAsyncWithAnAsyncMap_ThenKeepTheErr()
    {
        Result<int, string> result =
            await Task.FromResult(Result.Err<int, string>("failed"))
                      .MapAsync(value => Task.FromResult(value + 1));

        result.ShouldBeErrValue("failed");
    }

    [Fact]
    public async Task GivenOkValueTask_WhenMapAsyncWithASyncMap_ThenMapTheOk()
    {
        Result<int, string> result =
            await new ValueTask<Result<int, string>>(Result.Ok<int, string>(1))
               .MapAsync(value => value + 1);

        result.ShouldBeOkValue(2);
    }

    [Fact]
    public async Task GivenErrValueTask_WhenMapAsyncWithASyncMap_ThenKeepTheErr()
    {
        Result<int, string> result =
            await new ValueTask<Result<int, string>>(
                    Result.Err<int, string>("failed"))
               .MapAsync(value => value + 1);

        result.ShouldBeErrValue("failed");
    }

    [Fact]
    public async Task GivenOkValueTask_WhenMapAsyncWithAnAsyncMap_ThenMapTheOk()
    {
        Result<int, string> result =
            await new ValueTask<Result<int, string>>(Result.Ok<int, string>(1))
               .MapAsync(value => Task.FromResult(value + 1));

        result.ShouldBeOkValue(2);
    }

    [Fact]
    public async Task
        GivenErrValueTask_WhenMapAsyncWithAnAsyncMap_ThenKeepTheErr()
    {
        Result<int, string> result =
            await new ValueTask<Result<int, string>>(
                    Result.Err<int, string>("failed"))
               .MapAsync(value => Task.FromResult(value + 1));

        result.ShouldBeErrValue("failed");
    }
}
