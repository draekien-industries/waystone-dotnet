namespace Waystone.Monads.Iterators.Steps;

using System.Collections.Generic;
using System.Linq;
using Adapters;
using Extensions;
using Reqnroll;
using Shouldly;

[Binding]
public class FlatMapSteps(ScenarioContext context)
{
    [When("invoking FlatMap on the {string} of words to extract characters")]
    public void WhenInvokingFlatMapOnTheOfWordsToExtractCharacters(
        string enumerable)
    {
        var iter = context.Get<Iter<string>>(enumerable);
        FlatMap<string, char>
            flatMap = iter.FlatMap(word => word.ToCharArray());
        context.Set(flatMap, enumerable);
    }

    [Then("the {string} of characters FlatMap should return")]
    public void ThenTheOfCharactersFlatMapShouldReturn(
        string enumerable,
        Table table)
    {
        var flatMap = context.Get<FlatMap<string, char>>(enumerable);
        List<char> expected =
            table.Rows.Select(row => row["Value"][0]).ToList();
        List<char> actual = flatMap.Collect();

        actual.ShouldBe(expected);
    }
}
