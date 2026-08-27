namespace Waystone.Monads.Options.Extensions;

using JetBrains.Annotations;
using Shouldly;
using System;
using System.Threading.Tasks;
using Xunit;

[TestSubject(typeof(OptionExtensions))]
public sealed class AndThenExtensionsTests
{
    private static readonly Func<int, ValueTask<Option<int>>> AsyncDouble =
        async value =>
        {
            await Task.Yield();

            return Option.Some(value * 2);
        };

    private static readonly Func<int, ValueTask<Option<int>>> AsyncNone =
        async _ =>
        {
            await Task.Yield();

            return Option.None<int>();
        };

    private static readonly Func<int, Option<int>> SyncDouble =
        value => Option.Some(value * 2);

    private static readonly Func<int, Option<int>> SyncNone =
        _ => Option.None<int>();

    [Fact]
    public async Task GivenSome_WhenAndThenAsync_ThenReturnTheMappedOption()
    {
        Option<int> result = await Option.Some(10).AndThenAsync(AsyncDouble);

        result.ShouldBeSomeValue(20);
    }

    [Fact]
    public async Task GivenNone_WhenAndThenAsync_ThenReturnNone()
    {
        Option<int> result =
            await Option.None<int>().AndThenAsync(AsyncNone);

        result.ShouldBeNone();
    }

    [Fact]
    public async Task GivenSomeTask_WhenAndThenAsync_ThenReturnTheMappedOption()
    {
        Option<int> result = await Task.FromResult(Option.Some(10))
           .AndThenAsync(AsyncDouble);

        result.ShouldBeSomeValue(20);
    }

    [Fact]
    public async Task GivenNoneTask_WhenAndThenAsync_ThenReturnNone()
    {
        Option<int> result = await Task.FromResult(Option.None<int>())
           .AndThenAsync(AsyncNone);

        result.ShouldBeNone();
    }

    [Fact]
    public async Task
        GivenSomeTask_WhenAndThenAsyncWithASyncMap_ThenReturnTheMappedOption()
    {
        Option<int> result = await Task.FromResult(Option.Some(10))
           .AndThenAsync(SyncDouble);

        result.ShouldBeSomeValue(20);
    }

    [Fact]
    public async Task
        GivenSomeTask_WhenAndThenAsyncWithASyncMapReturningNone_ThenReturnNone()
    {
        Option<int> result = await Task.FromResult(Option.Some(10))
           .AndThenAsync(SyncNone);

        result.ShouldBeNone();
    }

    [Fact]
    public async Task
        GivenSomeValueTask_WhenAndThenAsync_ThenReturnTheMappedOption()
    {
        Option<int> result =
            await new ValueTask<Option<int>>(Option.Some(10))
               .AndThenAsync(AsyncDouble);

        result.ShouldBeSomeValue(20);
    }

    [Fact]
    public async Task GivenNoneValueTask_WhenAndThenAsync_ThenReturnNone()
    {
        Option<int> result =
            await new ValueTask<Option<int>>(Option.None<int>())
               .AndThenAsync(AsyncNone);

        result.ShouldBeNone();
    }

    [Fact]
    public async Task
        GivenSomeValueTask_WhenAndThenAsyncWithASyncMap_ThenReturnTheMappedOption()
    {
        Option<int> result =
            await new ValueTask<Option<int>>(Option.Some(10))
               .AndThenAsync(SyncDouble);

        result.ShouldBeSomeValue(20);
    }

    [Fact]
    public async Task
        GivenNoneValueTask_WhenAndThenAsyncWithASyncMap_ThenReturnNone()
    {
        Option<int> result =
            await new ValueTask<Option<int>>(Option.None<int>())
               .AndThenAsync(SyncNone);

        result.ShouldBeNone();
    }
}
