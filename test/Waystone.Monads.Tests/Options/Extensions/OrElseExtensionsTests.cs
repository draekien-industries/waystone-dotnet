namespace Waystone.Monads.Options.Extensions;

using JetBrains.Annotations;
using Monads.Extensions;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

[TestSubject(typeof(OptionExtensions))]
public sealed class OrElseExtensionsTests
{
    [Fact]
    public async Task GivenSomeTask_WhenOrElseAsync_ThenReturnThisOption()
    {
        Option<int> result = await Task.FromResult(Option.Some(1))
           .OrElseAsync(() => Option.Some(2));

        result.ShouldBeSomeValue(1);
    }

    [Fact]
    public async Task GivenNoneTask_WhenOrElseAsync_ThenReturnTheOtherOption()
    {
        Option<int> result = await Task.FromResult(Option.None<int>())
           .OrElseAsync(() => Option.Some(2));

        result.ShouldBeSomeValue(2);
    }

    [Fact]
    public async Task GivenSomeValueTask_WhenOrElseAsync_ThenReturnThisOption()
    {
        Option<int> result = await new ValueTask<Option<int>>(Option.Some(1))
           .OrElseAsync(() => Option.Some(2));

        result.ShouldBeSomeValue(1);
    }

    [Fact]
    public async Task
        GivenNoneValueTask_WhenOrElseAsync_ThenReturnTheOtherOption()
    {
        Option<int> result =
            await new ValueTask<Option<int>>(Option.None<int>())
               .OrElseAsync(() => Option.Some(2));

        result.ShouldBeSomeValue(2);
    }

    [Fact]
    public async Task
        GivenNoneTask_WhenOrElseAsyncWithAnAsyncFactory_ThenReturnTheOtherOption()
    {
        Option<int> result = await Task.FromResult(Option.None<int>())
           .OrElseAsync(() => new ValueTask<Option<int>>(Option.Some(2)));

        result.ShouldBeSomeValue(2);
    }

    [Fact]
    public async Task
        GivenNoneValueTask_WhenOrElseAsyncWithAnAsyncFactory_ThenReturnTheOtherOption()
    {
        Option<int> result =
            await new ValueTask<Option<int>>(Option.None<int>())
               .OrElseAsync(() => new ValueTask<Option<int>>(Option.Some(2)));

        result.ShouldBeSomeValue(2);
    }
}
