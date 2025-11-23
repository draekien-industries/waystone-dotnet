namespace Waystone.Monads.Options.Steps;

using System;
using System.Threading.Tasks;
using Extensions;
using Reqnroll;

[Binding]
public class FilterExtensionsSteps(ScenarioContext context)
{
    [Given("an async predicate that returns {string} for int value")]
    public void GivenAnAsyncPredicateThatReturnsBoolForIntValue(string result)
    {
        var predicate = new Func<int, Task<bool>>(async _ =>
        {
            await Task.Yield();

            return bool.Parse(result);
        });

        context.Set(predicate);
    }

    [When("invoking Filter on Option Task with the predicate")]
    public async Task WhenInvokingFilterOnOptionTaskWithThePredicate()
    {
        var predicate = context.Get<Func<int, Task<bool>>>();
        var optionTask = context.Get<Task<Option<int>>>();

        Option<int> resultTask = await optionTask.Filter(predicate);

        context.Set(resultTask, Constants.ResultKey);
    }

    [When("invoking Filter on Option ValueTask with the predicate")]
    public async Task WhenInvokingFilterOnOptionValueTaskWithThePredicate()
    {
        var predicate = context.Get<Func<int, Task<bool>>>();
        var optionValueTask = context.Get<ValueTask<Option<int>>>();

        Option<int> resultValueTask = await optionValueTask.Filter(predicate);

        context.Set(resultValueTask, Constants.ResultKey);
    }
}
