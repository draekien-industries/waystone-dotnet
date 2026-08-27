namespace Waystone.Monads.Options.Extensions;

using JetBrains.Annotations;
using Monads.Extensions;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

[TestSubject(typeof(ReduceExtensions))]
public sealed class ReduceExtensionsTests
{
    [Fact]
    public async Task GivenTwoSome_WhenReduceAsync_ThenCombineBothValues()
    {
        Option<int> some = Option.Some(1);

        Option<int> result = await some.ReduceAsync(
            Option.Some(2),
            (x, y) => Task.FromResult(x + y));

        result.ShouldBeSomeValue(3);
    }

    [Fact]
    public async Task GivenReduceProducesNull_WhenReduceAsync_ThenReturnNone()
    {
        Option<string> some = Option.Some("a");

        Option<string> result = await some.ReduceAsync(
            Option.Some("b"),
            (_, _) => Task.FromResult(default(string)!));

        result.ShouldBeNone();
    }

    [Fact]
    public async Task GivenOtherIsNone_WhenReduceAsync_ThenReturnThisOption()
    {
        Option<int> some = Option.Some(1);

        Option<int> result = await some.ReduceAsync(
            Option.None<int>(),
            (x, y) => Task.FromResult(x + y));

        result.ShouldBeSomeValue(1);
    }

    [Fact]
    public async Task GivenThisIsNone_WhenReduceAsync_ThenReturnTheOtherOption()
    {
        Option<int> none = Option.None<int>();

        Option<int> result = await none.ReduceAsync(
            Option.Some(2),
            (x, y) => Task.FromResult(x + y));

        result.ShouldBeSomeValue(2);
    }

    [Fact]
    public async Task GivenBothAreNone_WhenReduceAsync_ThenReturnNone()
    {
        Option<int> none = Option.None<int>();

        Option<int> result = await none.ReduceAsync(
            Option.None<int>(),
            (x, y) => Task.FromResult(x + y));

        result.ShouldBeNone();
    }

    [Fact]
    public async Task
        GivenSomeTask_WhenReduceAsyncWithASyncReduce_ThenCombineBothValues()
    {
        Option<int> result = await Task.FromResult(Option.Some(1))
           .ReduceAsync(Option.Some(2), (x, y) => x + y);

        result.ShouldBeSomeValue(3);
    }

    [Fact]
    public async Task
        GivenSomeTask_WhenReduceAsyncWithAnAsyncReduce_ThenCombineBothValues()
    {
        Option<int> result = await Task.FromResult(Option.Some(1))
           .ReduceAsync(Option.Some(2), (x, y) => Task.FromResult(x + y));

        result.ShouldBeSomeValue(3);
    }

    [Fact]
    public async Task GivenNoneTask_WhenReduceAsync_ThenReturnTheOtherOption()
    {
        Option<int> result = await Task.FromResult(Option.None<int>())
           .ReduceAsync(Option.Some(2), (x, y) => x + y);

        result.ShouldBeSomeValue(2);
    }

    [Fact]
    public async Task
        GivenSomeValueTask_WhenReduceAsyncWithASyncReduce_ThenCombineBothValues()
    {
        Option<int> result = await new ValueTask<Option<int>>(Option.Some(1))
           .ReduceAsync(Option.Some(2), (x, y) => x + y);

        result.ShouldBeSomeValue(3);
    }

    [Fact]
    public async Task
        GivenSomeValueTask_WhenReduceAsyncWithAnAsyncReduce_ThenCombineBothValues()
    {
        Option<int> result = await new ValueTask<Option<int>>(Option.Some(1))
           .ReduceAsync(Option.Some(2), (x, y) => Task.FromResult(x + y));

        result.ShouldBeSomeValue(3);
    }

    [Fact]
    public async Task
        GivenNoneValueTask_WhenReduceAsync_ThenReturnTheOtherOption()
    {
        Option<int> result =
            await new ValueTask<Option<int>>(Option.None<int>())
               .ReduceAsync(Option.Some(2), (x, y) => x + y);

        result.ShouldBeSomeValue(2);
    }
}
