namespace Waystone.Monads.Tests;

using Exceptions;

[TestSubject(typeof(Err<,>))]
public class ErrTest
{
    [Fact]
    public void WhenCreatingErr_ThenEvaluateToErr()
    {
        Result<int, string> result = Result.Err<int, string>("error");

        result.IsOk.Should().BeFalse();
        result.IsErr.Should().BeTrue();
    }

    [Fact]
    public void WhenIsOkAnd_ThenReturnFalse()
    {
        Result<int, string> result = Result.Err<int, string>("error");

        result.IsOkAnd(_ => true).Should().BeFalse();
        result.IsOkAnd(_ => false).Should().BeFalse();
    }

    [Fact]
    public void WhenIsErrAnd_ThenReturnPredicateResult()
    {
        Result<int, string> result = Result.Err<int, string>("error");
        result.IsErrAnd(_ => true).Should().BeTrue();
        result.IsErrAnd(_ => false).Should().BeFalse();
    }

    [Fact]
    public void GivenFunc_WhenMatch_ThenInvokeOnErr()
    {
        Result<int, string> result = Result.Err<int, string>("error");

        var onOk = Substitute.For<Func<int, bool>>();
        onOk.Invoke(1).Returns(true);

        var onErr = Substitute.For<Func<string, bool>>();
        onErr.Invoke(Arg.Any<string>()).Returns(false);

        bool output = result.Match(onOk, onErr);

        output.Should().BeFalse();
        onOk.DidNotReceive().Invoke(Arg.Any<int>());
        onErr.Received(1).Invoke("error");
    }

    [Fact]
    public void GivenAction_WhenMatch_ThenInvokeOnErr()
    {
        Result<int, string> result = Result.Err<int, string>("error");

        var onOk = Substitute.For<Action<int>>();
        var onErr = Substitute.For<Action<string>>();

        result.Match(onOk, onErr);

        onOk.DidNotReceive().Invoke(Arg.Any<int>());
        onErr.Received(1).Invoke("error");
    }

    [Fact]
    public void WhenAnd_ThenReturnError()
    {
        Result<int, string> result = Result.Err<int, string>("error");

        result.And(Result.Ok<string, string>("success"))
              .Should()
              .Be(Result.Err<string, string>("error"));

        result.And(Result.Err<bool, string>("error 2"))
              .Should()
              .Be(Result.Err<bool, string>("error"));
    }

    [Fact]
    public void WhenAndThen_ThenReturnError()
    {
        Result<int, string> result = Result.Err<int, string>("error");

        result.AndThen(_ => Result.Ok<string, string>("success"))
              .Should()
              .Be(Result.Err<string, string>("error"));

        result.AndThen(_ => Result.Err<bool, string>("error 2"))
              .Should()
              .Be(Result.Err<bool, string>("error"));
    }

    [Fact]
    public void WhenOr_ThenReturnOther()
    {
        Result<int, string> result = Result.Err<int, string>("error");

        result.Or(Result.Ok<int, bool>(1))
              .Should()
              .Be(Result.Ok<int, bool>(1));

        result.Or(Result.Err<int, bool>(false))
              .Should()
              .Be(Result.Err<int, bool>(false));
    }

    [Fact]
    public void WhenOrElse_ThenReturnOther()
    {
        Result<int, string> result = Result.Err<int, string>("error");

        result.OrElse(_ => Result.Ok<int, bool>(1))
              .Should()
              .Be(Result.Ok<int, bool>(1));

        result.OrElse(_ => Result.Err<int, bool>(false))
              .Should()
              .Be(Result.Err<int, bool>(false));
    }

    [Fact]
    public void WhenExpect_ThenThrowException()
    {
        Result<int, string> result = Result.Err<int, string>("error");

        result.Invoking(x => x.Expect("Error should not occur"))
              .Should()
              .Throw<UnmetExpectationException>()
              .WithMessage("Error should not occur: error");
    }

    [Fact]
    public void WhenExpectErr_ThenReturnErrValue()
    {
        Result<int, string> result = Result.Err<int, string>("error");

        result.ExpectErr("Error should have occurred").Should().Be("error");
    }

    [Fact]
    public void WhenUnwrap_ThenThrowException()
    {
        Result<int, string> result = Result.Err<int, string>("error");

        result.Invoking(x => x.Unwrap())
              .Should()
              .Throw<UnwrapException>();
    }

    [Fact]
    public void WhenUnwrapOr_ThenReturnOrValue()
    {
        Result<int, string> result = Result.Err<int, string>("error");

        result.UnwrapOr(10).Should().Be(10);
        result.UnwrapOrDefault().Should().Be(default);
        result.UnwrapOrElse(_ => 10).Should().Be(10);
    }

    [Fact]
    public void WhenUnwrapErr_ThenReturnErrValue()
    {
        Result<int, string> result = Result.Err<int, string>("error");

        result.UnwrapErr().Should().Be("error");
    }

    [Fact]
    public void WhenInspect_ThenDoNothing()
    {
        Result<int, string> result = Result.Err<int, string>("error");

        var action = Substitute.For<Action<int>>();

        result.Inspect(action).Should().Be(result);
        action.DidNotReceive().Invoke(Arg.Any<int>());
    }

    [Fact]
    public void WhenInspectErr_ThenInvokeInspect()
    {
        Result<int, string> result = Result.Err<int, string>("error");

        var action = Substitute.For<Action<string>>();

        result.InspectErr(action).Should().Be(result);
        action.Received(1).Invoke("error");
    }

    [Fact]
    public void WhenMapOr_ThenReturnOrValue()
    {
        Result<int, string> result = Result.Err<int, string>("error");

        result.MapOr(10, x => x + 1).Should().Be(10);
        result.MapOrElse(_ => 10, x => x + 1).Should().Be(10);
    }

    [Fact]
    public void WhenMapErr_ThenReturnMappedErrValue()
    {
        Result<int, string> result = Result.Err<int, string>("error");

        result.MapErr(x => $"{x} 1")
              .Should()
              .Be(Result.Err<int, string>("error 1"));
    }

    [Fact]
    public void WhenGetOk_ThenReturnNone()
    {
        Result<int, string> result = Result.Err<int, string>("error");

        result.GetOk().Should().Be(Option.None<int>());
    }

    [Fact]
    public void WhenGetErr_ThenReturnSome()
    {
        Result<int, string> result = Result.Err<int, string>("error");

        result.GetErr().Should().Be(Option.Some<string>("error"));
    }
}
