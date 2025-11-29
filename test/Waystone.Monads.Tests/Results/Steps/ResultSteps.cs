namespace Waystone.Monads.Results.Steps;

using System.Threading.Tasks;
using Reqnroll;
using Shouldly;

[Binding]
public class ResultSteps(ScenarioContext context)
{
    [Then("the result should be an Ok Result containing the value {int}")]
    public void ThenTheResultShouldBeAnOkResultContainingTheValue(int p0)
    {
        var result = context.Get<Result<int, string>>(Constants.ResultKey);
        result.IsOk.ShouldBe(true);
        result.Expect("Expected an Ok Result.").ShouldBe(p0);
    }

    [Then("the result should be an Error Result containing {string}")]
    public void ThenTheResultShouldBeAnErrorResultContaining(string p0)
    {
        var result = context.Get<Result<int, string>>(Constants.ResultKey);
        result.IsErr.ShouldBe(true);
        result.ExpectErr("Expected an Err Result.").ShouldBe(p0);
    }

    [Given("an OK result with value {int}")]
    public void GivenAnOkResultWithValue(int p0)
    {
        Result<int, string> result = Result.Ok<int, string>(p0);
        context.Set(result);
    }

    [Given("the result is wrapped in a Task")]
    public void GivenTheResultIsWrappedInATask()
    {
        var result = context.Get<Result<int, string>>();
        context.Set(Task.FromResult(result));
    }

    [Then("the output should be an OK result with value {int}")]
    public void ThenTheOutputShouldBeAnOkResultWithValue(int p0)
    {
        var result = context.Get<Result<int, string>>(Constants.ResultKey);
        result.IsOk.ShouldBe(true);
        result.Expect("Expected an Ok Result.").ShouldBe(p0);
    }

    [Then("the output should be an Error result with message {string}")]
    public void ThenTheOutputShouldBeAnErrorResultWithMessage(string p0)
    {
        var result = context.Get<Result<int, string>>(Constants.ResultKey);
        result.IsErr.ShouldBe(true);
        result.ExpectErr("Expected an Err Result.").ShouldBe(p0);
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
    public void GivenItIsNestedInAnErrorResultWithValue(string error)
    {
        var result = context.Get<Result<int, string>>();

        Result<Result<int, string>, string> nested =
            Result.Err<Result<int, string>, string>(error);

        context.Set(nested);
    }

    [Given("an Error result with value {string}")]
    public void GivenAnErrorResultWithValue(string error)
    {
        Result<int, string> result = Result.Err<int, string>(error);
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
    public void ThenTheOutputShouldBeTheValue(string p0)
    {
        var output = context.Get<string>(Constants.ResultKey);
        output.ShouldBe(p0);
    }
}
