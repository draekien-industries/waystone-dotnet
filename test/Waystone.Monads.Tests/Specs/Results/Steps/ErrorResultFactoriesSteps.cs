namespace Waystone.Monads.Specs.Results.Steps;

using Reqnroll;
using Shouldly;
using System.Threading.Tasks;
using System;
using Waystone.Monads.Exceptions;
using Waystone.Monads.Extensions;
using Waystone.Monads.Results.Errors;
using Waystone.Monads.Results.Extensions;
using Waystone.Monads.Results;

[Binding]
public sealed class ErrorResultFactoriesSteps(SpecContext context)
{
    [Given("an Error with code {string} and message {string}")]
    public void GivenAnErrorWithCodeAndMessage(string code, string message)
    {
        context.Error = new Error(code, message);
    }

    [When("creating an Ok result with the value {int}")]
    public void WhenCreatingAnOkResultWithTheValue(int value)
    {
        Result<int, Error> result = Result.Ok<int>(value);

        context.SetOutcome(result);
    }

    [When("creating an Err result from the Error")]
    public void WhenCreatingAnErrResultFromTheError()
    {
        var error = context.Error;

        Result<int, Error> result = Result.Err<int>(error);

        context.SetOutcome(result);
    }

    [When("trying a factory that returns {int}")]
    public void WhenTryingAFactoryThatReturns(int value)
    {
        Result<int, Error> result = Result.Try<int>(() => value);

        context.SetOutcome(result);
    }

    [When(
        "trying a factory that throws an InvalidOperationException with message {string}")]
    public void
        WhenTryingAFactoryThatThrowsAnInvalidOperationExceptionWithMessage(
            string message)
    {
        Result<int, Error> result =
            Result.Try<int>(
                () => throw new InvalidOperationException(message));

        context.SetOutcome(result);
    }

    [When("trying an async factory that returns {int}")]
    public async Task WhenTryingAnAsyncFactoryThatReturns(int value)
    {
        Result<int, Error> result =
            await Result.TryAsync<int>(() => Task.FromResult(value))
               .ConfigureAwait(false);

        context.SetOutcome(result);
    }

    [When(
        "trying an async factory that throws an InvalidOperationException with message {string}")]
    public async Task
        WhenTryingAnAsyncFactoryThatThrowsAnInvalidOperationExceptionWithMessage(
            string message)
    {
        Result<int, Error> result = await Result
           .TryAsync<int>(
                () => Task.FromException<int>(
                    new InvalidOperationException(message)))
           .ConfigureAwait(false);

        context.SetOutcome(result);
    }

    [Then("the error typed result should be Ok with the value {int}")]
    public void ThenTheErrorTypedResultShouldBeOkWithTheValue(int expected)
    {
        var result = context.Outcome<Result<int, Error>>();

        result.ShouldBeOk();
        result.ShouldBeOkValue(expected);
    }

    [Then(
        "the error typed result should be Err with code {string} and message {string}")]
    public void ThenTheErrorTypedResultShouldBeErrWithCodeAndMessage(
        string code,
        string message)
    {
        var result = context.Outcome<Result<int, Error>>();

        result.ShouldBeErr();

        Error error = result.UnwrapErr();
        error.Code.Value.ShouldBe(code);
        error.Message.ShouldBe(message);
    }

    [Then("the Error should have code {string} and message {string}")]
    public void ThenTheErrorShouldHaveCodeAndMessage(
        string code,
        string message)
    {
        var error = context.Error;

        error.Code.Value.ShouldBe(code);
        error.Message.ShouldBe(message);
    }
}
