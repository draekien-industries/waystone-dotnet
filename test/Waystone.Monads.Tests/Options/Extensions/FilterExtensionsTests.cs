namespace Waystone.Monads.Options.Extensions;

using JetBrains.Annotations;
using Shouldly;
using System;
using System.Threading.Tasks;
using Xunit;

[TestSubject(typeof(FilterExtensions))]
public sealed class FilterExtensionsTests
{
    private static Func<int, Task<bool>> AsyncPredicate(bool result) =>
        async _ =>
        {
            await Task.Yield();

            return result;
        };

    private static Func<int, bool> SyncPredicate(bool result) => _ => result;

    [Fact]
    public async Task
        GivenSomeTask_WhenFilterAsyncAndThePredicateHolds_ThenKeepTheValue()
    {
        Option<int> result = await Task.FromResult(Option.Some(10))
           .FilterAsync(AsyncPredicate(true));

        result.ShouldBeSomeValue(10);
    }

    [Fact]
    public async Task
        GivenSomeTask_WhenFilterAsyncAndThePredicateFails_ThenReturnNone()
    {
        Option<int> result = await Task.FromResult(Option.Some(20))
           .FilterAsync(AsyncPredicate(false));

        result.ShouldBeNone();
    }

    [Fact]
    public async Task GivenNoneTask_WhenFilterAsync_ThenReturnNone()
    {
        Option<int> result = await Task.FromResult(Option.None<int>())
           .FilterAsync(AsyncPredicate(true));

        result.ShouldBeNone();
    }

    [Fact]
    public async Task
        GivenSomeTask_WhenFilterAsyncWithASyncPredicateThatHolds_ThenKeepTheValue()
    {
        Option<int> result = await Task.FromResult(Option.Some(25))
           .FilterAsync(SyncPredicate(true));

        result.ShouldBeSomeValue(25);
    }

    [Fact]
    public async Task
        GivenSomeTask_WhenFilterAsyncWithASyncPredicateThatFails_ThenReturnNone()
    {
        Option<int> result = await Task.FromResult(Option.Some(25))
           .FilterAsync(SyncPredicate(false));

        result.ShouldBeNone();
    }

    [Fact]
    public async Task
        GivenNoneTask_WhenFilterAsyncWithASyncPredicate_ThenReturnNone()
    {
        Option<int> result = await Task.FromResult(Option.None<int>())
           .FilterAsync(SyncPredicate(true));

        result.ShouldBeNone();
    }

    [Fact]
    public async Task
        GivenSomeValueTask_WhenFilterAsyncAndThePredicateHolds_ThenKeepTheValue()
    {
        Option<int> result =
            await new ValueTask<Option<int>>(Option.Some(30))
               .FilterAsync(AsyncPredicate(true));

        result.ShouldBeSomeValue(30);
    }

    [Fact]
    public async Task
        GivenSomeValueTask_WhenFilterAsyncAndThePredicateFails_ThenReturnNone()
    {
        Option<int> result =
            await new ValueTask<Option<int>>(Option.Some(40))
               .FilterAsync(AsyncPredicate(false));

        result.ShouldBeNone();
    }

    [Fact]
    public async Task GivenNoneValueTask_WhenFilterAsync_ThenReturnNone()
    {
        Option<int> result =
            await new ValueTask<Option<int>>(Option.None<int>())
               .FilterAsync(AsyncPredicate(true));

        result.ShouldBeNone();
    }

    [Fact]
    public async Task
        GivenSomeValueTask_WhenFilterAsyncWithASyncPredicateThatHolds_ThenKeepTheValue()
    {
        Option<int> result =
            await new ValueTask<Option<int>>(Option.Some(50))
               .FilterAsync(SyncPredicate(true));

        result.ShouldBeSomeValue(50);
    }

    [Fact]
    public async Task
        GivenSomeValueTask_WhenFilterAsyncWithASyncPredicateThatFails_ThenReturnNone()
    {
        Option<int> result =
            await new ValueTask<Option<int>>(Option.Some(60))
               .FilterAsync(SyncPredicate(false));

        result.ShouldBeNone();
    }

    [Fact]
    public async Task
        GivenNoneValueTask_WhenFilterAsyncWithASyncPredicate_ThenReturnNone()
    {
        Option<int> result =
            await new ValueTask<Option<int>>(Option.None<int>())
               .FilterAsync(SyncPredicate(true));

        result.ShouldBeNone();
    }
}
