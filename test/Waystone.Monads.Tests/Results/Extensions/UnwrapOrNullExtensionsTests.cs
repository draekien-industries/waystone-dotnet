namespace Waystone.Monads.Results.Extensions;

using JetBrains.Annotations;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

[TestSubject(typeof(ResultExtensions))]
public sealed class UnwrapOrNullExtensionsTests
{
    [Fact]
    public void GivenOk_WhenUnwrapOrNull_ThenReturnTheValue() =>
        Result.Ok<int, string>(1).UnwrapOrNull().ShouldBe(1);

    [Fact]
    public void GivenErr_WhenUnwrapOrNull_ThenReturnNull() =>
        Result.Err<int, string>("failed").UnwrapOrNull().ShouldBeNull();

    [Fact]
    public void
        GivenErr_WhenUnwrapOrNull_ThenTheFailureIsDistinctFromUnwrapOrDefault()
    {
        Result<int, string> err = Result.Err<int, string>("failed");

        err.UnwrapOrDefault().ShouldBe(0);
        err.UnwrapOrNull().ShouldBeNull();
    }

    [Fact]
    public async Task GivenOkTask_WhenUnwrapOrNullAsync_ThenReturnTheValue()
    {
        int? value = await Task.FromResult(Result.Ok<int, string>(1))
           .UnwrapOrNullAsync();

        value.ShouldBe(1);
    }

    [Fact]
    public async Task GivenErrTask_WhenUnwrapOrNullAsync_ThenReturnNull()
    {
        int? value = await Task.FromResult(Result.Err<int, string>("failed"))
           .UnwrapOrNullAsync();

        value.ShouldBeNull();
    }

    [Fact]
    public async Task
        GivenOkValueTask_WhenUnwrapOrNullAsync_ThenReturnTheValue()
    {
        int? value =
            await new ValueTask<Result<int, string>>(
                    Result.Ok<int, string>(1))
               .UnwrapOrNullAsync();

        value.ShouldBe(1);
    }

    [Fact]
    public async Task GivenErrValueTask_WhenUnwrapOrNullAsync_ThenReturnNull()
    {
        int? value =
            await new ValueTask<Result<int, string>>(
                    Result.Err<int, string>("failed"))
               .UnwrapOrNullAsync();

        value.ShouldBeNull();
    }
}
