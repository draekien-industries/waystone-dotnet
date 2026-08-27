namespace Waystone.Monads.Results.Extensions;

using JetBrains.Annotations;
using Monads.Extensions;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

[TestSubject(typeof(ResultExtensions))]
public sealed class IsOkAndExtensionsTests
{
    [Fact]
    public async Task GivenOkTask_WhenIsOkAndAsync_ThenEvaluateThePredicate()
    {
        bool result = await Task.FromResult(Result.Ok<int, string>(1))
           .IsOkAndAsync(x => x > 0);

        result.ShouldBeTrue();
    }

    [Fact]
    public async Task GivenErrTask_WhenIsOkAndAsync_ThenReturnFalse()
    {
        bool result = await Task.FromResult(Result.Err<int, string>("error"))
           .IsOkAndAsync(x => x > 0);

        result.ShouldBeFalse();
    }

    [Fact]
    public async Task
        GivenOkValueTask_WhenIsOkAndAsync_ThenEvaluateThePredicate()
    {
        bool result =
            await new ValueTask<Result<int, string>>(
                    Result.Ok<int, string>(1))
               .IsOkAndAsync(x => x > 0);

        result.ShouldBeTrue();
    }

    [Fact]
    public async Task GivenErrValueTask_WhenIsOkAndAsync_ThenReturnFalse()
    {
        bool result =
            await new ValueTask<Result<int, string>>(
                    Result.Err<int, string>("error"))
               .IsOkAndAsync(x => x > 0);

        result.ShouldBeFalse();
    }
}
