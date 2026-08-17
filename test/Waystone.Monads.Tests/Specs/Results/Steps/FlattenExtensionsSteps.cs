namespace Waystone.Monads.Specs.Results.Steps;

using Reqnroll;
using System.Threading.Tasks;
using Waystone.Monads.Exceptions;
using Waystone.Monads.Results.Errors;
using Waystone.Monads.Results.Extensions;
using Waystone.Monads.Results;

[Binding]
public class FlattenExtensionsSteps(SpecContext context)
{
    [When("invoking flatten on the sync nested results")]
    public void WhenInvokingFlattenOnTheSyncNestedResults()
    {
        var result = context.Subject<Result<Result<int, string>, string>>();
        Result<int, string> flattened = result.Flatten();
        context.SetOutcome(flattened);
    }

    [When("invoking flatten on the async Task result")]
    public async Task WhenInvokingFlattenOnTheAsyncTaskResult()
    {
        var resultTask =
            context.Subject<Task<Result<Result<int, string>, string>>>();

        Result<int, string> flattened = await resultTask.FlattenAsync();
        context.SetOutcome(flattened);
    }

    [When("invoking flatten on the async ValueTask result")]
    public async Task WhenInvokingFlattenOnTheAsyncValueTaskResult()
    {
        var resultTask =
            context.Subject<ValueTask<Result<Result<int, string>, string>>>();

        Result<int, string> flattened = await resultTask.FlattenAsync();
        context.SetOutcome(flattened);
    }
}
