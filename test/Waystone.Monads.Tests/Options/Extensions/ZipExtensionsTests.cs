namespace Waystone.Monads.Options.Extensions;

using JetBrains.Annotations;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

[TestSubject(typeof(OptionExtensions))]
public sealed class ZipExtensionsTests
{
    [Fact]
    public async Task GivenSomeTask_WhenZipAsync_ThenReturnSomeTuple()
    {
        Option<(int, string)> result = await Task.FromResult(Option.Some(1))
                                                 .ZipAsync(Option.Some("value"));

        result.ShouldBeSomeValue((1, "value"));
    }

    [Fact]
    public async Task GivenNoneTask_WhenZipAsync_ThenReturnNone()
    {
        Option<(int, string)> result =
            await Task.FromResult(Option.None<int>())
                      .ZipAsync(Option.Some("value"));

        result.ShouldBeNone();
    }

    [Fact]
    public async Task GivenSomeValueTask_WhenZipAsync_ThenReturnSomeTuple()
    {
        Option<(int, string)> result =
            await new ValueTask<Option<int>>(Option.Some(1))
               .ZipAsync(Option.Some("value"));

        result.ShouldBeSomeValue((1, "value"));
    }

    [Fact]
    public async Task
        GivenSomeValueTaskAndNone_WhenZipAsync_ThenReturnNone()
    {
        Option<(int, string)> result =
            await new ValueTask<Option<int>>(Option.Some(1))
               .ZipAsync(Option.None<string>());

        result.ShouldBeNone();
    }
}
