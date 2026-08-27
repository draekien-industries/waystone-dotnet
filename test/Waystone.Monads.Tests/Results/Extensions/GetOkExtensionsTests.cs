namespace Waystone.Monads.Results.Extensions;

using JetBrains.Annotations;
using Options;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

[TestSubject(typeof(GetOkExtensions))]
public sealed class GetOkExtensionsTests
{
    [Fact]
    public async Task GivenOkTask_WhenGetOkAsync_ThenReturnSome()
    {
        Option<int> result = await Task.FromResult(Result.Ok<int, string>(1))
                                       .GetOkAsync();

        result.ShouldBeSomeValue(1);
    }

    [Fact]
    public async Task GivenErrTask_WhenGetOkAsync_ThenReturnNone()
    {
        Option<int> result =
            await Task.FromResult(Result.Err<int, string>("failed"))
                      .GetOkAsync();

        result.ShouldBeNone();
    }

    [Fact]
    public async Task GivenOkValueTask_WhenGetOkAsync_ThenReturnSome()
    {
        Option<int> result =
            await new ValueTask<Result<int, string>>(Result.Ok<int, string>(1))
               .GetOkAsync();

        result.ShouldBeSomeValue(1);
    }

    [Fact]
    public async Task GivenErrValueTask_WhenGetOkAsync_ThenReturnNone()
    {
        Option<int> result =
            await new ValueTask<Result<int, string>>(
                    Result.Err<int, string>("failed"))
               .GetOkAsync();

        result.ShouldBeNone();
    }
}
