namespace Waystone.Monads.Specs.Results.Steps;

using Reqnroll;
using System.Threading.Tasks;
using System;
using Waystone.Monads.Exceptions;
using Waystone.Monads.Results.Errors;
using Waystone.Monads.Results.Extensions;
using Waystone.Monads.Results;

[Binding]
public class AndThenExtensionsSteps(ScenarioContext context)
{
    [When(
        "invoking AndThenAsync with the async delegate that returns {string}")]
    public async Task WhenInvokingAndThenAsyncWithTheAsyncDelegate(string type)
    {
        var result = context.Get<Task<Result<int, string>>>();

        var asyncDelegate =
            context.Get<Func<int, Task<Result<int, string>>>>(
                type == "OK"
                    ? Constants.AsyncOkDelegate
                    : Constants.AsyncErrorDelegate);

        Result<int, string> finalResult =
            await result.AndThenAsync(asyncDelegate);

        context.Set(finalResult, Constants.ResultKey);
    }

    [When("invoking AndThenAsync with the sync delegate that returns {string}")]
    public async Task WhenInvokingAndThenAsyncWithTheSyncDelegate(string type)
    {
        var result = context.Get<Task<Result<int, string>>>();

        var syncDelegate =
            context.Get<Func<int, Result<int, string>>>(
                type == "OK"
                    ? Constants.SyncOkDelegate
                    : Constants.SyncErrorDelegate);

        Result<int, string> finalResult =
            await result.AndThenAsync(ok => syncDelegate.Invoke(ok));

        context.Set(finalResult, Constants.ResultKey);
    }

    [When(
        "invoking AndThenAsync on the ValueTask with the async delegate that returns {string}")]
    public async Task
        WhenInvokingAndThenAsyncOnTheValueTaskWithTheAsyncDelegateThatReturns(
            string oK)
    {
        var result = context.Get<ValueTask<Result<int, string>>>();

        var asyncDelegate =
            context.Get<Func<int, Task<Result<int, string>>>>(
                oK == "OK"
                    ? Constants.AsyncOkDelegate
                    : Constants.AsyncErrorDelegate);

        Result<int, string> finalResult =
            await result.AndThenAsync(asyncDelegate);

        context.Set(finalResult, Constants.ResultKey);
    }

    [When(
        "invoking AndThenAsync on the ValueTask with the sync delegate that returns {string}")]
    public async Task
        WhenInvokingAndThenAsyncOnTheValueTaskWithTheSyncDelegateThatReturns(
            string oK)
    {
        var result = context.Get<ValueTask<Result<int, string>>>();

        var syncDelegate =
            context.Get<Func<int, Result<int, string>>>(
                oK == "OK"
                    ? Constants.SyncOkDelegate
                    : Constants.SyncErrorDelegate);

        Result<int, string> finalResult =
            await result.AndThenAsync(ok => syncDelegate.Invoke(ok));

        context.Set(finalResult, Constants.ResultKey);
    }
}
