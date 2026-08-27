namespace Waystone.Monads.Results.Extensions;

using JetBrains.Annotations;
using Shouldly;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

[TestSubject(typeof(ResultExtensions))]
public sealed class AsEnumerableExtensionsTests
{
    [Fact]
    public async Task GivenOkTask_WhenAsEnumerableAsync_ThenYieldTheOkValue()
    {
        IEnumerable<int> result =
            await Task.FromResult(Result.Ok<int, string>(1))
               .AsEnumerableAsync();

        result.ShouldBe(new[] { 1 });
    }

    [Fact]
    public async Task GivenErrTask_WhenAsEnumerableAsync_ThenYieldNothing()
    {
        IEnumerable<int> result =
            await Task.FromResult(Result.Err<int, string>("error"))
               .AsEnumerableAsync();

        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task
        GivenOkValueTask_WhenAsEnumerableAsync_ThenYieldTheOkValue()
    {
        IEnumerable<int> result =
            await new ValueTask<Result<int, string>>(
                Result.Ok<int, string>(1)).AsEnumerableAsync();

        result.ShouldBe(new[] { 1 });
    }

    [Fact]
    public async Task GivenErrValueTask_WhenAsEnumerableAsync_ThenYieldNothing()
    {
        IEnumerable<int> result =
            await new ValueTask<Result<int, string>>(
                Result.Err<int, string>("error")).AsEnumerableAsync();

        result.ShouldBeEmpty();
    }
}
