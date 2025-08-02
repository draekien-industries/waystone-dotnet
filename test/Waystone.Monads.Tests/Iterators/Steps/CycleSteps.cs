namespace Waystone.Monads.Iterators.Steps;

using System.Collections.Generic;
using System.Linq;
using Adapters;
using Extensions;
using Fixtures;
using Options;
using Reqnroll;
using Shouldly;

[Binding]
public class CycleSteps(ScenarioContext context)
{
    [When("invoking cycle on {string} of cloneable types")]
    public void WhenInvokingCycle(string enumerable)
    {
        var iter = context.Get<Iter<TestCloneable>>(enumerable);
        Cycle<TestCloneable> cycle = iter.Cycle();
        context.Set(cycle, enumerable);
    }

    [Then("the elements of {string} should repeat indefinitely")]
    public void ThenTheElementsShouldRepeatIndefinitely(string enumerable)
    {
        var cycle = context.Get<Cycle<TestCloneable>>(enumerable);

        List<Option<TestCloneable>> elements = cycle.Take(10).ToList();
        elements.Count.ShouldBe(10);

        Option<TestCloneable> first = elements[0];
        Option<TestCloneable> second = elements[1];
        Option<TestCloneable> third = elements[2];
        elements[3].ShouldBeEquivalentTo(first);
        elements[4].ShouldBeEquivalentTo(second);
        elements[5].ShouldBeEquivalentTo(third);
        elements[6].ShouldBeEquivalentTo(first);
        elements[7].ShouldBeEquivalentTo(second);
        elements[8].ShouldBeEquivalentTo(third);
        elements[9].ShouldBeEquivalentTo(first);
    }
}
