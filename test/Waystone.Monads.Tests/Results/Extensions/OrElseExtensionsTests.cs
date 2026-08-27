namespace Waystone.Monads.Results.Extensions;

using JetBrains.Annotations;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

[TestSubject(typeof(ResultExtensions))]
public sealed class OrElseExtensionsTests
{
    [Fact]
    public async Task GivenErrTask_WhenOrElseAsyncWithASyncFactory_ThenRecover()
    {
        Result<int, string> result =
            await Task.FromResult(Result.Err<int, string>("failed"))
                      .OrElseAsync(
                           error => Result.Ok<int, string>(error.Length));

        result.ShouldBeOkValue(6);
    }

    [Fact]
    public async Task GivenOkTask_WhenOrElseAsyncWithASyncFactory_ThenKeepTheOk()
    {
        Result<int, string> result =
            await Task.FromResult(Result.Ok<int, string>(1))
                      .OrElseAsync(
                           error => Result.Ok<int, string>(error.Length));

        result.ShouldBeOkValue(1);
    }

    [Fact]
    public async Task
        GivenErrTask_WhenOrElseAsyncWithAnAsyncFactory_ThenRecover()
    {
        Result<int, string> result =
            await Task.FromResult(Result.Err<int, string>("failed"))
                      .OrElseAsync(
                           error => new ValueTask<Result<int, string>>(
                               Result.Ok<int, string>(error.Length)));

        result.ShouldBeOkValue(6);
    }

    [Fact]
    public async Task
        GivenErrValueTask_WhenOrElseAsyncWithASyncFactory_ThenRecover()
    {
        Result<int, string> result =
            await new ValueTask<Result<int, string>>(
                    Result.Err<int, string>("failed"))
               .OrElseAsync(error => Result.Ok<int, string>(error.Length));

        result.ShouldBeOkValue(6);
    }

    [Fact]
    public async Task
        GivenOkValueTask_WhenOrElseAsyncWithASyncFactory_ThenKeepTheOk()
    {
        Result<int, string> result =
            await new ValueTask<Result<int, string>>(
                    Result.Ok<int, string>(1))
               .OrElseAsync(error => Result.Ok<int, string>(error.Length));

        result.ShouldBeOkValue(1);
    }

    [Fact]
    public async Task
        GivenErrValueTask_WhenOrElseAsyncWithAnAsyncFactory_ThenRecover()
    {
        Result<int, string> result =
            await new ValueTask<Result<int, string>>(
                    Result.Err<int, string>("failed"))
               .OrElseAsync(
                    error => new ValueTask<Result<int, string>>(
                        Result.Ok<int, string>(error.Length)));

        result.ShouldBeOkValue(6);
    }
}
