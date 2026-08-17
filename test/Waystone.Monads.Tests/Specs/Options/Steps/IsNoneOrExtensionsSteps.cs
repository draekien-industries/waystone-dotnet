namespace Waystone.Monads.Specs.Options.Steps;

using JetBrains.Annotations;
using Reqnroll;
using System.Threading.Tasks;
using System;
using Waystone.Monads.Exceptions;
using Waystone.Monads.Options.Extensions;
using Waystone.Monads.Options;

[Binding, TestSubject(typeof(IsNoneOrExtensions))]
public class IsNoneOrExtensionsSteps(ScenarioContext context)
{
    [When(
        "invoking IsNoneOr on Option Task with sync predicate that returns {string}")]
    public async Task
        WhenInvokingIsNoneOrOnOptionTaskWithSyncPredicateThatReturnsString(
            bool predicateResult)
    {
        var optionTask = context.Get<Task<Option<int>>>();

        bool result = await optionTask.IsNoneOrAsync(_ => predicateResult);

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
