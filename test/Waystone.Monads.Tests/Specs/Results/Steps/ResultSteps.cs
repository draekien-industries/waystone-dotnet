namespace Waystone.Monads.Specs.Results.Steps;

using Reqnroll;
using Shouldly;
using System.Threading.Tasks;
using Waystone.Monads.Exceptions;
using Waystone.Monads.Results.Errors;
using Waystone.Monads.Results.Extensions;
using Waystone.Monads.Results;

[Binding]
public class ResultSteps(ScenarioContext context)
{
    [Then("the result should be an Ok Result containing the value {int}")]
    public void ThenTheResultShouldBeAnOkResultContainingTheValue(int value)
    {
        var result = context.Get<Result<int, string>>(Constants.ResultKey);
        result.IsOk.ShouldBe(true);
        result.Expect("Expected an Ok Result.").ShouldBe(value);
    }

    [Then("the result should be an Error Result containing {string}")]
    public void ThenTheResultShouldBeAnErrorResultContaining(string type)
    {
        var result = context.Get<Result<int, string>>(Constants.ResultKey);
        result.IsErr.ShouldBe(true);
        result.ExpectErr("Expected an Err Result.").ShouldBe(type);
    }

    [Given("an OK result with value {int}")]
    public void GivenAnOkResultWithValue(int value)
    {
        Result<int, string> result = Result.Ok<int, string>(value);
        context.Set(result);
    }

    [Given("the result is wrapped in a Task")]
    public void GivenTheResultIsWrappedInATask()
    {
        var result = context.Get<Result<int, string>>();
        context.Set(Task.FromResult(result));
    }

    [Then("the output should be an OK result with value {int}")]
    public void ThenTheOutputShouldBeAnOkResultWithValue(int value)
    {
        var result = context.Get<Result<int, string>>(Constants.ResultKey);
        result.IsOk.ShouldBe(true);
        result.Expect("Expected an Ok Result.").ShouldBe(value);
    }

    [Then("the output should be an Error result with message {string}")]
    public void ThenTheOutputShouldBeAnErrorResultWithMessage(string message)
    {
        var result = context.Get<Result<int, string>>(Constants.ResultKey);
        result.IsErr.ShouldBe(true);
        result.ExpectErr("Expected an Err Result.").ShouldBe(message);
    }

    [Given("the result is wrapped in a ValueTask")]
    public void GivenTheResultIsWrappedInAValueTask()
    {
        var result = context.Get<Result<int, string>>();
        context.Set(new ValueTask<Result<int, string>>(result));
    }

    [Given("it is nested in an OK result")]
    public void GivenItIsNestedInAnOkResult()
    {
        var result = context.Get<Result<int, string>>();

        Result<Result<int, string>, string> nested =
            Result.Ok<Result<int, string>, string>(result);

        context.Set(nested);
    }

    [Given("it is nested in an Error result with value {string}")]
    public void GivenItIsNestedInAnErrorResultWithValue(string value)
    {
        Result<Result<int, string>, string> nested =
            Result.Err<Result<int, string>, string>(value);

        context.Set(nested);
    }

    [Given("an Error result with value {string}")]
    public void GivenAnErrorResultWithValue(string value)
    {
        Result<int, string> result = Result.Err<int, string>(value);
        context.Set(result);
    }

    [Given("the outer result is wrapped in a Task")]
    public void GivenTheOuterResultIsWrappedInATask()
    {
        var result = context.Get<Result<Result<int, string>, string>>();
        context.Set(Task.FromResult(result));
    }

    [Given("the outer result is wrapped in a ValueTask")]
    public void GivenTheOuterResultIsWrappedInAValueTask()
    {
        var result = context.Get<Result<Result<int, string>, string>>();
        context.Set(new ValueTask<Result<Result<int, string>, string>>(result));
    }

    [Then("the output should be the value {string}")]
    public void ThenTheOutputShouldBeTheValue(string value)
    {
        var output = context.Get<string>(Constants.ResultKey);
        output.ShouldBe(value);
    }
}
