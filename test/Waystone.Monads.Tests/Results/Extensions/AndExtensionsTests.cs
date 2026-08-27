namespace Waystone.Monads.Results.Extensions;

using JetBrains.Annotations;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

[TestSubject(typeof(AndExtensions))]
public sealed class AndExtensionsTests
{
    [Fact]
    public async Task GivenOkTask_WhenAndAsync_ThenReturnTheOtherResult()
    {
        Result<string, string> result =
            await Task.FromResult(Result.Ok<int, string>(1))
                      .AndAsync(Result.Ok<string, string>("value"));

        result.ShouldBeOkValue("value");
    }

    [Fact]
    public async Task GivenErrTask_WhenAndAsync_ThenKeepTheErr()
    {
        Result<string, string> result =
            await Task.FromResult(Result.Err<int, string>("failed"))
                      .AndAsync(Result.Ok<string, string>("value"));

        result.ShouldBeErrValue("failed");
    }

    [Fact]
    public async Task GivenOkValueTask_WhenAndAsync_ThenReturnTheOtherResult()
    {
        Result<string, string> result =
            await new ValueTask<Result<int, string>>(Result.Ok<int, string>(1))
               .AndAsync(Result.Ok<string, string>("value"));

        result.ShouldBeOkValue("value");
    }

    [Fact]
    public async Task GivenErrValueTask_WhenAndAsync_ThenKeepTheErr()
    {
        Result<string, string> result =
            await new ValueTask<Result<int, string>>(
                    Result.Err<int, string>("failed"))
               .AndAsync(Result.Ok<string, string>("value"));

        result.ShouldBeErrValue("failed");
    }
}
