namespace Waystone.Monads.Options.Extensions;

using JetBrains.Annotations;
using Monads.Extensions;
using System.Threading.Tasks;
using Xunit;

[TestSubject(typeof(AndExtensions))]
public sealed class AndExtensionsTests
{
    [Fact]
    public async Task GivenSomeTask_WhenAndAsync_ThenReturnTheOtherOption()
    {
        Option<string> result = await Task.FromResult(Option.Some(1))
           .AndAsync(Option.Some("value"));

        result.ShouldBeSomeValue("value");
    }

    [Fact]
    public async Task GivenNoneTask_WhenAndAsync_ThenReturnNone()
    {
        Option<string> result = await Task.FromResult(Option.None<int>())
           .AndAsync(Option.Some("value"));

        result.ShouldBeNone();
    }

    [Fact]
    public async Task
        GivenSomeValueTask_WhenAndAsync_ThenReturnTheOtherOption()
    {
        Option<string> result =
            await new ValueTask<Option<int>>(Option.Some(1))
               .AndAsync(Option.Some("value"));

        result.ShouldBeSomeValue("value");
    }

    [Fact]
    public async Task GivenNoneValueTask_WhenAndAsync_ThenReturnNone()
    {
        Option<string> result =
            await new ValueTask<Option<int>>(Option.None<int>())
               .AndAsync(Option.Some("value"));

        result.ShouldBeNone();
    }
}
