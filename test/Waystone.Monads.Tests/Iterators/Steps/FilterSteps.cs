namespace Waystone.Monads.Iterators.Steps;

using System.Collections.Generic;
using System.Linq;
using Adapters;
using Extensions;
using Reqnroll;
using Shouldly;

[Binding]
public class FilterSteps(ScenarioContext context)
{
    [When(
        "invoking filter on the {string} iterator of integers with a predicate that returns true for even numbers")]
    public void
        WhenInvokingFilterOnTheIteratorOfIntegersWithAPredicateThatReturnsTrueForEvenNumbers(
            string enumerable)
    {
        var iter = context.Get<Iter<int>>(enumerable);
        Filter<int> filter = iter.Filter(x => x % 2 == 0);
        context.Set(filter, enumerable);
    }

    [Then("the {string} integer Filter should return")]
    public void ThenTheIntegerFilterShouldReturn(string enumerable, Table table)
    {
        var filter = context.Get<Filter<int>>(enumerable);
        List<int> expected = table.Rows.Select(row => int.Parse(row["Value"]))
                                  .ToList();
        List<int> actual = filter.Collect();

        actual.ShouldBe(expected);
    }
}
