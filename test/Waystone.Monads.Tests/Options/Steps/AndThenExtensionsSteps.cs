namespace Waystone.Monads.Options.Steps;

using System;
using System.Threading.Tasks;
using Reqnroll;
using Results;
using Results.Extensions;

[Binding]
public class AndThenExtensionsSteps(ScenarioContext context)
{
    [When("invoking AndThenAsync with the async delegate")]
    public async Task WhenInvokingAndThenAsyncWithTheAsyncDelegate()
    {
        var result = context.Get<Task<Result<int, string>>>();

        var asyncDelegate =
            context.Get<Func<int, Task<Result<int, string>>>>(
                Constants.AsyncOkDelegate);

        Result<int, string> finalResult =
            await result.AndThenAsync(asyncDelegate);

        context.Set(finalResult, Constants.ResultKey);
    }
}
