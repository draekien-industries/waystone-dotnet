﻿namespace Waystone.Monads.Options.Extensions;

using Results;

[TestSubject(typeof(OptionOfTExtensions))]
public sealed class OptionOfTExtensionsTests
{
#region awaited

    [Fact]
    public async Task GivenSomeOptionOfTask_WhenAwaited_ThenReturnTaskOfOption()
    {
        Option<Task<int>> optionOfTask = Option.Some(Task.FromResult(42));
        Option<int> result = await optionOfTask.Awaited();
        result.Should().Be(Option.Some(42));
    }

    [Fact]
    public async Task
        GivenSomeOptionOfTaskAndCallback_WhenAwaited_ThenReturnTaskOfOption()
    {
        Option<Task<int>> optionOfTask = Option.Some(Task.FromResult(42));
        var callback = Substitute.For<Action<Exception>>();
        Option<int> result = await optionOfTask.Awaited(callback);
        result.Should().Be(Option.Some(42));
        callback.DidNotReceive().Invoke(Arg.Any<Exception>());
    }

    [Fact]
    public async Task
        GivenNoneOption_AndOptionOfTaskThatSucceeds_WhenAwaited_ThenReturnNone()
    {
        async Task<int> PerformTask(int x) => await Task.FromResult(x + 1);

        var callback = Substitute.For<Action<Exception>>();

        Option<Task<int>> option =
            Option.None<int>().Map(PerformTask);

        Option<int> result = await option.Awaited(callback);
        result.Should().Be(Option.None<int>());
        callback.DidNotReceive().Invoke(Arg.Any<Exception>());
    }

    [Fact]
    public async Task
        GivenNoneOption_AndNoCallback_AndOptionOfTaskThatSucceeds_WhenAwaited_ThenReturnNone()
    {
        async Task<int> PerformTask(int x) => await Task.FromResult(x + 1);

        Option<Task<int>> option =
            Option.None<int>().Map(PerformTask);

        Option<int> result = await option.Awaited();
        result.Should().Be(Option.None<int>());
    }

    [Fact]
    public async Task GivenOptionOfTaskThatThrows_WhenAwaited_ThenReturnNone()
    {
        async Task<int> PerformTask(int x)
        {
            await Task.Delay(10);
            throw new Exception();
        }

        var callback = Substitute.For<Action<Exception>>();

        Option<Task<int>> option =
            Option.Some(10).Map(PerformTask);

        Option<int> result = await option.Awaited(callback);
        result.Should().Be(Option.None<int>());
        callback.Received(1).Invoke(Arg.Any<Exception>());
    }

#endregion awaited

#region unzip

    [Fact]
    public void GivenSomeWithTuple_WhenUnzip_ThenReturnSome()
    {
        Option<int> some1 = Option.Some(1);
        Option<int> some2 = Option.Some(2);
        Option<(int, int)> zipped = some1.Zip(some2);
        (Option<int>, Option<int>) result = zipped.Unzip();
        result.Should().Be((some1, some2));
    }

    [Fact]
    public void GivenNoneNoneTuple_WhenUnzip_ThenReturnNone()
    {
        Option<int> none1 = Option.None<int>();
        Option<int> none2 = Option.None<int>();
        Option<(int, int)> zipped = none1.Zip(none2);
        (Option<int>, Option<int>) result = zipped.Unzip();
        result.Should().Be((none1, none2));
    }

    [Fact]
    public void GivenNoneSomeTuple_WhenUnzip_ThenReturnNoneNone()
    {
        Option<int> none = Option.None<int>();
        Option<int> some = Option.Some(1);
        Option<(int, int)> zipped = none.Zip(some);
        (Option<int>, Option<int>) result = zipped.Unzip();
        result.Should().Be((none, none));
    }

#endregion unzip

#region flatten

    [Fact]
    public void GivenSomeOfSome_WhenFlatten_ThenReturnOption()
    {
        Option<Option<int>> some = Option.Some(Option.Some(1));
        Option<int> result = some.Flatten();
        result.Should().Be(Option.Some(1));
    }

    [Fact]
    public void GivenNoneOfSome_WhenFlatten_ThenReturnNone()
    {
        Option<int> none = Option.None<int>();
        Option<Option<int>> nested = none.Map(Option.Some);
        Option<int> result = nested.Flatten();
        result.Should().Be(Option.None<int>());
    }

#endregion flatten

#region transpose

    [Fact]
    public void GivenSomeOfOk_WhenTranspose_ThenReturnOkOfSome()
    {
        Option<Result<int, int>> option = Option.Some(Result.Ok<int, int>(1));
        Result<Option<int>, int> result = option.Transpose();
        result.Should().Be(Result.Ok<Option<int>, int>(Option.Some(1)));
    }

    [Fact]
    public void GivenSomeOfErr_WhenTranspose_ThenReturnErrOfSome()
    {
        Option<Result<int, int>>
            option = Option.Some(Result.Err<int, int>(1));
        Result<Option<int>, int> result = option.Transpose();
        result.Should().Be(Result.Err<Option<int>, int>(1));
    }

    [Fact]
    public void GivenNoneOfOk_WhenTranspose_ThenReturnOkOfNone()
    {
        Option<Result<int, int>> option =
            Option.None<int>().Map(Result.Ok<int, int>);

        Result<Option<int>, int> result = option.Transpose();

        result.Should().Be(Result.Ok<Option<int>, int>(Option.None<int>()));
    }

    [Fact]
    public void GivenNoneOfErr_WhenTranspose_ThenReturnOkOfNone()
    {
        Option<Result<int, int>> option =
            Option.None<int>().Map(Result.Err<int, int>);

        Result<Option<int>, int> result = option.Transpose();

        result.Should().Be(Result.Ok<Option<int>, int>(Option.None<int>()));
    }

#endregion transpose
}
