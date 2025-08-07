namespace Waystone.Monads.Iterators.Steps;

using System.Collections.Generic;
using System.Linq;
using Adapters;
using Extensions;
using Options;
using Reqnroll;
using Shouldly;

[Binding]
public class TakeSteps(ScenarioContext context)
{
    [When("taking the first {int} elements from the {string} integer iterator")]
    public void WhenTakingTheFirstElementsFromTheIntegerIterator(
        int p0,
        string enumerable)
    {
        var iter = context.Get<Iter<int>>(enumerable);
        Take<int> take = iter.Take(p0);
        context.Set(take, enumerable);
    }

    [Then("the {string} integer Take should return")]
    public void ThenTheIntegerTakeShouldReturn(string enumerable, Table table)
    {
        var take = context.Get<Take<int>>(enumerable);
        List<int> expected = table.Rows.Select(row => int.Parse(row["Value"]))
                                  .ToList();
        List<int> actual = take.Collect();

        actual.ShouldBe(expected);
    }

    [When("getting the {string} element of the {string} integer Take")]
    public void WhenGettingTheElementOfTheIntegerTake(
        string next,
        string enumerable)
    {
        var take = context.Get<Take<int>>(enumerable);
        Option<int> value = take.Next();
        context.Set(value, next);
    }

    [Then("the result of the {string} integer find should be None")]
    public void ThenTheResultOfTheIntegerFindShouldBeNone(string next)
    {
        var value = context.Get<Option<int>>(next);
        value.IsNone.ShouldBeTrue();
    }
}
