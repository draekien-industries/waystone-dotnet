namespace Waystone.Monads.Results.Steps;

using System.Threading.Tasks;
using Exceptions;
using Monads.Extensions;
using Extensions;
using Reqnroll;
using Shouldly;

[Binding]
public sealed class UnwrapExtensionsSteps(ScenarioContext context)
{
    [When("unwrapping the Task Result")]
    public async Task WhenUnwrappingTheTaskResult()
    {
        var resultTask = context.Get<Task<Result<int, string>>>();

        await context.CaptureAsync(() => resultTask.UnwrapAsync());
    }

    [When("unwrapping the ValueTask Result")]
    public async Task WhenUnwrappingTheValueTaskResult()
    {
        var resultTask = context.Get<ValueTask<Result<int, string>>>();

        await context.CaptureAsync(() => resultTask.UnwrapAsync());
    }

    [When("unwrapping the error of the Task Result")]
    public async Task WhenUnwrappingTheErrorOfTheTaskResult()
    {
        var resultTask = context.Get<Task<Result<int, string>>>();

        await context.CaptureAsync(() => resultTask.UnwrapErrAsync());
    }

    [When("unwrapping the error of the ValueTask Result")]
    public async Task WhenUnwrappingTheErrorOfTheValueTaskResult()
    {
        var resultTask = context.Get<ValueTask<Result<int, string>>>();

        await context.CaptureAsync(() => resultTask.UnwrapErrAsync());
    }

    [When("unwrapping the Task Result with a default of {int}")]
    public async Task WhenUnwrappingTheTaskResultWithADefaultOf(int @default)
    {
        var resultTask = context.Get<Task<Result<int, string>>>();

        int output = await resultTask.UnwrapOrAsync(@default)
           .ConfigureAwait(false);

        context.Set(output, Constants.ResultKey);
    }

    [When("unwrapping the ValueTask Result with a default of {int}")]
    public async Task WhenUnwrappingTheValueTaskResultWithADefaultOf(
        int @default)
    {
        var resultTask = context.Get<ValueTask<Result<int, string>>>();

        int output = await resultTask.UnwrapOrAsync(@default)
           .ConfigureAwait(false);

        context.Set(output, Constants.ResultKey);
    }

    [When("unwrapping the Task Result or its default")]
    public async Task WhenUnwrappingTheTaskResultOrItsDefault()
    {
        var resultTask = context.Get<Task<Result<int, string>>>();

        int output = await resultTask.UnwrapOrDefaultAsync()
           .ConfigureAwait(false);

        context.Set(output, Constants.ResultKey);
    }

    [When("unwrapping the ValueTask Result or its default")]
    public async Task WhenUnwrappingTheValueTaskResultOrItsDefault()
    {
        var resultTask = context.Get<ValueTask<Result<int, string>>>();

        int output = await resultTask.UnwrapOrDefaultAsync()
           .ConfigureAwait(false);

        context.Set(output, Constants.ResultKey);
    }

    [Then("the unwrapped value should be {int}")]
    public void ThenTheUnwrappedValueShouldBe(int expected)
    {
        context.Get<int>(Constants.ResultKey).ShouldBe(expected);
    }

    [Then("the unwrapped error should be {string}")]
    public void ThenTheUnwrappedErrorShouldBe(string expected)
    {
        context.Get<string>(Constants.ResultKey).ShouldBe(expected);
    }

    [Then("an UnwrapException should be thrown")]
    public void ThenAnUnwrapExceptionShouldBeThrown()
    {
        context.GetCapturedException().ShouldBeAssignableTo<UnwrapException>();
    }
}
