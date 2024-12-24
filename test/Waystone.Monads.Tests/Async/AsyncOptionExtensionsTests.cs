namespace Waystone.Monads.Tests.Async;

using Monads.Async;

public sealed class AsyncOptionExtensionsTests
{
    [Fact]
    public async Task
        GivenAsyncOptionIsSome_WhenMatchAsync_ThenOnSomeAsyncIsCalled()
    {
        Task<Option<int>> option = Option.BindAsync(() => Task.FromResult(42));
        var onSomeAsync = Substitute.For<Func<int, Task<string>>>();
        var onNoneAsync = Substitute.For<Func<Task<string>>>();

        onSomeAsync.Invoke(Arg.Any<int>()).Returns(Task.FromResult("foo"));

        string result = await option.MatchAsync(onSomeAsync, onNoneAsync);

        result.Should().Be("foo");
        await onSomeAsync.Received(1).Invoke(Arg.Any<int>());
        await onNoneAsync.DidNotReceive().Invoke();
    }

    [Fact]
    public async Task
        GivenAsyncOptionIsNone_WhenMatchAsync_ThenOnNoneAsyncIsCalled()
    {
        Task<Option<int>> option = Task.FromResult(Option.None<int>());
        var onSomeAsync = Substitute.For<Func<int, Task<string>>>();
        var onNoneAsync = Substitute.For<Func<Task<string>>>();

        onNoneAsync.Invoke().Returns(Task.FromResult("foo"));

        string result = await option.MatchAsync(onSomeAsync, onNoneAsync);

        result.Should().Be("foo");
        await onSomeAsync.DidNotReceive().Invoke(Arg.Any<int>());
        await onNoneAsync.Received(1).Invoke();
    }

    [Fact]
    public async Task
        GivenAsyncOptionIsSome_WhenMatchAsyncWithNoReturn_ThenOnSomeAsyncIsCalled()
    {
        Task<Option<int>> option = Option.BindAsync(() => Task.FromResult(42));
        var onSomeAsync = Substitute.For<Func<int, Task>>();
        var onNoneAsync = Substitute.For<Func<Task>>();

        await option.MatchAsync(onSomeAsync, onNoneAsync);

        await onSomeAsync.Received(1).Invoke(Arg.Any<int>());
        await onNoneAsync.DidNotReceive().Invoke();
    }

    [Fact]
    public async Task
        GivenAsyncOptionIsNone_WhenMatchAsyncWithNoReturn_ThenOnNoneAsyncIsCalled()
    {
        Task<Option<int>> option = Task.FromResult(Option.None<int>());
        var onSomeAsync = Substitute.For<Func<int, Task>>();
        var onNoneAsync = Substitute.For<Func<Task>>();

        await option.MatchAsync(onSomeAsync, onNoneAsync);

        await onSomeAsync.DidNotReceive().Invoke(Arg.Any<int>());
        await onNoneAsync.Received(1).Invoke();
    }

    [Fact]
    public async Task
        GivenAsyncOptionWithTupleIsSome_WhenUnzipAsync_ThenReturnTupleOfSomes()
    {
        Task<Option<(int, string)>> option =
            Task.FromResult(Option.Some((42, "foo")));

        (Option<int>, Option<string>) result = await option.UnzipAsync();

        result.Item1.Should().Be(Option.Some(42));
        result.Item2.Should().Be(Option.Some("foo"));
    }

    [Fact]
    public async Task
        GivenAsyncOptionWithTupleIsNone_WhenUnzipAsync_ThenReturnTupleOfNones()
    {
        Task<Option<(int, string)>> option =
            Task.FromResult(Option.None<(int, string)>());

        (Option<int>, Option<string>) result = await option.UnzipAsync();

        result.Item1.Should().Be(Option.None<int>());
        result.Item2.Should().Be(Option.None<string>());
    }
}
