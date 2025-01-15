﻿namespace Waystone.Monads.Options
{
    using System;
    using System.Threading.Tasks;
    using Exceptions;
    using FluentAssertions;
    using JetBrains.Annotations;
    using NSubstitute;
    using Xunit;

    [TestSubject(typeof(None<>))]
    public class NoneTest
    {
        [Fact]
        public void GivenNone_WhenAccessingValue_ThenThrow()
        {
            Option<int> none = Option.None<int>();

            none.IsSome.Should().BeFalse();
            none.IsNone.Should().BeTrue();

            none.IsSomeAnd(_ => true).Should().BeFalse();
            none.IsNoneOr(_ => false).Should().BeTrue();

            none.Invoking(x => x.Unwrap()).Should().Throw<UnwrapException>();
            none.Invoking(x => x.Expect("value is 1"))
                .Should()
                .Throw<UnmetExpectationException>()
                .WithMessage("Value is 1");
        }

        [Fact]
        public void
            GivenNone_WhenAccessingValueWithFallback_ThenReturnFallback()
        {
            Option<int> none = Option.None<int>();

            none.UnwrapOr(10).Should().Be(10);
            none.UnwrapOrDefault().Should().Be(default);
            none.UnwrapOrElse(() => 10).Should().Be(10);
        }

        [Fact]
        public void GivenNone_WhenMatchWithActions_ThenInvokeOnNone()
        {
            Option<int> none = Option.None<int>();

            var onSome = Substitute.For<Action<int>>();
            var onNone = Substitute.For<Action>();

            none.Match(onSome, onNone);

            onSome.DidNotReceive().Invoke(Arg.Any<int>());
            onNone.Received(1).Invoke();
        }

        [Fact]
        public void GivenNone_WhenMatchWithFunc_ThenInvokeOnNone()
        {
            Option<int> none = Option.None<int>();

            var onSome = Substitute.For<Func<int, int>>();
            onSome.Invoke(Arg.Any<int>()).Returns(1);

            var onNone = Substitute.For<Func<int>>();
            onNone.Invoke().Returns(10);

            int output = none.Match(onSome, onNone);

            output.Should().Be(10);
        }

        [Fact]
        public void GivenNone_WhenMapWithFallback_ThenReturnDefault()
        {
            Option<int> none = Option.None<int>();

            none.MapOr(10, x => x + 1).Should().Be(10);
            none.MapOrElse(() => 10, x => x + 1).Should().Be(10);
        }

        [Fact]
        public void GivenNone_WhenInspect_ThenDoNothing()
        {
            Option<int> none = Option.None<int>();

            var action = Substitute.For<Action<int>>();

            Option<int> result = none.Inspect(action);

            result.Should().Be(none);
            action.DidNotReceive().Invoke(Arg.Any<int>());
        }

        [Fact]
        public void GivenNone_WhenFilter_ThenDoNothing()
        {
            Option<int> none = Option.None<int>();

            var filter = Substitute.For<Func<int, bool>>();

            Option<int> result = none.Filter(filter);

            result.Should().Be(none);
            filter.DidNotReceive().Invoke(Arg.Any<int>());
        }

        [Fact]
        public void GivenNone_WhenOr_ThenReturnOther()
        {
            Option<int> none = Option.None<int>();

            none.Or(Option.None<int>()).Should().Be(none);
            none.Or(Option.Some(1)).Should().Be(Option.Some(1));

            none.OrElse(Option.None<int>).Should().Be(Option.None<int>());
            none.OrElse(() => Option.Some(1)).Should().Be(Option.Some(1));
        }

        [Fact]
        public void GivenNone_WhenXor_ThenReturnExpected()
        {
            Option<int> none = Option.None<int>();

            none.Xor(Option.None<int>()).Should().Be(Option.None<int>());
            none.Xor(Option.Some(1)).Should().Be(Option.Some(1));
        }

        [Fact]
        public async Task GivenNone_WhenAccessingValueAsync_ThenReturnFallback()
        {
            Option<int> none = Option.None<int>();

            bool isSome = await none.IsSomeAnd(_ => Task.FromResult(true));
            bool isNone = await none.IsNoneOr(_ => Task.FromResult(false));

            isSome.Should().BeFalse();
            isNone.Should().BeTrue();

            int value = await none.UnwrapOrElse(() => Task.FromResult(10));
            value.Should().Be(10);
        }

        [Fact]
        public async Task
            GivenNone_WhenAccessingValueAsyncWithValueTask_ThenReturnFallback()
        {
            Option<int> none = Option.None<int>();

            bool isSome = await none.IsSomeAnd(_ => new ValueTask<bool>(true));
            bool isNone = await none.IsNoneOr(_ => new ValueTask<bool>(false));

            isSome.Should().BeFalse();
            isNone.Should().BeTrue();

            int value = await none.UnwrapOrElse(() => new ValueTask<int>(10));
            value.Should().Be(10);
        }

        [Fact]
        public async Task GivenNone_WhenMatchWithFuncAsync_ThenInvokeOnNone()
        {
            Option<int> none = Option.None<int>();

            var onSome = Substitute.For<Func<int, Task<int>>>();
            var onNone = Substitute.For<Func<Task<int>>>();
            onNone.Invoke().Returns(Task.FromResult(10));

            int result = await none.Match(onSome, onNone);

            result.Should().Be(10);
            await onNone.Received(1).Invoke();
            await onSome.DidNotReceive().Invoke(Arg.Any<int>());
        }

        [Fact]
        public async Task
            GivenNone_WhenMatchWithFuncAsyncWithValueTask_ThenInvokeOnNone()
        {
            Option<int> none = Option.None<int>();

            var onSome = Substitute.For<Func<int, ValueTask<int>>>();
            var onNone = Substitute.For<Func<ValueTask<int>>>();
            onNone.Invoke().Returns(new ValueTask<int>(10));

            int result = await none.Match(onSome, onNone);

            result.Should().Be(10);
            await onNone.Received(1).Invoke();
            await onSome.DidNotReceive().Invoke(Arg.Any<int>());
        }

        [Fact]
        public async Task GivenNone_WhenMapAsync_ThenReturnNone()
        {
            Option<int> none = Option.None<int>();

            Option<int> result = await none.Map(x => Task.FromResult(x + 1));

            result.Should().Be(none);
        }

        [Fact]
        public async Task GivenNone_WhenMapAsyncWithValueTask_ThenReturnNone()
        {
            Option<int> none = Option.None<int>();

            Option<int> result = await none.Map(x => new ValueTask<int>(x + 1));

            result.Should().Be(none);
        }

        [Fact]
        public async Task GivenNone_WhenMapOrAsync_ThenReturnDefault()
        {
            Option<int> none = Option.None<int>();

            int result = await none.MapOr(10, x => Task.FromResult(x + 1));

            result.Should().Be(10);
        }

        [Fact]
        public async Task
            GivenNone_WhenMapOrAsyncWithValueTask_ThenReturnDefault()
        {
            Option<int> none = Option.None<int>();

            int result = await none.MapOr(10, x => new ValueTask<int>(x + 1));

            result.Should().Be(10);
        }

        [Fact]
        public async Task GivenNone_WhenMapOrElseAsync_ThenReturnDefault()
        {
            Option<int> none = Option.None<int>();

            int result = await none.MapOrElse(
                () => Task.FromResult(10),
                x => Task.FromResult(x + 1));

            result.Should().Be(10);
        }

        [Fact]
        public async Task
            GivenNone_WhenMapOrElseAsyncWithValueTask_ThenReturnDefault()
        {
            Option<int> none = Option.None<int>();

            int result = await none.MapOrElse(
                () => new ValueTask<int>(10),
                x => new ValueTask<int>(x + 1));

            result.Should().Be(10);
        }

        [Fact]
        public async Task GivenNone_WhenInspectAsync_ThenDoNothing()
        {
            Option<int> none = Option.None<int>();

            var action = Substitute.For<Func<int, Task>>();

            Option<int> result = await none.Inspect(action);

            result.Should().Be(none);
            await action.DidNotReceive().Invoke(Arg.Any<int>());
        }

        [Fact]
        public async Task
            GivenNone_WhenInspectAsyncWithValueTask_ThenDoNothing()
        {
            Option<int> none = Option.None<int>();

            var action = Substitute.For<Func<int, ValueTask>>();

            Option<int> result = await none.Inspect(action);

            result.Should().Be(none);
            await action.DidNotReceive().Invoke(Arg.Any<int>());
        }

        [Fact]
        public async Task GivenNone_WhenFilterAsync_ThenDoNothing()
        {
            Option<int> none = Option.None<int>();

            var filter = Substitute.For<Func<int, Task<bool>>>();

            Option<int> result = await none.Filter(filter);

            result.Should().Be(none);
            await filter.DidNotReceive().Invoke(Arg.Any<int>());
        }

        [Fact]
        public async Task GivenNone_WhenFilterAsyncWithValueTask_ThenDoNothing()
        {
            Option<int> none = Option.None<int>();

            var filter = Substitute.For<Func<int, ValueTask<bool>>>();

            Option<int> result = await none.Filter(filter);

            result.Should().Be(none);
            await filter.DidNotReceive().Invoke(Arg.Any<int>());
        }

        [Fact]
        public async Task GivenNone_WhenOrElseAsync_ThenReturnOther()
        {
            Option<int> none = Option.None<int>();

            Option<int> result =
                await none.OrElse(() => Task.FromResult(Option.Some(1)));

            result.Should().Be(Option.Some(1));
        }

        [Fact]
        public async Task
            GivenNone_WhenOrElseAsyncWithValueTask_ThenReturnOther()
        {
            Option<int> none = Option.None<int>();

            Option<int> result = await none.OrElse(
                () => new ValueTask<Option<int>>(Option.Some(1)));

            result.Should().Be(Option.Some(1));
        }
    }
}
