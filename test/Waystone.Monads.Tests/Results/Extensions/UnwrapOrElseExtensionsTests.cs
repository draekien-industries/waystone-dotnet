namespace Waystone.Monads.Results.Extensions;

using JetBrains.Annotations;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

[TestSubject(typeof(ResultExtensions))]
public sealed class UnwrapOrElseExtensionsTests
{
    [Fact]
    public async Task
        GivenErrTask_WhenUnwrapOrElseAsyncWithASyncFactory_ThenComputeFromTheError()
    {
        int result = await Task.FromResult(Result.Err<int, string>("failed"))
                               .UnwrapOrElseAsync(error => error.Length);

        result.ShouldBe(6);
    }

    [Fact]
    public async Task
        GivenOkTask_WhenUnwrapOrElseAsyncWithASyncFactory_ThenReturnTheOkValue()
    {
        int result = await Task.FromResult(Result.Ok<int, string>(1))
                               .UnwrapOrElseAsync(error => error.Length);

        result.ShouldBe(1);
    }

    [Fact]
    public async Task
        GivenErrTask_WhenUnwrapOrElseAsyncWithAnAsyncFactory_ThenComputeFromTheError()
    {
        int result = await Task.FromResult(Result.Err<int, string>("failed"))
                               .UnwrapOrElseAsync(
                                    error => Task.FromResult(error.Length));

        result.ShouldBe(6);
    }

    [Fact]
    public async Task
        GivenErrValueTask_WhenUnwrapOrElseAsyncWithASyncFactory_ThenComputeFromTheError()
    {
        int result = await new ValueTask<Result<int, string>>(
                Result.Err<int, string>("failed"))
           .UnwrapOrElseAsync(error => error.Length);

        result.ShouldBe(6);
    }

    [Fact]
    public async Task
        GivenOkValueTask_WhenUnwrapOrElseAsyncWithASyncFactory_ThenReturnTheOkValue()
    {
        int result = await new ValueTask<Result<int, string>>(
                Result.Ok<int, string>(1))
           .UnwrapOrElseAsync(error => error.Length);

        result.ShouldBe(1);
    }

    [Fact]
    public async Task
        GivenErrValueTask_WhenUnwrapOrElseAsyncWithAnAsyncFactory_ThenComputeFromTheError()
    {
        int result = await new ValueTask<Result<int, string>>(
                Result.Err<int, string>("failed"))
           .UnwrapOrElseAsync(error => Task.FromResult(error.Length));

        result.ShouldBe(6);
    }
}
