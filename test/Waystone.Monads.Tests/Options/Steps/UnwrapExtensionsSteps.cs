namespace Waystone.Monads.Options.Steps;

using System;
using System.Threading.Tasks;
using Exceptions;
using Extensions;
using Reqnroll;
using Shouldly;

[Binding]
public sealed class UnwrapExtensionsSteps(ScenarioContext context)
{
    private const string ExceptionKey = "exception";

    [When("unwrapping the Task Option")]
    public async Task WhenUnwrappingTheTaskOption()
    {
        var optionTask = context.Get<Task<Option<int>>>();

        await CaptureAsync(() => optionTask.UnwrapAsync());
    }

    [When("unwrapping the ValueTask Option")]
    public async Task WhenUnwrappingTheValueTaskOption()
    {
        var optionTask = context.Get<ValueTask<Option<int>>>();

        await CaptureAsync(() => optionTask.UnwrapAsync());
    }

    [When("unwrapping the Task Option with a default of {int}")]
    public async Task WhenUnwrappingTheTaskOptionWithADefaultOf(int @default)
    {
        var optionTask = context.Get<Task<Option<int>>>();

        int output = await optionTask.UnwrapOrAsync(@default)
           .ConfigureAwait(false);

        context.Set(output, Constants.ResultKey);
    }

    [When("unwrapping the ValueTask Option with a default of {int}")]
    public async Task WhenUnwrappingTheValueTaskOptionWithADefaultOf(
        int @default)
    {
        var optionTask = context.Get<ValueTask<Option<int>>>();

        int output = await optionTask.UnwrapOrAsync(@default)
           .ConfigureAwait(false);

        context.Set(output, Constants.ResultKey);
    }

    [When("unwrapping the Task Option or its default")]
    public async Task WhenUnwrappingTheTaskOptionOrItsDefault()
    {
        var optionTask = context.Get<Task<Option<int>>>();

        int output = await optionTask.UnwrapOrDefaultAsync()
           .ConfigureAwait(false);

        context.Set(output, Constants.ResultKey);
    }

    [When("unwrapping the ValueTask Option or its default")]
    public async Task WhenUnwrappingTheValueTaskOptionOrItsDefault()
    {
        var optionTask = context.Get<ValueTask<Option<int>>>();

        int output = await optionTask.UnwrapOrDefaultAsync()
           .ConfigureAwait(false);

        context.Set(output, Constants.ResultKey);
    }

    [Then("the unwrapped Option value should be {int}")]
    public void ThenTheUnwrappedOptionValueShouldBe(int expected)
    {
        context.Get<int>(Constants.ResultKey).ShouldBe(expected);
    }

    [Then("an Option UnwrapException should be thrown")]
    public void ThenAnOptionUnwrapExceptionShouldBeThrown()
    {
        context.Get<Exception>(ExceptionKey)
           .ShouldBeAssignableTo<UnwrapException>();
    }

    private async Task CaptureAsync(Func<Task<int>> unwrap)
    {
        try
        {
            context.Set(
                await unwrap().ConfigureAwait(false),
                Constants.ResultKey);
        }
        catch (Exception ex)
        {
            context.Set(ex, ExceptionKey);
        }
    }
}
