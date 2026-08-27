namespace Waystone.Monads.Options.Extensions;

using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Shouldly;
using Xunit;

[TestSubject(typeof(OptionExtensions))]
public sealed class AsEnumerableExtensionsTests
{
    [Fact]
    public async Task GivenSomeTask_WhenAsEnumerableAsync_ThenYieldTheValue()
    {
        IEnumerable<int> result = await Task.FromResult(Option.Some(1))
           .AsEnumerableAsync();

        result.ShouldBe(new[] { 1 });
    }

    [Fact]
    public async Task GivenNoneTask_WhenAsEnumerableAsync_ThenYieldNothing()
    {
        IEnumerable<int> result = await Task.FromResult(Option.None<int>())
           .AsEnumerableAsync();

        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task
        GivenSomeValueTask_WhenAsEnumerableAsync_ThenYieldTheValue()
    {
        IEnumerable<int> result =
            await new ValueTask<Option<int>>(Option.Some(1))
               .AsEnumerableAsync();

        result.ShouldBe(new[] { 1 });
    }

    [Fact]
    public async Task
        GivenNoneValueTask_WhenAsEnumerableAsync_ThenYieldNothing()
    {
        IEnumerable<int> result =
            await new ValueTask<Option<int>>(Option.None<int>())
               .AsEnumerableAsync();

        result.ShouldBeEmpty();
    }
}
