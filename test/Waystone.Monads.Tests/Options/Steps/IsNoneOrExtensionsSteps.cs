namespace Waystone.Monads.Options.Steps;

using System;
using System.Threading.Tasks;
using Extensions;
using JetBrains.Annotations;
using Reqnroll;

[Binding, TestSubject(typeof(IsNoneOrExtensions))]
public class IsNoneOrExtensionsSteps(ScenarioContext context)
{
    [When(
        "invoking IsNoneOr on Option Task with sync predicate that returns {string}")]
    public async Task
        WhenInvokingIsNoneOrOnOptionTaskWithSyncPredicateThatReturnsString(
            string predicateResult)
    {
        var optionTask = context.Get<Task<Option<int>>>();

        bool result = await optionTask.IsNoneOrAsync(_ =>
            bool.Parse(predicateResult));

        context.Set(result, Constants.ResultKey);
    }

    [When("invoking IsNoneOr on Option Task with async predicate")]
    public async Task WhenInvokingIsNoneOrOnOptionTaskWithAsyncPredicate()
    {
        var optionTask = context.Get<Task<Option<int>>>();
        var predicate = context.Get<Func<int, Task<bool>>>();

        bool result = await optionTask.IsNoneOrAsync(predicate);

        context.Set(result, Constants.ResultKey);
    }

    [When("invoking IsNoneOr on Option ValueTask with async predicate")]
    public async Task WhenInvokingIsNoneOrOnOptionValueTaskWithAsyncPredicate()
    {
        var optionTask = context.Get<ValueTask<Option<int>>>();
        var predicate = context.Get<Func<int, Task<bool>>>();

        bool result = await optionTask.IsNoneOrAsync(predicate);

        context.Set(result, Constants.ResultKey);
    }

    [When("invoking IsNoneOr on Option Task with sync predicate")]
    public async Task WhenInvokingIsNoneOrOnOptionTaskWithSyncPredicate()
    {
        var optionTask = context.Get<Task<Option<int>>>();
        var predicate = context.Get<Func<int, bool>>();

        bool result = await optionTask.IsNoneOrAsync(predicate);

        context.Set(result, Constants.ResultKey);
    }
}
