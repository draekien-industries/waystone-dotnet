namespace Waystone.Monads.Iterators.Steps;

using System.Collections.Generic;
using System.Linq;
using Adapters;
using Extensions;
using Reqnroll;
using Shouldly;

[Binding]
public class EnumerateSteps(ScenarioContext context)
{
    [When("invoking enumerate on the {string} iterator of integers")]
    public void WhenInvokingEnumerateOnTheIteratorOfIntegers(string enumerable)
    {
        var iter = context.Get<Iter<int>>(enumerable);
        Enumerate<int> enumerate = iter.Enumerate();
        context.Set(enumerate, enumerable);
    }

    [Then("the {string} Enumerable should return")]
    public void ThenTheEnumerableShouldReturn(string enumerable, Table table)
    {
        var enumerate = context.Get<Enumerate<int>>(enumerable);
        List<(int Index, int Item)> expected = table.Rows
           .Select(row => (Index: int.Parse(row["Index"]),
                       Item: int.Parse(row["Value"])))
           .ToList();
        List<(int Index, int Item)> actual = enumerate.Collect();

        actual.ShouldBe(expected);
    }
}
