namespace Waystone.Monads.Results.Steps;

using System;
using System.Threading.Tasks;
using Errors;
using Reqnroll;
using Shouldly;

[Binding]
public sealed class ErrorResultFactoriesSteps(ScenarioContext context)
{
    private const string ErrorKey = "error";

    private enum TestErrorCodes
    {
        NotFound,
    }

    [Given("an Error with code {string} and message {string}")]
    public void GivenAnErrorWithCodeAndMessage(string code, string message)
    {
        context.Set(new Error(code, message), ErrorKey);
    }

    [When("creating an Ok result with the value {int}")]
    public void WhenCreatingAnOkResultWithTheValue(int value)
    {
        Result<int, Error> result = Result.Ok<int>(value);

        context.Set(result, Constants.ResultKey);
    }

    [When("creating an Err result from the Error")]
    public void WhenCreatingAnErrResultFromTheError()
    {
        var error = context.Get<Error>(ErrorKey);

        Result<int, Error> result = Result.Err<int>(error);

        context.Set(result, Constants.ResultKey);
    }

    [When(
        "creating an Err result from the NotFound enum value and message {string}")]
    public void WhenCreatingAnErrResultFromTheNotFoundEnumValueAndMessage(
        string message)
    {
        Result<int, Error> result =
            Result.Err<int>(TestErrorCodes.NotFound, message);

        context.Set(result, Constants.ResultKey);
    }

    [When("creating an Error from the NotFound enum value and message {string}")]
    public void WhenCreatingAnErrorFromTheNotFoundEnumValueAndMessage(
        string message)
    {
        context.Set(
            Error.FromEnum(TestErrorCodes.NotFound, message),
            ErrorKey);
    }

    [When("trying a factory that returns {int}")]
    public void WhenTryingAFactoryThatReturns(int value)
    {
        Result<int, Error> result = Result.Try<int>(() => value);

        context.Set(result, Constants.ResultKey);
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

        context.Set(result, Constants.ResultKey);
    }

    [When("trying an async factory that returns {int}")]
    public async Task WhenTryingAnAsyncFactoryThatReturns(int value)
    {
        Result<int, Error> result =
            await Result.TryAsync<int>(() => Task.FromResult(value))
               .ConfigureAwait(false);

        context.Set(result, Constants.ResultKey);
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

        context.Set(result, Constants.ResultKey);
    }

    [Then("the error typed result should be Ok with the value {int}")]
    public void ThenTheErrorTypedResultShouldBeOkWithTheValue(int expected)
    {
        var result = context.Get<Result<int, Error>>(Constants.ResultKey);

        result.IsOk.ShouldBeTrue();
        result.Unwrap().ShouldBe(expected);
    }

    [Then(
        "the error typed result should be Err with code {string} and message {string}")]
    public void ThenTheErrorTypedResultShouldBeErrWithCodeAndMessage(
        string code,
        string message)
    {
        var result = context.Get<Result<int, Error>>(Constants.ResultKey);

        result.IsErr.ShouldBeTrue();

        Error error = result.UnwrapErr();
        error.Code.Value.ShouldBe(code);
        error.Message.ShouldBe(message);
    }

    [Then("the Error should have code {string} and message {string}")]
    public void ThenTheErrorShouldHaveCodeAndMessage(
        string code,
        string message)
    {
        var error = context.Get<Error>(ErrorKey);

        error.Code.Value.ShouldBe(code);
        error.Message.ShouldBe(message);
    }
}
