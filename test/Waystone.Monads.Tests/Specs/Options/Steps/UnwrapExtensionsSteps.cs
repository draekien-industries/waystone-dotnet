namespace Waystone.Monads.Specs.Options.Steps;

using Reqnroll;
using Shouldly;
using System.Threading.Tasks;
using Waystone.Monads.Exceptions;
using Waystone.Monads.Options.Extensions;
using Waystone.Monads.Options;

[Binding]
public sealed class UnwrapExtensionsSteps(SpecContext context)
{
    [When("unwrapping the Task Option")]
    public async Task WhenUnwrappingTheTaskOption()
    {
        var optionTask = context.Subject<Task<Option<int>>>();

        await context.CaptureAsync(() => optionTask.UnwrapAsync());
    }

    [When("unwrapping the ValueTask Option")]
    public async Task WhenUnwrappingTheValueTaskOption()
    {
        var optionTask = context.Subject<ValueTask<Option<int>>>();

        await context.CaptureAsync(() => optionTask.UnwrapAsync());
    }

    [When("unwrapping the Task Option with a default of {int}")]
    public async Task WhenUnwrappingTheTaskOptionWithADefaultOf(int @default)
    {
        var optionTask = context.Subject<Task<Option<int>>>();

        int output = await optionTask.UnwrapOrAsync(@default)
           .ConfigureAwait(false);

        context.SetOutcome(output);
    }

    [When("unwrapping the ValueTask Option with a default of {int}")]
    public async Task WhenUnwrappingTheValueTaskOptionWithADefaultOf(
        int @default)
    {
        var optionTask = context.Subject<ValueTask<Option<int>>>();

        int output = await optionTask.UnwrapOrAsync(@default)
           .ConfigureAwait(false);

        context.SetOutcome(output);
    }

    [When("unwrapping the Task Option or its default")]
    public async Task WhenUnwrappingTheTaskOptionOrItsDefault()
    {
        var optionTask = context.Subject<Task<Option<int>>>();

        int output = await optionTask.UnwrapOrDefaultAsync()
           .ConfigureAwait(false);

        context.SetOutcome(output);
    }

    [When("unwrapping the ValueTask Option or its default")]
    public async Task WhenUnwrappingTheValueTaskOptionOrItsDefault()
    {
        var optionTask = context.Subject<ValueTask<Option<int>>>();

        int output = await optionTask.UnwrapOrDefaultAsync()
           .ConfigureAwait(false);

        context.SetOutcome(output);
    }

    [Then("the unwrapped Option value should be {int}")]
    public void ThenTheUnwrappedOptionValueShouldBe(int expected)
    {
        context.Outcome<int>().ShouldBe(expected);
    }

    [Then("an Option UnwrapException should be thrown")]
    public void ThenAnOptionUnwrapExceptionShouldBeThrown()
    {
        context.CapturedException.ShouldBeAssignableTo<UnwrapException>();
    }
}
