namespace Waystone.Monads.Options.Steps;

using System;
using System.Threading.Tasks;
using Extensions;
using JetBrains.Annotations;
using Reqnroll;

[Binding, TestSubject(typeof(FlatMapExtensions))]
public class FlatMapExtensionsSteps(ScenarioContext context)
{
    [Given(
        "an async map function that returns a Some with value multiplied by {int}")]
    public void GivenAnAsyncMapFunctionThatReturnsASomeWithValueMultipliedByInt(
        int p0)
    {
        context.Set<Func<int, Task<Option<int>>>>(
            async x =>
            {
                await Task.Yield();

                return Option.Some(x * p0);
            },
            Constants.MapKey);
    }

    [When("invoking async FlatMap on Option Task")]
    public async Task WhenInvokingAsyncFlatMapOnOptionTask()
    {
        var option = context.Get<Task<Option<int>>>();
        var map = context.Get<Func<int, Task<Option<int>>>>(Constants.MapKey);

        Option<int> result =
            await option.FlatMap(map).ConfigureAwait(false);

        context.Set(result, Constants.ResultKey);
    }

    [When("invoking async FlatMap on Option")]
    public async Task WhenInvokingAsyncFlatMapOnOption()
    {
        var option = context.Get<Option<int>>();
        var map = context.Get<Func<int, Task<Option<int>>>>(Constants.MapKey);

        Option<int> result =
            await option.FlatMap(map).ConfigureAwait(false);

        context.Set(result, Constants.ResultKey);
    }

    [Given("an async map function that returns a None")]
    public void GivenAnAsyncMapFunctionThatReturnsANone()
    {
        context.Set<Func<int, Task<Option<int>>>>(
            async _ =>
            {
                await Task.Yield();

                return Option.None<int>();
            },
            Constants.MapKey);
    }

    [Given(
        "a sync map function that returns a Some with value multiplied by {int}")]
    public void GivenASyncMapFunctionThatReturnsASomeWithValueMultipliedByInt(
        int p0)
    {
        context.Set<Func<int, Option<int>>>(
            x => Option.Some(x * p0),
            Constants.MapKey);
    }

    [Given("a sync map function that returns a None")]
    public void GivenASyncMapFunctionThatReturnsANone()
    {
        context.Set<Func<int, Option<int>>>(
            _ => Option.None<int>(),
            Constants.MapKey);
    }

    [When("invoking async FlatMap on Option ValueTask")]
    public async Task WhenInvokingAsyncFlatMapOnOptionValueTask()
    {
        var option = context.Get<ValueTask<Option<int>>>();
        var map = context.Get<Func<int, Task<Option<int>>>>(Constants.MapKey);

        Option<int> result =
            await option.FlatMap(map).ConfigureAwait(false);

        context.Set(result, Constants.ResultKey);
    }

    [When("invoking sync FlatMap on Option Task")]
    public async Task WhenInvokingSyncFlatMapOnOptionTask()
    {
        var option = context.Get<Task<Option<int>>>();
        var map = context.Get<Func<int, Option<int>>>(Constants.MapKey);

        Option<int> result =
            await option.FlatMap(map).ConfigureAwait(false);

        context.Set(result, Constants.ResultKey);
    }

    [When("invoking sync FlatMap on Option ValueTask")]
    public async Task WhenInvokingSyncFlatMapOnOptionValueTask()
    {
        var option = context.Get<ValueTask<Option<int>>>();
        var map = context.Get<Func<int, Option<int>>>(Constants.MapKey);

        Option<int> result =
            await option.FlatMap(map).ConfigureAwait(false);

        context.Set(result, Constants.ResultKey);
    }
}
