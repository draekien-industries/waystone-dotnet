namespace Waystone.Monads.Specs.Options.Steps;

using JetBrains.Annotations;
using Reqnroll;
using System.Threading.Tasks;
using System;
using Waystone.Monads.Exceptions;
using Waystone.Monads.Options.Extensions;
using Waystone.Monads.Options;

[Binding, TestSubject(typeof(FilterExtensions))]
public class FilterExtensionsSteps(SpecContext context)
{
    [Given("an async predicate that returns {string} for int value")]
    public void GivenAnAsyncPredicateThatReturnsBoolForIntValue(bool result)
    {
        var predicate = new Func<int, Task<bool>>(async _ =>
        {
            await Task.Yield();

            return result;
        });

        context.SetSubject(predicate);
    }

    [When("invoking Filter on Option Task with the async predicate")]
    public async Task WhenInvokingFilterOnOptionTaskWithThePredicate()
    {
        var predicate = context.Subject<Func<int, Task<bool>>>();
        var optionTask = context.Subject<Task<Option<int>>>();

        Option<int> resultTask = await optionTask.FilterAsync(predicate);

        context.SetOutcome(resultTask);
    }

    [When("invoking Filter on Option ValueTask with the async predicate")]
    public async Task WhenInvokingFilterOnOptionValueTaskWithThePredicate()
    {
        var predicate = context.Subject<Func<int, Task<bool>>>();
        var optionValueTask = context.Subject<ValueTask<Option<int>>>();

        Option<int> resultValueTask =
            await optionValueTask.FilterAsync(predicate);

        context.SetOutcome(resultValueTask);
    }

    [Given("a sync predicate that returns {string} for int value")]
    public void GivenASyncPredicateThatReturnsForIntValue(bool result)
    {
        var predicate = new Func<int, bool>(_ => result);

        context.SetSubject(predicate);
    }

    [When("invoking Filter on Option ValueTask with the sync predicate")]
    public async Task WhenInvokingFilterOnOptionValueTaskWithTheSyncPredicate()
    {
        var predicate = context.Subject<Func<int, bool>>();
        var optionValueTask = context.Subject<ValueTask<Option<int>>>();

        Option<int> result = await optionValueTask.FilterAsync(predicate);
        context.SetOutcome(result);
    }

    [When("invoking Filter on Option Task with the sync predicate")]
    public async Task WhenInvokingFilterOnOptionTaskWithTheSyncPredicate()
    {
        var predicate = context.Subject<Func<int, bool>>();
        var optionTask = context.Subject<Task<Option<int>>>();

        Option<int> result = await optionTask.FilterAsync(predicate);
        context.SetOutcome(result);
    }
}
