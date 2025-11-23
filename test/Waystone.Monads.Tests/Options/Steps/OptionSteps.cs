namespace Waystone.Monads.Options.Steps;

using System.Threading.Tasks;
using Reqnroll;
using Shouldly;

[Binding]
public sealed class OptionSteps(ScenarioContext context)
{
    [Given("Option is None")]
    public void GivenOptionIsNone()
    {
        Option<int> option = Option.None<int>();

        context.Set(option);
    }

    [Given("Option is Some with value {int}")]
    public void GivenOptionIsSomeWithValueInt(int optionValue)
    {
        Option<int> option = Option.Some(optionValue);

        context.Set(option);
    }

    [Given("Option is wrapped in a Task")]
    public void GivenOptionIsWrappedInATask()
    {
        var option = context.Get<Option<int>>();
        Task<Option<int>> taskOption = Task.FromResult(option);
        context.Set(taskOption);
    }

    [Given("Option is wrapped in a ValueTask")]
    public void GivenOptionIsWrappedInAValueTask()
    {
        var option = context.Get<Option<int>>();
        var taskOption = new ValueTask<Option<int>>(option);
        context.Set(taskOption);
    }

    [Then("the result Option should be Some with value {int}")]
    public void ThenTheResultOptionShouldBeSomeWithValueInt(int p0)
    {
        var result = context.Get<Option<int>>(Constants.ResultKey);
        result.IsSome.ShouldBeTrue();
        result.Unwrap().ShouldBe(p0);
    }

    [Then("the result Option should be None")]
    public void ThenTheResultOptionShouldBeNone()
    {
        var result = context.Get<Option<int>>(Constants.ResultKey);
        result.IsNone.ShouldBeTrue();
    }
}
