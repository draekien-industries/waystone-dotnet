namespace Waystone.Monads.Specs.Options.Steps;

using JetBrains.Annotations;
using Reqnroll;
using System.Threading.Tasks;
using System;
using Waystone.Monads.Exceptions;
using Waystone.Monads.Options.Extensions;
using Waystone.Monads.Options;

[Binding, TestSubject(typeof(IsSomeAndExtensions))]
public class IsSomeAndExtensionsSteps(ScenarioContext context)
{
    [When("invoking IsSomeAnd on Option Task with the async predicate")]
    public async Task WhenInvokingIsSomeAndOnOptionTaskWithTheAsyncPredicate()
    {
        var optionTask = context.Get<Task<Option<int>>>();
        var predicate = context.Get<Func<int, Task<bool>>>();

        bool result = await optionTask.IsSomeAndAsync(predicate);

        context.Set(result, Constants.ResultKey);
    }

    [When("invoking IsSomeAnd on Option ValueTask with the async predicate")]
    public async Task
        WhenInvokingIsSomeAndOnOptionValueTaskWithTheAsyncPredicate()
    {
        var optionTask = context.Get<ValueTask<Option<int>>>();
        var predicate = context.Get<Func<int, Task<bool>>>();

        bool result = await optionTask.IsSomeAndAsync(predicate);

        context.Set(result, Constants.ResultKey);
    }
}
