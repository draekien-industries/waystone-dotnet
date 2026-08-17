namespace Waystone.Monads.Specs.Results.Steps;

using Reqnroll;
using Shouldly;
using System.Threading.Tasks;
using Waystone.Monads.Exceptions;
using Waystone.Monads.Results.Errors;
using Waystone.Monads.Results.Extensions;
using Waystone.Monads.Results;

[Binding]
public sealed class UnwrapExtensionsSteps(SpecContext context)
{
    [When("unwrapping the Task Result")]
    public async Task WhenUnwrappingTheTaskResult()
    {
        var resultTask = context.Subject<Task<Result<int, string>>>();

        await context.CaptureAsync(() => resultTask.UnwrapAsync());
    }

    [When("unwrapping the ValueTask Result")]
    public async Task WhenUnwrappingTheValueTaskResult()
    {
        var resultTask = context.Subject<ValueTask<Result<int, string>>>();

        await context.CaptureAsync(() => resultTask.UnwrapAsync());
    }

    [When("unwrapping the error of the Task Result")]
    public async Task WhenUnwrappingTheErrorOfTheTaskResult()
    {
        var resultTask = context.Subject<Task<Result<int, string>>>();

        await context.CaptureAsync(() => resultTask.UnwrapErrAsync());
    }

    [When("unwrapping the error of the ValueTask Result")]
    public async Task WhenUnwrappingTheErrorOfTheValueTaskResult()
    {
        var resultTask = context.Subject<ValueTask<Result<int, string>>>();

        await context.CaptureAsync(() => resultTask.UnwrapErrAsync());
    }

    [When("unwrapping the Task Result with a default of {int}")]
    public async Task WhenUnwrappingTheTaskResultWithADefaultOf(int @default)
    {
        var resultTask = context.Subject<Task<Result<int, string>>>();

        int output = await resultTask.UnwrapOrAsync(@default)
           .ConfigureAwait(false);

        context.SetOutcome(output);
    }

    [When("unwrapping the ValueTask Result with a default of {int}")]
    public async Task WhenUnwrappingTheValueTaskResultWithADefaultOf(
        int @default)
    {
        var resultTask = context.Subject<ValueTask<Result<int, string>>>();

        int output = await resultTask.UnwrapOrAsync(@default)
           .ConfigureAwait(false);

        context.SetOutcome(output);
    }

    [When("unwrapping the Task Result or its default")]
    public async Task WhenUnwrappingTheTaskResultOrItsDefault()
    {
        var resultTask = context.Subject<Task<Result<int, string>>>();

        int output = await resultTask.UnwrapOrDefaultAsync()
           .ConfigureAwait(false);

        context.SetOutcome(output);
    }

    [When("unwrapping the ValueTask Result or its default")]
    public async Task WhenUnwrappingTheValueTaskResultOrItsDefault()
    {
        var resultTask = context.Subject<ValueTask<Result<int, string>>>();

        int output = await resultTask.UnwrapOrDefaultAsync()
           .ConfigureAwait(false);

        context.SetOutcome(output);
    }

    [Then("the unwrapped value should be {int}")]
    public void ThenTheUnwrappedValueShouldBe(int expected)
    {
        context.Outcome<int>().ShouldBe(expected);
    }

    [Then("the unwrapped error should be {string}")]
    public void ThenTheUnwrappedErrorShouldBe(string expected)
    {
        context.Outcome<string>().ShouldBe(expected);
    }

    [Then("an UnwrapException should be thrown")]
    public void ThenAnUnwrapExceptionShouldBeThrown()
    {
        context.CapturedException.ShouldBeAssignableTo<UnwrapException>();
    }
}
