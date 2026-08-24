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

[TestSubject(typeof(Err<,>))]
public class ErrTests
{
    [Fact]
    public void WhenCreatingErr_ThenEvaluateToErr()
    {
        Result<int, string> result = Result.Err<int, string>("error");

        result.IsOk.ShouldBeFalse();
    }

    [Fact]
    public void WhenIsOkAnd_ThenReturnFalse()
    {
        Result<int, string> result = Result.Err<int, string>("error");

        result.IsOkAnd(_ => true).ShouldBeFalse();
        result.IsOkAnd(_ => false).ShouldBeFalse();
    }

    [Fact]
    public void WhenIsErrAnd_ThenReturnPredicateResult()
    {
        Result<int, string> result = Result.Err<int, string>("error");
        result.IsErrAnd(_ => true).ShouldBeTrue();
        result.IsErrAnd(_ => false).ShouldBeFalse();
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

        output.ShouldBeFalse();
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
           .ShouldBe(Result.Err<string, string>("error"));

        result.And(Result.Err<bool, string>("error 2"))
           .ShouldBe(Result.Err<bool, string>("error"));
    }

    [Fact]
    public void WhenAndThen_ThenReturnError()
    {
        Result<int, string> result = Result.Err<int, string>("error");

        result.AndThen(_ => Result.Ok<string, string>("success"))
           .ShouldBe(Result.Err<string, string>("error"));

        result.AndThen(_ => Result.Err<bool, string>("error 2"))
           .ShouldBe(Result.Err<bool, string>("error"));
    }

    [Fact]
    public void GivenState_WhenAndThen_ThenReturnError()
    {
        Result<int, string> err = Result.Err<int, string>("error");

        err.AndThen(10, static (x, state) => Result.Ok<int, string>(x + state))
           .ShouldBe(Result.Err<int, string>("error"));

        err.AndThen(10, static (_, state) => Result.Err<int, string>($"e{state}"))
           .ShouldBe(Result.Err<int, string>("error"));
    }

    [Fact]
    public void WhenOr_ThenReturnOther()
    {
        Result<int, string> result = Result.Err<int, string>("error");

        result.Or(Result.Ok<int, bool>(1))
           .ShouldBe(Result.Ok<int, bool>(1));

        result.Or(Result.Err<int, bool>(false))
           .ShouldBe(Result.Err<int, bool>(false));
    }

    [Fact]
    public void WhenOrElse_ThenReturnOther()
    {
        Result<int, string> result = Result.Err<int, string>("error");

        result.OrElse(_ => Result.Ok<int, bool>(1))
           .ShouldBe(Result.Ok<int, bool>(1));

        result.OrElse(_ => Result.Err<int, bool>(false))
           .ShouldBe(Result.Err<int, bool>(false));
    }

    [Fact]
    public void WhenExpect_ThenThrowException()
    {
        Result<int, string> result = Result.Err<int, string>("error");

        Should.Throw<UnmetExpectationException>(() => result.Expect(
                "Error should not occur"))
           .Message.ShouldBe("Error should not occur: error");
    }

    [Fact]
    public void WhenExpectErr_ThenReturnErrValue()
    {
        Result<int, string> result = Result.Err<int, string>("error");

        result.ExpectErr("Error should have occurred").ShouldBe("error");
    }

    [Fact]
    public void WhenUnwrap_ThenThrowException()
    {
        Result<int, string> result = Result.Err<int, string>("error");

        Should.Throw<UnwrapException>(() => result.Unwrap());
    }

    [Fact]
    public void WhenUnwrapOr_ThenReturnOrValue()
    {
        Result<int, string> result = Result.Err<int, string>("error");

        result.UnwrapOr(10).ShouldBe(10);
        result.UnwrapOrDefault().ShouldBe(default);
        result.UnwrapOrElse(_ => 10).ShouldBe(10);
    }

    [Fact]
    public void WhenUnwrapErr_ThenReturnErrValue()
    {
        Result<int, string> result = Result.Err<int, string>("error");

        result.UnwrapErr().ShouldBe("error");
    }

    [Fact]
    public void WhenInspect_ThenDoNothing()
    {
        Result<int, string> result = Result.Err<int, string>("error");

        var action = Substitute.For<Action<int>>();

        result.Inspect(action).ShouldBe(result);
        action.DidNotReceive().Invoke(Arg.Any<int>());
    }

    [Fact]
    public void WhenInspectErr_ThenInvokeInspect()
    {
        Result<int, string> result = Result.Err<int, string>("error");

        var action = Substitute.For<Action<string>>();

        result.InspectErr(action).ShouldBe(result);
        action.Received(1).Invoke("error");
    }

    [Fact]
    public void WhenMapOr_ThenReturnOrValue()
    {
        Result<int, string> result = Result.Err<int, string>("error");

        result.MapOr(10, x => x + 1).ShouldBe(10);
        result.MapOrElse(_ => 10, x => x + 1).ShouldBe(10);
    }

    [Fact]
    public void WhenMapErr_ThenReturnMappedErrValue()
    {
        Result<int, string> result = Result.Err<int, string>("error");

        result.MapErr(x => $"{x} 1")
           .ShouldBe(Result.Err<int, string>("error 1"));
    }

    [Fact]
    public void GivenState_WhenMap_ThenDoNothing()
    {
        Result<int, string> result = Result.Err<int, string>("error");

        var map = Substitute.For<Func<int, int, int>>();

        result.Map(10, map).ShouldBe(result);
        map.DidNotReceive().Invoke(Arg.Any<int>(), Arg.Any<int>());
    }

    [Fact]
    public void GivenState_WhenMapOr_ThenReturnOrValue()
    {
        Result<int, string> result = Result.Err<int, string>("error");

        result.MapOr(100, 10, static (x, state) => x + state).ShouldBe(10);

        result.MapOrElse(
                  10,
                  static (_, state) => state,
                  static (x, state) => x + state)
           .ShouldBe(10);
    }

    [Fact]
    public void GivenState_WhenMapErr_ThenReturnMappedErrValue()
    {
        Result<int, string> result = Result.Err<int, string>("error");

        result.MapErr(1, static (x, state) => $"{x} {state}")
           .ShouldBe(Result.Err<int, string>("error 1"));
    }

    [Fact]
    public void WhenGetOk_ThenReturnNone()
    {
        Result<int, string> result = Result.Err<int, string>("error");

        result.GetOk().ShouldBe(Option.None<int>());
    }

    [Fact]
    public void WhenGetErr_ThenReturnSome()
    {
        Result<int, string> result = Result.Err<int, string>("error");

        result.GetErr().ShouldBe(Option.Some("error"));
    }

    [Fact]
    public async Task WhenIsOkAndAsync_ThenReturnFalse()
    {
        Result<int, string> err = Result.Err<int, string>("error");

        (await err.IsOkAndAsync(_ => Task.FromResult(true))).ShouldBeFalse();
        (await err.IsOkAndAsync(_ => Task.FromResult(false))).ShouldBeFalse();
    }

    [Fact]
    public async Task WhenIsErrAndAsync_ThenReturnPredicateResult()
    {
        Result<int, string> err = Result.Err<int, string>("error");

        (await err.IsErrAndAsync(_ => Task.FromResult(true))).ShouldBeTrue();
        (await err.IsErrAndAsync(_ => Task.FromResult(false))).ShouldBeFalse();
    }

    [Fact]
    public async Task GivenFunc_WhenMatchAsync_ThenInvokeOnErr()
    {
        Result<int, string> err = Result.Err<int, string>("error");

        var onOk = Substitute.For<Func<int, Task<bool>>>();
        onOk.Invoke(Arg.Any<int>()).Returns(Task.FromResult(true));

        var onErr = Substitute.For<Func<string, Task<bool>>>();
        onErr.Invoke("error").Returns(Task.FromResult(false));

        bool result = await err.MatchAsync(onOk, onErr);

        result.ShouldBeFalse();
        await onOk.DidNotReceive().Invoke(Arg.Any<int>());
        await onErr.Received(1).Invoke("error");
    }

    [Fact]
    public async Task WhenAndThenAsync_ThenReturnError()
    {
        Result<int, string> err = Result.Err<int, string>("error");

        Result<string, string> result =
            await err.AndThenAsync(x => Task.FromResult(
                Result.Ok<string, string>(x.ToString())));

        result.ShouldBe(Result.Err<string, string>("error"));
    }

    [Fact]
    public async Task WhenOrElseAsync_ThenReturnOther()
    {
        Result<int, string> err = Result.Err<int, string>("error");

        (await err.OrElseAsync(_ => Task.FromResult(Result.Ok<int, bool>(1))))
           .ShouldBe(Result.Ok<int, bool>(1));

        (await err.OrElseAsync(_ => Task.FromResult(
                Result.Err<int, bool>(false))))
           .ShouldBe(Result.Err<int, bool>(false));
    }

    [Fact]
    public async Task WhenInspectAsync_ThenDoNothing()
    {
        Result<int, string> err = Result.Err<int, string>("error");

        var inspect = Substitute.For<Func<int, Task>>();

        (await err.InspectAsync(inspect)).ShouldBe(err);
        await inspect.DidNotReceive().Invoke(Arg.Any<int>());
    }

    [Fact]
    public async Task WhenInspectErrAsync_ThenInvokeInspect()
    {
        Result<int, string> err = Result.Err<int, string>("error");

        var inspect = Substitute.For<Func<string, Task>>();

        (await err.InspectErrAsync(inspect)).ShouldBe(err);
        await inspect.Received(1).Invoke("error");
    }

    [Fact]
    public async Task WhenMapAsync_ThenReturnError()
    {
        Result<int, string> err = Result.Err<int, string>("error");

        (await err.MapAsync(x => Task.FromResult(x + 1))).ShouldBe(
            Result.Err<int, string>("error"));
    }

    [Fact]
    public async Task WhenMapOrAsync_ThenReturnDefault()
    {
        Result<int, string> err = Result.Err<int, string>("error");

        (await err.MapOrAsync(10, x => Task.FromResult(x + 1))).ShouldBe(10);
    }

    [Fact]
    public async Task WhenMapOrElseAsync_ThenReturnDefault()
    {
        Result<int, string> err = Result.Err<int, string>("error");

        (await err.MapOrElseAsync(
                _ => Task.FromResult(10),
                x => Task.FromResult(x + 1)))
           .ShouldBe(10);
    }

    [Fact]
    public async Task WhenUnwrapOrElseAsync_ThenReturnValue()
    {
        Result<int, string> err = Result.Err<int, string>("error");

        int value = await err.UnwrapOrElseAsync(_ => Task.FromResult(10));
        value.ShouldBe(10);
    }

    [Fact]
    public async Task WhenMapErrAsync_ThenReturnMappedError()
    {
        Result<int, string> err = Result.Err<int, string>("error");

        Result<int, int> result =
            await err.MapErrAsync(_ => Task.FromResult(10));

        result.ShouldBe(Result.Err<int, int>(10));
    }

    [Fact]
    public void WhenMapOrDefault_ThenReturnTheDefault()
    {
        Result<int, string> err = Result.Err<int, string>("error");

        err.MapOrDefault(value => value + 1).ShouldBe(0);
        err.MapOrDefault(value => value.ToString()).ShouldBeNull();
    }

    [Fact]
    public void WhenAsEnumerable_ThenYieldNothing()
    {
        Result<int, string> err = Result.Err<int, string>("error");

        err.AsEnumerable().ShouldBeEmpty();
    }
    [Fact]
    public void GivenState_WhenIsOkAnd_ThenReturnFalse()
    {
        Result<int, string> err = Result.Err<int, string>("error");
        var predicate = Substitute.For<Func<int, int, bool>>();

        err.IsOkAnd(10, predicate).ShouldBeFalse();

        predicate.DidNotReceiveWithAnyArgs().Invoke(default, default);
    }

    [Fact]
    public void GivenState_WhenIsErrAnd_ThenReturnThePredicateResult()
    {
        Result<int, string> err = Result.Err<int, string>("error");

        err.IsErrAnd("error", static (x, state) => x == state).ShouldBeTrue();
        err.IsErrAnd("other", static (x, state) => x == state).ShouldBeFalse();
    }

    [Fact]
    public void GivenState_WhenMatchingWithFuncs_ThenInvokeOnErr()
    {
        Result<int, string> err = Result.Err<int, string>("error");

        int result = err.Match(
            10,
            static (x, state) => x + state,
            static (_, state) => state * 100);

        result.ShouldBe(1000);
    }

    [Fact]
    public void GivenState_WhenMatchingWithActions_ThenInvokeOnErr()
    {
        Result<int, string> err = Result.Err<int, string>("error");
        var onOk = Substitute.For<Action<int, int>>();
        var onErr = Substitute.For<Action<string, int>>();

        err.Match(10, onOk, onErr);

        onErr.Received().Invoke("error", 10);
        onOk.DidNotReceiveWithAnyArgs().Invoke(default, default);
    }

    [Fact]
    public void GivenState_WhenOrElse_ThenReturnTheCreatedResult()
    {
        Result<int, string> err = Result.Err<int, string>("error");

        Result<int, int> result = err.OrElse(
            10,
            static (_, state) => Result.Err<int, int>(state));

        result.ShouldBe(Result.Err<int, int>(10));
    }

    [Fact]
    public void GivenState_WhenUnwrapOrElse_ThenReturnTheComputedValue()
    {
        Result<int, string> err = Result.Err<int, string>("error");

        err.UnwrapOrElse(10, static (_, state) => state).ShouldBe(10);
    }

    [Fact]
    public void GivenState_WhenInspect_ThenDoNotInvokeTheAction()
    {
        Result<int, string> err = Result.Err<int, string>("error");
        var action = Substitute.For<Action<int, int>>();

        err.Inspect(10, action).ShouldBe(err);

        action.DidNotReceiveWithAnyArgs().Invoke(default, default);
    }

    [Fact]
    public void GivenState_WhenInspectErr_ThenInvokeTheAction()
    {
        Result<int, string> err = Result.Err<int, string>("error");
        var action = Substitute.For<Action<string, int>>();

        err.InspectErr(10, action).ShouldBe(err);

        action.Received().Invoke("error", 10);
    }

    [Fact]
    public void GivenState_WhenMapOrDefault_ThenReturnTheDefault()
    {
        Result<int, string> err = Result.Err<int, string>("error");

        err.MapOrDefault(10, static (x, state) => x + state).ShouldBe(0);
    }
}
