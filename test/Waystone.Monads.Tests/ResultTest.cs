namespace Waystone.Monads.Tests;

[TestSubject(typeof(Result))]
public class ResultTest
{
    [Fact]
    public void WhenFlatteningResult_ThenReduceNestingByOne()
    {
        Result<int, string> ok = Result.Ok<int, string>(1);
        Result<Result<string, string>, string> nested =
            ok.Map(_ => Result.Ok<string, string>("1"));

        Result<string, string> flattened = nested.Flatten();

        flattened.Unwrap().Should().Be("1");
    }

    [Fact]
    public void GivenOkResultOfSome_WhenTranspose_ThenReturnSomeOfOk()
    {
        Result<int, string> ok = Result.Ok<int, string>(1);
        Result<IOption<int>, string> okOfSome = ok.Map(Option.Some);

        IOption<Result<int, string>> result = okOfSome.Transpose();

        result.IsSome.Should().BeTrue();
        result.Unwrap().Should().Be(ok);
    }

    [Fact]
    public void GivenOkResultOfNone_WhenTranspose_ThenReturnNone()
    {
        Result<int, string> ok = Result.Ok<int, string>(1);
        Result<IOption<int>, string> none = ok.Map(_ => Option.None<int>());

        IOption<Result<int, string>> result = none.Transpose();

        result.IsNone.Should().BeTrue();
    }

    [Fact]
    public void GivenErrOfSome_WhenTranspose_ThenReturnSomeOfErr()
    {
        Result<int, string> err = Result.Err<int, string>("failed");
        Result<IOption<int>, string> errOfSome = err.Map(Option.Some);

        IOption<Result<int, string>> result = errOfSome.Transpose();

        result.IsSome.Should().BeTrue();
        result.Unwrap().Should().Be(err);
    }

    [Fact]
    public void GivenErrOfNone_WhenTranspose_ThenReturnSomeOfErr()
    {
        Result<int, string> err = Result.Err<int, string>("failed");
        Result<IOption<int>, string> errOfNone =
            err.Map(_ => Option.None<int>());

        IOption<Result<int, string>> result = errOfNone.Transpose();

        result.IsSome.Should().BeTrue();
        result.Unwrap().Should().Be(err);
    }
}
