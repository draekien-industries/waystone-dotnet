namespace Waystone.Monads.Iterators.Steps;

using System.Collections.Generic;
using System.Linq;
using Adapters;
using Extensions;
using Reqnroll;
using Shouldly;

[Binding]
public class MapSteps(ScenarioContext context)
{
    [When("mapping {string} of integers into strings")]
    public void WhenMappingOfIntegersIntoStrings(string enumerable)
    {
        var elements = context.Get<Iter<int>>(enumerable);
        Map<int, string> map = elements.Map(x => x.ToString());
        context.Set(map, enumerable);
    }


    [Then("the elements of {string} Map should be the string values")]
    public void ThenTheElementsOfMapShouldBeTheStringValues(
        string enumerable,
        Table table)
    {
        var map = context.Get<Map<int, string>>(enumerable);
        List<string> expected = table.Rows.Select(row => row["Value"]).ToList();
        List<string> actual = map.Collect();

        actual.ShouldBeEquivalentTo(expected);
    }
}
