namespace Waystone.Monads.Specs.Results.Steps;

using Reqnroll;
using System.Threading.Tasks;
using System;
using Waystone.Monads.Exceptions;
using Waystone.Monads.Results.Errors;
using Waystone.Monads.Results.Extensions;
using Waystone.Monads.Results;

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
