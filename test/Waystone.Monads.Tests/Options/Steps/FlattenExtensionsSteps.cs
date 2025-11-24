namespace Waystone.Monads.Options.Steps;

using System.Threading.Tasks;
using Extensions;
using Reqnroll;

[Binding]
public class FlattenExtensionsSteps(ScenarioContext context)
{
    [When("invoking flatten on the Task of Option Option")]
    public async Task WhenInvokingFlattenOnTheTaskOfOptionOption()
    {
        var nestedOptionTask = context.Get<Task<Option<Option<int>>>>();

        Option<int> result = await nestedOptionTask.FlattenAsync();

        context.Set(result, Constants.ResultKey);
    }

    [When("invoking flatten on the ValueTask of Option Option")]
    public void WhenInvokingFlattenOnTheValueTaskOfOptionOption()
    {
        var nestedOptionTask =
            context.Get<ValueTask<Option<Option<int>>>>();

        Task<Option<int>> flattenTask = nestedOptionTask.FlattenAsync();

        Option<int> result = flattenTask.GetAwaiter().GetResult();

        context.Set(result, Constants.ResultKey);
    }
}
