namespace Waystone.Monads.Options.Steps;

using System;
using System.Threading.Tasks;
using Extensions;
using Reqnroll;

[Binding]
public class InspectExtensionsSteps(ScenarioContext context)
{
    [When("invoking InspectAsync on the Option Task with the async delegate")]
    public async Task
        WhenInvokingInspectAsyncOnTheOptionTaskWithTheAsyncDelegate()
    {
        var optionTask = context.Get<Task<Option<int>>>();
        var asyncDelegate = context.Get<Func<int, Task>>();

        Option<int> result = await optionTask.InspectAsync(asyncDelegate);
        context.Set(result, Constants.ResultKey);
    }

    [When(
        "invoking InspectAsync on the Option Task with the synchronous delegate")]
    public async Task
        WhenInvokingInspectAsyncOnTheOptionTaskWithTheSynchronousDelegate()
    {
        var optionTask = context.Get<Task<Option<int>>>();
        var syncDelegate = context.Get<Action<int>>();

        Option<int> result = await optionTask.InspectAsync(syncDelegate);
        context.Set(result, Constants.ResultKey);
    }

    [When(
        "invoking InspectAsync on the Option ValueTask with the async delegate")]
    public async Task
        WhenInvokingInspectAsyncOnTheOptionValueTaskWithTheAsyncDelegate()
    {
        var optionTask = context.Get<ValueTask<Option<int>>>();
        var asyncDelegate = context.Get<Func<int, Task>>();

        Option<int> result = await optionTask.InspectAsync(asyncDelegate);
        context.Set(result, Constants.ResultKey);
    }

    [When(
        "invoking InspectAsync on the Option ValueTask with the synchronous delegate")]
    public async Task
        WhenInvokingInspectAsyncOnTheOptionValueTaskWithTheSynchronousDelegate()
    {
        var optionTask = context.Get<ValueTask<Option<int>>>();
        var syncDelegate = context.Get<Action<int>>();

        Option<int> result = await optionTask.InspectAsync(syncDelegate);
        context.Set(result, Constants.ResultKey);
    }
}
