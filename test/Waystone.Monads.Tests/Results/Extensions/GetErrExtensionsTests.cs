namespace Waystone.Monads.Results.Extensions;

using JetBrains.Annotations;
using Options;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

[TestSubject(typeof(ResultExtensions))]
public sealed class GetErrExtensionsTests
{
    [Fact]
    public async Task GivenErrTask_WhenGetErrAsync_ThenReturnSome()
    {
        Option<string> result =
            await Task.FromResult(Result.Err<int, string>("failed"))
                      .GetErrAsync();

        result.ShouldBeSomeValue("failed");
    }

    [Fact]
    public async Task GivenOkTask_WhenGetErrAsync_ThenReturnNone()
    {
        Option<string> result =
            await Task.FromResult(Result.Ok<int, string>(1)).GetErrAsync();

        result.ShouldBeNone();
    }

    [Fact]
    public async Task GivenErrValueTask_WhenGetErrAsync_ThenReturnSome()
    {
        Option<string> result =
            await new ValueTask<Result<int, string>>(
                    Result.Err<int, string>("failed"))
               .GetErrAsync();

        result.ShouldBeSomeValue("failed");
    }

    [Fact]
    public async Task GivenOkValueTask_WhenGetErrAsync_ThenReturnNone()
    {
        Option<string> result =
            await new ValueTask<Result<int, string>>(Result.Ok<int, string>(1))
               .GetErrAsync();

        result.ShouldBeNone();
    }
}
