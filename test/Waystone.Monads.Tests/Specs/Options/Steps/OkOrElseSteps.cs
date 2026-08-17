namespace Waystone.Monads.Specs.Options.Steps;

using Reqnroll;
using System.Threading.Tasks;
using System;
using Waystone.Monads.Exceptions;
using Waystone.Monads.Options.Extensions;
using Waystone.Monads.Options;
using Waystone.Monads.Results;

[Binding]
public class OkOrElseSteps(SpecContext context)
{
    [When("invoking OkOrElse on the Option with the async Error delegate")]
    public async Task WhenInvokingOkOrElseOnTheOptionWithTheAsyncErrorDelegate()
    {
        var option = context.Subject<Option<int>>();

        var asyncErrDelegate =
            context.Slot<Func<Task<string>>>(SpecContext.AsyncErrorSlot);

        Result<int, string> result =
            await option.OkOrElseAsync(asyncErrDelegate);

        context.SetOutcome(result);
    }

    [When("invoking OkOrElse on the Option Task with the async Error delegate")]
    public async Task
        WhenInvokingOkOrElseOnTheOptionTaskWithTheAsyncErrorDelegate()
    {
        var optionTask = context.Subject<Task<Option<int>>>();

        var asyncErrDelegate =
            context.Slot<Func<Task<string>>>(SpecContext.AsyncErrorSlot);

        Result<int, string> result =
            await optionTask.OkOrElseAsync(asyncErrDelegate);

        context.SetOutcome(result);
    }

    [When(
        "invoking OkOrElse on the Option ValueTask with the async Error delegate")]
    public async Task
        WhenInvokingOkOrElseOnTheOptionValueTaskWithTheAsyncErrorDelegate()
    {
        var optionTask = context.Subject<ValueTask<Option<int>>>();

        var asyncErrDelegate =
            context.Slot<Func<Task<string>>>(SpecContext.AsyncErrorSlot);

        Result<int, string> result =
            await optionTask.OkOrElseAsync(asyncErrDelegate);

        context.SetOutcome(result);
    }

    [When(
        "invoking OkOrElse on the Task Option with the synchronous Error delegate")]
    public async Task
        WhenInvokingOkOrElseOnTheOptionWithTheSynchronousErrorDelegate()
    {
        var option = context.Subject<Task<Option<int>>>();

        var syncErrDelegate =
            context.Slot<Func<string>>(SpecContext.SyncErrorSlot);

        Result<int, string> result =
            await option.OkOrElseAsync(syncErrDelegate);

        context.SetOutcome(result);
    }

    [When(
        "invoking OkOrElse on the ValueTask Option with the synchronous Error delegate")]
    public async Task
        WhenInvokingOkOrElseOnTheValueTaskOptionWithTheSynchronousErrorDelegate()
    {
        var option = context.Subject<ValueTask<Option<int>>>();

        var syncErrDelegate =
            context.Slot<Func<string>>(SpecContext.SyncErrorSlot);

        Result<int, string> result =
            await option.OkOrElseAsync(syncErrDelegate);

        context.SetOutcome(result);
    }
}
