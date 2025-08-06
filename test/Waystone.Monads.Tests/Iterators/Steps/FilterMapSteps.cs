namespace Waystone.Monads.Iterators.Steps;

using System.Collections.Generic;
using System.Linq;
using Adapters;
using Extensions;
using Options;
using Reqnroll;
using Shouldly;

[Binding]
public class FilterMapSteps(ScenarioContext context)
{
    [When(
        "filtering {string} of integers to only even numbers and mapping them into strings")]
    public void
        WhenFilteringOfIntegersToOnlyEvenNumbersAndMappingThemIntoStrings(
            string enumerable)
    {
        var elements = context.Get<Iter<int>>(enumerable);
        FilterMap<int, string> filterMap =
            elements.FilterMap(x => x % 2 == 0
                                   ? Option.Some(x.ToString())
                                   : Option.None<string>());
        context.Set(filterMap, enumerable);
    }

    [Then("the elements of {string} FilterMap should be the string values")]
    public void ThenTheElementsOfFilterMapShouldBeTheStringValues(
        string enumerable,
        Table table)
    {
        var filterMap = context.Get<FilterMap<int, string>>(enumerable);
        List<string> expected = table.Rows.Select(row => row["Value"]).ToList();
        List<string> actual = filterMap.Collect();

        actual.ShouldBeEquivalentTo(expected);
    }
}
