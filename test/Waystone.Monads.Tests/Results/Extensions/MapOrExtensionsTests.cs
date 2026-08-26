namespace Waystone.Monads.Results.Extensions;

using JetBrains.Annotations;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

/// <remarks>
/// This family was converted to generated awaited receivers by DRA-110, and its
/// four non-state awaited overloads went in untested. DRA-133 found the gap while
/// closing the same one on <c>MapExtensions</c> and <c>MapErrExtensions</c>. The
/// state overload is covered in <c>AwaitedStateOverloadTests</c>.
/// </remarks>
[TestSubject(typeof(MapOrExtensions))]
public sealed class MapOrExtensionsTests
{
    [Fact]
    public async Task GivenOkTask_WhenMapOrAsyncWithASyncMap_ThenMapTheOk()
    {
        int result = await Task.FromResult(Result.Ok<int, string>(1))
                               .MapOrAsync(-1, value => value + 1);

        result.ShouldBe(2);
    }

    [Fact]
    public async Task GivenErrTask_WhenMapOrAsyncWithASyncMap_ThenUseTheDefault()
    {
        int result = await Task.FromResult(Result.Err<int, string>("failed"))
                               .MapOrAsync(-1, value => value + 1);

        result.ShouldBe(-1);
    }

    [Fact]
    public async Task GivenOkTask_WhenMapOrAsyncWithAnAsyncMap_ThenMapTheOk()
    {
        int result = await Task.FromResult(Result.Ok<int, string>(1))
                               .MapOrAsync(-1, value => Task.FromResult(value + 1));

        result.ShouldBe(2);
    }

    [Fact]
    public async Task
        GivenErrTask_WhenMapOrAsyncWithAnAsyncMap_ThenUseTheDefault()
    {
        int result = await Task.FromResult(Result.Err<int, string>("failed"))
                               .MapOrAsync(-1, value => Task.FromResult(value + 1));

        result.ShouldBe(-1);
    }

    [Fact]
    public async Task GivenOkValueTask_WhenMapOrAsyncWithASyncMap_ThenMapTheOk()
    {
        int result =
            await new ValueTask<Result<int, string>>(Result.Ok<int, string>(1))
               .MapOrAsync(-1, value => value + 1);

        result.ShouldBe(2);
    }

    [Fact]
    public async Task
        GivenErrValueTask_WhenMapOrAsyncWithASyncMap_ThenUseTheDefault()
    {
        int result =
            await new ValueTask<Result<int, string>>(
                    Result.Err<int, string>("failed"))
               .MapOrAsync(-1, value => value + 1);

        result.ShouldBe(-1);
    }

    [Fact]
    public async Task
        GivenOkValueTask_WhenMapOrAsyncWithAnAsyncMap_ThenMapTheOk()
    {
        int result =
            await new ValueTask<Result<int, string>>(Result.Ok<int, string>(1))
               .MapOrAsync(-1, value => Task.FromResult(value + 1));

        result.ShouldBe(2);
    }

    [Fact]
    public async Task
        GivenErrValueTask_WhenMapOrAsyncWithAnAsyncMap_ThenUseTheDefault()
    {
        int result =
            await new ValueTask<Result<int, string>>(
                    Result.Err<int, string>("failed"))
               .MapOrAsync(-1, value => Task.FromResult(value + 1));

        result.ShouldBe(-1);
    }
}
