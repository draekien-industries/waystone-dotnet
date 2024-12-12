﻿namespace Waystone.Monads.Tests;

[TestSubject(typeof(Result))]
public class ResultTest
{
    [Fact]
    public void GivenFactoryThatSucceeds_WhenBindingFactory_ThenReturnOk()
    {
        var callback = Substitute.For<Func<Exception, string>>();
        Result<int, string>
            result = Result.Bind(() => 1, callback);
        result.Should().Be(Result.Ok<int, string>(1));
        callback.DidNotReceive().Invoke(Arg.Any<Exception>());
    }

    [Fact]
    public void GivenFactoryThatFails_WhenBindingFactory_ThenReturnError()
    {
        var callback = Substitute.For<Func<Exception, string>>();
        callback.Invoke(Arg.Any<Exception>()).Returns("error");
        Result<int, string> result = Result.Bind<int, string>(
            () => throw new Exception(),
            callback);
        result.Should().Be(Result.Err<int, string>("error"));
        callback.Received(1).Invoke(Arg.Any<Exception>());
    }

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

    [Fact]
    public void WhenImplicitlyCreatingResult_ThenReturnExpected()
    {
        Result<int, string> ok = 1;
        Result<int, string> err = "error";

        ok.Should().Be(Result.Ok<int, string>(1));
        err.Should().Be(Result.Err<int, string>("error"));
    }
}
