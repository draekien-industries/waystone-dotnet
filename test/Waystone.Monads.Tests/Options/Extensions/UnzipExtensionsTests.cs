namespace Waystone.Monads.Options.Extensions;

using System.Threading.Tasks;
using JetBrains.Annotations;
using Shouldly;
using Xunit;

[TestSubject(typeof(OptionExtensions))]
public sealed class UnzipExtensionsTests
{
    [Fact]
    public async Task GivenSomeTask_WhenUnzipAsync_ThenReturnTwoSome()
    {
        (Option<int> first, Option<int> second) =
            await Task.FromResult(Option.Some((1, 2))).UnzipAsync();

        first.ShouldBeSomeValue(1);
        second.ShouldBeSomeValue(2);
    }

    [Fact]
    public async Task GivenNoneTask_WhenUnzipAsync_ThenReturnTwoNone()
    {
        (Option<int> first, Option<int> second) =
            await Task.FromResult(Option.None<(int, int)>()).UnzipAsync();

        first.ShouldBeNone();
        second.ShouldBeNone();
    }

    [Fact]
    public async Task
        GivenSomeTaskWithDefaultComponents_WhenUnzipAsync_ThenReturnTwoSome()
    {
        (Option<int> first, Option<bool> second) =
            await Task.FromResult(Option.Some((0, false))).UnzipAsync();

        first.ShouldBeSomeValue(0);
        second.ShouldBeSomeValue(false);
    }

    [Fact]
    public async Task GivenSomeValueTask_WhenUnzipAsync_ThenReturnTwoSome()
    {
        (Option<int> first, Option<int> second) =
            await new ValueTask<Option<(int, int)>>(Option.Some((1, 2)))
               .UnzipAsync();

        first.ShouldBeSomeValue(1);
        second.ShouldBeSomeValue(2);
    }

    [Fact]
    public async Task GivenNoneValueTask_WhenUnzipAsync_ThenReturnTwoNone()
    {
        (Option<int> first, Option<int> second) =
            await new ValueTask<Option<(int, int)>>(
                Option.None<(int, int)>()).UnzipAsync();

        first.ShouldBeNone();
        second.ShouldBeNone();
    }

    [Fact]
    public async Task
        GivenSomeValueTaskWithDefaultComponents_WhenUnzipAsync_ThenReturnTwoSome()
    {
        (Option<int> first, Option<bool> second) =
            await new ValueTask<Option<(int, bool)>>(Option.Some((0, false)))
               .UnzipAsync();

        first.ShouldBeSomeValue(0);
        second.ShouldBeSomeValue(false);
    }
}
