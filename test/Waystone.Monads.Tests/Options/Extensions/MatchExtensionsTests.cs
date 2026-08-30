namespace Waystone.Monads.Options.Extensions;

using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using NSubstitute;
using Results;
using Shouldly;
using Xunit;

/// <remarks>
/// The fully-asynchronous shape on both awaited receivers was once all this
/// family had, which was enough while every overload here was hand-written.
/// Putting the family on the generator makes the rest of the surface
/// load-bearing, so these are the cases that pin it: each of the three
/// synchronous-receiver overloads on both branches, and each shape the generator
/// derives from the core member.
/// <para>
/// The synchronous-receiver overloads are the ones worth guarding hardest. The
/// generator lifts only members whose receiver is not itself awaitable, so
/// deleting one of them would silently take two awaited overloads with it and
/// the baseline is the only other thing that would notice.
/// </para>
/// </remarks>
[TestSubject(typeof(OptionExtensions))]
public sealed class MatchExtensionsTests
{
    [Fact]
    public async Task GivenSome_WhenMatchAsyncWithAsyncBranches_ThenAwaitOnSome()
    {
        int result = await Option.Some(1)
           .MatchAsync(
                value => Task.FromResult(value + 1),
                () => Task.FromResult(0));

        result.ShouldBe(2);
    }

    [Fact]
    public async Task GivenNone_WhenMatchAsyncWithAsyncBranches_ThenAwaitOnNone()
    {
        int result = await Option.None<int>()
           .MatchAsync(
                value => Task.FromResult(value + 1),
                () => Task.FromResult(99));

        result.ShouldBe(99);
    }

    [Fact]
    public async Task GivenSome_WhenMatchAsyncWithAsyncOnNone_ThenRunOnSome()
    {
        int result = await Option.Some(1)
           .MatchAsync(value => value + 1, () => Task.FromResult(0));

        result.ShouldBe(2);
    }

    [Fact]
    public async Task GivenNone_WhenMatchAsyncWithAsyncOnNone_ThenAwaitOnNone()
    {
        int result = await Option.None<int>()
           .MatchAsync(value => value + 1, () => Task.FromResult(99));

        result.ShouldBe(99);
    }

    [Fact]
    public async Task GivenSome_WhenMatchAsyncWithAsyncOnSome_ThenAwaitOnSome()
    {
        int result = await Option.Some(1)
           .MatchAsync(value => Task.FromResult(value + 1), () => 0);

        result.ShouldBe(2);
    }

    [Fact]
    public async Task GivenNone_WhenMatchAsyncWithAsyncOnSome_ThenRunOnNone()
    {
        int result = await Option.None<int>()
           .MatchAsync(value => Task.FromResult(value + 1), () => 99);

        result.ShouldBe(99);
    }

    /// <summary>
    /// The asynchronous branch must not run for the case that did not match, which
    /// a returned value alone cannot show — a branch that ran and was discarded
    /// looks identical.
    /// </summary>
    [Fact]
    public async Task GivenSome_WhenMatchAsync_ThenLeaveOnNoneUninvoked()
    {
        var onNone = Substitute.For<Func<Task<int>>>();
        onNone.Invoke().Returns(Task.FromResult(0));

        await Option.Some(1).MatchAsync(value => value + 1, onNone);

        await onNone.DidNotReceive().Invoke();
    }

    [Fact]
    public async Task GivenNone_WhenMatchAsync_ThenLeaveOnSomeUninvoked()
    {
        var onSome = Substitute.For<Func<int, Task<int>>>();
        onSome.Invoke(Arg.Any<int>()).Returns(Task.FromResult(0));

        await Option.None<int>().MatchAsync(onSome, () => 99);

        await onSome.DidNotReceiveWithAnyArgs().Invoke(default);
    }

    [Fact]
    public async Task
        GivenSomeTask_WhenMatchAsyncWithAsyncOnSome_ThenAwaitOnSome()
    {
        int result = await Task.FromResult(Option.Some(1))
           .MatchAsync(value => Task.FromResult(value + 1), () => 0);

        result.ShouldBe(2);
    }

    [Fact]
    public async Task
        GivenNoneValueTask_WhenMatchAsyncWithAsyncOnSome_ThenRunOnNone()
    {
        int result = await new ValueTask<Option<int>>(Option.None<int>())
           .MatchAsync(value => Task.FromResult(value + 1), () => 99);

        result.ShouldBe(99);
    }

    [Fact]
    public async Task
        GivenNoneTask_WhenMatchAsyncWithAsyncOnNone_ThenAwaitOnNone()
    {
        int result = await Task.FromResult(Option.None<int>())
           .MatchAsync(value => value + 1, () => Task.FromResult(99));

        result.ShouldBe(99);
    }

    [Fact]
    public async Task
        GivenSomeValueTask_WhenMatchAsyncWithAsyncOnNone_ThenRunOnSome()
    {
        int result = await new ValueTask<Option<int>>(Option.Some(1))
           .MatchAsync(value => value + 1, () => Task.FromResult(0));

        result.ShouldBe(2);
    }

    [Fact]
    public async Task GivenSomeTask_WhenMatchAsyncWithFuncs_ThenInvokeOnSome()
    {
        int result = await Task.FromResult(Option.Some(1))
           .MatchAsync(value => value + 1, () => 0);

        result.ShouldBe(2);
    }

    [Fact]
    public async Task
        GivenNoneValueTask_WhenMatchAsyncWithFuncs_ThenInvokeOnNone()
    {
        int result = await new ValueTask<Option<int>>(Option.None<int>())
           .MatchAsync(value => value + 1, () => 99);

        result.ShouldBe(99);
    }

    [Fact]
    public async Task GivenSomeTask_WhenMatchAsyncWithActions_ThenInvokeOnSome()
    {
        var onSome = Substitute.For<Action<int>>();
        var onNone = Substitute.For<Action>();

        await Task.FromResult(Option.Some(1)).MatchAsync(onSome, onNone);

        onSome.Received().Invoke(1);
        onNone.DidNotReceive().Invoke();
    }

    [Fact]
    public async Task
        GivenNoneValueTask_WhenMatchAsyncWithActions_ThenInvokeOnNone()
    {
        var onSome = Substitute.For<Action<int>>();
        var onNone = Substitute.For<Action>();

        await new ValueTask<Option<int>>(Option.None<int>())
           .MatchAsync(onSome, onNone);

        onNone.Received().Invoke();
        onSome.DidNotReceiveWithAnyArgs().Invoke(default);
    }

    [Fact]
    public async Task GivenSomeTask_AndState_WhenMatchAsync_ThenInvokeOnSome()
    {
        int result = await Task.FromResult(Option.Some(1))
           .MatchAsync(
                10,
                static (value, state) => value + state,
                static state => state * 100);

        result.ShouldBe(11);
    }

    [Fact]
    public async Task
        GivenNoneValueTask_AndState_WhenMatchAsync_ThenInvokeOnNone()
    {
        int result = await new ValueTask<Option<int>>(Option.None<int>())
           .MatchAsync(
                10,
                static (value, state) => value + state,
                static state => state * 100);

        result.ShouldBe(1000);
    }

    [Fact]
    public async Task
        GivenSomeTask_AndState_WhenMatchAsyncWithActions_ThenInvokeOnSome()
    {
        var onSome = Substitute.For<Action<int, int>>();
        var onNone = Substitute.For<Action<int>>();

        await Task.FromResult(Option.Some(1)).MatchAsync(10, onSome, onNone);

        onSome.Received().Invoke(1, 10);
        onNone.DidNotReceiveWithAnyArgs().Invoke(default);
    }

    [Fact]
    public async Task
        GivenNoneValueTask_AndState_WhenMatchAsyncWithActions_ThenInvokeOnNone()
    {
        var onSome = Substitute.For<Action<int, int>>();
        var onNone = Substitute.For<Action<int>>();

        await new ValueTask<Option<int>>(Option.None<int>())
           .MatchAsync(10, onSome, onNone);

        onNone.Received().Invoke(10);
        onSome.DidNotReceiveWithAnyArgs().Invoke(default, default);
    }

    /// <summary>
    /// <c>OkOrElseAsync</c> forwards through <c>MatchAsync</c> with a
    /// value-returning <c>async</c> lambda in the <see cref="None{T}" /> branch.
    /// Converting this family without the synchronous-receiver overloads made
    /// overload resolution pick the generated <c>Action</c> shape instead, which
    /// fails as CS8030 in a file with nothing wrong in it. This is the call site
    /// that would catch that regression.
    /// </summary>
    [Fact]
    public async Task GivenNoneTask_WhenOkOrElseAsync_ThenAwaitTheErrorFactory()
    {
        Result<int, string> result =
            await Task.FromResult(Option.None<int>())
               .OkOrElseAsync(() => Task.FromResult("error"));

        result.ShouldBeErr();
        result.ExpectErr("Expected Err but found Ok.").ShouldBe("error");
    }

    [Fact]
    public async Task
        GivenSomeTask_WhenMatchAsyncWithAsyncBranches_ThenAwaitOnSome()
    {
        string result = await Task.FromResult(Option.Some(42))
           .MatchAsync(
                value => Task.FromResult("Value is " + value),
                () => Task.FromResult("No Value"));

        result.ShouldBe("Value is 42");
    }

    [Fact]
    public async Task
        GivenNoneTask_WhenMatchAsyncWithAsyncBranches_ThenAwaitOnNone()
    {
        string result = await Task.FromResult(Option.None<int>())
           .MatchAsync(
                value => Task.FromResult("Value is " + value),
                () => Task.FromResult("No Value"));

        result.ShouldBe("No Value");
    }

    [Fact]
    public async Task
        GivenSomeValueTask_WhenMatchAsyncWithAsyncBranches_ThenAwaitOnSome()
    {
        string result = await new ValueTask<Option<int>>(Option.Some(100))
           .MatchAsync(
                value => Task.FromResult("Value is " + value),
                () => Task.FromResult("No Value"));

        result.ShouldBe("Value is 100");
    }

    [Fact]
    public async Task
        GivenNoneValueTask_WhenMatchAsyncWithAsyncBranches_ThenAwaitOnNone()
    {
        string result = await new ValueTask<Option<int>>(Option.None<int>())
           .MatchAsync(
                value => Task.FromResult("Value is " + value),
                () => Task.FromResult("No Value"));

        result.ShouldBe("No Value");
    }
}
