namespace Waystone.Monads.Results.Extensions;

using JetBrains.Annotations;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

/// <remarks>
/// Named outer-first. The specs these replaced distinguished an outer
/// <c>Err</c> holding an <c>Ok</c> from one holding an <c>Err</c>, which reads
/// as two cases and is one: an outer <c>Err</c> has no inner result to hold, so
/// both were the same call.
/// </remarks>
[TestSubject(typeof(ResultExtensions))]
public sealed class FlattenExtensionsTests
{
    private static Result<Result<int, string>, string> OuterOk(
        Result<int, string> inner) =>
        Result.Ok<Result<int, string>, string>(inner);

    private static Result<Result<int, string>, string> OuterErr(
        string error) =>
        Result.Err<Result<int, string>, string>(error);

    [Fact]
    public void GivenOkOfOk_WhenFlatten_ThenReturnTheInnerOk()
    {
        Result<int, string> result =
            OuterOk(Result.Ok<int, string>(10)).Flatten();

        result.ShouldBeOkValue(10);
    }

    [Fact]
    public void GivenOkOfErr_WhenFlatten_ThenReturnTheInnerErr()
    {
        Result<int, string> result =
            OuterOk(Result.Err<int, string>("Error")).Flatten();

        result.ShouldBeErrValue("Error");
    }

    [Fact]
    public void GivenErr_WhenFlatten_ThenReturnTheOuterErr()
    {
        Result<int, string> result = OuterErr("Error").Flatten();

        result.ShouldBeErrValue("Error");
    }

    [Fact]
    public async Task GivenATaskOfOkOfOk_WhenFlattenAsync_ThenReturnTheInnerOk()
    {
        Result<int, string> result =
            await Task.FromResult(OuterOk(Result.Ok<int, string>(20)))
               .FlattenAsync();

        result.ShouldBeOkValue(20);
    }

    [Fact]
    public async Task
        GivenATaskOfOkOfErr_WhenFlattenAsync_ThenReturnTheInnerErr()
    {
        Result<int, string> result =
            await Task.FromResult(
                    OuterOk(Result.Err<int, string>("Async Error")))
               .FlattenAsync();

        result.ShouldBeErrValue("Async Error");
    }

    [Fact]
    public async Task GivenATaskOfErr_WhenFlattenAsync_ThenReturnTheOuterErr()
    {
        Result<int, string> result =
            await Task.FromResult(OuterErr("Async Error")).FlattenAsync();

        result.ShouldBeErrValue("Async Error");
    }

    [Fact]
    public async Task
        GivenAValueTaskOfOkOfOk_WhenFlattenAsync_ThenReturnTheInnerOk()
    {
        Result<int, string> result =
            await new ValueTask<Result<Result<int, string>, string>>(
                OuterOk(Result.Ok<int, string>(30))).FlattenAsync();

        result.ShouldBeOkValue(30);
    }

    [Fact]
    public async Task
        GivenAValueTaskOfOkOfErr_WhenFlattenAsync_ThenReturnTheInnerErr()
    {
        Result<int, string> result =
            await new ValueTask<Result<Result<int, string>, string>>(
                    OuterOk(Result.Err<int, string>("Async VT Error")))
               .FlattenAsync();

        result.ShouldBeErrValue("Async VT Error");
    }

    [Fact]
    public async Task
        GivenAValueTaskOfErr_WhenFlattenAsync_ThenReturnTheOuterErr()
    {
        Result<int, string> result =
            await new ValueTask<Result<Result<int, string>, string>>(
                OuterErr("Async VT Error")).FlattenAsync();

        result.ShouldBeErrValue("Async VT Error");
    }
}
