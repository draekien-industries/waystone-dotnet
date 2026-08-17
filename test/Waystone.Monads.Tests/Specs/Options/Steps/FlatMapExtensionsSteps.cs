namespace Waystone.Monads.Specs.Options.Steps;

using JetBrains.Annotations;
using Reqnroll;
using System.Threading.Tasks;
using System;
using Waystone.Monads.Exceptions;
using Waystone.Monads.Options.Extensions;
using Waystone.Monads.Options;

[Binding, TestSubject(typeof(FlatMapExtensions))]
public class FlatMapExtensionsSteps(SpecContext context)
{
    [Given(
        "an async map function that returns a Some with value multiplied by {int}")]
    public void GivenAnAsyncMapFunctionThatReturnsASomeWithValueMultipliedByInt(
        int value)
    {
        context.SetSlot<Func<int, Task<Option<int>>>>(async x =>
            {
                await Task.Yield();

                return Option.Some(x * value);
            }, SpecContext.MapSlot);
    }

    [When("invoking async FlatMap on Option Task")]
    public async Task WhenInvokingAsyncFlatMapOnOptionTask()
    {
        var option = context.Subject<Task<Option<int>>>();
        var map = context.Slot<Func<int, Task<Option<int>>>>(SpecContext.MapSlot);

        Option<int> result =
            await option.FlatMapAsync(map).ConfigureAwait(false);

        context.SetOutcome(result);
    }

    [When("invoking async FlatMap on Option")]
    public async Task WhenInvokingAsyncFlatMapOnOption()
    {
        var option = context.Subject<Option<int>>();
        var map = context.Slot<Func<int, Task<Option<int>>>>(SpecContext.MapSlot);

        Option<int> result =
            await option.FlatMapAsync(map).ConfigureAwait(false);

        context.SetOutcome(result);
    }

    [Given("an async map function that returns a None")]
    public void GivenAnAsyncMapFunctionThatReturnsANone()
    {
        context.SetSlot<Func<int, Task<Option<int>>>>(async _ =>
            {
                await Task.Yield();

                return Option.None<int>();
            }, SpecContext.MapSlot);
    }

    [Given(
        "a sync map function that returns a Some with value multiplied by {int}")]
    public void GivenASyncMapFunctionThatReturnsASomeWithValueMultipliedByInt(
        int value)
    {
        context.SetSlot<Func<int, Option<int>>>(x => Option.Some(x * value), SpecContext.MapSlot);
    }

    [Given("a sync map function that returns a None")]
    public void GivenASyncMapFunctionThatReturnsANone()
    {
        context.SetSlot<Func<int, Option<int>>>(_ => Option.None<int>(), SpecContext.MapSlot);
    }

    [When("invoking async FlatMap on Option ValueTask")]
    public async Task WhenInvokingAsyncFlatMapOnOptionValueTask()
    {
        var option = context.Subject<ValueTask<Option<int>>>();
        var map = context.Slot<Func<int, Task<Option<int>>>>(SpecContext.MapSlot);

        Option<int> result =
            await option.FlatMapAsync(map).ConfigureAwait(false);

        context.SetOutcome(result);
    }

    [When("invoking sync FlatMap on Option Task")]
    public async Task WhenInvokingSyncFlatMapOnOptionTask()
    {
        var option = context.Subject<Task<Option<int>>>();
        var map = context.Slot<Func<int, Option<int>>>(SpecContext.MapSlot);

        Option<int> result =
            await option.FlatMapAsync(map).ConfigureAwait(false);

        context.SetOutcome(result);
    }

    [When("invoking sync FlatMap on Option ValueTask")]
    public async Task WhenInvokingSyncFlatMapOnOptionValueTask()
    {
        var option = context.Subject<ValueTask<Option<int>>>();
        var map = context.Slot<Func<int, Option<int>>>(SpecContext.MapSlot);

        Option<int> result =
            await option.FlatMapAsync(map).ConfigureAwait(false);

        context.SetOutcome(result);
    }
}
