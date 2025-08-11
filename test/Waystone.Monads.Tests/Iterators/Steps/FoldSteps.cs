namespace Waystone.Monads.Iterators.Steps;

using Reqnroll;
using Shouldly;

[Binding]
public class FoldSteps(ScenarioContext context)
{
    [When("folding the {string} integer iterator with an add operation")]
    public void WhenFoldingTheIntegerIteratorWithAnAddOperation(
        string enumerable)
    {
        var iter = context.Get<Iter<int>>(enumerable);
        int result = iter.Fold(0, (acc, x) => acc + x);
        context.Set(result, enumerable);
    }

    [Then("the result of the folded {string} should be {int}")]
    public void ThenTheResultOfTheFoldedShouldBe(string enumerable, int p1)
    {
        var result = context.Get<int>(enumerable);
        result.ShouldBe(p1);
    }
}
