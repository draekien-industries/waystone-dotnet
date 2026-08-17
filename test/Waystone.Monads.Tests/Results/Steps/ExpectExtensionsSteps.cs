namespace Waystone.Monads.Results.Steps;

using System.Threading.Tasks;
using Exceptions;
using Monads.Extensions;
using Extensions;
using Reqnroll;
using Shouldly;

[Binding]
public sealed class ExpectExtensionsSteps(ScenarioContext context)
{
    [When("expecting an Ok from the Task Result with message {string}")]
    public async Task WhenExpectingAnOkFromTheTaskResultWithMessage(
        string message)
    {
        var resultTask = context.Get<Task<Result<int, string>>>();

        await context.CaptureAsync(() => resultTask.ExpectAsync(message));
    }

    [When("expecting an Ok from the ValueTask Result with message {string}")]
    public async Task WhenExpectingAnOkFromTheValueTaskResultWithMessage(
        string message)
    {
        var resultTask = context.Get<ValueTask<Result<int, string>>>();

        await context.CaptureAsync(() => resultTask.ExpectAsync(message));
    }

    [When("expecting an Err from the Task Result with message {string}")]
    public async Task WhenExpectingAnErrFromTheTaskResultWithMessage(
        string message)
    {
        var resultTask = context.Get<Task<Result<int, string>>>();

        await context.CaptureAsync(() => resultTask.ExpectErrAsync(message));
    }

    [When("expecting an Err from the ValueTask Result with message {string}")]
    public async Task WhenExpectingAnErrFromTheValueTaskResultWithMessage(
        string message)
    {
        var resultTask = context.Get<ValueTask<Result<int, string>>>();

        await context.CaptureAsync(() => resultTask.ExpectErrAsync(message));
    }

    [Then("the expected value should be {int}")]
    public void ThenTheExpectedValueShouldBe(int expected)
    {
        context.Get<int>(Constants.ResultKey).ShouldBe(expected);
    }

    [Then("the expected error should be {string}")]
    public void ThenTheExpectedErrorShouldBe(string expected)
    {
        context.Get<string>(Constants.ResultKey).ShouldBe(expected);
    }

    [Then("an UnmetExpectationException should be thrown containing {string}")]
    public void ThenAnUnmetExpectationExceptionShouldBeThrownContaining(
        string message)
    {
        var exception = context.GetCapturedException();
        exception.ShouldBeOfType<UnmetExpectationException>();
        exception.Message.ShouldContain(message);
    }
}
