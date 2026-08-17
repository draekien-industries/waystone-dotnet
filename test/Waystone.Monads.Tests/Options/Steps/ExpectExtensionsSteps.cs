namespace Waystone.Monads.Options.Steps;

using System;
using System.Threading.Tasks;
using Exceptions;
using Extensions;
using Reqnroll;
using Shouldly;

[Binding]
public sealed class ExpectExtensionsSteps(ScenarioContext context)
{
    private const string ExceptionKey = "exception";

    [When("expecting a Some from the Task Option with message {string}")]
    public async Task WhenExpectingASomeFromTheTaskOptionWithMessage(
        string message)
    {
        var optionTask = context.Get<Task<Option<int>>>();

        await CaptureAsync(() => optionTask.ExpectAsync(message));
    }

    [When("expecting a Some from the ValueTask Option with message {string}")]
    public async Task WhenExpectingASomeFromTheValueTaskOptionWithMessage(
        string message)
    {
        var optionTask = context.Get<ValueTask<Option<int>>>();

        await CaptureAsync(() => optionTask.ExpectAsync(message));
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
        var exception = context.Get<Exception>(ExceptionKey);
        exception.ShouldBeOfType<UnmetExpectationException>();
        exception.Message.ShouldContain(message);
    }

    private async Task CaptureAsync(Func<Task<int>> expect)
    {
        try
        {
            context.Set(
                await expect().ConfigureAwait(false),
                Constants.ResultKey);
        }
        catch (Exception ex)
        {
            context.Set(ex, ExceptionKey);
        }
    }
}
