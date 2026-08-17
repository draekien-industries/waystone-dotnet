namespace Waystone.Monads.Specs.Results.Steps;

using Reqnroll;
using System.Threading.Tasks;
using System;
using Waystone.Monads.Exceptions;
using Waystone.Monads.Results.Errors;
using Waystone.Monads.Results.Extensions;
using Waystone.Monads.Results;

[Binding]
public class InspectErrExtensionsSteps(SpecContext context)
{
    [When("invoking InspectErrAsync on result with async delegate")]
    public async Task WhenInvokingInspectErrAsyncOnResultWithAsyncDelegate()
    {
        var result = context.Subject<Result<int, string>>();
        var asyncDelegate = context.Subject<Func<string, Task>>();

        Result<int, string> output =
            await result.InspectErrAsync(asyncDelegate);

        context.SetOutcome(output);
    }

    [When("invoking InspectErrAsync on async Task result with async delegate")]
    public async Task
        WhenInvokingInspectErrAsyncOnAsyncTaskResultWithAsyncDelegate()
    {
        var resultTask = context.Subject<Task<Result<int, string>>>();
        var asyncDelegate = context.Subject<Func<string, Task>>();

        Result<int, string> output =
            await resultTask.InspectErrAsync(asyncDelegate);

        context.SetOutcome(output);
    }

    [When(
        "invoking InspectErrAsync on async ValueTask result with async delegate")]
    public async Task
        WhenInvokingInspectErrAsyncOnAsyncValueTaskResultWithAsyncDelegate()
    {
        var resultTask = context.Subject<ValueTask<Result<int, string>>>();
        var asyncDelegate = context.Subject<Func<string, Task>>();

        Result<int, string> output =
            await resultTask.InspectErrAsync(asyncDelegate);

        context.SetOutcome(output);
    }
}
