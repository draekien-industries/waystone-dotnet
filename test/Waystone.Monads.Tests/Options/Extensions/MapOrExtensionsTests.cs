namespace Waystone.Monads.Options.Extensions;

using JetBrains.Annotations;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

[TestSubject(typeof(MapOrExtensions))]
public sealed class MapOrExtensionsTests
{
    private const string Fallback = "defaultValue";

    [Fact]
    public async Task GivenSomeTask_WhenMapOrAsync_ThenReturnTheMappedValue()
    {
        string result = await Task.FromResult(Option.Some(10))
           .MapOrAsync(Fallback, value => Task.FromResult("mapped" + value));

        result.ShouldBe("mapped10");
    }

    [Fact]
    public async Task GivenNoneTask_WhenMapOrAsync_ThenReturnTheFallback()
    {
        string result = await Task.FromResult(Option.None<int>())
           .MapOrAsync(Fallback, value => Task.FromResult("mapped" + value));

        result.ShouldBe(Fallback);
    }

    [Fact]
    public async Task
        GivenSomeValueTask_WhenMapOrAsync_ThenReturnTheMappedValue()
    {
        string result = await new ValueTask<Option<int>>(Option.Some(20))
           .MapOrAsync(Fallback, value => Task.FromResult("value" + value));

        result.ShouldBe("value20");
    }

    [Fact]
    public async Task GivenNoneValueTask_WhenMapOrAsync_ThenReturnTheFallback()
    {
        string result = await new ValueTask<Option<int>>(Option.None<int>())
           .MapOrAsync(Fallback, value => Task.FromResult("value" + value));

        result.ShouldBe(Fallback);
    }

    [Fact]
    public async Task
        GivenSomeTask_WhenMapOrAsyncWithASyncMap_ThenReturnTheMappedValue()
    {
        string result = await Task.FromResult(Option.Some(30))
           .MapOrAsync(Fallback, value => "syncMapped" + value);

        result.ShouldBe("syncMapped30");
    }
}
