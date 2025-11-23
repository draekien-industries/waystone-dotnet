namespace Waystone.Monads.Options.Steps;

using Reqnroll;

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
}
