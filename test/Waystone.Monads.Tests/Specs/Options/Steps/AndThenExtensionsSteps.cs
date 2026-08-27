namespace Waystone.Monads.Specs.Options.Steps;

using JetBrains.Annotations;
using Reqnroll;
using System.Threading.Tasks;
using System;
using Waystone.Monads.Exceptions;
using Waystone.Monads.Options.Extensions;
using Waystone.Monads.Options;

[Binding, TestSubject(typeof(AndThenExtensions))]
public class AndThenExtensionsSteps(SpecContext context)
{
    [Given(
        "an async map function that returns a Some with value multiplied by {int}")]
    public void GivenAnAsyncMapFunctionThatReturnsASomeWithValueMultipliedByInt(
        int value)
    {
        context.SetSlot<Func<int, ValueTask<Option<int>>>>(async x =>
            {
                await Task.Yield();

                return Option.Some(x * value);
            }, SpecContext.MapSlot);
    }

    [When("invoking async AndThen on Option Task")]
    public async Task WhenInvokingAsyncAndThenOnOptionTask()
    {
        var option = context.Subject<Task<Option<int>>>();
        var map = context.Slot<Func<int, ValueTask<Option<int>>>>(
            SpecContext.MapSlot);

        Option<int> result =
            await option.AndThenAsync(map).ConfigureAwait(false);

        context.SetOutcome(result);
    }

    [When("invoking async AndThen on Option")]
    public async Task WhenInvokingAsyncAndThenOnOption()
    {
        var option = context.Subject<Option<int>>();
        var map = context.Slot<Func<int, ValueTask<Option<int>>>>(
            SpecContext.MapSlot);

        Option<int> result =
            await option.AndThenAsync(map).ConfigureAwait(false);

        context.SetOutcome(result);
    }

    [Given("an async map function that returns a None")]
    public void GivenAnAsyncMapFunctionThatReturnsANone()
    {
        context.SetSlot<Func<int, ValueTask<Option<int>>>>(async _ =>
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

    [When("invoking async AndThen on Option ValueTask")]
    public async Task WhenInvokingAsyncAndThenOnOptionValueTask()
    {
        var option = context.Subject<ValueTask<Option<int>>>();
        var map = context.Slot<Func<int, ValueTask<Option<int>>>>(
            SpecContext.MapSlot);

        Option<int> result =
            await option.AndThenAsync(map).ConfigureAwait(false);

        context.SetOutcome(result);
    }

    [When("invoking sync AndThen on Option Task")]
    public async Task WhenInvokingSyncAndThenOnOptionTask()
    {
        var option = context.Subject<Task<Option<int>>>();
        var map = context.Slot<Func<int, Option<int>>>(SpecContext.MapSlot);

        Option<int> result =
            await option.AndThenAsync(map).ConfigureAwait(false);

        context.SetOutcome(result);
    }

    [When("invoking sync AndThen on Option ValueTask")]
    public async Task WhenInvokingSyncAndThenOnOptionValueTask()
    {
        var option = context.Subject<ValueTask<Option<int>>>();
        var map = context.Slot<Func<int, Option<int>>>(SpecContext.MapSlot);

        Option<int> result =
            await option.AndThenAsync(map).ConfigureAwait(false);

        context.SetOutcome(result);
    }
}
