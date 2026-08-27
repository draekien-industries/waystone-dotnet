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
/// <para>
/// Each case maps the error to its length, so a passing assertion also shows the
/// delegate received the error rather than the ok value.
/// </para>
/// </remarks>
[TestSubject(typeof(ResultExtensions))]
public sealed class MapErrExtensionsTests
{
    [Fact]
    public async Task GivenErrTask_WhenMapErrAsyncWithASyncMap_ThenMapTheErr()
    {
        Result<int, int> result =
            await Task.FromResult(Result.Err<int, string>("failed"))
                      .MapErrAsync(error => error.Length);

        result.ShouldBeErrValue(6);
    }

    [Fact]
    public async Task GivenOkTask_WhenMapErrAsyncWithASyncMap_ThenKeepTheOk()
    {
        Result<int, int> result =
            await Task.FromResult(Result.Ok<int, string>(1))
                      .MapErrAsync(error => error.Length);

        result.ShouldBeOkValue(1);
    }

    [Fact]
    public async Task GivenErrTask_WhenMapErrAsyncWithAnAsyncMap_ThenMapTheErr()
    {
        Result<int, int> result =
            await Task.FromResult(Result.Err<int, string>("failed"))
                      .MapErrAsync(error => Task.FromResult(error.Length));

        result.ShouldBeErrValue(6);
    }

    [Fact]
    public async Task GivenOkTask_WhenMapErrAsyncWithAnAsyncMap_ThenKeepTheOk()
    {
        Result<int, int> result =
            await Task.FromResult(Result.Ok<int, string>(1))
                      .MapErrAsync(error => Task.FromResult(error.Length));

        result.ShouldBeOkValue(1);
    }

    [Fact]
    public async Task
        GivenErrValueTask_WhenMapErrAsyncWithASyncMap_ThenMapTheErr()
    {
        Result<int, int> result =
            await new ValueTask<Result<int, string>>(
                    Result.Err<int, string>("failed"))
               .MapErrAsync(error => error.Length);

        result.ShouldBeErrValue(6);
    }

    [Fact]
    public async Task GivenOkValueTask_WhenMapErrAsyncWithASyncMap_ThenKeepTheOk()
    {
        Result<int, int> result =
            await new ValueTask<Result<int, string>>(Result.Ok<int, string>(1))
               .MapErrAsync(error => error.Length);

        result.ShouldBeOkValue(1);
    }

    [Fact]
    public async Task
        GivenErrValueTask_WhenMapErrAsyncWithAnAsyncMap_ThenMapTheErr()
    {
        Result<int, int> result =
            await new ValueTask<Result<int, string>>(
                    Result.Err<int, string>("failed"))
               .MapErrAsync(error => Task.FromResult(error.Length));

        result.ShouldBeErrValue(6);
    }

    [Fact]
    public async Task
        GivenOkValueTask_WhenMapErrAsyncWithAnAsyncMap_ThenKeepTheOk()
    {
        Result<int, int> result =
            await new ValueTask<Result<int, string>>(Result.Ok<int, string>(1))
               .MapErrAsync(error => Task.FromResult(error.Length));

        result.ShouldBeOkValue(1);
    }
}
