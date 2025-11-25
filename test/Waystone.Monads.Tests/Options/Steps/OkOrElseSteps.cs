namespace Waystone.Monads.Options.Steps;

using System;
using System.Threading.Tasks;
using Extensions;
using Reqnroll;
using Results;

[Binding]
public class OkOrElseSteps(ScenarioContext context)
{
    [When("invoking OkOrElse on the Option with the async Error delegate")]
    public async Task WhenInvokingOkOrElseOnTheOptionWithTheAsyncErrorDelegate()
    {
        var option = context.Get<Option<int>>();

        var asyncErrDelegate =
            context.Get<Func<Task<string>>>(Constants.AsyncErrorDelegate);

        Result<int, string> result =
            await option.OkOrElseAsync(asyncErrDelegate);

        context.Set(result, Constants.ResultKey);
    }

    [When("invoking OkOrElse on the Option Task with the async Error delegate")]
    public async Task
        WhenInvokingOkOrElseOnTheOptionTaskWithTheAsyncErrorDelegate()
    {
        var optionTask = context.Get<Task<Option<int>>>();

        var asyncErrDelegate =
            context.Get<Func<Task<string>>>(Constants.AsyncErrorDelegate);

        Result<int, string> result =
            await optionTask.OkOrElseAsync(asyncErrDelegate);

        context.Set(result, Constants.ResultKey);
    }

    [When(
        "invoking OkOrElse on the Option ValueTask with the async Error delegate")]
    public async Task
        WhenInvokingOkOrElseOnTheOptionValueTaskWithTheAsyncErrorDelegate()
    {
        var optionTask = context.Get<ValueTask<Option<int>>>();

        var asyncErrDelegate =
            context.Get<Func<Task<string>>>(Constants.AsyncErrorDelegate);

        Result<int, string> result =
            await optionTask.OkOrElseAsync(asyncErrDelegate);

        context.Set(result, Constants.ResultKey);
    }

    [When(
        "invoking OkOrElse on the Task Option with the synchronous Error delegate")]
    public async Task
        WhenInvokingOkOrElseOnTheOptionWithTheSynchronousErrorDelegate()
    {
        var option = context.Get<Task<Option<int>>>();

        var syncErrDelegate =
            context.Get<Func<string>>(Constants.SyncErrorDelegate);

        Result<int, string> result =
            await option.OkOrElseAsync(syncErrDelegate);

        context.Set(result, Constants.ResultKey);
    }

    [When(
        "invoking OkOrElse on the ValueTask Option with the synchronous Error delegate")]
    public async Task
        WhenInvokingOkOrElseOnTheValueTaskOptionWithTheSynchronousErrorDelegate()
    {
        var option = context.Get<ValueTask<Option<int>>>();

        var syncErrDelegate =
            context.Get<Func<string>>(Constants.SyncErrorDelegate);

        Result<int, string> result =
            await option.OkOrElseAsync(syncErrDelegate);

        context.Set(result, Constants.ResultKey);
    }
}
