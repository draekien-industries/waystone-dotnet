namespace Waystone.Monads.Options.Extensions;

using JetBrains.Annotations;
using Shouldly;
using System;
using System.Threading.Tasks;
using Xunit;

[TestSubject(typeof(OptionExtensions))]
public sealed class IsNoneOrExtensionsTests
{
    private static Func<int, Task<bool>> AsyncPredicate(bool result) =>
        async _ =>
        {
            await Task.Yield();

            return result;
        };

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task
        GivenSomeTask_WhenIsNoneOrAsync_ThenReturnThePredicateResult(
            bool predicate)
    {
        bool result = await Task.FromResult(Option.Some(55))
           .IsNoneOrAsync(AsyncPredicate(predicate));

        result.ShouldBe(predicate);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task
        GivenNoneTask_WhenIsNoneOrAsync_ThenReturnTrueWhateverThePredicateSays(
            bool predicate)
    {
        bool result = await Task.FromResult(Option.None<int>())
           .IsNoneOrAsync(AsyncPredicate(predicate));

        result.ShouldBeTrue();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task
        GivenSomeValueTask_WhenIsNoneOrAsync_ThenReturnThePredicateResult(
            bool predicate)
    {
        bool result = await new ValueTask<Option<int>>(Option.Some(75))
           .IsNoneOrAsync(AsyncPredicate(predicate));

        result.ShouldBe(predicate);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task
        GivenNoneValueTask_WhenIsNoneOrAsync_ThenReturnTrueWhateverThePredicateSays(
            bool predicate)
    {
        bool result = await new ValueTask<Option<int>>(Option.None<int>())
           .IsNoneOrAsync(AsyncPredicate(predicate));

        result.ShouldBeTrue();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task
        GivenSomeTask_WhenIsNoneOrAsyncWithASyncPredicate_ThenReturnItsResult(
            bool predicate)
    {
        bool result = await Task.FromResult(Option.Some(85))
           .IsNoneOrAsync(_ => predicate);

        result.ShouldBe(predicate);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task
        GivenNoneTask_WhenIsNoneOrAsyncWithASyncPredicate_ThenReturnTrue(
            bool predicate)
    {
        bool result = await Task.FromResult(Option.None<int>())
           .IsNoneOrAsync(_ => predicate);

        result.ShouldBeTrue();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task
        GivenSomeValueTask_WhenIsNoneOrAsyncWithASyncPredicate_ThenReturnItsResult(
            bool predicate)
    {
        bool result = await new ValueTask<Option<int>>(Option.Some(95))
           .IsNoneOrAsync(_ => predicate);

        result.ShouldBe(predicate);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task
        GivenNoneValueTask_WhenIsNoneOrAsyncWithASyncPredicate_ThenReturnTrue(
            bool predicate)
    {
        bool result = await new ValueTask<Option<int>>(Option.None<int>())
           .IsNoneOrAsync(_ => predicate);

        result.ShouldBeTrue();
    }
}
