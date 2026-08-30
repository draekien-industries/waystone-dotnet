namespace Waystone.Monads.Options;

using System;
using System.Threading.Tasks;
using Extensions;
using JetBrains.Annotations;
using NSubstitute;
using Results;
using Shouldly;
using Xunit;

[TestSubject(typeof(Some<>))]
public sealed class SomeTests
{
    [Fact]
    public void GivenNull_WhenCreatingSome_ThenThrow()
    {
        Func<Option<string>> someNullString =
            () => Option.Some(default(string)!);

        Func<Option<object>> someNullObject =
            () => Option.Some(default(object)!);

        someNullString.ShouldThrow<ArgumentNullException>();
        someNullObject.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void GivenTheDefaultOfAValueType_WhenCreatingSome_ThenReturnSome()
    {
        Option.Some(0).ShouldBe(Option.Some(0));
        Option.Some(0).ShouldBeSomeValue(0);
        Option.Some(false).Unwrap().ShouldBeFalse();
        Option.Some('\0').ShouldBeSomeValue('\0');
        Option.Some(default(Guid)).ShouldBeSomeValue(Guid.Empty);
        Option.Some(TimeSpan.Zero).ShouldBeSomeValue(TimeSpan.Zero);
        Option.Some(DateTime.MinValue).ShouldBeSomeValue(DateTime.MinValue);
    }

    [Fact]
    public void GivenTheDefaultOfAValueType_WhenCreatingSome_ThenItIsNotNone()
    {
        Option.Some(0).ShouldBeSome();
        Option.Some(0).ShouldNotBe(Option.None<int>());
    }

    [Fact]
    public void GivenSome_WhenAccessingValue_ThenReturnValue()
    {
        Option<int> some = Option.Some(1);

        some.ShouldBeSome();

        some.ShouldBeSomeValue(1);
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

    [Theory, InlineData(1, true), InlineData(2, false)]
    public void GivenPredicate_WhenInvokingIsSomeAnd_ThenReturnExpected(
        int value,
        bool expected)
    {
        Option<int> some = Option.Some(value);

        bool result = some.IsSomeAnd(x => x == 1);

        result.ShouldBe(expected);
    }

    [Theory, InlineData(1, true), InlineData(2, false)]
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

        result.ShouldBeSomeValue(2);
    }

    [Fact]
    public void WhenMapProducesADefault_ThenReturnSome()
    {
        Option<int> some = Option.Some(1);

        Option<int> result = some.Map(x => x - x);

        result.ShouldBeSomeValue(0);
    }

    [Fact]
    public void WhenMapProducesNull_ThenThrow()
    {
        Option<int> some = Option.Some(1);

        Func<Option<string>> mapToNull = () => some.Map(_ => default(string)!);

        mapToNull.ShouldThrow<ArgumentNullException>()
                 .ParamName.ShouldBe("map");
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
    public void GivenState_WhenMap_ThenReturnMappedOption()
    {
        Option<int> some = Option.Some(1);

        Option<int> result = some.Map(10, static (x, state) => x + state);

        result.ShouldBeSomeValue(11);
    }

    [Fact]
    public void GivenState_WhenMapProducesNull_ThenThrow()
    {
        Option<int> some = Option.Some(1);

        Func<Option<string>> mapToNull =
            () => some.Map(10, static (_, _) => default(string)!);

        mapToNull.ShouldThrow<ArgumentNullException>()
                 .ParamName.ShouldBe("map");
    }

    [Fact]
    public void GivenState_WhenMapOr_ThenReturnMappedValue()
    {
        Option<int> some = Option.Some(1);

        int result = some.MapOr(10, 100, static (x, state) => x + state);

        result.ShouldBe(11);
    }

    [Fact]
    public void GivenState_WhenMapOrElse_ThenReturnMappedValue()
    {
        Option<int> some = Option.Some(1);

        int result = some.MapOrElse(
            10,
            static state => state * 100,
            static (x, state) => x + state);

        result.ShouldBe(11);
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
    public void GivenState_AndPredicateIsTrue_WhenFilter_ThenReturnSome()
    {
        Option<int> some = Option.Some(1);
        Option<int> result = some.Filter(1, static (x, state) => x == state);
        result.ShouldBe(some);
    }

    [Fact]
    public void GivenState_AndPredicateIsFalse_WhenFilter_ThenReturnNone()
    {
        Option<int> some = Option.Some(1);
        Option<int> result = some.Filter(2, static (x, state) => x == state);
        result.ShouldBe(Option.None<int>());
    }

    [Fact]
    public void GivenState_WhenIsSomeAnd_ThenReturnThePredicateResult()
    {
        Option<int> some = Option.Some(1);

        some.IsSomeAnd(1, static (x, state) => x == state).ShouldBeTrue();
        some.IsSomeAnd(2, static (x, state) => x == state).ShouldBeFalse();
    }

    [Fact]
    public void GivenState_WhenIsNoneOr_ThenReturnThePredicateResult()
    {
        Option<int> some = Option.Some(1);

        some.IsNoneOr(1, static (x, state) => x == state).ShouldBeTrue();
        some.IsNoneOr(2, static (x, state) => x == state).ShouldBeFalse();
    }

    [Fact]
    public void GivenState_WhenMatchingWithFuncs_ThenInvokeOnSome()
    {
        Option<int> some = Option.Some(1);

        int result = some.Match(
            10,
            static (x, state) => x + state,
            static state => state * 100);

        result.ShouldBe(11);
    }

    [Fact]
    public void GivenState_WhenMatchingWithActions_ThenInvokeOnSome()
    {
        Option<int> some = Option.Some(1);
        var onSome = Substitute.For<Action<int, int>>();
        var onNone = Substitute.For<Action<int>>();

        some.Match(10, onSome, onNone);

        onSome.Received().Invoke(1, 10);
        onNone.DidNotReceiveWithAnyArgs().Invoke(default);
    }

    [Fact]
    public void GivenState_WhenInspect_ThenInvokeAction()
    {
        Option<int> some = Option.Some(1);
        var action = Substitute.For<Action<int, int>>();

        some.Inspect(10, action).ShouldBe(some);

        action.Received().Invoke(1, 10);
    }

    [Fact]
    public void GivenState_WhenMapOrDefault_ThenReturnMappedValue()
    {
        Option<int> some = Option.Some(1);

        int result = some.MapOrDefault(10, static (x, state) => x + state);

        result.ShouldBe(11);
    }

    [Fact]
    public void GivenState_WhenUnwrapOrElse_ThenReturnTheContainedValue()
    {
        Option<int> some = Option.Some(1);

        some.UnwrapOrElse(10, static state => state).ShouldBe(1);
    }

    [Fact]
    public void GivenState_WhenOrElse_ThenReturnTheOriginalOption()
    {
        Option<int> some = Option.Some(1);

        some.OrElse(10, static state => Option.Some(state)).ShouldBe(some);
    }

    [Fact]
    public void GivenState_WhenOkOrElse_ThenReturnOk()
    {
        Option<int> some = Option.Some(1);

        Result<int, string> result =
            some.OkOrElse("boom", static state => state);

        result.ShouldBe(Result.Ok<int, string>(1));
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
            await some.OrElseAsync(
                () => new ValueTask<Option<int>>(Option.Some(2)));

        Option<int> resultOrElse =
            await some.OrElseAsync(
                () => new ValueTask<Option<int>>(Option.Some(2)));

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

        result.ShouldBeSomeValue(2);
    }

    [Fact]
    public async Task WhenMapAsyncProducesNull_ThenThrow()
    {
        Option<int> some = Option.Some(1);

        Func<Task> mapToNull = async () =>
            await some.MapAsync(_ => Task.FromResult(default(string)!));

        await mapToNull.ShouldThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task WhenMapOrAsync_ThenReturnMappedValue()
    {
        Option<int> some = Option.Some(1);

        int result = await some.MapOrAsync(10, x => Task.FromResult(x + 1));

        result.ShouldBe(2);
    }

    [Fact]
    public async Task WhenMapOrElseAsync_ThenReturnMappedValue()
    {
        Option<int> some = Option.Some(1);

        int result = await some.MapOrElseAsync(
            () => Task.FromResult(10),
            x => Task.FromResult(x + 1));

        result.ShouldBe(2);
    }

    [Fact]
    public async Task WhenInspectAsync_ThenInvokeAction()
    {
        Option<int> some = Option.Some(1);
        var action = Substitute.For<Func<int, Task>>();
        await some.InspectAsync(action);

        await action.Received().Invoke(1);
    }

    [Fact]
    public async Task
        GivenPredicateEvaluatesToTrue_WhenFilterAsync_ThenReturnSome()
    {
        Option<int> some = Option.Some(1);

        Option<int> result =
            await some.FilterAsync(x => Task.FromResult(x == 1));

        result.ShouldBe(some);
    }

    [Fact]
    public async Task
        GivenPredicateEvaluatesToFalse_WhenFilterAsync_ThenReturnNone()
    {
        Option<int> some = Option.Some(1);

        Option<int> result =
            await some.FilterAsync(x => Task.FromResult(x == 2));

        result.ShouldBe(Option.None<int>());
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
    public void WhenAndThen_ThenReturnMappedOption()
    {
        Option<int> some = Option.Some(1);
        Option<int> result = some.AndThen(x => Option.Some(x + 1));
        result.ShouldBeSomeValue(2);
    }

    [Fact]
    public void GivenState_WhenAndThen_ThenReturnMappedOption()
    {
        Option<int> some = Option.Some(1);

        Option<int> result = some.AndThen(
            10,
            static (x, state) => Option.Some(x + state));

        result.ShouldBeSomeValue(11);
    }

    [Fact]
    public void WhenAndThenProducesANullOption_ThenThrow()
    {
        Option<int> some = Option.Some(1);

        Func<Option<int>> andThenNull =
            () => some.AndThen(_ => default(Option<int>)!);

        andThenNull.ShouldThrow<ArgumentNullException>()
                   .ParamName.ShouldBe("optionFactory");
    }

    [Fact]
    public void GivenState_WhenAndThenProducesANullOption_ThenThrow()
    {
        Option<int> some = Option.Some(1);

        Func<Option<int>> andThenNull = () =>
            some.AndThen(10, static (_, _) => default(Option<int>)!);

        andThenNull.ShouldThrow<ArgumentNullException>()
                   .ParamName.ShouldBe("optionFactory");
    }

    [Fact]
    public async Task WhenAndThenAsync_ThenReturnMappedOption()
    {
        Option<int> some = Option.Some(1);

        Option<int> result = await some.AndThenAsync(x =>
            new ValueTask<Option<int>>(Option.Some(x + 1)));

        result.ShouldBeSomeValue(2);
    }

    [Fact]
    public async Task GivenAPendingFactory_WhenAndThenAsync_ThenReturnMappedOption()
    {
        Option<int> some = Option.Some(1);

        Option<int> result = await some.AndThenAsync(async x =>
        {
            await Task.Yield();

            return Option.Some(x + 1);
        });

        result.ShouldBeSomeValue(2);
    }

    [Fact]
    public void
        GivenACompletedFactory_WhenAndThenAsyncProducesANullOption_ThenThrowFromTheCall()
    {
        Option<int> some = Option.Some(1);

        Action andThenNull = () => _ = some.AndThenAsync(
            _ => new ValueTask<Option<int>>(default(Option<int>)!));

        andThenNull.ShouldThrow<ArgumentNullException>()
                   .ParamName.ShouldBe("optionFactory");
    }

    /// <summary>
    /// The gate holds the factory's task incomplete until after the call returns,
    /// which is what puts the guard on its awaiting path. <c>await Task.Yield()</c>
    /// does not: on an idle thread pool it can resume before the guard reads
    /// <c>IsCompletedSuccessfully</c>, and the throw then lands at the call
    /// instead.
    /// </summary>
    [Fact]
    public async Task
        GivenAPendingFactory_WhenAndThenAsyncProducesANullOption_ThenFaultTheReturnedTask()
    {
        Option<int> some = Option.Some(1);
        var gate = new TaskCompletionSource<bool>();

        ValueTask<Option<int>> pending = some.AndThenAsync(
            async ValueTask<Option<int>> (_) =>
            {
                await gate.Task;

                return default(Option<int>)!;
            });

        gate.SetResult(true);

        Func<Task> consume = async () => await pending;

        (await consume.ShouldThrowAsync<ArgumentNullException>())
           .ParamName.ShouldBe("optionFactory");
    }

    [Fact]
    public async Task GivenAFaultingFactory_WhenAndThenAsync_ThenRethrow()
    {
        Option<int> some = Option.Some(1);

        Func<Task> andThenThrows = async () => await some.AndThenAsync(
            async ValueTask<Option<int>> (_) =>
            {
                await Task.Yield();

                throw new InvalidOperationException("boom");
            });

        (await andThenThrows.ShouldThrowAsync<InvalidOperationException>())
           .Message.ShouldBe("boom");
    }

    [Fact]
    public void WhenOkOr_ThenReturnOk()
    {
        Option<int> some = Option.Some(1);
        Result<int, string> result = some.OkOr("Error");
        result.ShouldBeOkValue(1);
    }

    [Fact]
    public async Task WhenOkOrAsync_ThenReturnOk()
    {
        Task<Option<int>> some = Task.FromResult(Option.Some(1));
        Result<int, string> result = await some.OkOrAsync("Error");
        result.ShouldBeOkValue(1);
    }

    [Fact]
    public async Task WhenOkOrAsyncWithValueTask_ThenReturnOk()
    {
        ValueTask<Option<int>> some = new(Option.Some(1));
        Result<int, string> result = await some.OkOrAsync("Error");
        result.ShouldBeOkValue(1);
    }

    [Fact]
    public void WhenOkOrElse_ThenReturnOk()
    {
        Option<int> some = Option.Some(1);
        Result<int, string> result = some.OkOrElse(() => "Error");
        result.ShouldBeOkValue(1);
    }

    [Fact]
    public async Task GivenOptionTask_WhenOkOrElseAsync_ThenReturnOk()
    {
        Task<Option<int>> some = Task.FromResult(Option.Some(1));

        Result<int, string> result =
            await some.OkOrElseAsync(() => Task.FromResult("Error"));

        result.ShouldBeOkValue(1);
    }

    [Fact]
    public async Task GivenOption_WhenOkOrElseAsync_ThenReturnOk()
    {
        Option<int> some = Option.Some(1);

        Result<int, string> result =
            await some.OkOrElseAsync(() => Task.FromResult("Error"));

        result.ShouldBeOkValue(1);
    }

    [Fact]
    public void GivenOtherIsSome_WhenZipWith_ThenReturnSome()
    {
        Option<int> self = Option.Some(1);
        Option<int> other = Option.Some(2);
        Option<int> result = self.ZipWith(other, (x, y) => x + y);
        result.ShouldBeSomeValue(3);
    }

    [Fact]
    public void GivenOtherIsNone_WhenZipWith_ThenReturnNone()
    {
        Option<int> self = Option.Some(1);
        Option<int> other = Option.None<int>();
        Option<int> result = self.ZipWith(other, (x, y) => x + y);
        result.ShouldBeNone();
    }

    [Fact]
    public async Task GivenOtherIsSome_WhenZipWithAsync_ThenReturnSome()
    {
        Option<int> self = Option.Some(1);
        Option<int> other = Option.Some(2);

        Option<int> result = await self.ZipWithAsync(
            other,
            (x, y) => Task.FromResult(x + y));

        result.ShouldBeSomeValue(3);
    }

    [Fact]
    public async Task GivenOtherIsSome_WhenZipWithAsyncProducesNull_ThenThrow()
    {
        Option<int> self = Option.Some(1);
        Option<int> other = Option.Some(2);

        Func<Task> zipToNull = async () => await self.ZipWithAsync(
            other,
            (_, _) => Task.FromResult(default(string)!));

        await zipToNull.ShouldThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task
        GivenOtherIsSome_AndSelfIsTask_WhenZipWithAsync_ThenReturnSome()
    {
        Task<Option<int>> self = Task.FromResult(Option.Some(1));
        Option<int> other = Option.Some(2);

        Option<int> result = await self.ZipWithAsync(
            other,
            (x, y) => Task.FromResult(x + y));

        result.ShouldBeSomeValue(3);
    }

    [Fact]
    public async Task GivenOtherIsNone_WhenZipWithAsync_ThenReturnNone()
    {
        Option<int> self = Option.Some(1);
        Option<int> other = Option.None<int>();

        Option<int> result = await self.ZipWithAsync(
            other,
            (x, y) => Task.FromResult(x + y));

        result.ShouldBeNone();
    }

    [Fact]
    public async Task
        GivenOtherIsNone_AndSelfIsTask_WhenZipWithAsync_ThenReturnNone()
    {
        Task<Option<int>> self = Task.FromResult(Option.Some(1));
        Option<int> other = Option.None<int>();

        Option<int> result = await self.ZipWithAsync(
            other,
            (x, y) => Task.FromResult(x + y));

        result.ShouldBeNone();
    }

    [Fact]
    public void WhenAndGivenSome_ThenReturnTheOtherOption()
    {
        Option<int> some = Option.Some(1);

        some.And(Option.Some("value")).ShouldBeSomeValue("value");
    }

    [Fact]
    public void WhenAndGivenNone_ThenReturnNone()
    {
        Option<int> some = Option.Some(1);

        some.And(Option.None<string>()).ShouldBeNone();
    }

    [Fact]
    public void WhenMapOrDefault_ThenReturnTheMappedValue()
    {
        Option<int> some = Option.Some(1);

        some.MapOrDefault(x => x + 1).ShouldBe(2);
    }

    [Fact]
    public void WhenReduceGivenSome_ThenCombineBothValues()
    {
        Option<int> some = Option.Some(1);

        some.Reduce(Option.Some(2), (x, y) => x + y).ShouldBeSomeValue(3);
    }

    [Fact]
    public void WhenReduceGivenNone_ThenReturnThisOption()
    {
        Option<int> some = Option.Some(1);

        some.Reduce(Option.None<int>(), (x, y) => x + y)
           .ShouldBeSomeValue(1);
    }

    [Fact]
    public void WhenReduceProducesADefault_ThenReturnSome()
    {
        Option<int> some = Option.Some(1);

        some.Reduce(Option.Some(-1), (x, y) => x + y).ShouldBeSomeValue(0);
    }

    [Fact]
    public void WhenReduceProducesNull_ThenThrow()
    {
        Option<string> some = Option.Some("a");

        Func<Option<string>> reduceToNull = () =>
            some.Reduce(Option.Some("b"), (_, _) => default(string)!);

        reduceToNull.ShouldThrow<ArgumentNullException>()
                    .ParamName.ShouldBe("reduce");
    }

    [Fact]
    public void WhenAsEnumerable_ThenYieldTheValueOnce()
    {
        Option<int> some = Option.Some(1);

        some.AsEnumerable().ShouldBe(new[] { 1 });
    }
}
