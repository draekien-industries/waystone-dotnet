namespace Waystone.Monads.Options.Extensions;

using JetBrains.Annotations;
using Monads.Extensions;
using System.Threading.Tasks;
using Xunit;

[TestSubject(typeof(UnzipExtensions))]
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
}
