namespace Waystone.Monads.Options.Extensions;

using System.Threading.Tasks;
using JetBrains.Annotations;
using Shouldly;
using Xunit;

[TestSubject(typeof(OptionExtensions))]
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

    [Fact]
    public async Task GivenSomeTask_WhenMapOrElseAsync_ThenReturnTheMappedValue()
    {
        string result = await Task.FromResult(Option.Some(10))
           .MapOrElseAsync(
                () => Task.FromResult("fallback"),
                value => Task.FromResult("mapped" + value));

        result.ShouldBe("mapped10");
    }

    [Fact]
    public async Task GivenNoneTask_WhenMapOrElseAsync_ThenReturnTheFallback()
    {
        string result = await Task.FromResult(Option.None<int>())
           .MapOrElseAsync(
                () => Task.FromResult("fallback"),
                value => Task.FromResult("mapped" + value));

        result.ShouldBe("fallback");
    }

    [Fact]
    public async Task
        GivenSomeValueTask_WhenMapOrElseAsyncWithAsyncBranches_ThenReturnTheMappedValue()
    {
        string result = await new ValueTask<Option<int>>(Option.Some(20))
           .MapOrElseAsync(
                () => Task.FromResult("default"),
                value => Task.FromResult("value" + value));

        result.ShouldBe("value20");
    }

    [Fact]
    public async Task
        GivenNoneValueTask_WhenMapOrElseAsyncWithAsyncBranches_ThenReturnTheFallback()
    {
        string result = await new ValueTask<Option<int>>(Option.None<int>())
           .MapOrElseAsync(
                () => Task.FromResult("default"),
                value => Task.FromResult("value" + value));

        result.ShouldBe("default");
    }

    [Fact]
    public async Task
        GivenSomeTask_WhenMapOrElseAsyncWithASyncFallbackAndAnAsyncMap_ThenReturnTheMappedValue()
    {
        string result = await Task.FromResult(Option.Some(30))
           .MapOrElseAsync(
                () => "syncFallback",
                value => Task.FromResult("asyncMapped" + value));

        result.ShouldBe("asyncMapped30");
    }

    [Fact]
    public async Task
        GivenSomeTask_WhenMapOrElseAsyncWithAnAsyncFallbackAndASyncMap_ThenReturnTheMappedValue()
    {
        string result = await Task.FromResult(Option.Some(40))
           .MapOrElseAsync(
                () => Task.FromResult("asyncFallback"),
                value => "syncMapped" + value);

        result.ShouldBe("syncMapped40");
    }

    [Fact]
    public async Task
        GivenSomeTask_WhenMapOrElseAsyncWithSyncBranches_ThenReturnTheMappedValue()
    {
        string result = await Task.FromResult(Option.Some(50))
           .MapOrElseAsync(() => "syncFallback", value => "syncMapped" + value);

        result.ShouldBe("syncMapped50");
    }
}
