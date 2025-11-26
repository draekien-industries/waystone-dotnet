namespace Waystone.Monads.Results;

using System;
using System.Threading.Tasks;
using Exceptions;
using Extensions;
using JetBrains.Annotations;
using NSubstitute;
using Options;
using Shouldly;
using Xunit;

[TestSubject(typeof(Ok<,>))]
public class OkTests
{
    [Fact]
    public void WhenCreatingOk_ThenEvaluateToOk()
    {
        Result<int, string> ok = Result.Ok<int, string>(1);

        ok.IsOk.ShouldBeTrue();
        ok.IsErr.ShouldBeFalse();
    }

    [Fact]
    public void WhenIsOkAnd_ThenReturnPredicateResult()
    {
        Result<int, string> ok = Result.Ok<int, string>(1);

        ok.IsOkAnd(_ => false).ShouldBeFalse();
        ok.IsOkAnd(_ => true).ShouldBeTrue();
    }

    [Fact]
    public void WhenIsErrAnd_ThenReturnFalse()
    {
        Result<int, string> ok = Result.Ok<int, string>(1);

        ok.IsErrAnd(_ => true).ShouldBeFalse();
        ok.IsErrAnd(_ => false).ShouldBeFalse();
    }

    [Fact]
    public void GivenFunc_WhenMatch_ThenInvokeOnOk()
    {
        Result<int, string> ok = Result.Ok<int, string>(1);

        var onOk = Substitute.For<Func<int, bool>>();
        onOk.Invoke(1).Returns(true);

        var onErr = Substitute.For<Func<string, bool>>();
        onErr.Invoke(Arg.Any<string>()).Returns(false);

        bool result = ok.Match(onOk, onErr);

        result.ShouldBeTrue();
        onOk.Received(1).Invoke(1);
        onErr.DidNotReceive().Invoke(Arg.Any<string>());
    }

    [Fact]
    public void GivenAction_WhenMatch_ThenInvokeOnOk()
    {
        Result<int, string> ok = Result.Ok<int, string>(1);

        var onOk = Substitute.For<Action<int>>();

        var onErr = Substitute.For<Action<string>>();

        ok.Match(onOk, onErr);

        onOk.Received(1).Invoke(1);
        onErr.DidNotReceive().Invoke(Arg.Any<string>());
    }

    [Fact]
    public void WhenAnd_ThenReturnOther()
    {
        Result<int, string> ok = Result.Ok<int, string>(1);

        Result<string, string> result =
            ok.And(Result.Ok<string, string>("2"));

        result.ShouldBe(Result.Ok<string, string>("2"));
    }

    [Fact]
    public void WhenAndThen_ThenReturnOther()
    {
        Result<int, string> ok = Result.Ok<int, string>(1);

        Result<string, string> result =
            ok.AndThen(x => Result.Ok<string, string>(x.ToString()));

        result.ShouldBe(Result.Ok<string, string>("1"));
    }

    [Fact]
    public void WhenOr_ThenReturnOk()
    {
        Result<int, string> ok = Result.Ok<int, string>(1);

        ok.Or(Result.Ok<int, bool>(2)).ShouldBe(Result.Ok<int, bool>(1));

        ok.Or(Result.Err<int, bool>(false))
           .ShouldBe(Result.Ok<int, bool>(1));
    }

    [Fact]
    public void WhenOrElse_ThenReturnOk()
    {
        Result<int, string> ok = Result.Ok<int, string>(1);

        ok.OrElse(_ => Result.Ok<int, bool>(2))
           .ShouldBe(Result.Ok<int, bool>(1));

        ok.OrElse(_ => Result.Err<int, bool>(false))
           .ShouldBe(Result.Ok<int, bool>(1));
    }

    [Fact]
    public void WhenExpect_ThenReturnValue()
    {
        Result<int, string> ok = Result.Ok<int, string>(1);

        ok.Expect("value is 1").ShouldBe(1);

        Should.Throw<UnmetExpectationException>(() => ok.ExpectErr(
                "Value should not be 1"))
           .Message.ShouldBe("Value should not be 1: 1");
    }

    [Fact]
    public void WhenUnwrap_ThenReturnValue()
    {
        Result<int, string> ok = Result.Ok<int, string>(1);

        ok.Unwrap().ShouldBe(1);
        ok.UnwrapOr(10).ShouldBe(1);
        ok.UnwrapOrDefault().ShouldBe(1);
        ok.UnwrapOrElse(_ => 10).ShouldBe(1);

        Should.Throw<UnwrapException>(() => Result.Err<int, string>("test")
           .Unwrap());
    }

    [Fact]
    public void WhenInspect_ThenInvokeInspect()
    {
        Result<int, string> ok = Result.Ok<int, string>(1);

        var inspect = Substitute.For<Action<int>>();

        ok.Inspect(inspect).ShouldBe(ok);
        inspect.Received(1).Invoke(1);
    }

    [Fact]
    public void WhenInspectErr_ThenDoNothing()
    {
        Result<int, string> ok = Result.Ok<int, string>(1);

        var inspect = Substitute.For<Action<string>>();

        ok.InspectErr(inspect).ShouldBe(ok);
        inspect.DidNotReceive().Invoke(Arg.Any<string>());
    }

    [Fact]
    public void WhenMapOr_ThenReturnMappedValue()
    {
        Result<int, string> ok = Result.Ok<int, string>(1);

        ok.MapOr(10, x => x + 1).ShouldBe(2);
        ok.MapOrElse(_ => 10, x => x + 1).ShouldBe(2);
    }

    [Fact]
    public void WhenMapErr_ThenDoNothing()
    {
        Result<int, string> ok = Result.Ok<int, string>(1);

        ok.MapErr(_ => 10).ShouldBe(Result.Ok<int, int>(1));
    }

    [Fact]
    public void WhenGetOk_ThenReturnSome()
    {
        Result<int, string> ok = Result.Ok<int, string>(1);

        ok.GetOk().ShouldBe(Option.Some(1));
    }

    [Fact]
    public void WhenGetErr_ThenReturnNone()
    {
        Result<int, string> ok = Result.Ok<int, string>(1);
        ok.GetErr().ShouldBe(Option.None<string>());
    }

    [Fact]
    public async Task WhenIsOkAndAsync_ThenReturnPredicateResult()
    {
        Result<int, string> ok = Result.Ok<int, string>(1);

        (await ok.IsOkAndAsync(_ => Task.FromResult(false))).ShouldBeFalse();
        (await ok.IsOkAndAsync(_ => Task.FromResult(true))).ShouldBeTrue();
    }

    [Fact]
    public async Task WhenIsErrAndAsync_ThenReturnFalse()
    {
        Result<int, string> ok = Result.Ok<int, string>(1);

        (await ok.IsErrAndAsync(_ => Task.FromResult(true))).ShouldBeFalse();
        (await ok.IsErrAndAsync(_ => Task.FromResult(false))).ShouldBeFalse();
    }

    [Fact]
    public async Task GivenFunc_WhenMatchAsync_ThenInvokeOnOk()
    {
        Result<int, string> ok = Result.Ok<int, string>(1);

        var onOk = Substitute.For<Func<int, Task<bool>>>();
        onOk.Invoke(1).Returns(Task.FromResult(true));

        var onErr = Substitute.For<Func<string, Task<bool>>>();
        onErr.Invoke(Arg.Any<string>()).Returns(Task.FromResult(false));

        bool result = await ok.MatchAsync(onOk, onErr);

        result.ShouldBeTrue();
        await onOk.Received(1).Invoke(1);
        await onErr.DidNotReceive().Invoke(Arg.Any<string>());
    }

    [Fact]
    public async Task WhenAndThenAsync_ThenReturnOther()
    {
        Result<int, string> ok = Result.Ok<int, string>(1);

        Result<string, string> result =
            await ok.AndThenAsync(x => Task.FromResult(
                Result.Ok<string, string>(x.ToString())));

        result.ShouldBe(Result.Ok<string, string>("1"));
    }

    [Fact]
    public async Task WhenOrElseAsync_ThenReturnOk()
    {
        Result<int, string> ok = Result.Ok<int, string>(1);

        (await ok.OrElseAsync(_ => Task.FromResult(Result.Ok<int, bool>(2))))
           .ShouldBe(Result.Ok<int, bool>(1));

        (
                await ok.OrElseAsync(_ => Task.FromResult(
                    Result.Err<int, bool>(false))))
           .ShouldBe(Result.Ok<int, bool>(1));
    }

    [Fact]
    public async Task WhenInspectAsync_ThenInvokeInspect()
    {
        Result<int, string> ok = Result.Ok<int, string>(1);

        var inspect = Substitute.For<Func<int, Task>>();

        (await ok.InspectAsync(inspect)).ShouldBe(ok);
    }

    [Fact]
    public async Task WhenInspectErrAsync_ThenDoNothing()
    {
        Result<int, string> ok = Result.Ok<int, string>(1);

        var inspect = Substitute.For<Func<string, Task>>();

        (await ok.InspectErrAsync(inspect)).ShouldBe(ok);
        await inspect.DidNotReceive().Invoke(Arg.Any<string>());
    }

    [Fact]
    public async Task WhenMapAsync_ThenReturnMappedValue()
    {
        Result<int, string> ok = Result.Ok<int, string>(1);

        (await ok.MapAsync(x => Task.FromResult(x + 1))).ShouldBe(
            Result.Ok<int, string>(2));
    }

    [Fact]
    public async Task WhenMapOrAsync_ThenReturnMappedValue()
    {
        Result<int, string> ok = Result.Ok<int, string>(1);

        (await ok.MapOrAsync(10, x => Task.FromResult(x + 1))).ShouldBe(2);
    }

    [Fact]
    public async Task WhenMapOrElseAsync_ThenReturnMappedValue()
    {
        Result<int, string> ok = Result.Ok<int, string>(1);

        (await ok.MapOrElseAsync(
            _ => Task.FromResult(10),
            x => Task.FromResult(x + 1))).ShouldBe(2);
    }

    [Fact]
    public async Task WhenUnwrapOrElseAsync_ThenReturnValue()
    {
        Result<int, string> ok = Result.Ok<int, string>(1);

        int value = await ok.UnwrapOrElseAsync(_ => Task.FromResult(10));
        value.ShouldBe(1);
    }

    [Fact]
    public async Task WhenUnwrapOrElseValueTaskAsync_ThenReturnValue()
    {
        Result<int, string> ok = Result.Ok<int, string>(1);

        int value = await ok.UnwrapOrElseAsync(_ => new ValueTask<int>(10));
        value.ShouldBe(1);
    }

    [Fact]
    public async Task WhenMapErrAsync_ThenDoNothing()
    {
        Result<int, string> ok = Result.Ok<int, string>(1);

        Result<int, int> result =
            await ok.MapErrAsync(_ => Task.FromResult(10));

        result.ShouldBe(Result.Ok<int, int>(1));
    }
}
