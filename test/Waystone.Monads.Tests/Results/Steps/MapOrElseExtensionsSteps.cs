namespace Waystone.Monads.Results.Steps;

using System;
using System.Threading.Tasks;
using Extensions;
using Reqnroll;

[Binding]
public class MapOrElseExtensionsSteps(ScenarioContext context)
{
    [When(
        "invoking MapOrElse with the async factory and async map on the sync Result")]
    public async Task
        WhenInvokingMapOrElseWithTheAsyncFactoryAndAsyncMapOnTheSyncResult()
    {
        var result = context.Get<Result<int, string>>();

        var factory =
            context.Get<Func<string, Task<string>>>(
                Constants.AsyncErrorDelegate);

        var map =
            context.Get<Func<int, Task<string>>>(Constants.AsyncOkDelegate);

        string output = await result.MapOrElseAsync(factory, map);

        context.Set(output, Constants.ResultKey);
    }
}
