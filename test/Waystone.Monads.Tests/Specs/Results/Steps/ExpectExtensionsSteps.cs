namespace Waystone.Monads.Specs.Results.Steps;

using Reqnroll;
using Shouldly;
using System.Threading.Tasks;
using Waystone.Monads.Exceptions;
using Waystone.Monads.Results.Errors;
using Waystone.Monads.Results.Extensions;
using Waystone.Monads.Results;

[Binding]
public sealed class ExpectExtensionsSteps(SpecContext context)
{
    [When("expecting an Ok from the Task Result with message {string}")]
    public async Task WhenExpectingAnOkFromTheTaskResultWithMessage(
        string message)
    {
        var resultTask = context.Subject<Task<Result<int, string>>>();

        await context.CaptureAsync(() => resultTask.ExpectAsync(message));
    }

    [When("expecting an Ok from the ValueTask Result with message {string}")]
    public async Task WhenExpectingAnOkFromTheValueTaskResultWithMessage(
        string message)
    {
        var resultTask = context.Subject<ValueTask<Result<int, string>>>();

        await context.CaptureAsync(() => resultTask.ExpectAsync(message));
    }

    [When("expecting an Err from the Task Result with message {string}")]
    public async Task WhenExpectingAnErrFromTheTaskResultWithMessage(
        string message)
    {
        var resultTask = context.Subject<Task<Result<int, string>>>();

        await context.CaptureAsync(() => resultTask.ExpectErrAsync(message));
    }

    [When("expecting an Err from the ValueTask Result with message {string}")]
    public async Task WhenExpectingAnErrFromTheValueTaskResultWithMessage(
        string message)
    {
        var resultTask = context.Subject<ValueTask<Result<int, string>>>();

        await context.CaptureAsync(() => resultTask.ExpectErrAsync(message));
    }

    [Then("the expected value should be {int}")]
    public void ThenTheExpectedValueShouldBe(int expected)
    {
        context.Outcome<int>().ShouldBe(expected);
    }

    [Then("the expected error should be {string}")]
    public void ThenTheExpectedErrorShouldBe(string expected)
    {
        context.Outcome<string>().ShouldBe(expected);
    }

    [Then("an UnmetExpectationException should be thrown containing {string}")]
    public void ThenAnUnmetExpectationExceptionShouldBeThrownContaining(
        string message)
    {
        var exception = context.CapturedException;
        exception.ShouldBeOfType<UnmetExpectationException>();
        exception.Message.ShouldContain(message);
    }
}
