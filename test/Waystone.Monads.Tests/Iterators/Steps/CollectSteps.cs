namespace Waystone.Monads.Iterators.Steps;

using System.Collections.Generic;
using System.Linq;
using Extensions;
using Reqnroll;
using Results;
using Results.Errors;
using Shouldly;

[Binding]
public class CollectSteps(ScenarioContext context)
{
    [When("collecting {string} of integers into a list")]
    public void WhenCollectingOfIntegersIntoAList(string enumerable)
    {
        var stored = context.Get<IEnumerable<int>>(enumerable);
        List<int> result = stored.IntoIter().Collect();
        context.Set(result, enumerable);
    }

    [Then("the {string} list of integers should contain")]
    public void ThenTheListOfIntegersShouldContain(
        string enumerable,
        Table table)
    {
        List<int> expected =
            table.Rows.Select(row => int.Parse(row["Value"])).ToList();
        var actual = context.Get<List<int>>(enumerable);

        actual.ShouldBe(expected);
    }

    [When("collecting {string} of integer results into a list")]
    public void WhenCollectingOfIntegerResultsIntoAList(string enumerable)
    {
        var stored = context.Get<List<Result<int, Error>>>(enumerable);
        Iter<Result<int, Error>> iter = stored.IntoIter();
        Result<List<int>, Error> result = iter.Collect();
        context.Set(result, enumerable);
    }

    [Then("the {string} should contain a list of integers with values")]
    public void ThenTheResultShouldContainAListOfIntegersWithValues(
        string enumerable,
        Table table)
    {
        var stored = context.Get<Result<List<int>, Error>>(enumerable);
        stored.IsOk.ShouldBeTrue();
        stored.Unwrap()
              .ShouldBe(
                   table.Rows.Select(row => int.Parse(row["Value"])).ToList());
    }

    [Then("the {string} should be an Err with message {string}")]
    public void ThenTheShouldBeAnErr(string enumerable, string message)
    {
        var stored = context.Get<Result<List<int>, Error>>(enumerable);
        stored.IsErr.ShouldBeTrue();
        stored.UnwrapErr().Message.ShouldBe(message);
    }
}
