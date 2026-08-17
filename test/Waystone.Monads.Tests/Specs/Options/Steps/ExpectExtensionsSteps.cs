namespace Waystone.Monads.Specs.Options.Steps;

using Reqnroll;
using Shouldly;
using System.Threading.Tasks;
using Waystone.Monads.Exceptions;
using Waystone.Monads.Options.Extensions;
using Waystone.Monads.Options;

[Binding]
public sealed class ExpectExtensionsSteps(SpecContext context)
{
    [When("expecting a Some from the Task Option with message {string}")]
    public async Task WhenExpectingASomeFromTheTaskOptionWithMessage(
        string message)
    {
        var optionTask = context.Subject<Task<Option<int>>>();

        await context.CaptureAsync(() => optionTask.ExpectAsync(message));
    }

    [When("expecting a Some from the ValueTask Option with message {string}")]
    public async Task WhenExpectingASomeFromTheValueTaskOptionWithMessage(
        string message)
    {
        var optionTask = context.Subject<ValueTask<Option<int>>>();

        await context.CaptureAsync(() => optionTask.ExpectAsync(message));
    }

    [Then("the expected Option value should be {int}")]
    public void ThenTheExpectedOptionValueShouldBe(int expected)
    {
        context.Outcome<int>().ShouldBe(expected);
    }

    [Then(
        "an Option UnmetExpectationException should be thrown containing {string}")]
    public void ThenAnOptionUnmetExpectationExceptionShouldBeThrownContaining(
        string message)
    {
        var exception = context.CapturedException;
        exception.ShouldBeOfType<UnmetExpectationException>();
        exception.Message.ShouldContain(message);
    }
}
