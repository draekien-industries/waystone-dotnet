namespace Waystone.Monads.Options.Extensions;

using JetBrains.Annotations;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

[TestSubject(typeof(MapOrDefaultExtensions))]
public sealed class MapOrDefaultExtensionsTests
{
    [Fact]
    public async Task GivenSome_WhenMapOrDefaultAsync_ThenReturnTheMappedValue()
    {
        Option<int> some = Option.Some(1);

        int result =
            await some.MapOrDefaultAsync(x => Task.FromResult(x + 1));

        result.ShouldBe(2);
    }

    [Fact]
    public async Task GivenNone_WhenMapOrDefaultAsync_ThenReturnTheDefault()
    {
        Option<int> none = Option.None<int>();

        int result =
            await none.MapOrDefaultAsync(x => Task.FromResult(x + 1));

        result.ShouldBe(0);
    }

    [Fact]
    public async Task
        GivenSomeTask_WhenMapOrDefaultAsyncWithASyncMap_ThenReturnTheMappedValue()
    {
        int result = await Task.FromResult(Option.Some(1))
           .MapOrDefaultAsync(x => x + 1);

        result.ShouldBe(2);
    }

    [Fact]
    public async Task
        GivenNoneTask_WhenMapOrDefaultAsyncWithASyncMap_ThenReturnTheDefault()
    {
        int result = await Task.FromResult(Option.None<int>())
           .MapOrDefaultAsync(x => x + 1);

        result.ShouldBe(0);
    }

    [Fact]
    public async Task
        GivenSomeTask_WhenMapOrDefaultAsyncWithAnAsyncMap_ThenReturnTheMappedValue()
    {
        int result = await Task.FromResult(Option.Some(1))
           .MapOrDefaultAsync(x => Task.FromResult(x + 1));

        result.ShouldBe(2);
    }

    [Fact]
    public async Task
        GivenNoneTask_WhenMapOrDefaultAsyncWithAnAsyncMap_ThenReturnTheDefault()
    {
        int result = await Task.FromResult(Option.None<int>())
           .MapOrDefaultAsync(x => Task.FromResult(x + 1));

        result.ShouldBe(0);
    }

    [Fact]
    public async Task
        GivenSomeValueTask_WhenMapOrDefaultAsyncWithASyncMap_ThenReturnTheMappedValue()
    {
        int result = await new ValueTask<Option<int>>(Option.Some(1))
           .MapOrDefaultAsync(x => x + 1);

        result.ShouldBe(2);
    }

    [Fact]
    public async Task
        GivenNoneValueTask_WhenMapOrDefaultAsyncWithASyncMap_ThenReturnTheDefault()
    {
        int result = await new ValueTask<Option<int>>(Option.None<int>())
           .MapOrDefaultAsync(x => x + 1);

        result.ShouldBe(0);
    }

    [Fact]
    public async Task
        GivenSomeValueTask_WhenMapOrDefaultAsyncWithAnAsyncMap_ThenReturnTheMappedValue()
    {
        int result = await new ValueTask<Option<int>>(Option.Some(1))
           .MapOrDefaultAsync(x => Task.FromResult(x + 1));

        result.ShouldBe(2);
    }

    [Fact]
    public async Task
        GivenNoneValueTask_WhenMapOrDefaultAsyncWithAnAsyncMap_ThenReturnTheDefault()
    {
        int result = await new ValueTask<Option<int>>(Option.None<int>())
           .MapOrDefaultAsync(x => Task.FromResult(x + 1));

        result.ShouldBe(0);
    }

    [Fact]
    public async Task
        GivenNone_WhenMapOrDefaultAsyncToAReferenceType_ThenReturnNull()
    {
        Option<int> none = Option.None<int>();

        string? result = await none.MapOrDefaultAsync(
            x => Task.FromResult(x.ToString()));

        result.ShouldBeNull();
    }
}
