namespace Waystone.Monads.Tests;

using Exceptions;

[TestSubject(typeof(Ok<,>))]
public class OkTest
{
    [Fact]
    public void WhenCreatingOk_ThenEvaluateToOk()
    {
        IResult<int, string> ok = Result.Ok<int, string>(1);

        ok.IsOk.Should().BeTrue();
        ok.IsErr.Should().BeFalse();
    }

    [Fact]
    public void WhenIsOkAnd_ThenReturnPredicateResult()
    {
        IResult<int, string> ok = Result.Ok<int, string>(1);

        ok.IsOkAnd(_ => false).Should().BeFalse();
        ok.IsOkAnd(_ => true).Should().BeTrue();
    }

    [Fact]
    public void WhenIsErrAnd_ThenReturnFalse()
    {
        IResult<int, string> ok = Result.Ok<int, string>(1);

        ok.IsErrAnd(_ => true).Should().BeFalse();
        ok.IsErrAnd(_ => false).Should().BeFalse();
    }

    [Fact]
    public void GivenFunc_WhenMatch_ThenInvokeOnOk()
    {
        IResult<int, string> ok = Result.Ok<int, string>(1);

        var onOk = Substitute.For<Func<int, bool>>();
        onOk.Invoke(1).Returns(true);

        var onErr = Substitute.For<Func<string, bool>>();
        onErr.Invoke(Arg.Any<string>()).Returns(false);

        bool result = ok.Match(onOk, onErr);

        result.Should().BeTrue();
        onOk.Received(1).Invoke(1);
        onErr.DidNotReceive().Invoke(Arg.Any<string>());
    }

    [Fact]
    public void GivenAction_WhenMatch_ThenInvokeOnOk()
    {
        IResult<int, string> ok = Result.Ok<int, string>(1);

        var onOk = Substitute.For<Action<int>>();

        var onErr = Substitute.For<Action<string>>();

        ok.Match(onOk, onErr);

        onOk.Received(1).Invoke(1);
        onErr.DidNotReceive().Invoke(Arg.Any<string>());
    }

    [Fact]
    public void WhenAnd_ThenReturnOther()
    {
        IResult<int, string> ok = Result.Ok<int, string>(1);

        IResult<string, string> result = ok.And(Result.Ok<string, string>("2"));

        result.Should().Be(Result.Ok<string, string>("2"));
    }

    [Fact]
    public void WhenAndThen_ThenReturnOther()
    {
        IResult<int, string> ok = Result.Ok<int, string>(1);
        IResult<string, string> result =
            ok.AndThen(x => Result.Ok<string, string>(x.ToString()));
        result.Should().Be(Result.Ok<string, string>("1"));
    }

    [Fact]
    public void WhenOr_ThenReturnOk()
    {
        IResult<int, string> ok = Result.Ok<int, string>(1);

        ok.Or(Result.Ok<int, bool>(2)).Should().Be(Result.Ok<int, bool>(1));
        ok.Or(Result.Err<int, bool>(false))
          .Should()
          .Be(Result.Ok<int, bool>(1));
    }

    [Fact]
    public void WhenOrElse_ThenReturnOk()
    {
        IResult<int, string> ok = Result.Ok<int, string>(1);

        ok.OrElse(_ => Result.Ok<int, bool>(2))
          .Should()
          .Be(Result.Ok<int, bool>(1));
        ok.OrElse(_ => Result.Err<int, bool>(false))
          .Should()
          .Be(Result.Ok<int, bool>(1));
    }

    [Fact]
    public void WhenExpect_ThenReturnValue()
    {
        IResult<int, string> ok = Result.Ok<int, string>(1);

        ok.Expect("value is 1").Should().Be(1);
        ok.Invoking(x => x.ExpectErr("value should not be 1"))
          .Should()
          .Throw<UnmetExpectationException>()
          .WithMessage("Value should not be 1: 1");
    }

    [Fact]
    public void WhenUnwrap_ThenReturnValue()
    {
        IResult<int, string> ok = Result.Ok<int, string>(1);

        ok.Unwrap().Should().Be(1);
        ok.UnwrapOr(10).Should().Be(1);
        ok.UnwrapOrDefault().Should().Be(1);
        ok.UnwrapOrElse(_ => 10).Should().Be(1);
        ok.Invoking(x => x.UnwrapErr()).Should().Throw<UnwrapException>();
    }

    [Fact]
    public void WhenInspect_ThenInvokeInspect()
    {
        IResult<int, string> ok = Result.Ok<int, string>(1);

        var inspect = Substitute.For<Action<int>>();

        ok.Inspect(inspect).Should().Be(ok);
        inspect.Received(1).Invoke(1);
    }

    [Fact]
    public void WhenInspectErr_ThenDoNothing()
    {
        IResult<int, string> ok = Result.Ok<int, string>(1);

        var inspect = Substitute.For<Action<string>>();

        ok.InspectErr(inspect).Should().Be(ok);
        inspect.DidNotReceive().Invoke(Arg.Any<string>());
    }

    [Fact]
    public void WhenMapOr_ThenReturnMappedValue()
    {
        IResult<int, string> ok = Result.Ok<int, string>(1);

        ok.MapOr(10, x => x + 1).Should().Be(2);
        ok.MapOrElse(_ => 10, x => x + 1).Should().Be(2);
    }

    [Fact]
    public void WhenMapErr_ThenDoNothing()
    {
        IResult<int, string> ok = Result.Ok<int, string>(1);

        ok.MapErr(_ => 10).Should().Be(Result.Ok<int, int>(1));
    }

    [Fact]
    public void WhenGetOk_ThenReturnSome()
    {
        IResult<int, string> ok = Result.Ok<int, string>(1);

        ok.GetOk().Should().Be(Option.Some(1));
    }

    [Fact]
    public void WhenGetErr_ThenReturnNone()
    {
        IResult<int, string> ok = Result.Ok<int, string>(1);
        ok.GetErr().Should().Be(Option.None<string>());
    }
}
