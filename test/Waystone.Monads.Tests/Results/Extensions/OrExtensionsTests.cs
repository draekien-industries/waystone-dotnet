namespace Waystone.Monads.Results.Extensions;

using JetBrains.Annotations;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

/// <remarks>
/// The other result carries a different error type, so <c>OrAsync</c> changes the
/// error half of the receiver's type while keeping the success half. That is the
/// part worth pinning: an <see cref="Ok{TOk,TErr}" /> survives with its value
/// intact but re-typed, which a test asserting only on the error branch would miss.
/// </remarks>
[TestSubject(typeof(ResultExtensions))]
public sealed class OrExtensionsTests
{
    [Fact]
    public async Task GivenOkTask_WhenOrAsync_ThenKeepTheOk()
    {
        Result<int, int> result =
            await Task.FromResult(Result.Ok<int, string>(1))
                      .OrAsync(Result.Ok<int, int>(2));

        result.ShouldBeOkValue(1);
    }

    [Fact]
    public async Task GivenErrTask_WhenOrAsync_ThenReturnTheOtherResult()
    {
        Result<int, int> result =
            await Task.FromResult(Result.Err<int, string>("failed"))
                      .OrAsync(Result.Ok<int, int>(2));

        result.ShouldBeOkValue(2);
    }

    [Fact]
    public async Task GivenOkValueTask_WhenOrAsync_ThenKeepTheOk()
    {
        Result<int, int> result =
            await new ValueTask<Result<int, string>>(Result.Ok<int, string>(1))
               .OrAsync(Result.Ok<int, int>(2));

        result.ShouldBeOkValue(1);
    }

    [Fact]
    public async Task GivenErrValueTask_WhenOrAsync_ThenReturnTheOtherErr()
    {
        Result<int, int> result =
            await new ValueTask<Result<int, string>>(
                    Result.Err<int, string>("failed"))
               .OrAsync(Result.Err<int, int>(9));

        result.ShouldBeErrValue(9);
    }
}
