namespace Waystone.Monads.Options.Extensions;

using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using NSubstitute;
using Shouldly;
using Xunit;

[TestSubject(typeof(MatchExtensions))]
public sealed class MatchExtensionsTests
{
    [Fact]
    public async Task GivenSome_WhenMatch_ThenInvokeSomeFunc()
    {
        Option<int> some = Option.Some(42);
        var onSome = Substitute.For<Func<int, Task<int>>>();
        var onNone = Substitute.For<Func<Task<int>>>();

        onSome.Invoke(Arg.Any<int>()).Returns(Task.FromResult(42));
        onNone.Invoke().Returns(Task.FromResult(0));

        int result = await Task.FromResult(some).Match(onSome, onNone);

        result.ShouldBe(42);
        await onSome.Received(1).Invoke(42);
        await onNone.DidNotReceive().Invoke();
    }

    [Fact]
    public async Task GivenNone_WhenMatch_ThenInvokeNoneFunc()
    {
        Option<int> none = Option.None<int>();
        var onSome = Substitute.For<Func<int, Task<int>>>();
        var onNone = Substitute.For<Func<Task<int>>>();

        onSome.Invoke(Arg.Any<int>()).Returns(Task.FromResult(42));
        onNone.Invoke().Returns(Task.FromResult(0));

        int result = await Task.FromResult(none).Match(onSome, onNone);

        result.ShouldBe(0);
        await onSome.DidNotReceive().Invoke(Arg.Any<int>());
        await onNone.Received(1).Invoke();
    }
}
