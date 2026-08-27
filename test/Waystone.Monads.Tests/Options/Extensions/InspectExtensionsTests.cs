namespace Waystone.Monads.Options.Extensions;

using JetBrains.Annotations;
using NSubstitute;
using System;
using System.Threading.Tasks;
using Xunit;

[TestSubject(typeof(InspectExtensions))]
public sealed class InspectExtensionsTests
{
    [Fact]
    public async Task GivenSomeTask_WhenInspectAsync_ThenInvokeTheDelegate()
    {
        var inspect = Substitute.For<Func<int, Task>>();

        await Task.FromResult(Option.Some(10)).InspectAsync(inspect);

        await inspect.Received(1).Invoke(10);
    }

    [Fact]
    public async Task
        GivenSomeTask_WhenInspectAsyncWithASyncDelegate_ThenInvokeIt()
    {
        var inspect = Substitute.For<Action<int>>();

        await Task.FromResult(Option.Some(20)).InspectAsync(inspect);

        inspect.Received(1).Invoke(20);
    }

    [Fact]
    public async Task GivenNoneTask_WhenInspectAsync_ThenSkipTheDelegate()
    {
        var inspect = Substitute.For<Func<int, Task>>();

        await Task.FromResult(Option.None<int>()).InspectAsync(inspect);

        await inspect.DidNotReceive().Invoke(Arg.Any<int>());
    }

    [Fact]
    public async Task
        GivenNoneTask_WhenInspectAsyncWithASyncDelegate_ThenSkipIt()
    {
        var inspect = Substitute.For<Action<int>>();

        await Task.FromResult(Option.None<int>()).InspectAsync(inspect);

        inspect.DidNotReceive().Invoke(Arg.Any<int>());
    }

    [Fact]
    public async Task
        GivenSomeValueTask_WhenInspectAsync_ThenInvokeTheDelegate()
    {
        var inspect = Substitute.For<Func<int, Task>>();

        await new ValueTask<Option<int>>(Option.Some(30))
           .InspectAsync(inspect);

        await inspect.Received(1).Invoke(30);
    }

    [Fact]
    public async Task GivenNoneValueTask_WhenInspectAsync_ThenSkipTheDelegate()
    {
        var inspect = Substitute.For<Func<int, Task>>();

        await new ValueTask<Option<int>>(Option.None<int>())
           .InspectAsync(inspect);

        await inspect.DidNotReceive().Invoke(Arg.Any<int>());
    }

    [Fact]
    public async Task
        GivenSomeValueTask_WhenInspectAsyncWithASyncDelegate_ThenInvokeIt()
    {
        var inspect = Substitute.For<Action<int>>();

        await new ValueTask<Option<int>>(Option.Some(40))
           .InspectAsync(inspect);

        inspect.Received(1).Invoke(40);
    }

    [Fact]
    public async Task
        GivenNoneValueTask_WhenInspectAsyncWithASyncDelegate_ThenSkipIt()
    {
        var inspect = Substitute.For<Action<int>>();

        await new ValueTask<Option<int>>(Option.None<int>())
           .InspectAsync(inspect);

        inspect.DidNotReceive().Invoke(Arg.Any<int>());
    }
}
