namespace Waystone.Monads.Iterators.Steps;

using System.Collections.Generic;
using System.Linq;
using Extensions;
using Fixtures;
using Reqnroll;
using Shouldly;

[Binding]
public sealed class ClonedSteps(ScenarioContext context)
{
    [Given("an iterator {string} of a cloneable types")]
    public void GivenAnIteratorOfACloneableTypes(string iter)
    {
        List<TestCloneable> items = [new(), new(), new()];
        Iter<TestCloneable> cloneableIter = items.IntoIter();
        context.Set(cloneableIter, iter);
    }

    [When("cloning {string} into {string}")]
    public void WhenCloning(string iter, string clone)
    {
        var sourceIter = context.Get<Iter<TestCloneable>>(iter);
        Iter<TestCloneable> clonedIter = sourceIter.Cloned();
        context.Set(clonedIter, clone);
    }

    [Then(
        "the cloned iterator {string} should yield the cloned values of {string}")]
    public void ThenTheClonedIteratorShouldYieldTheSameValuesAs(
        string clone,
        string iter)
    {
        var clonedIter = context.Get<Iter<TestCloneable>>(clone);
        var sourceIter = context.Get<Iter<TestCloneable>>(iter);

        sourceIter.ShouldNotBeSameAs(clonedIter);
        sourceIter.Elements.ShouldNotBeSameAs(clonedIter.Elements);
        sourceIter.Elements.Select(x => x.Value)
                  .ShouldBe(clonedIter.Elements.Select(x => x.Value));
    }
}
