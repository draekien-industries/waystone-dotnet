namespace Waystone.Monads.Specs.Results.Steps;

using Reqnroll;
using System.Threading.Tasks;
using System;
using Waystone.Monads.Exceptions;
using Waystone.Monads.Results.Errors;
using Waystone.Monads.Results.Extensions;
using Waystone.Monads.Results;

[Binding]
public class AndThenExtensionsSteps(SpecContext context)
{
    [When(
        "invoking AndThenAsync with the async delegate that returns {string}")]
    public async Task WhenInvokingAndThenAsyncWithTheAsyncDelegate(string type)
    {
        var result = context.Subject<Task<Result<int, string>>>();

        var asyncDelegate =
            context.Slot<Func<int, Task<Result<int, string>>>>(type == "OK"
                    ? SpecContext.AsyncOkSlot
                    : SpecContext.AsyncErrorSlot);

        Result<int, string> finalResult =
            await result.AndThenAsync(asyncDelegate);

        context.SetOutcome(finalResult);
    }

    [When("invoking AndThenAsync with the sync delegate that returns {string}")]
    public async Task WhenInvokingAndThenAsyncWithTheSyncDelegate(string type)
    {
        var result = context.Subject<Task<Result<int, string>>>();

        var syncDelegate =
            context.Slot<Func<int, Result<int, string>>>(type == "OK"
                    ? SpecContext.SyncOkSlot
                    : SpecContext.SyncErrorSlot);

        Result<int, string> finalResult =
            await result.AndThenAsync(ok => syncDelegate.Invoke(ok));

        context.SetOutcome(finalResult);
    }

    [When(
        "invoking AndThenAsync on the ValueTask with the async delegate that returns {string}")]
    public async Task
        WhenInvokingAndThenAsyncOnTheValueTaskWithTheAsyncDelegateThatReturns(
            string oK)
    {
        var result = context.Subject<ValueTask<Result<int, string>>>();

        var asyncDelegate =
            context.Slot<Func<int, Task<Result<int, string>>>>(oK == "OK"
                    ? SpecContext.AsyncOkSlot
                    : SpecContext.AsyncErrorSlot);

        Result<int, string> finalResult =
            await result.AndThenAsync(asyncDelegate);

        context.SetOutcome(finalResult);
    }

    [When(
        "invoking AndThenAsync on the ValueTask with the sync delegate that returns {string}")]
    public async Task
        WhenInvokingAndThenAsyncOnTheValueTaskWithTheSyncDelegateThatReturns(
            string oK)
    {
        var result = context.Subject<ValueTask<Result<int, string>>>();

        var syncDelegate =
            context.Slot<Func<int, Result<int, string>>>(oK == "OK"
                    ? SpecContext.SyncOkSlot
                    : SpecContext.SyncErrorSlot);

        Result<int, string> finalResult =
            await result.AndThenAsync(ok => syncDelegate.Invoke(ok));

        context.SetOutcome(finalResult);
    }
}
