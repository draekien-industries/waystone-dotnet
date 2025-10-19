namespace Waystone.Monads.Iterators.Steps;

using System.Linq;
using Extensions;
using Reqnroll;
using Shouldly;

[Binding]
public class FlattenSteps(ScenarioContext context)
{
    [When("the {string} 2D iterator is flattened into {string}")]
    public void WhenTheDIteratorIsFlattened(string p0, string p1)
    {
        var iter = context.Get<Iter<Iter<int>>>(p0);
        Iter<int> flattened = iter.Flatten();
        context.Set(flattened, p1);
    }

    [Then(
        "the {string} result should be a 1D iterator containing the integers")]
    public void ThenTheResultShouldBeAdIteratorContainingTheIntegers(string p0)
    {
        var flattened = context.Get<Iter<int>>(p0);
        int[] expected = [1, 2, 3, 4, 5, 6, 7, 8, 9];
        int[] actual = flattened.Collect().ToArray();

        actual.ShouldBe(expected);
    }

    [Given("a 2D iterator of integers {string}")]
    public void GivenAdIteratorOfIntegers(string p1)
    {
        int[][] arrays =
        [
            [1, 2, 3],
            [4, 5, 6],
            [7, 8, 9],
        ];
        Iter<Iter<int>> iter2D = arrays
                                .Select(arr => arr.IntoIter())
                                .IntoIter();

        context.Set(iter2D, p1);
    }
}
