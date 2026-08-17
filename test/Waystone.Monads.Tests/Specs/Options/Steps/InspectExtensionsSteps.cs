namespace Waystone.Monads.Specs.Options.Steps;

using Reqnroll;
using System.Threading.Tasks;
using System;
using Waystone.Monads.Exceptions;
using Waystone.Monads.Options.Extensions;
using Waystone.Monads.Options;

[Binding]
public class InspectExtensionsSteps(SpecContext context)
{
    [When("invoking InspectAsync on the Option Task with the async delegate")]
    public async Task
        WhenInvokingInspectAsyncOnTheOptionTaskWithTheAsyncDelegate()
    {
        var optionTask = context.Subject<Task<Option<int>>>();
        var asyncDelegate = context.Subject<Func<int, Task>>();

        Option<int> result = await optionTask.InspectAsync(asyncDelegate);
        context.SetOutcome(result);
    }

    [When(
        "invoking InspectAsync on the Option Task with the synchronous delegate")]
    public async Task
        WhenInvokingInspectAsyncOnTheOptionTaskWithTheSynchronousDelegate()
    {
        var optionTask = context.Subject<Task<Option<int>>>();
        var syncDelegate = context.Subject<Action<int>>();

        Option<int> result = await optionTask.InspectAsync(syncDelegate);
        context.SetOutcome(result);
    }

    [When(
        "invoking InspectAsync on the Option ValueTask with the async delegate")]
    public async Task
        WhenInvokingInspectAsyncOnTheOptionValueTaskWithTheAsyncDelegate()
    {
        var optionTask = context.Subject<ValueTask<Option<int>>>();
        var asyncDelegate = context.Subject<Func<int, Task>>();

        Option<int> result = await optionTask.InspectAsync(asyncDelegate);
        context.SetOutcome(result);
    }

    [When(
        "invoking InspectAsync on the Option ValueTask with the synchronous delegate")]
    public async Task
        WhenInvokingInspectAsyncOnTheOptionValueTaskWithTheSynchronousDelegate()
    {
        var optionTask = context.Subject<ValueTask<Option<int>>>();
        var syncDelegate = context.Subject<Action<int>>();

        Option<int> result = await optionTask.InspectAsync(syncDelegate);
        context.SetOutcome(result);
    }
}
