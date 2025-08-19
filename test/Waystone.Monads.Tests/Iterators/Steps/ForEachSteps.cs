namespace Waystone.Monads.Iterators.Steps;

using System;
using NSubstitute;
using Reqnroll;

[Binding]
public sealed class ForEachSteps(ScenarioContext context)
{
    [When("I apply a mock action to each number")]
    public void WhenIApplyAMockActionToEachNumber()
    {
        var action = Substitute.For<Action<int>>();
        context.Set(action, "NumbersAction");
        var numbers = context.Get<Iter<int>>("Numbers");
        numbers.ForEach(action);
    }

    [Then("the action should be called {int} times")]
    public void ThenTheActionShouldBeCalledTimes(int p0)
    {
        var action = context.Get<Action<int>>("NumbersAction");
        action.Received(p0).Invoke(Arg.Any<int>());
    }
}
