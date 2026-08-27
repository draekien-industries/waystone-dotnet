namespace Waystone.Monads.Options.Extensions;

using JetBrains.Annotations;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

[TestSubject(typeof(MapExtensions))]
public sealed class MapExtensionsTests
{
    [Fact]
    public async Task GivenSomeTask_WhenMapAsync_ThenReturnTheMappedValue()
    {
        Option<string> result = await Task.FromResult(Option.Some(10))
           .MapAsync(async value =>
            {
                await Task.Yield();

                return "mapped" + value;
            });

        result.ShouldBeSomeValue("mapped10");
    }

    [Fact]
    public async Task GivenSomeValueTask_WhenMapAsync_ThenReturnTheMappedValue()
    {
        Option<string> result =
            await new ValueTask<Option<int>>(Option.Some(20))
               .MapAsync(async value =>
                {
                    await Task.Yield();

                    return "value" + value;
                });

        result.ShouldBeSomeValue("value20");
    }

    [Fact]
    public async Task
        GivenSomeTask_WhenMapAsyncWithASyncMap_ThenReturnTheMappedValue()
    {
        Option<string> result = await Task.FromResult(Option.Some(30))
           .MapAsync(value => "syncMapped" + value);

        result.ShouldBeSomeValue("syncMapped30");
    }

    [Fact]
    public async Task GivenNoneTask_WhenMapAsync_ThenStayNoneWithoutMapping()
    {
        Option<string> result = await Task.FromResult(Option.None<int>())
           .MapAsync(async value =>
            {
                await Task.Yield();

                return "mapped" + value;
            });

        result.ShouldBeNone();
    }

    [Fact]
    public async Task
        GivenNoneValueTask_WhenMapAsync_ThenStayNoneWithoutMapping()
    {
        Option<string> result =
            await new ValueTask<Option<int>>(Option.None<int>())
               .MapAsync(async value =>
                {
                    await Task.Yield();

                    return "mapped" + value;
                });

        result.ShouldBeNone();
    }
}
