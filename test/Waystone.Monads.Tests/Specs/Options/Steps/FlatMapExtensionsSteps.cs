namespace Waystone.Monads.Specs.Options.Steps;

using JetBrains.Annotations;
using Reqnroll;
using System.Threading.Tasks;
using System;
using Waystone.Monads.Exceptions;
using Waystone.Monads.Options.Extensions;
using Waystone.Monads.Options;

[Binding, TestSubject(typeof(FlatMapExtensions))]
public class FlatMapExtensionsSteps(ScenarioContext context)
{
    [Given(
        "an async map function that returns a Some with value multiplied by {int}")]
    public void GivenAnAsyncMapFunctionThatReturnsASomeWithValueMultipliedByInt(
        int value)
    {
        context.Set<Func<int, Task<Option<int>>>>(
            async x =>
            {
                await Task.Yield();

                return Option.Some(x * value);
            },
            Constants.MapKey);
    }

    [When("invoking async FlatMap on Option Task")]
    public async Task WhenInvokingAsyncFlatMapOnOptionTask()
    {
        var option = context.Get<Task<Option<int>>>();
        var map = context.Get<Func<int, Task<Option<int>>>>(Constants.MapKey);

        Option<int> result =
            await option.FlatMapAsync(map).ConfigureAwait(false);

        context.Set(result, Constants.ResultKey);
    }

    [When("invoking async FlatMap on Option")]
    public async Task WhenInvokingAsyncFlatMapOnOption()
    {
        var option = context.Get<Option<int>>();
        var map = context.Get<Func<int, Task<Option<int>>>>(Constants.MapKey);

        Option<int> result =
            await option.FlatMapAsync(map).ConfigureAwait(false);

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
        int value)
    {
        context.Set<Func<int, Option<int>>>(
            x => Option.Some(x * value),
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
            await option.FlatMapAsync(map).ConfigureAwait(false);

        context.Set(result, Constants.ResultKey);
    }

    [When("invoking sync FlatMap on Option Task")]
    public async Task WhenInvokingSyncFlatMapOnOptionTask()
    {
        var option = context.Get<Task<Option<int>>>();
        var map = context.Get<Func<int, Option<int>>>(Constants.MapKey);

        Option<int> result =
            await option.FlatMapAsync(map).ConfigureAwait(false);

        context.Set(result, Constants.ResultKey);
    }

    [When("invoking sync FlatMap on Option ValueTask")]
    public async Task WhenInvokingSyncFlatMapOnOptionValueTask()
    {
        var option = context.Get<ValueTask<Option<int>>>();
        var map = context.Get<Func<int, Option<int>>>(Constants.MapKey);

        Option<int> result =
            await option.FlatMapAsync(map).ConfigureAwait(false);

        context.Set(result, Constants.ResultKey);
    }
}
