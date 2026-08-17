namespace Waystone.Monads.Specs.Options.Steps;

using Reqnroll;
using Shouldly;
using System.Threading.Tasks;
using Waystone.Monads.Exceptions;
using Waystone.Monads.Options.Extensions;
using Waystone.Monads.Options;

[Binding]
public sealed class ExpectExtensionsSteps(ScenarioContext context)
{
    [When("expecting a Some from the Task Option with message {string}")]
    public async Task WhenExpectingASomeFromTheTaskOptionWithMessage(
        string message)
    {
        var optionTask = context.Get<Task<Option<int>>>();

        await context.CaptureAsync(() => optionTask.ExpectAsync(message));
    }

    [When("expecting a Some from the ValueTask Option with message {string}")]
    public async Task WhenExpectingASomeFromTheValueTaskOptionWithMessage(
        string message)
    {
        var optionTask = context.Get<ValueTask<Option<int>>>();

        await context.CaptureAsync(() => optionTask.ExpectAsync(message));
    }

    [Then("the expected Option value should be {int}")]
    public void ThenTheExpectedOptionValueShouldBe(int expected)
    {
        context.Get<int>(Constants.ResultKey).ShouldBe(expected);
    }

    [Then(
        "an Option UnmetExpectationException should be thrown containing {string}")]
    public void ThenAnOptionUnmetExpectationExceptionShouldBeThrownContaining(
        string message)
    {
        var exception = context.GetCapturedException();
        exception.ShouldBeOfType<UnmetExpectationException>();
        exception.Message.ShouldContain(message);
    }
}
