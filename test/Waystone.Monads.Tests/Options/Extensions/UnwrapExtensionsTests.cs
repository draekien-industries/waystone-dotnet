namespace Waystone.Monads.Options.Extensions;

using JetBrains.Annotations;
using Shouldly;
using System.Threading.Tasks;
using Waystone.Monads.Exceptions;
using Xunit;

[TestSubject(typeof(OptionExtensions))]
public sealed class UnwrapExtensionsTests
{
    [Fact]
    public async Task GivenSomeTask_WhenUnwrapAsync_ThenReturnTheValue()
    {
        int result = await Task.FromResult(Option.Some(10)).UnwrapAsync();

        result.ShouldBe(10);
    }

    [Fact]
    public async Task GivenSomeValueTask_WhenUnwrapAsync_ThenReturnTheValue()
    {
        int result = await new ValueTask<Option<int>>(Option.Some(20))
           .UnwrapAsync();

        result.ShouldBe(20);
    }

    [Fact]
    public async Task GivenNoneTask_WhenUnwrapAsync_ThenThrow()
    {
        await Should.ThrowAsync<UnwrapException>(
            async () =>
                await Task.FromResult(Option.None<int>()).UnwrapAsync());
    }

    [Fact]
    public async Task GivenNoneValueTask_WhenUnwrapAsync_ThenThrow()
    {
        await Should.ThrowAsync<UnwrapException>(
            async () => await new ValueTask<Option<int>>(Option.None<int>())
               .UnwrapAsync());
    }

    [Fact]
    public async Task GivenSomeTask_WhenUnwrapOrAsync_ThenIgnoreTheDefault()
    {
        int result =
            await Task.FromResult(Option.Some(10)).UnwrapOrAsync(99);

        result.ShouldBe(10);
    }

    [Fact]
    public async Task
        GivenSomeValueTask_WhenUnwrapOrAsync_ThenIgnoreTheDefault()
    {
        int result = await new ValueTask<Option<int>>(Option.Some(30))
           .UnwrapOrAsync(99);

        result.ShouldBe(30);
    }

    [Fact]
    public async Task GivenNoneTask_WhenUnwrapOrAsync_ThenReturnTheDefault()
    {
        int result =
            await Task.FromResult(Option.None<int>()).UnwrapOrAsync(99);

        result.ShouldBe(99);
    }

    [Fact]
    public async Task
        GivenNoneValueTask_WhenUnwrapOrAsync_ThenReturnTheDefault()
    {
        int result = await new ValueTask<Option<int>>(Option.None<int>())
           .UnwrapOrAsync(99);

        result.ShouldBe(99);
    }

    [Fact]
    public async Task
        GivenSomeTask_WhenUnwrapOrDefaultAsync_ThenReturnTheValue()
    {
        int result = await Task.FromResult(Option.Some(10))
           .UnwrapOrDefaultAsync();

        result.ShouldBe(10);
    }

    [Fact]
    public async Task
        GivenSomeValueTask_WhenUnwrapOrDefaultAsync_ThenReturnTheValue()
    {
        int result = await new ValueTask<Option<int>>(Option.Some(40))
           .UnwrapOrDefaultAsync();

        result.ShouldBe(40);
    }

    [Fact]
    public async Task
        GivenNoneTask_WhenUnwrapOrDefaultAsync_ThenReturnTheTypeDefault()
    {
        int result = await Task.FromResult(Option.None<int>())
           .UnwrapOrDefaultAsync();

        result.ShouldBe(0);
    }

    [Fact]
    public async Task
        GivenNoneValueTask_WhenUnwrapOrDefaultAsync_ThenReturnTheTypeDefault()
    {
        int result = await new ValueTask<Option<int>>(Option.None<int>())
           .UnwrapOrDefaultAsync();

        result.ShouldBe(0);
    }
}
