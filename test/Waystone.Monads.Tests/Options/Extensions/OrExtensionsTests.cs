namespace Waystone.Monads.Options.Extensions;

using System.Threading.Tasks;
using JetBrains.Annotations;
using Shouldly;
using Xunit;

[TestSubject(typeof(OptionExtensions))]
public sealed class OrExtensionsTests
{
    [Fact]
    public async Task GivenSomeTask_WhenOrAsync_ThenKeepTheSome()
    {
        Option<int> result = await Task.FromResult(Option.Some(1))
                                       .OrAsync(Option.Some(2));

        result.ShouldBeSomeValue(1);
    }

    [Fact]
    public async Task GivenNoneTask_WhenOrAsync_ThenReturnTheOtherOption()
    {
        Option<int> result = await Task.FromResult(Option.None<int>())
                                       .OrAsync(Option.Some(2));

        result.ShouldBeSomeValue(2);
    }

    [Fact]
    public async Task GivenSomeValueTask_WhenOrAsync_ThenKeepTheSome()
    {
        Option<int> result =
            await new ValueTask<Option<int>>(Option.Some(1))
               .OrAsync(Option.Some(2));

        result.ShouldBeSomeValue(1);
    }

    [Fact]
    public async Task GivenNoneValueTask_WhenOrAsync_ThenReturnNone()
    {
        Option<int> result =
            await new ValueTask<Option<int>>(Option.None<int>())
               .OrAsync(Option.None<int>());

        result.ShouldBeNone();
    }
}
