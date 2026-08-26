namespace Waystone.Monads.Options.Extensions;

using JetBrains.Annotations;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

[TestSubject(typeof(IsSomeAndExtensions))]
public sealed class IsSomeAndExtensionsTests
{
    [Fact]
    public async Task
        GivenSomeValueTask_WhenIsSomeAndAsyncWithASyncPredicate_ThenEvaluateIt()
    {
        bool result = await new ValueTask<Option<int>>(Option.Some(2))
           .IsSomeAndAsync(value => value > 1);

        result.ShouldBeTrue();
    }

    [Fact]
    public async Task
        GivenNoneValueTask_WhenIsSomeAndAsyncWithASyncPredicate_ThenReturnFalse()
    {
        bool result = await new ValueTask<Option<int>>(Option.None<int>())
           .IsSomeAndAsync(value => value > 1);

        result.ShouldBeFalse();
    }
}
