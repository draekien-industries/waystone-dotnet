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

    [TestSubject(typeof(Ok<,>))]
    public class OkTests
    {
        [Fact]
        public void WhenCreatingOk_ThenEvaluateToOk()
        {
            Result<int, string> ok = Result.Ok<int, string>(1);

            ok.IsOk.Should().BeTrue();
            ok.IsErr.Should().BeFalse();
        }

        [Fact]
        public void WhenIsOkAnd_ThenReturnPredicateResult()
        {
            Result<int, string> ok = Result.Ok<int, string>(1);

            ok.IsOkAnd(_ => false).Should().BeFalse();
            ok.IsOkAnd(_ => true).Should().BeTrue();
        }

        [Fact]
        public void WhenIsErrAnd_ThenReturnFalse()
        {
            Result<int, string> ok = Result.Ok<int, string>(1);

            ok.IsErrAnd(_ => true).Should().BeFalse();
            ok.IsErrAnd(_ => false).Should().BeFalse();
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

            result.Should().BeTrue();
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

            result.Should().Be(Result.Ok<string, string>("2"));
        }

        [Fact]
        public void WhenAndThen_ThenReturnOther()
        {
            Result<int, string> ok = Result.Ok<int, string>(1);
            Result<string, string> result =
                ok.AndThen(x => Result.Ok<string, string>(x.ToString()));
            result.Should().Be(Result.Ok<string, string>("1"));
        }

        [Fact]
        public void WhenOr_ThenReturnOk()
        {
            Result<int, string> ok = Result.Ok<int, string>(1);

            ok.Or(Result.Ok<int, bool>(2)).Should().Be(Result.Ok<int, bool>(1));
            ok.Or(Result.Err<int, bool>(false))
              .Should()
              .Be(Result.Ok<int, bool>(1));
        }

        [Fact]
        public void WhenOrElse_ThenReturnOk()
        {
            Result<int, string> ok = Result.Ok<int, string>(1);

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
            Result<int, string> ok = Result.Ok<int, string>(1);

            ok.Expect("value is 1").Should().Be(1);
            ok.Invoking(x => x.ExpectErr("value should not be 1"))
              .Should()
              .Throw<UnmetExpectationException>()
              .WithMessage("Value should not be 1: 1");
        }

        [Fact]
        public void WhenUnwrap_ThenReturnValue()
        {
            Result<int, string> ok = Result.Ok<int, string>(1);

            ok.Unwrap().Should().Be(1);
            ok.UnwrapOr(10).Should().Be(1);
            ok.UnwrapOrDefault().Should().Be(1);
            ok.UnwrapOrElse(_ => 10).Should().Be(1);
            ok.Invoking(x => x.UnwrapErr()).Should().Throw<UnwrapException>();
        }

        [Fact]
        public void WhenInspect_ThenInvokeInspect()
        {
            Result<int, string> ok = Result.Ok<int, string>(1);

            var inspect = Substitute.For<Action<int>>();

            ok.Inspect(inspect).Should().Be(ok);
            inspect.Received(1).Invoke(1);
        }

        [Fact]
        public void WhenInspectErr_ThenDoNothing()
        {
            Result<int, string> ok = Result.Ok<int, string>(1);

            var inspect = Substitute.For<Action<string>>();

            ok.InspectErr(inspect).Should().Be(ok);
            inspect.DidNotReceive().Invoke(Arg.Any<string>());
        }

        [Fact]
        public void WhenMapOr_ThenReturnMappedValue()
        {
            Result<int, string> ok = Result.Ok<int, string>(1);

            ok.MapOr(10, x => x + 1).Should().Be(2);
            ok.MapOrElse(_ => 10, x => x + 1).Should().Be(2);
        }

        [Fact]
        public void WhenMapErr_ThenDoNothing()
        {
            Result<int, string> ok = Result.Ok<int, string>(1);

            ok.MapErr(_ => 10).Should().Be(Result.Ok<int, int>(1));
        }

        [Fact]
        public void WhenGetOk_ThenReturnSome()
        {
            Result<int, string> ok = Result.Ok<int, string>(1);

            ok.GetOk().Should().Be(Option.Some(1));
        }

        [Fact]
        public void WhenGetErr_ThenReturnNone()
        {
            Result<int, string> ok = Result.Ok<int, string>(1);
            ok.GetErr().Should().Be(Option.None<string>());
        }

        [Fact]
        public async Task WhenIsOkAndAsync_ThenReturnPredicateResult()
        {
            Result<int, string> ok = Result.Ok<int, string>(1);

            (await ok.IsOkAnd(_ => Task.FromResult(false))).Should().BeFalse();
            (await ok.IsOkAnd(_ => Task.FromResult(true))).Should().BeTrue();
        }

        [Fact]
        public async Task WhenIsOkAndValueTaskAsync_ThenReturnPredicateResult()
        {
            Result<int, string> ok = Result.Ok<int, string>(1);

            (await ok.IsOkAnd(_ => new ValueTask<bool>(false))).Should()
               .BeFalse();
            (await ok.IsOkAnd(_ => new ValueTask<bool>(true))).Should()
               .BeTrue();
        }

        [Fact]
        public async Task WhenIsErrAndAsync_ThenReturnFalse()
        {
            Result<int, string> ok = Result.Ok<int, string>(1);

            (await ok.IsErrAnd(_ => Task.FromResult(true))).Should().BeFalse();
            (await ok.IsErrAnd(_ => Task.FromResult(false))).Should().BeFalse();
        }

        [Fact]
        public async Task WhenIsErrAndValueTaskAsync_ThenReturnFalse()
        {
            Result<int, string> ok = Result.Ok<int, string>(1);

            (await ok.IsErrAnd(_ => new ValueTask<bool>(true))).Should()
               .BeFalse();
            (await ok.IsErrAnd(_ => new ValueTask<bool>(false))).Should()
               .BeFalse();
        }

        [Fact]
        public async Task GivenFunc_WhenMatchAsync_ThenInvokeOnOk()
        {
            Result<int, string> ok = Result.Ok<int, string>(1);

            var onOk = Substitute.For<Func<int, Task<bool>>>();
            onOk.Invoke(1).Returns(Task.FromResult(true));

            var onErr = Substitute.For<Func<string, Task<bool>>>();
            onErr.Invoke(Arg.Any<string>()).Returns(Task.FromResult(false));

            bool result = await ok.Match(onOk, onErr);

            result.Should().BeTrue();
            await onOk.Received(1).Invoke(1);
            await onErr.DidNotReceive().Invoke(Arg.Any<string>());
        }

        [Fact]
        public async Task GivenFunc_WhenMatchValueTaskAsync_ThenInvokeOnOk()
        {
            Result<int, string> ok = Result.Ok<int, string>(1);

            var onOk = Substitute.For<Func<int, ValueTask<bool>>>();
            onOk.Invoke(1).Returns(new ValueTask<bool>(true));

            var onErr = Substitute.For<Func<string, ValueTask<bool>>>();
            onErr.Invoke(Arg.Any<string>()).Returns(new ValueTask<bool>(false));

            bool result = await ok.Match(onOk, onErr);

            result.Should().BeTrue();
            await onOk.Received(1).Invoke(1);
            await onErr.DidNotReceive().Invoke(Arg.Any<string>());
        }

        [Fact]
        public async Task WhenAndThenAsync_ThenReturnOther()
        {
            Result<int, string> ok = Result.Ok<int, string>(1);
            Result<string, string> result = await ok.AndThen(
                x => Task.FromResult(Result.Ok<string, string>(x.ToString())));
            result.Should().Be(Result.Ok<string, string>("1"));
        }

        [Fact]
        public async Task WhenAndThenValueTaskAsync_ThenReturnOther()
        {
            Result<int, string> ok = Result.Ok<int, string>(1);
            Result<string, string> result = await ok.AndThen(
                x => new ValueTask<Result<string, string>>(
                    Result.Ok<string, string>(x.ToString())));
            result.Should().Be(Result.Ok<string, string>("1"));
        }

        [Fact]
        public async Task WhenOrElseAsync_ThenReturnOk()
        {
            Result<int, string> ok = Result.Ok<int, string>(1);

            (await ok.OrElse(_ => Task.FromResult(Result.Ok<int, bool>(2))))
               .Should()
               .Be(Result.Ok<int, bool>(1));
            (await ok.OrElse(
                    _ => Task.FromResult(Result.Err<int, bool>(false))))
               .Should()
               .Be(Result.Ok<int, bool>(1));
        }

        [Fact]
        public async Task WhenOrElseValueTaskAsync_ThenReturnOk()
        {
            Result<int, string> ok = Result.Ok<int, string>(1);

            (await ok.OrElse(
                    _ => new ValueTask<Result<int, bool>>(
                        Result.Ok<int, bool>(2))))
               .Should()
               .Be(Result.Ok<int, bool>(1));
            (await ok.OrElse(
                    _ => new ValueTask<Result<int, bool>>(
                        Result.Err<int, bool>(false)))).Should()
               .Be(Result.Ok<int, bool>(1));
        }

        [Fact]
        public async Task WhenInspectAsync_ThenInvokeInspect()
        {
            Result<int, string> ok = Result.Ok<int, string>(1);

            var inspect = Substitute.For<Func<int, Task>>();

            (await ok.Inspect(inspect)).Should().Be(ok);
            await inspect.Received(1).Invoke(1);
        }

        [Fact]
        public async Task WhenInspectValueTaskAsync_ThenInvokeInspect()
        {
            Result<int, string> ok = Result.Ok<int, string>(1);

            var inspect = Substitute.For<Func<int, ValueTask>>();

            (await ok.Inspect(inspect)).Should().Be(ok);
            await inspect.Received(1).Invoke(1);
        }

        [Fact]
        public async Task WhenInspectErrAsync_ThenDoNothing()
        {
            Result<int, string> ok = Result.Ok<int, string>(1);

            var inspect = Substitute.For<Func<string, Task>>();

            (await ok.InspectErr(inspect)).Should().Be(ok);
            await inspect.DidNotReceive().Invoke(Arg.Any<string>());
        }

        [Fact]
        public async Task WhenInspectErrValueTaskAsync_ThenDoNothing()
        {
            Result<int, string> ok = Result.Ok<int, string>(1);

            var inspect = Substitute.For<Func<string, ValueTask>>();

            (await ok.InspectErr(inspect)).Should().Be(ok);
            await inspect.DidNotReceive().Invoke(Arg.Any<string>());
        }

        [Fact]
        public async Task WhenMapAsync_ThenReturnMappedValue()
        {
            Result<int, string> ok = Result.Ok<int, string>(1);

            (await ok.Map(x => Task.FromResult(x + 1))).Should()
               .Be(
                    Result.Ok<int, string>(
                        2));
        }

        [Fact]
        public async Task WhenMapValueTaskAsync_ThenReturnMappedValue()
        {
            Result<int, string> ok = Result.Ok<int, string>(1);

            (await ok.Map(x => new ValueTask<int>(x + 1))).Should()
               .Be(Result.Ok<int, string>(2));
        }

        [Fact]
        public async Task WhenMapOrAsync_ThenReturnMappedValue()
        {
            Result<int, string> ok = Result.Ok<int, string>(1);

            (await ok.MapOr(10, x => Task.FromResult(x + 1))).Should().Be(2);
        }

        [Fact]
        public async Task WhenMapOrValueTaskAsync_ThenReturnMappedValue()
        {
            Result<int, string> ok = Result.Ok<int, string>(1);

            (await ok.MapOr(10, x => new ValueTask<int>(x + 1))).Should().Be(2);
        }

        [Fact]
        public async Task WhenMapOrElseAsync_ThenReturnMappedValue()
        {
            Result<int, string> ok = Result.Ok<int, string>(1);

            (await ok.MapOrElse(
                    _ => Task.FromResult(10),
                    x => Task.FromResult(x + 1))).Should()
                                                 .Be(2);
        }

        [Fact]
        public async Task WhenMapOrElseValueTaskAsync_ThenReturnMappedValue()
        {
            Result<int, string> ok = Result.Ok<int, string>(1);

            (await ok.MapOrElse(
                    _ => new ValueTask<int>(10),
                    x => new ValueTask<int>(x + 1))).Should()
               .Be(2);
        }
    }
}
