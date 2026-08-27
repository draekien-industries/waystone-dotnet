namespace Waystone.Monads.Options.Extensions;

using JetBrains.Annotations;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

[TestSubject(typeof(FlattenExtensions))]
public sealed class FlattenExtensionsTests
{
    [Fact]
    public async Task GivenATaskOfSomeSome_WhenFlattenAsync_ThenReturnTheInner()
    {
        Option<int> result =
            await Task.FromResult(Option.Some(Option.Some(10)))
               .FlattenAsync();

        result.ShouldBeSomeValue(10);
    }

    [Fact]
    public async Task GivenATaskOfSomeNone_WhenFlattenAsync_ThenReturnNone()
    {
        Option<int> result =
            await Task.FromResult(Option.Some(Option.None<int>()))
               .FlattenAsync();

        result.ShouldBeNone();
    }

    [Fact]
    public async Task
        GivenAValueTaskOfSomeSome_WhenFlattenAsync_ThenReturnTheInner()
    {
        Option<int> result =
            await new ValueTask<Option<Option<int>>>(
                Option.Some(Option.Some(10))).FlattenAsync();

        result.ShouldBeSomeValue(10);
    }

    [Fact]
    public async Task
        GivenAValueTaskOfSomeNone_WhenFlattenAsync_ThenReturnNone()
    {
        Option<int> result =
            await new ValueTask<Option<Option<int>>>(
                Option.Some(Option.None<int>())).FlattenAsync();

        result.ShouldBeNone();
    }

    [Fact]
    public async Task GivenATaskOfNone_WhenFlattenAsync_ThenReturnNone()
    {
        Option<int> result =
            await Task.FromResult(Option.None<Option<int>>()).FlattenAsync();

        result.ShouldBeNone();
    }

    [Fact]
    public async Task GivenAValueTaskOfNone_WhenFlattenAsync_ThenReturnNone()
    {
        Option<int> result =
            await new ValueTask<Option<Option<int>>>(
                Option.None<Option<int>>()).FlattenAsync();

        result.ShouldBeNone();
    }
}
