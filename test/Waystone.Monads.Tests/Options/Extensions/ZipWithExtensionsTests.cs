namespace Waystone.Monads.Options.Extensions;

using System.Threading.Tasks;
using JetBrains.Annotations;
using Shouldly;
using Xunit;

/// <remarks>
/// The overloads taking the other option inside a task had no tests at all until
/// DRA-110 renamed their parameters and the coverage gate found them.
/// </remarks>
[TestSubject(typeof(OptionExtensions))]
public sealed class ZipWithExtensionsTests
{
    [Fact]
    public async Task GivenTwoSomeTasks_WhenZipWithAsync_ThenZipTheValues()
    {
        Option<int> result = await Task.FromResult(Option.Some(2))
           .ZipWithAsync(
                Task.FromResult(Option.Some(3)),
                (left, right) => Task.FromResult(left * right));

        result.ShouldBeSomeValue(6);
    }

    [Fact]
    public async Task GivenTheOtherTaskIsNone_WhenZipWithAsync_ThenReturnNone()
    {
        Option<int> result = await Task.FromResult(Option.Some(2))
           .ZipWithAsync(
                Task.FromResult(Option.None<int>()),
                (left, right) => Task.FromResult(left * right));

        result.ShouldBeNone();
    }

    [Fact]
    public async Task
        GivenSomeValueTaskAndSomeOption_WhenZipWithAsync_ThenZipTheValues()
    {
        Option<int> result = await new ValueTask<Option<int>>(Option.Some(2))
           .ZipWithAsync(
                Option.Some(3),
                (left, right) => Task.FromResult(left * right));

        result.ShouldBeSomeValue(6);
    }

    [Fact]
    public async Task
        GivenNoneValueTaskAndSomeOption_WhenZipWithAsync_ThenReturnNone()
    {
        Option<int> result =
            await new ValueTask<Option<int>>(Option.None<int>())
               .ZipWithAsync(
                    Option.Some(3),
                    (left, right) => Task.FromResult(left * right));

        result.ShouldBeNone();
    }

    [Fact]
    public async Task GivenTwoSomeValueTasks_WhenZipWithAsync_ThenZipTheValues()
    {
        Option<int> result = await new ValueTask<Option<int>>(Option.Some(2))
           .ZipWithAsync(
                new ValueTask<Option<int>>(Option.Some(3)),
                (left, right) => Task.FromResult(left * right));

        result.ShouldBeSomeValue(6);
    }

    [Fact]
    public async Task
        GivenTheOtherValueTaskIsNone_WhenZipWithAsync_ThenReturnNone()
    {
        Option<int> result = await new ValueTask<Option<int>>(Option.Some(2))
           .ZipWithAsync(
                new ValueTask<Option<int>>(Option.None<int>()),
                (left, right) => Task.FromResult(left * right));

        result.ShouldBeNone();
    }

    [Fact]
    public async Task
        GivenASomeTask_AndState_WhenZipWithAsync_ThenCombineWithTheState()
    {
        Option<int> result = await Task.FromResult(Option.Some(2))
           .ZipWithAsync(
                10,
                Option.Some(3),
                static (left, right, state) => left + right + state);

        result.ShouldBeSomeValue(15);
    }

    [Fact]
    public async Task
        GivenANoneTask_AndState_WhenZipWithAsync_ThenReturnNone()
    {
        Option<int> result = await Task.FromResult(Option.None<int>())
           .ZipWithAsync(
                10,
                Option.Some(3),
                static (left, right, state) => left + right + state);

        result.ShouldBeNone();
    }

    [Fact]
    public async Task
        GivenASomeValueTask_AndState_WhenZipWithAsync_ThenCombineWithTheState()
    {
        Option<int> result = await new ValueTask<Option<int>>(Option.Some(2))
           .ZipWithAsync(
                10,
                Option.Some(3),
                static (left, right, state) => left + right + state);

        result.ShouldBeSomeValue(15);
    }

    [Fact]
    public async Task
        GivenTheOtherIsNone_AndState_WhenZipWithAsync_ThenReturnNone()
    {
        Option<int> result = await new ValueTask<Option<int>>(Option.Some(2))
           .ZipWithAsync(
                10,
                Option.None<int>(),
                static (left, right, state) => left + right + state);

        result.ShouldBeNone();
    }
}
