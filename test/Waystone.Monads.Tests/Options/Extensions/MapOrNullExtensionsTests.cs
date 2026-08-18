namespace Waystone.Monads.Options.Extensions;

using JetBrains.Annotations;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

[TestSubject(typeof(MapOrNullExtensions))]
public sealed class MapOrNullExtensionsTests
{
    [Fact]
    public void GivenSome_WhenMapOrNull_ThenReturnTheMappedValue() =>
        Option.Some(1).MapOrNull(x => x + 1).ShouldBe(2);

    [Fact]
    public void GivenNone_WhenMapOrNull_ThenReturnNull() =>
        Option.None<int>().MapOrNull(x => x + 1).ShouldBeNull();

    [Fact]
    public void GivenSome_WhenMapOrNullToTheDefault_ThenReturnTheDefault() =>
        Option.Some(1).MapOrNull(_ => 0).ShouldBe(0);

    [Fact]
    public async Task GivenSome_WhenMapOrNullAsync_ThenReturnTheMappedValue()
    {
        int? value = await Option.Some(1)
           .MapOrNullAsync(x => Task.FromResult(x + 1));

        value.ShouldBe(2);
    }

    [Fact]
    public async Task GivenNone_WhenMapOrNullAsync_ThenReturnNull()
    {
        int? value = await Option.None<int>()
           .MapOrNullAsync(x => Task.FromResult(x + 1));

        value.ShouldBeNull();
    }

    [Fact]
    public async Task
        GivenSomeTask_WhenMapOrNullAsyncWithASyncMap_ThenReturnTheMappedValue()
    {
        int? value = await Task.FromResult(Option.Some(1))
           .MapOrNullAsync(x => x + 1);

        value.ShouldBe(2);
    }

    [Fact]
    public async Task
        GivenNoneTask_WhenMapOrNullAsyncWithASyncMap_ThenReturnNull()
    {
        int? value = await Task.FromResult(Option.None<int>())
           .MapOrNullAsync(x => x + 1);

        value.ShouldBeNull();
    }

    [Fact]
    public async Task
        GivenSomeTask_WhenMapOrNullAsyncWithAnAsyncMap_ThenReturnTheMappedValue()
    {
        int? value = await Task.FromResult(Option.Some(1))
           .MapOrNullAsync(x => Task.FromResult(x + 1));

        value.ShouldBe(2);
    }

    [Fact]
    public async Task
        GivenNoneTask_WhenMapOrNullAsyncWithAnAsyncMap_ThenReturnNull()
    {
        int? value = await Task.FromResult(Option.None<int>())
           .MapOrNullAsync(x => Task.FromResult(x + 1));

        value.ShouldBeNull();
    }

    [Fact]
    public async Task
        GivenSomeValueTask_WhenMapOrNullAsyncWithASyncMap_ThenReturnTheMappedValue()
    {
        int? value = await new ValueTask<Option<int>>(Option.Some(1))
           .MapOrNullAsync(x => x + 1);

        value.ShouldBe(2);
    }

    [Fact]
    public async Task
        GivenNoneValueTask_WhenMapOrNullAsyncWithASyncMap_ThenReturnNull()
    {
        int? value = await new ValueTask<Option<int>>(Option.None<int>())
           .MapOrNullAsync(x => x + 1);

        value.ShouldBeNull();
    }

    [Fact]
    public async Task
        GivenSomeValueTask_WhenMapOrNullAsyncWithAnAsyncMap_ThenReturnTheMappedValue()
    {
        int? value = await new ValueTask<Option<int>>(Option.Some(1))
           .MapOrNullAsync(x => Task.FromResult(x + 1));

        value.ShouldBe(2);
    }

    [Fact]
    public async Task
        GivenNoneValueTask_WhenMapOrNullAsyncWithAnAsyncMap_ThenReturnNull()
    {
        int? value = await new ValueTask<Option<int>>(Option.None<int>())
           .MapOrNullAsync(x => Task.FromResult(x + 1));

        value.ShouldBeNull();
    }
}
