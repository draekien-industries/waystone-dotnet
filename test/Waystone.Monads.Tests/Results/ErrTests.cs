namespace Waystone.Monads.Results
{
    using System;
    using System.Threading.Tasks;
    using Exceptions;
    using FluentAssertions;
    using JetBrains.Annotations;
    using NSubstitute;
    using Options;
    using Xunit;

    [TestSubject(typeof(Err<,>))]
    public class ErrTests
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

            result.GetErr().Should().Be(Option.Some("error"));
        }

        [Fact]
        public async Task WhenIsOkAndAsync_ThenReturnFalse()
        {
            Result<int, string> err = Result.Err<int, string>("error");

            (await err.IsOkAnd(_ => Task.FromResult(true))).Should().BeFalse();
            (await err.IsOkAnd(_ => Task.FromResult(false))).Should().BeFalse();
        }

        [Fact]
        public async Task WhenIsOkAndValueTaskAsync_ThenReturnFalse()
        {
            Result<int, string> err = Result.Err<int, string>("error");

            (await err.IsOkAnd(_ => new ValueTask<bool>(true))).Should()
               .BeFalse();
            (await err.IsOkAnd(_ => new ValueTask<bool>(false))).Should()
               .BeFalse();
        }

        [Fact]
        public async Task WhenIsErrAndAsync_ThenReturnPredicateResult()
        {
            Result<int, string> err = Result.Err<int, string>("error");

            (await err.IsErrAnd(_ => Task.FromResult(true))).Should().BeTrue();
            (await err.IsErrAnd(_ => Task.FromResult(false))).Should()
               .BeFalse();
        }

        [Fact]
        public async Task WhenIsErrAndValueTaskAsync_ThenReturnPredicateResult()
        {
            Result<int, string> err = Result.Err<int, string>("error");

            (await err.IsErrAnd(_ => new ValueTask<bool>(true))).Should()
               .BeTrue();
            (await err.IsErrAnd(_ => new ValueTask<bool>(false))).Should()
               .BeFalse();
        }

        [Fact]
        public async Task GivenFunc_WhenMatchAsync_ThenInvokeOnErr()
        {
            Result<int, string> err = Result.Err<int, string>("error");

            var onOk = Substitute.For<Func<int, Task<bool>>>();
            onOk.Invoke(Arg.Any<int>()).Returns(Task.FromResult(true));

            var onErr = Substitute.For<Func<string, Task<bool>>>();
            onErr.Invoke("error").Returns(Task.FromResult(false));

            bool result = await err.Match(onOk, onErr);

            result.Should().BeFalse();
            await onOk.DidNotReceive().Invoke(Arg.Any<int>());
            await onErr.Received(1).Invoke("error");
        }

        [Fact]
        public async Task GivenFunc_WhenMatchValueTaskAsync_ThenInvokeOnErr()
        {
            Result<int, string> err = Result.Err<int, string>("error");

            var onOk = Substitute.For<Func<int, ValueTask<bool>>>();
            onOk.Invoke(Arg.Any<int>()).Returns(new ValueTask<bool>(true));

            var onErr = Substitute.For<Func<string, ValueTask<bool>>>();
            onErr.Invoke("error").Returns(new ValueTask<bool>(false));

            bool result = await err.Match(onOk, onErr);

            result.Should().BeFalse();
            await onOk.DidNotReceive().Invoke(Arg.Any<int>());
            await onErr.Received(1).Invoke("error");
        }

        [Fact]
        public async Task WhenAndThenAsync_ThenReturnError()
        {
            Result<int, string> err = Result.Err<int, string>("error");
            Result<string, string> result = await err.AndThen(
                x => Task.FromResult(Result.Ok<string, string>(x.ToString())));
            result.Should().Be(Result.Err<string, string>("error"));
        }

        [Fact]
        public async Task WhenAndThenValueTaskAsync_ThenReturnError()
        {
            Result<int, string> err = Result.Err<int, string>("error");
            Result<string, string> result = await err.AndThen(
                x => new ValueTask<Result<string, string>>(
                    Result.Ok<string, string>(x.ToString())));
            result.Should().Be(Result.Err<string, string>("error"));
        }

        [Fact]
        public async Task WhenOrElseAsync_ThenReturnOther()
        {
            Result<int, string> err = Result.Err<int, string>("error");

            (await err.OrElse(_ => Task.FromResult(Result.Ok<int, bool>(1))))
               .Should()
               .Be(Result.Ok<int, bool>(1));
            (await err.OrElse(
                    _ => Task.FromResult(Result.Err<int, bool>(false))))
               .Should()
               .Be(Result.Err<int, bool>(false));
        }

        [Fact]
        public async Task WhenOrElseValueTaskAsync_ThenReturnOther()
        {
            Result<int, string> err = Result.Err<int, string>("error");

            (await err.OrElse(
                    _ => new ValueTask<Result<int, bool>>(
                        Result.Ok<int, bool>(1))))
               .Should()
               .Be(Result.Ok<int, bool>(1));
            (await err.OrElse(
                    _ => new ValueTask<Result<int, bool>>(
                        Result.Err<int, bool>(false)))).Should()
               .Be(
                    Result.Err<int, bool>(
                        false));
        }

        [Fact]
        public async Task WhenInspectAsync_ThenDoNothing()
        {
            Result<int, string> err = Result.Err<int, string>("error");

            var inspect = Substitute.For<Func<int, Task>>();

            (await err.Inspect(inspect)).Should().Be(err);
            await inspect.DidNotReceive().Invoke(Arg.Any<int>());
        }

        [Fact]
        public async Task WhenInspectValueTaskAsync_ThenDoNothing()
        {
            Result<int, string> err = Result.Err<int, string>("error");

            var inspect = Substitute.For<Func<int, ValueTask>>();

            (await err.Inspect(inspect)).Should().Be(err);
            await inspect.DidNotReceive().Invoke(Arg.Any<int>());
        }

        [Fact]
        public async Task WhenInspectErrAsync_ThenInvokeInspect()
        {
            Result<int, string> err = Result.Err<int, string>("error");

            var inspect = Substitute.For<Func<string, Task>>();

            (await err.InspectErr(inspect)).Should().Be(err);
            await inspect.Received(1).Invoke("error");
        }

        [Fact]
        public async Task WhenInspectErrValueTaskAsync_ThenInvokeInspect()
        {
            Result<int, string> err = Result.Err<int, string>("error");

            var inspect = Substitute.For<Func<string, ValueTask>>();

            (await err.InspectErr(inspect)).Should().Be(err);
            await inspect.Received(1).Invoke("error");
        }

        [Fact]
        public async Task WhenMapAsync_ThenReturnError()
        {
            Result<int, string> err = Result.Err<int, string>("error");

            (await err.Map(x => Task.FromResult(x + 1))).Should()
               .Be(Result.Err<int, string>("error"));
        }

        [Fact]
        public async Task WhenMapValueTaskAsync_ThenReturnError()
        {
            Result<int, string> err = Result.Err<int, string>("error");

            (await err.Map(x => new ValueTask<int>(x + 1))).Should()
               .Be(Result.Err<int, string>("error"));
        }

        [Fact]
        public async Task WhenMapOrAsync_ThenReturnDefault()
        {
            Result<int, string> err = Result.Err<int, string>("error");

            (await err.MapOr(10, x => Task.FromResult(x + 1))).Should().Be(10);
        }

        [Fact]
        public async Task WhenMapOrValueTaskAsync_ThenReturnDefault()
        {
            Result<int, string> err = Result.Err<int, string>("error");

            (await err.MapOr(10, x => new ValueTask<int>(x + 1))).Should()
               .Be(10);
        }

        [Fact]
        public async Task WhenMapOrElseAsync_ThenReturnDefault()
        {
            Result<int, string> err = Result.Err<int, string>("error");

            (await err.MapOrElse(
                    _ => Task.FromResult(10),
                    x => Task.FromResult(x + 1))).Should()
                                                 .Be(10);
        }

        [Fact]
        public async Task WhenMapOrElseValueTaskAsync_ThenReturnDefault()
        {
            Result<int, string> err = Result.Err<int, string>("error");

            (await err.MapOrElse(
                    _ => new ValueTask<int>(10),
                    x => new ValueTask<int>(x + 1))).Should()
               .Be(10);
        }

        [Fact]
        public async Task WhenUnwrapOrElseAsync_ThenReturnValue()
        {
            Result<int, string> err = Result.Err<int, string>("error");

            int value = await err.UnwrapOrElse(_ => Task.FromResult(10));
            value.Should().Be(10);
        }

        [Fact]
        public async Task WhenUnwrapOrElseValueTaskAsync_ThenReturnValue()
        {
            Result<int, string> err = Result.Err<int, string>("error");

            int value = await err.UnwrapOrElse(_ => new ValueTask<int>(10));
            value.Should().Be(10);
        }

        [Fact]
        public async Task WhenMapErrAsync_ThenReturnMappedError()
        {
            Result<int, string> err = Result.Err<int, string>("error");

            Result<int, int> result =
                await err.MapErr(_ => Task.FromResult(10));
            result.Should().Be(Result.Err<int, int>(10));
        }

        [Fact]
        public async Task WhenMapErrValueTaskAsync_ThenReturnMappedError()
        {
            Result<int, string> err = Result.Err<int, string>("error");

            Result<int, int> result =
                await err.MapErr(_ => new ValueTask<int>(10));
            result.Should().Be(Result.Err<int, int>(10));
        }
    }
}
