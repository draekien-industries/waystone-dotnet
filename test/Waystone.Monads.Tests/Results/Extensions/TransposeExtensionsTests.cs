namespace Waystone.Monads.Results.Extensions;

using JetBrains.Annotations;
using Options;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

[TestSubject(typeof(ResultExtensions))]
public sealed class TransposeExtensionsTests
{
#region flatten

    [Fact]
    public void WhenFlatteningResult_ThenReduceNestingByOne()
    {
        Result<int, string> ok = Result.Ok<int, string>(1);

        Result<Result<string, string>, string> nested =
            ok.Map(_ => Result.Ok<string, string>("1"));

        Result<string, string> flattened = nested.Flatten();

        flattened.ShouldBeOkValue("1");
    }

#endregion flatten

#region transpose

    [Fact]
    public void GivenOkResultOfSome_WhenTranspose_ThenReturnSomeOfOk()
    {
        Result<int, string> ok = Result.Ok<int, string>(1);
        Result<Option<int>, string> okOfSome = ok.Map(Option.Some);

        Option<Result<int, string>> result = okOfSome.Transpose();

        result.ShouldBeSome();
        result.ShouldBeSomeValue(ok);
    }

    [Fact]
    public void GivenOkResultOfNone_WhenTranspose_ThenReturnNone()
    {
        Result<int, string> ok = Result.Ok<int, string>(1);
        Result<Option<int>, string> none = ok.Map(_ => Option.None<int>());

        Option<Result<int, string>> result = none.Transpose();

        result.ShouldBeNone();
    }

    [Fact]
    public void GivenErrOfSome_WhenTranspose_ThenReturnSomeOfErr()
    {
        Result<int, string> err = Result.Err<int, string>("failed");
        Result<Option<int>, string> errOfSome = err.Map(Option.Some);

        Option<Result<int, string>> result = errOfSome.Transpose();

        result.ShouldBeSome();
        result.ShouldBeSomeValue(err);
    }

    [Fact]
    public void GivenErrOfNone_WhenTranspose_ThenReturnSomeOfErr()
    {
        Result<int, string> err = Result.Err<int, string>("failed");

        Result<Option<int>, string> errOfNone =
            err.Map(_ => Option.None<int>());

        Option<Result<int, string>> result = errOfNone.Transpose();

        result.ShouldBeSome();
        result.ShouldBeSomeValue(err);
    }

    [Fact]
    public async Task GivenOkOfSomeTask_WhenTransposeAsync_ThenReturnSomeOfOk()
    {
        Result<int, string> ok = Result.Ok<int, string>(1);

        Option<Result<int, string>> result =
            await Task.FromResult(ok.Map(Option.Some)).TransposeAsync();

        result.ShouldBeSomeValue(ok);
    }

    [Fact]
    public async Task GivenOkOfNoneTask_WhenTransposeAsync_ThenReturnNone()
    {
        Result<Option<int>, string> okOfNone =
            Result.Ok<Option<int>, string>(Option.None<int>());

        Option<Result<int, string>> result =
            await Task.FromResult(okOfNone).TransposeAsync();

        result.ShouldBeNone();
    }

    [Fact]
    public async Task
        GivenErrValueTask_WhenTransposeAsync_ThenReturnSomeOfErr()
    {
        Result<int, string> err = Result.Err<int, string>("failed");

        Option<Result<int, string>> result =
            await new ValueTask<Result<Option<int>, string>>(
                    err.Map(Option.Some))
               .TransposeAsync();

        result.ShouldBeSomeValue(err);
    }

    [Fact]
    public async Task
        GivenOkOfSomeValueTask_WhenTransposeAsync_ThenReturnSomeOfOk()
    {
        Result<int, string> ok = Result.Ok<int, string>(1);

        Option<Result<int, string>> result =
            await new ValueTask<Result<Option<int>, string>>(
                    ok.Map(Option.Some))
               .TransposeAsync();

        result.ShouldBeSomeValue(ok);
    }

#endregion transpose
}
