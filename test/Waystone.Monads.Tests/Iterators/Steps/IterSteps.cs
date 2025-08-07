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

    [Then(
        "the {string} iterator of integers should be equal to {string} iterator of integers")]
    public void ThenTheIteratorShouldBeEqualToIterator(string p0, string p1)
    {
        var left = context.Get<Iter<int>>(p0);
        var right = context.Get<Iter<int>>(p1);
        left.Equals(right).ShouldBeTrue();
    }

    [Then(
        "the {string} enumerable of integers should be equal to {string} enumerable of integers")]
    public void ThenTheEnumerableShouldBeEqualToEnumerable(string p0, string p1)
    {
        var left = context.Get<IEnumerable<int>>(p0);
        var right = context.Get<IEnumerable<int>>(p1);
        left.IntoIter().Equals(right.IntoIter()).ShouldBeTrue();
    }

    [Then(
        "the {string} iterator of integers should not be equal to {string} iterator of integers")]
    public void ThenTheIteratorOfIntegersShouldNotBeEqualToIteratorOfIntegers(
        string p0,
        string p1)
    {
        var left = context.Get<Iter<int>>(p0);
        var right = context.Get<Iter<int>>(p1);
        left.Equals(right).ShouldBeFalse();
    }

    [Given("an {string} of chars from {string} to {string}")]
    public void GivenAnOfCharsFromTo(string p0, char a, char e)
    {
        IEnumerable<char> enumerable = Enumerable.Range(a, e - a + 1)
                                                 .Select(i => (char)i);
        context.Set(enumerable, p0);
    }

    [When("converting {string} of chars to an iterator")]
    public void WhenConvertingOfCharsToAnIterator(string p0)
    {
        var enumerable = context.Get<IEnumerable<char>>(p0);
        Iter<char> iter = enumerable.IntoIter();
        context.Set(iter, p0);
    }

    [Then(
        "the {string} iterator of chars should not be equal to {string} iterator of integers")]
    public void ThenTheIteratorOfCharsShouldNotBeEqualToIteratorOfIntegers(
        string p0,
        string p1)
    {
        var left = context.Get<Iter<char>>(p0);
        var right = context.Get<Iter<int>>(p1);
        // ReSharper disable once SuspiciousTypeConversion.Global
        left.Equals(right).ShouldBeFalse();
    }

    [When("getting the {string} element of the {string} integer iterator")]
    public void WhenGettingTheNextElementOfTheIntegerIterator(
        string nextKey,
        string enumerable)
    {
        var iter = context.Get<Iter<int>>(enumerable);
        Option<int> next = iter.Next();
        context.Set(next, nextKey);
    }
}
