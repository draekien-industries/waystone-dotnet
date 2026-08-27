namespace Waystone.Monads.Options.Extensions;

using JetBrains.Annotations;
using Monads.Extensions;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

[TestSubject(typeof(MapOrElseExtensions))]
public sealed class MapOrElseExtensionsTests
{
    [Fact]
    public async Task
        GivenSomeValueTask_WhenMapOrElseAsync_ThenReturnTheMappedValue()
    {
        int result = await new ValueTask<Option<int>>(Option.Some(1))
           .MapOrElseAsync(() => 0, x => x + 1);

        result.ShouldBe(2);
    }

    [Fact]
    public async Task
        GivenNoneValueTask_WhenMapOrElseAsync_ThenReturnTheFallback()
    {
        int result = await new ValueTask<Option<int>>(Option.None<int>())
           .MapOrElseAsync(() => 0, x => x + 1);

        result.ShouldBe(0);
    }

    [Fact]
    public async Task
        GivenNone_WhenMapOrElseAsyncWithASyncDefaultAndAnAsyncMap_ThenReturnTheDefault()
    {
        int result = await Option.None<int>()
                                .MapOrElseAsync(
                                     () => -1,
                                     value => Task.FromResult(value + 1));

        result.ShouldBe(-1);
    }

    [Fact]
    public async Task
        GivenNone_WhenMapOrElseAsyncWithAnAsyncDefaultAndASyncMap_ThenReturnTheDefault()
    {
        int result = await Option.None<int>()
                                .MapOrElseAsync(
                                     () => Task.FromResult(-1),
                                     value => value + 1);

        result.ShouldBe(-1);
    }

    [Fact]
    public async Task
        GivenNoneTask_WhenMapOrElseAsyncWithSyncBranches_ThenReturnTheDefault()
    {
        int result = await Task.FromResult(Option.None<int>())
           .MapOrElseAsync(() => -1, value => value + 1);

        result.ShouldBe(-1);
    }

    [Fact]
    public async Task
        GivenNoneTask_WhenMapOrElseAsyncWithASyncDefaultAndAnAsyncMap_ThenReturnTheDefault()
    {
        int result = await Task.FromResult(Option.None<int>())
           .MapOrElseAsync(() => -1, value => Task.FromResult(value + 1));

        result.ShouldBe(-1);
    }

    [Fact]
    public async Task
        GivenNoneTask_WhenMapOrElseAsyncWithAnAsyncDefaultAndASyncMap_ThenReturnTheDefault()
    {
        int result = await Task.FromResult(Option.None<int>())
           .MapOrElseAsync(() => Task.FromResult(-1), value => value + 1);

        result.ShouldBe(-1);
    }

    [Fact]
    public async Task
        GivenNoneValueTask_WhenMapOrElseAsyncWithASyncDefaultAndAnAsyncMap_ThenReturnTheDefault()
    {
        int result = await new ValueTask<Option<int>>(Option.None<int>())
           .MapOrElseAsync(() => -1, value => Task.FromResult(value + 1));

        result.ShouldBe(-1);
    }

    [Fact]
    public async Task
        GivenNoneValueTask_WhenMapOrElseAsyncWithAnAsyncDefaultAndASyncMap_ThenReturnTheDefault()
    {
        int result = await new ValueTask<Option<int>>(Option.None<int>())
           .MapOrElseAsync(() => Task.FromResult(-1), value => value + 1);

        result.ShouldBe(-1);
    }
}
