#pragma warning disable CS0618
namespace Waystone.Monads.Options;

using Extensions;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

public sealed class ObsoleteForwardingTests
{
    [Fact]
    public void FlatMapForwardsToAndThen()
    {
        Option<int> some = Option.Some(1);

        some.FlatMap(x => Option.Some(x + 1))
           .ShouldBe(some.AndThen(x => Option.Some(x + 1)));
    }

    [Fact]
    public async Task FlatMapAsyncOnAnOptionForwardsToAndThenAsync()
    {
        Option<int> some = Option.Some(1);

        Option<int> result = await some.FlatMapAsync(
            x => Task.FromResult(Option.Some(x + 1)));

        result.ShouldBe(Option.Some(2));
    }

    [Fact]
    public async Task
        FlatMapAsyncOnATaskWithAnAsyncMapForwardsToAndThenAsync()
    {
        Option<int> result = await Task.FromResult(Option.Some(1))
           .FlatMapAsync(x => Task.FromResult(Option.Some(x + 1)));

        result.ShouldBe(Option.Some(2));
    }

    [Fact]
    public async Task FlatMapAsyncOnATaskWithASyncMapForwardsToAndThenAsync()
    {
        Option<int> result = await Task.FromResult(Option.Some(1))
           .FlatMapAsync(x => Option.Some(x + 1));

        result.ShouldBe(Option.Some(2));
    }

    [Fact]
    public async Task
        FlatMapAsyncOnAValueTaskWithAnAsyncMapForwardsToAndThenAsync()
    {
        Option<int> result = await new ValueTask<Option<int>>(Option.Some(1))
           .FlatMapAsync(x => Task.FromResult(Option.Some(x + 1)));

        result.ShouldBe(Option.Some(2));
    }

    [Fact]
    public async Task
        FlatMapAsyncOnAValueTaskWithASyncMapForwardsToAndThenAsync()
    {
        Option<int> result = await new ValueTask<Option<int>>(Option.Some(1))
           .FlatMapAsync(x => Option.Some(x + 1));

        result.ShouldBe(Option.Some(2));
    }
}
