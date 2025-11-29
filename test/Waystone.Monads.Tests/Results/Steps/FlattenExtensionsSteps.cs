namespace Waystone.Monads.Results.Steps;

using Extensions;
using Reqnroll;

[Binding]
public class FlattenExtensionsSteps(ScenarioContext context)
{
    [When("invoking flatten on the sync nested results")]
    public void WhenInvokingFlattenOnTheSyncNestedResults()
    {
        var result = context.Get<Result<Result<int, string>, string>>();
        Result<int, string> flattened = result.Flatten();
        context.Set(flattened, Constants.ResultKey);
    }
}
