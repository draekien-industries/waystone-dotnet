namespace Waystone.Monads.Options.Steps;

using System.Threading.Tasks;
using Extensions;
using Reqnroll;

[Binding]
public class IsNoneOrExtensionsSteps(ScenarioContext context)
{
    [When(
        "invoking IsNoneOr on Option Task with async predicate that returns {string}")]
    public async Task
        WhenInvokingIsNoneOrOnOptionTaskWithAsyncPredicateThatReturnsString(
            string predicateResult)
    {
        var optionTask = context.Get<Task<Option<int>>>();

        bool result = await optionTask.IsNoneOr(async value =>
        {
            await Task.Yield();

            return bool.Parse(predicateResult);
        });

        context.Set(result, Constants.ResultKey);
    }

    [When(
        "invoking IsNoneOr on Option ValueTask with async predicate that returns {string}")]
    public async Task
        WhenInvokingIsNoneOrOnOptionValueTaskWithAsyncPredicateThatReturnsString(
            string predicateResult)
    {
        var optionTask = context.Get<ValueTask<Option<int>>>();

        bool result = await optionTask.IsNoneOr(async value =>
        {
            await Task.Yield();

            return bool.Parse(predicateResult);
        });

        context.Set(result, Constants.ResultKey);
    }

    [When(
        "invoking IsNoneOr on Option Task with sync predicate that returns {string}")]
    public async Task
        WhenInvokingIsNoneOrOnOptionTaskWithSyncPredicateThatReturnsString(
            string predicateResult)
    {
        var optionTask = context.Get<Task<Option<int>>>();

        bool result = await optionTask.IsNoneOr(value =>
            bool.Parse(predicateResult));

        context.Set(result, Constants.ResultKey);
    }
}
