namespace Waystone.Monads.Options.Extensions;

using JetBrains.Annotations;
using Monads.Extensions;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

[TestSubject(typeof(UnwrapOrElseExtensions))]
public sealed class UnwrapOrElseExtensionsTests
{
    [Fact]
    public async Task GivenSomeTask_WhenUnwrapOrElseAsync_ThenReturnTheValue()
    {
        int result = await Task.FromResult(Option.Some(1))
           .UnwrapOrElseAsync(() => 2);

        result.ShouldBe(1);
    }

    [Fact]
    public async Task
        GivenNoneTask_WhenUnwrapOrElseAsync_ThenReturnTheFallback()
    {
        int result = await Task.FromResult(Option.None<int>())
           .UnwrapOrElseAsync(() => 2);

        result.ShouldBe(2);
    }

    [Fact]
    public async Task
        GivenSomeValueTask_WhenUnwrapOrElseAsync_ThenReturnTheValue()
    {
        int result = await new ValueTask<Option<int>>(Option.Some(1))
           .UnwrapOrElseAsync(() => 2);

        result.ShouldBe(1);
    }

    [Fact]
    public async Task
        GivenNoneValueTask_WhenUnwrapOrElseAsync_ThenReturnTheFallback()
    {
        int result = await new ValueTask<Option<int>>(Option.None<int>())
           .UnwrapOrElseAsync(() => 2);

        result.ShouldBe(2);
    }
}
