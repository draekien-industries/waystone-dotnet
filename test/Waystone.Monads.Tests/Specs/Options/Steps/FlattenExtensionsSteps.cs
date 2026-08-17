namespace Waystone.Monads.Specs.Options.Steps;

using Reqnroll;
using System.Threading.Tasks;
using Waystone.Monads.Exceptions;
using Waystone.Monads.Options.Extensions;
using Waystone.Monads.Options;

[Binding]
public class FlattenExtensionsSteps(SpecContext context)
{
    [When("invoking flatten on the Task of Option Option")]
    public async Task WhenInvokingFlattenOnTheTaskOfOptionOption()
    {
        var nestedOptionTask = context.Subject<Task<Option<Option<int>>>>();

        Option<int> result = await nestedOptionTask.FlattenAsync();

        context.SetOutcome(result);
    }

    [When("invoking flatten on the ValueTask of Option Option")]
    public void WhenInvokingFlattenOnTheValueTaskOfOptionOption()
    {
        var nestedOptionTask =
            context.Subject<ValueTask<Option<Option<int>>>>();

        Task<Option<int>> flattenTask = nestedOptionTask.FlattenAsync();

        Option<int> result = flattenTask.GetAwaiter().GetResult();

        context.SetOutcome(result);
    }
}
