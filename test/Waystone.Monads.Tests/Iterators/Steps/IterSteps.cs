namespace Waystone.Monads.Iterators.Steps;

using System.Collections.Generic;
using System.Linq;
using Extensions;
using Options;
using Reqnroll;
using Results;
using Results.Errors;
using Shouldly;

[Binding]
public sealed class IterSteps(ScenarioContext context)
{
    [Given("an {string} of integers from {int} to {int}")]
    public void GivenAnEnumerableOfIntegersFromTo(
        string key,
        int start,
        int end)
    {
        IEnumerable<int> enumerable = Enumerable.Range(start, end - start + 1);
        context.Set(enumerable, key);
    }

    [When("converting {string} of integers to an iterator")]
    public void WhenIConvertItToAnIterator(string key)
    {
        var enumerable = context.Get<IEnumerable<int>>(key);
        Iter<int> iter = enumerable.IntoIter();
        context.Set(iter, key);
    }

    [Then("the {string} iterator of integers should yield")]
    public void ThenTheIteratorOfIntegersShouldYield(
        string enumerable,
        Table table)
    {
        var iter = context.Get<Iter<int>>(enumerable);
        List<int> expected =
            table.Rows.Select(row => int.Parse(row["Value"])).ToList();
        List<int> actual = iter.Elements.ToList();

        actual.ShouldBe(expected);
    }

    [Given("an empty {string} of integers")]
    public void GivenAnEmptyOfIntegers(string key)
    {
        IEnumerable<int> enumerable = [];
        context.Set(enumerable, key);
    }

    [Given("an {string} of integer results")]
    public void GivenAnOfIntegerResults(string enumerable, Table table)
    {
        List<Result<int, Error>> results = [];
        results.AddRange(
            table.Rows.Select(row => row["Type"] == "Ok"
                                  ? Result.Ok<int, Error>(
                                      int.Parse(row["Value"]))
                                  : Result.Err<int, Error>(
                                      new Error("testing", row["Value"]))));

        context.Set(results, enumerable);
    }

    [Then(
        "the size hint of the {string} iterator should have a lower bound of {int}")]
    public void ThenTheSizeHintOfTheIteratorShouldHaveALowerBoundOf(
        string enumerable,
        int p1)
    {
        var iter = context.Get<Iter<int>>(enumerable);
        (int Lower, Option<int> Upper) sizeHint = iter.SizeHint();
        sizeHint.Lower.ShouldBe(p1);
    }

    [Then(
        "the size hint of the {string} iterator should have an upper bound of {int}")]
    public void ThenTheSizeHintOfTheIteratorShouldHaveAnUpperBoundOf(
        string enumerable,
        int p1)
    {
        var iter = context.Get<Iter<int>>(enumerable);
        (int Lower, Option<int> Upper) sizeHint = iter.SizeHint();
        sizeHint.Upper.IsSome.ShouldBeTrue();
        sizeHint.Upper.Unwrap().ShouldBe(p1);
    }

    [When("invoking Next on {string} iterator")]
    public void WhenInvokingNextOnIterator(string enumerable)
    {
        var iter = context.Get<Iter<int>>(enumerable);
        Option<int> next = iter.Next();
        context.Set(iter, enumerable);
        context.Set(next, $"{enumerable}.Next");
    }

    [Then(
        "the size hint of the {string} iterator should have an upper bound of None")]
    public void ThenTheSizeHintOfTheIteratorShouldHaveAnUpperBoundOfNone(
        string enumerable)
    {
        var iter = context.Get<Iter<int>>(enumerable);
        (int Lower, Option<int> Upper) sizeHint = iter.SizeHint();
        sizeHint.Upper.IsNone.ShouldBeTrue();
    }
}
