namespace Waystone.Monads.Results.Extensions;

using Options;

[TestSubject(typeof(Result))]
public sealed class ResultOfTOkTErrExtensionsTests
{
#region flatten

    [Fact]
    public void WhenFlatteningResult_ThenReduceNestingByOne()
    {
        Result<int, string> ok = Result.Ok<int, string>(1);
        Result<Result<string, string>, string> nested =
            ok.Map(_ => Result.Ok<string, string>("1"));

        Result<string, string> flattened = nested.Flatten();

        flattened.Unwrap().Should().Be("1");
    }

#endregion flatten

#region awaited

    [Fact]
    public async Task GivenResultOfTaskThatSucceeds_WhenAwaited_ThenReturnNone()
    {
        async Task<int> PerformTask(int x) => await Task.FromResult(x + 1);

        var callback = Substitute.For<Func<Exception, string>>();

        Result<Task<int>, string> result =
            Result.Ok<int, string>(1).Map(PerformTask);

        Result<int, string> awaitedResult = await result.Awaited(callback);
        awaitedResult.Should().Be(Result.Ok<int, string>(2));
        callback.DidNotReceive().Invoke(Arg.Any<Exception>());
    }

    [Fact]
    public async Task
        GivenErrResult_AndResultOfTask_WhenAwaited_ThenReturnNone()
    {
        async Task<int> PerformTask(int x) => await Task.FromResult(x + 1);

        var callback = Substitute.For<Func<Exception, string>>();

        Result<Task<int>, string> result =
            Result.Err<int, string>("error").Map(PerformTask);

        Result<int, string> awaitedResult = await result.Awaited(callback);
        awaitedResult.Should().Be(Result.Err<int, string>("error"));
        callback.DidNotReceive().Invoke(Arg.Any<Exception>());
    }

    [Fact]
    public async Task GivenResultOfTaskThatThrows_WhenAwaited_ThenReturnNone()
    {
        async Task<int> PerformTask(int x)
        {
            await Task.Delay(10);
            throw new Exception();
        }

        var callback = Substitute.For<Func<Exception, string>>();
        callback.Invoke(Arg.Any<Exception>()).Returns("error");

        Result<Task<int>, string> result =
            Result.Ok<int, string>(1).Map(PerformTask);

        Result<int, string> awaitedResult = await result.Awaited(callback);
        awaitedResult.Should().Be(Result.Err<int, string>("error"));
        callback.Received().Invoke(Arg.Any<Exception>());
    }

#endregion awaited

#region transpose

    [Fact]
    public void GivenOkResultOfSome_WhenTranspose_ThenReturnSomeOfOk()
    {
        Result<int, string> ok = Result.Ok<int, string>(1);
        Result<Option<int>, string> okOfSome = ok.Map(Option.Some);

        Option<Result<int, string>> result = okOfSome.Transpose();

        result.IsSome.Should().BeTrue();
        result.Unwrap().Should().Be(ok);
    }

    [Fact]
    public void GivenOkResultOfNone_WhenTranspose_ThenReturnNone()
    {
        Result<int, string> ok = Result.Ok<int, string>(1);
        Result<Option<int>, string> none = ok.Map(_ => Option.None<int>());

        Option<Result<int, string>> result = none.Transpose();

        result.IsNone.Should().BeTrue();
    }

    [Fact]
    public void GivenErrOfSome_WhenTranspose_ThenReturnSomeOfErr()
    {
        Result<int, string> err = Result.Err<int, string>("failed");
        Result<Option<int>, string> errOfSome = err.Map(Option.Some);

        Option<Result<int, string>> result = errOfSome.Transpose();

        result.IsSome.Should().BeTrue();
        result.Unwrap().Should().Be(err);
    }

    [Fact]
    public void GivenErrOfNone_WhenTranspose_ThenReturnSomeOfErr()
    {
        Result<int, string> err = Result.Err<int, string>("failed");
        Result<Option<int>, string> errOfNone =
            err.Map(_ => Option.None<int>());

        Option<Result<int, string>> result = errOfNone.Transpose();

        result.IsSome.Should().BeTrue();
        result.Unwrap().Should().Be(err);
    }

#endregion transpose
}
