namespace Waystone.Monads.Iterators.Steps;

using System;
using System.Collections.Generic;
using Extensions;
using Reqnroll;
using Shouldly;

[Binding]
public class CompareSteps(ScenarioContext context)
{
    [When("comparing {string} and {string} of integers")]
    public void WhenComparingAndOfIntegers(string leftKey, string rightKey)
    {
        Iter<int> left = context.Get<IEnumerable<int>>(leftKey).IntoIter();
        Iter<int> right = context.Get<IEnumerable<int>>(rightKey).IntoIter();

        Ordering result = left.Compare(right);

        context.Set(result);
    }

    [Then("the Ordering should be {string}")]
    public void ThenTheOrderingShouldBe(string equal)
    {
        var result = context.Get<Ordering>();
        if (!Enum.TryParse(equal, out Ordering expected))
        {
            throw new ArgumentException($"Unknown ordering: {equal}");
        }

        result.ShouldBe(expected);
    }
}
