namespace Waystone.Monads.Specs.Options.Steps;

using Reqnroll;
using System.Threading.Tasks;
using Waystone.Monads.Exceptions;
using Waystone.Monads.Options.Extensions;
using Waystone.Monads.Options;

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
