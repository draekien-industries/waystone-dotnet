namespace Waystone.Monads.Options;

using System;
using System.Threading.Tasks;
using Extensions;
using JetBrains.Annotations;
using NSubstitute;
using Shouldly;
using Xunit;

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

        someDefaultNumber.ShouldThrow<InvalidOperationException>();
        someDefaultString.ShouldThrow<InvalidOperationException>();
        someDefaultObject.ShouldThrow<InvalidOperationException>();
    }

    [Fact]
    public void GivenSome_WhenAccessingValue_ThenReturnValue()
    {
        Option<int> some = Option.Some(1);

        some.IsSome.ShouldBeTrue();
        some.IsNone.ShouldBeFalse();

        some.Unwrap().ShouldBe(1);
        some.UnwrapOr(10).ShouldBe(1);
        some.UnwrapOrDefault().ShouldBe(1);
        some.UnwrapOrElse(() => 10).ShouldBe(1);

        some.Expect("value is 1").ShouldBe(1);
    }

    [Fact]
    public void WhenComputingSomeOrOption_ThenReturnSome()
    {
        Option<int> some = Option.Some(1);

        some.Or(Option.Some(2)).ShouldBe(some);
        some.OrElse(() => Option.Some(2)).ShouldBe(some);
    }

    [Fact]
    public void WhenComputingSomeXorSome_ThenReturnNone()
    {
        Option<int> some = Option.Some(1);

        some.Xor(Option.Some(2)).ShouldBe(Option.None<int>());
    }

    [Fact]
    public void WhenComputingSomeXorNone_ThenReturnSome()
    {
        Option<int> some = Option.Some(1);

        some.Xor(Option.None<int>()).ShouldBe(some);
    }

    [Fact]
    public void
        GivenTwoOptionsWithTheSameValue_WhenComparingThem_ThenReturnsTrue()
    {
        Option<int> some = Option.Some(1);
        Option<int> other = Option.Some(1);

        some.ShouldBe(other);
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

        result.ShouldBe(expected);
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

        result.ShouldBe(expected);
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

        result.ShouldBeTrue();
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

        result.Unwrap().ShouldBe(2);
    }

    [Fact]
    public void WhenMapOr_ThenReturnMappedValue()
    {
        Option<int> some = Option.Some(1);

        int result = some.MapOr(10, x => x + 1);

        result.ShouldBe(2);
    }

    [Fact]
    public void WhenMapOrElse_ThenReturnMappedValue()
    {
        Option<int> some = Option.Some(1);

        int result = some.MapOrElse(() => 10, x => x + 1);

        result.ShouldBe(2);
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
        result.ShouldBe(some);
    }

    [Fact]
    public void GivenPredicateEvaluatesToFalse_WhenFilter_ThenReturnNone()
    {
        Option<int> some = Option.Some(1);
        Option<int> result = some.Filter(x => x == 2);
        result.ShouldBe(Option.None<int>());
    }

    [Fact]
    public void GivenSome_AndSome_WhenZip_ThenReturnSome()
    {
        Option<int> some1 = Option.Some(1);
        Option<int> some2 = Option.Some(2);
        Option<(int, int)> result = some1.Zip(some2);
        result.ShouldBe(Option.Some((1, 2)));
    }

    [Fact]
    public void GivenSome_AndNone_WhenZip_ThenReturnNone()
    {
        Option<int> some = Option.Some(1);
        Option<(int, int)> result = some.Zip(Option.None<int>());
        result.ShouldBe(Option.None<(int, int)>());
    }

    [Fact]
    public async Task GivenSome_WhenAccessingValueAsync_ThenReturnValue()
    {
        Option<int> some = Option.Some(1);

        bool isSome = await some.IsSomeAndAsync(_ => Task.FromResult(true));
        bool isNone = await some.IsNoneOrAsync(_ => Task.FromResult(false));

        isSome.ShouldBeTrue();
        isNone.ShouldBeFalse();

        int value = await some.UnwrapOrElseAsync(() => Task.FromResult(10));
        int valueOr = await some.UnwrapOrElseAsync(() => Task.FromResult(10));
        int valueOrDefault =
            await some.UnwrapOrElseAsync(() => Task.FromResult(0));
        int valueOrElse =
            await some.UnwrapOrElseAsync(() => Task.FromResult(10));

        value.ShouldBe(1);
        valueOr.ShouldBe(1);
        valueOrDefault.ShouldBe(1);
        valueOrElse.ShouldBe(1);

        int expectedValue =
            await some.UnwrapOrElseAsync(() => Task.FromResult(1));
        expectedValue.ShouldBe(1);
    }

    [Fact]
    public async Task WhenComputingSomeOrOptionAsync_ThenReturnSome()
    {
        Option<int> some = Option.Some(1);

        Option<int> resultOr =
            await some.OrElse(() => Task.FromResult(Option.Some(2)));
        Option<int> resultOrElse =
            await some.OrElse(() => Task.FromResult(Option.Some(2)));

        resultOr.ShouldBe(some);
        resultOrElse.ShouldBe(some);
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

        result.ShouldBeTrue();
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

        Option<int> result = await some.MapAsync(x => Task.FromResult(x + 1));

        result.Unwrap().ShouldBe(2);
    }

    [Fact]
    public async Task WhenMapOrAsync_ThenReturnMappedValue()
    {
        Option<int> some = Option.Some(1);

        int result = await some.MapOr(10, x => Task.FromResult(x + 1));

        result.ShouldBe(2);
    }

    [Fact]
    public async Task WhenMapOrElseAsync_ThenReturnMappedValue()
    {
        Option<int> some = Option.Some(1);

        int result = await some.MapOrElse(
            () => Task.FromResult(10),
            x => Task.FromResult(x + 1));

        result.ShouldBe(2);
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
        Option<int> result =
            await some.Filter(x => Task.FromResult(x == 1));
        result.ShouldBe(some);
    }

    [Fact]
    public async Task
        GivenPredicateEvaluatesToFalse_WhenFilterAsync_ThenReturnNone()
    {
        Option<int> some = Option.Some(1);
        Option<int> result =
            await some.Filter(x => Task.FromResult(x == 2));
        result.ShouldBe(Option.None<int>());
    }

    [Fact]
    public async Task
        GivenSome_WhenAccessingValueAsyncWithValueTask_ThenReturnValue()
    {
        Option<int> some = Option.Some(1);

        bool isSome =
            await some.IsSomeAndAsync(_ => new ValueTask<bool>(true));
        bool isNone =
            await some.IsNoneOrAsync(_ => new ValueTask<bool>(false));

        isSome.ShouldBeTrue();
        isNone.ShouldBeFalse();

        int value =
            await some.UnwrapOrElseAsync(() => new ValueTask<int>(10));
        int valueOr =
            await some.UnwrapOrElseAsync(() => new ValueTask<int>(10));
        int valueOrDefault =
            await some.UnwrapOrElseAsync(() => new ValueTask<int>(0));
        int valueOrElse =
            await some.UnwrapOrElseAsync(() => new ValueTask<int>(10));

        value.ShouldBe(1);
        valueOr.ShouldBe(1);
        valueOrDefault.ShouldBe(1);
        valueOrElse.ShouldBe(1);

        int expectedValue =
            await some.UnwrapOrElseAsync(() => new ValueTask<int>(1));
        expectedValue.ShouldBe(1);
    }

    [Fact]
    public async Task
        WhenComputingSomeOrOptionAsyncWithValueTask_ThenReturnSome()
    {
        Option<int> some = Option.Some(1);

        Option<int> resultOr = await some
               .OrElse(() => new ValueTask<Option<int>>(
                           Option.Some(2)))
            ;
        Option<int> resultOrElse = await some
               .OrElse(() => new ValueTask<Option<int>>(
                           Option.Some(2)))
            ;

        resultOr.ShouldBe(some);
        resultOrElse.ShouldBe(some);
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

        result.ShouldBeTrue();
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
            await some.MapAsync(x => new ValueTask<int>(x + 1));

        result.Unwrap().ShouldBe(2);
    }

    [Fact]
    public async Task WhenMapOrAsyncWithValueTask_ThenReturnMappedValue()
    {
        Option<int> some = Option.Some(1);

        int result =
            await some.MapOr(10, x => new ValueTask<int>(x + 1));

        result.ShouldBe(2);
    }

    [Fact]
    public async Task
        WhenMapOrElseAsyncWithValueTask_ThenReturnMappedValue()
    {
        Option<int> some = Option.Some(1);

        int result = await some.MapOrElse(
            () => new ValueTask<int>(10),
            x => new ValueTask<int>(x + 1));

        result.ShouldBe(2);
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
        result.ShouldBe(some);
    }

    [Fact]
    public async Task
        GivenPredicateEvaluatesToFalse_WhenFilterAsyncWithValueTask_ThenReturnNone()
    {
        Option<int> some = Option.Some(1);
        Option<int> result =
            await some.Filter(x => new ValueTask<bool>(x == 2));
        result.ShouldBe(Option.None<int>());
    }

    [Fact]
    public void WhenFlatMap_ThenReturnMappedOption()
    {
        Option<int> some = Option.Some(1);
        Option<int> result = some.FlatMap(x => Option.Some(x + 1));
        result.Unwrap().ShouldBe(2);
    }

    [Fact]
    public async Task WhenFlatMapAsync_ThenReturnMappedOption()
    {
        Option<int> some = Option.Some(1);
        Option<int> result = await some.FlatMapAsync(x =>
                Task.FromResult(
                    Option.Some(x + 1)));
        result.Unwrap().ShouldBe(2);
    }

    [Fact]
    public async Task WhenFlatMapAsyncWithValueTask_ThenReturnMappedOption()
    {
        Option<int> some = Option.Some(1);
        Option<int> result = await some.FlatMapAsync(x =>
                new ValueTask<Option<int>>(
                    Option.Some(x + 1)));
        result.Unwrap().ShouldBe(2);
    }
}
