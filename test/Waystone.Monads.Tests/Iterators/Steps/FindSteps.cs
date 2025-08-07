namespace Waystone.Monads.Iterators.Steps;

using Options;
using Reqnroll;
using Shouldly;

[Binding]
public class FindSteps(ScenarioContext context)
{
    [When(
        "finding the {string} element of {string} integer iterator that is greater than {int}")]
    public void WhenFindingTheFirstElementOfIntegerIteratorThatIsGreaterThan(
        string actualKey,
        string enumerable,
        int p1)
    {
        var elements = context.Get<Iter<int>>(enumerable);
        Option<int> first = elements.Find(x => x > p1);
        context.Set(first, actualKey);
    }

    [Then("the result of the {string} integer find should be {int}")]
    public void ThenTheResultOfTheIntegerFindShouldBe(string key, int p1)
    {
        var first = context.Get<Option<int>>(key);
        first.IsSome.ShouldBeTrue();
        first.Unwrap().ShouldBe(p1);
    }
}
