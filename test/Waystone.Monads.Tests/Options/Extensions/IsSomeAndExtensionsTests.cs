namespace Waystone.Monads.Options.Extensions;

using JetBrains.Annotations;
using Shouldly;
using System;
using System.Threading.Tasks;
using Xunit;

[TestSubject(typeof(OptionExtensions))]
public sealed class IsSomeAndExtensionsTests
{
    private static Func<int, Task<bool>> AsyncPredicate(bool result) =>
        async _ =>
        {
            await Task.Yield();

            return result;
        };

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

    [Theory]
    [InlineData(15, true)]
    [InlineData(25, false)]
    public async Task
        GivenSomeTask_WhenIsSomeAndAsync_ThenReturnThePredicateResult(
            int value,
            bool predicate)
    {
        bool result = await Task.FromResult(Option.Some(value))
           .IsSomeAndAsync(AsyncPredicate(predicate));

        result.ShouldBe(predicate);
    }

    [Theory]
    [InlineData(35, true)]
    [InlineData(45, false)]
    public async Task
        GivenSomeValueTask_WhenIsSomeAndAsync_ThenReturnThePredicateResult(
            int value,
            bool predicate)
    {
        bool result = await new ValueTask<Option<int>>(Option.Some(value))
           .IsSomeAndAsync(AsyncPredicate(predicate));

        result.ShouldBe(predicate);
    }

    [Fact]
    public async Task
        GivenNoneTask_WhenIsSomeAndAsync_ThenReturnFalseWithoutCallingThePredicate()
    {
        bool result = await Task.FromResult(Option.None<int>())
           .IsSomeAndAsync(AsyncPredicate(true));

        result.ShouldBeFalse();
    }

    [Fact]
    public async Task
        GivenNoneValueTask_WhenIsSomeAndAsync_ThenReturnFalseWithoutCallingThePredicate()
    {
        bool result = await new ValueTask<Option<int>>(Option.None<int>())
           .IsSomeAndAsync(AsyncPredicate(true));

        result.ShouldBeFalse();
    }
}
