namespace Waystone.Monads.Options.Steps;

using System;
using System.Threading.Tasks;
using Reqnroll;
using Results;
using Results.Extensions;

[Binding]
public class InspectErrExtensionsSteps(ScenarioContext context)
{
    [When("invoking InspectErrAsync on result with async delegate")]
    public async Task WhenInvokingInspectErrAsyncOnResultWithAsyncDelegate()
    {
        var result = context.Get<Result<int, string>>();
        var asyncDelegate = context.Get<Func<string, Task>>();

        Result<int, string> output =
            await result.InspectErrAsync(asyncDelegate);

        context.Set(output, Constants.ResultKey);
    }

    [When("invoking InspectErrAsync on async Task result with async delegate")]
    public async Task
        WhenInvokingInspectErrAsyncOnAsyncTaskResultWithAsyncDelegate()
    {
        var resultTask = context.Get<Task<Result<int, string>>>();
        var asyncDelegate = context.Get<Func<string, Task>>();

        Result<int, string> output =
            await resultTask.InspectErrAsync(asyncDelegate);

        context.Set(output, Constants.ResultKey);
    }

    [When(
        "invoking InspectErrAsync on async ValueTask result with async delegate")]
    public async Task
        WhenInvokingInspectErrAsyncOnAsyncValueTaskResultWithAsyncDelegate()
    {
        var resultTask = context.Get<ValueTask<Result<int, string>>>();
        var asyncDelegate = context.Get<Func<string, Task>>();

        Result<int, string> output =
            await resultTask.InspectErrAsync(asyncDelegate);

        context.Set(output, Constants.ResultKey);
    }
}
