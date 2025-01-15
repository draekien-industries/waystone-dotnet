﻿namespace Waystone.Monads.Options;

[TestSubject(typeof(Some<>))]
public sealed class SomeTests
{
    [Fact]
    public void GivenDefault_WhenCreatingSome_ThenThrow()
    {
        Func<Option<int>> someDefaultNumber = () => Option.Some(0);
        Func<Option<string>> someDefaultString =
            () => Option.Some(default(string)!);
        Func<Option<object>> someDefaultObject =
            () => Option.Some(default(object)!);

        someDefaultNumber.Should().Throw<InvalidOperationException>();
        someDefaultString.Should().Throw<InvalidOperationException>();
        someDefaultObject.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void GivenSome_WhenAccessingValue_ThenReturnValue()
    {
        Option<int> some = Option.Some(1);

        some.IsSome.Should().BeTrue();
        some.IsNone.Should().BeFalse();

        some.Unwrap().Should().Be(1);
        some.UnwrapOr(10).Should().Be(1);
        some.UnwrapOrDefault().Should().Be(1);
        some.UnwrapOrElse(() => 10).Should().Be(1);

        some.Expect("value is 1").Should().Be(1);
    }

    [Fact]
    public void WhenComputingSomeOrOption_ThenReturnSome()
    {
        Option<int> some = Option.Some(1);

        some.Or(Option.Some(2)).Should().Be(some);
        some.OrElse(() => Option.Some(2)).Should().Be(some);
    }

    [Fact]
    public void WhenComputingSomeXorSome_ThenReturnNone()
    {
        Option<int> some = Option.Some(1);

        some.Xor(Option.Some(2)).Should().Be(Option.None<int>());
    }

    [Fact]
    public void WhenComputingSomeXorNone_ThenReturnSome()
    {
        Option<int> some = Option.Some(1);

        some.Xor(Option.None<int>()).Should().Be(some);
    }

    [Fact]
    public void
        GivenTwoOptionsWithTheSameValue_WhenComparingThem_ThenReturnsTrue()
    {
        Option<int> some = Option.Some(1);
        Option<int> other = Option.Some(1);

        some.Should().Be(other);
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(2, false)]
    public void GivenPredicate_WhenInvokingIsSomeAnd_ThenReturnExpected(
        int value,
        bool expected)
    {
        Option<int> some = Option.Some(value);

        bool result = some.IsSomeAnd(x => x == 1);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(2, false)]
    public void GivenPredicate_WhenInvokingIsNoneOr_ThenReturnExpected(
        int value,
        bool expected)
    {
        Option<int> some = Option.Some(value);

        bool result = some.IsNoneOr(x => x == 1);

        result.Should().Be(expected);
    }

    [Fact]
    public void GivenFunc_WhenMatchingOption_ThenInvokeOnSome()
    {
        Option<int> some = Option.Some(1);

        var onSome = Substitute.For<Func<int, bool>>();
        onSome.Invoke(Arg.Any<int>()).Returns(true);

        var onNone = Substitute.For<Func<bool>>();
        onNone.Invoke().Returns(false);

        bool result = some.Match(onSome, onNone);

        result.Should().BeTrue();
    }

    [Fact]
    public void GivenAction_WhenMatchingOption_ThenInvokeOnSome()
    {
        Option<int> some = Option.Some(1);

        var onSome = Substitute.For<Action<int>>();

        var onNone = Substitute.For<Action>();

        some.Match(onSome, onNone);

        onSome.Received(1).Invoke(1);
    }

    [Fact]
    public void WhenMap_ThenReturnMappedOption()
    {
        Option<int> some = Option.Some(1);

        Option<int> result = some.Map(x => x + 1);

        result.Unwrap().Should().Be(2);
    }

    [Fact]
    public void WhenMapOr_ThenReturnMappedValue()
    {
        Option<int> some = Option.Some(1);

        int result = some.MapOr(10, x => x + 1);

        result.Should().Be(2);
    }

    [Fact]
    public void WhenMapOrElse_ThenReturnMappedValue()
    {
        Option<int> some = Option.Some(1);

        int result = some.MapOrElse(() => 10, x => x + 1);

        result.Should().Be(2);
    }

    [Fact]
    public void WhenInspect_ThenInvokeAction()
    {
        Option<int> some = Option.Some(1);
        var action = Substitute.For<Action<int>>();
        some.Inspect(action);

        action.Received().Invoke(1);
    }

    [Fact]
    public void GivenPredicateEvaluatesToTrue_WhenFilter_ThenReturnSome()
    {
        Option<int> some = Option.Some(1);
        Option<int> result = some.Filter(x => x == 1);
        result.Should().Be(some);
    }

    [Fact]
    public void GivenPredicateEvaluatesToFalse_WhenFilter_ThenReturnNone()
    {
        Option<int> some = Option.Some(1);
        Option<int> result = some.Filter(x => x == 2);
        result.Should().Be(Option.None<int>());
    }

    [Fact]
    public void GivenSome_AndSome_WhenZip_ThenReturnSome()
    {
        Option<int> some1 = Option.Some(1);
        Option<int> some2 = Option.Some(2);
        Option<(int, int)> result = some1.Zip(some2);
        result.Should().Be(Option.Some((1, 2)));
    }

    [Fact]
    public void GivenSome_AndNone_WhenZip_ThenReturnNone()
    {
        Option<int> some = Option.Some(1);
        Option<(int, int)> result = some.Zip(Option.None<int>());
        result.Should().Be(Option.None<(int, int)>());
    }

    [Fact]
    public async Task GivenSome_WhenAccessingValueAsync_ThenReturnValue()
    {
        Option<int> some = Option.Some(1);

        bool isSome = await some.IsSomeAnd(_ => Task.FromResult(true));
        bool isNone = await some.IsNoneOr(_ => Task.FromResult(false));

        isSome.Should().BeTrue();
        isNone.Should().BeFalse();

        int value = await some.UnwrapOrElse(() => Task.FromResult(10));
        int valueOr = await some.UnwrapOrElse(() => Task.FromResult(10));
        int valueOrDefault = await some.UnwrapOrElse(() => Task.FromResult(0));
        int valueOrElse = await some.UnwrapOrElse(() => Task.FromResult(10));

        value.Should().Be(1);
        valueOr.Should().Be(1);
        valueOrDefault.Should().Be(1);
        valueOrElse.Should().Be(1);

        int expectedValue = await some.UnwrapOrElse(() => Task.FromResult(1));
        expectedValue.Should().Be(1);
    }

    [Fact]
    public async Task WhenComputingSomeOrOptionAsync_ThenReturnSome()
    {
        Option<int> some = Option.Some(1);

        Option<int> resultOr =
            await some.OrElse(() => Task.FromResult(Option.Some(2)));
        Option<int> resultOrElse =
            await some.OrElse(() => Task.FromResult(Option.Some(2)));

        resultOr.Should().Be(some);
        resultOrElse.Should().Be(some);
    }

    [Fact]
    public async Task GivenFunc_WhenMatchingOptionAsync_ThenInvokeOnSome()
    {
        Option<int> some = Option.Some(1);

        var onSome = Substitute.For<Func<int, Task<bool>>>();
        onSome.Invoke(Arg.Any<int>()).Returns(Task.FromResult(true));

        var onNone = Substitute.For<Func<Task<bool>>>();
        onNone.Invoke().Returns(Task.FromResult(false));

        bool result = await some.Match(onSome, onNone);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task GivenAction_WhenMatchingOptionAsync_ThenInvokeOnSome()
    {
        Option<int> some = Option.Some(1);

        var onSome = Substitute.For<Func<int, Task>>();
        var onNone = Substitute.For<Func<Task>>();

        await some.Match(onSome, onNone);

        await onSome.Received(1).Invoke(1);
    }

    [Fact]
    public async Task WhenMapAsync_ThenReturnMappedOption()
    {
        Option<int> some = Option.Some(1);

        Option<int> result = await some.Map(x => Task.FromResult(x + 1));

        result.Unwrap().Should().Be(2);
    }

    [Fact]
    public async Task WhenMapOrAsync_ThenReturnMappedValue()
    {
        Option<int> some = Option.Some(1);

        int result = await some.MapOr(10, x => Task.FromResult(x + 1));

        result.Should().Be(2);
    }

    [Fact]
    public async Task WhenMapOrElseAsync_ThenReturnMappedValue()
    {
        Option<int> some = Option.Some(1);

        int result = await some.MapOrElse(
            () => Task.FromResult(10),
            x => Task.FromResult(x + 1));

        result.Should().Be(2);
    }

    [Fact]
    public async Task WhenInspectAsync_ThenInvokeAction()
    {
        Option<int> some = Option.Some(1);
        var action = Substitute.For<Func<int, Task>>();
        await some.Inspect(action);

        await action.Received().Invoke(1);
    }

    [Fact]
    public async Task
        GivenPredicateEvaluatesToTrue_WhenFilterAsync_ThenReturnSome()
    {
        Option<int> some = Option.Some(1);
        Option<int> result = await some.Filter(x => Task.FromResult(x == 1));
        result.Should().Be(some);
    }

    [Fact]
    public async Task
        GivenPredicateEvaluatesToFalse_WhenFilterAsync_ThenReturnNone()
    {
        Option<int> some = Option.Some(1);
        Option<int> result = await some.Filter(x => Task.FromResult(x == 2));
        result.Should().Be(Option.None<int>());
    }

    [Fact]
    public async Task
        GivenSome_WhenAccessingValueAsyncWithValueTask_ThenReturnValue()
    {
        Option<int> some = Option.Some(1);

        bool isSome =
            await some.IsSomeAnd(_ => new ValueTask<bool>(true));
        bool isNone =
            await some.IsNoneOr(_ => new ValueTask<bool>(false));

        isSome.Should().BeTrue();
        isNone.Should().BeFalse();

        int value =
            await some.UnwrapOrElse(() => new ValueTask<int>(10));
        int valueOr =
            await some.UnwrapOrElse(() => new ValueTask<int>(10));
        int valueOrDefault =
            await some.UnwrapOrElse(() => new ValueTask<int>(0));
        int valueOrElse =
            await some.UnwrapOrElse(() => new ValueTask<int>(10));

        value.Should().Be(1);
        valueOr.Should().Be(1);
        valueOrDefault.Should().Be(1);
        valueOrElse.Should().Be(1);

        int expectedValue =
            await some.UnwrapOrElse(() => new ValueTask<int>(1));
        expectedValue.Should().Be(1);
    }

    [Fact]
    public async Task
        WhenComputingSomeOrOptionAsyncWithValueTask_ThenReturnSome()
    {
        Option<int> some = Option.Some(1);

        Option<int> resultOr = await some
               .OrElse(
                    () => new ValueTask<Option<int>>(
                        Option.Some(2)))
            ;
        Option<int> resultOrElse = await some
               .OrElse(
                    () => new ValueTask<Option<int>>(
                        Option.Some(2)))
            ;

        resultOr.Should().Be(some);
        resultOrElse.Should().Be(some);
    }

    [Fact]
    public async Task
        GivenFunc_WhenMatchingOptionAsyncWithValueTask_ThenInvokeOnSome()
    {
        Option<int> some = Option.Some(1);

        var onSome = Substitute.For<Func<int, ValueTask<bool>>>();
        onSome.Invoke(Arg.Any<int>()).Returns(new ValueTask<bool>(true));

        var onNone = Substitute.For<Func<ValueTask<bool>>>();
        onNone.Invoke().Returns(new ValueTask<bool>(false));

        bool result = await some.Match(onSome, onNone);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task
        GivenAction_WhenMatchingOptionAsyncWithValueTask_ThenInvokeOnSome()
    {
        Option<int> some = Option.Some(1);

        var onSome = Substitute.For<Func<int, ValueTask>>();
        var onNone = Substitute.For<Func<ValueTask>>();

        await some.Match(onSome, onNone);

        await onSome.Received(1).Invoke(1);
    }

    [Fact]
    public async Task WhenMapAsyncWithValueTask_ThenReturnMappedOption()
    {
        Option<int> some = Option.Some(1);

        Option<int> result =
            await some.Map(x => new ValueTask<int>(x + 1));

        result.Unwrap().Should().Be(2);
    }

    [Fact]
    public async Task WhenMapOrAsyncWithValueTask_ThenReturnMappedValue()
    {
        Option<int> some = Option.Some(1);

        int result =
            await some.MapOr(10, x => new ValueTask<int>(x + 1));

        result.Should().Be(2);
    }

    [Fact]
    public async Task WhenMapOrElseAsyncWithValueTask_ThenReturnMappedValue()
    {
        Option<int> some = Option.Some(1);

        int result = await some.MapOrElse(
            () => new ValueTask<int>(10),
            x => new ValueTask<int>(x + 1));

        result.Should().Be(2);
    }

    [Fact]
    public async Task WhenInspectAsyncWithValueTask_ThenInvokeAction()
    {
        Option<int> some = Option.Some(1);
        var action = Substitute.For<Func<int, ValueTask>>();
        await some.Inspect(action);

        await action.Received().Invoke(1);
    }

    [Fact]
    public async Task
        GivenPredicateEvaluatesToTrue_WhenFilterAsyncWithValueTask_ThenReturnSome()
    {
        Option<int> some = Option.Some(1);
        Option<int> result =
            await some.Filter(x => new ValueTask<bool>(x == 1));
        result.Should().Be(some);
    }

    [Fact]
    public async Task
        GivenPredicateEvaluatesToFalse_WhenFilterAsyncWithValueTask_ThenReturnNone()
    {
        Option<int> some = Option.Some(1);
        Option<int> result =
            await some.Filter(x => new ValueTask<bool>(x == 2));
        result.Should().Be(Option.None<int>());
    }
}
