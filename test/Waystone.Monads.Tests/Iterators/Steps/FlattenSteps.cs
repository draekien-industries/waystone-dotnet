namespace Waystone.Monads.Iterators.Steps;

using Extensions;
using Reqnroll;
using Shouldly;

[Binding]
public class FlattenSteps(ScenarioContext context)
{
    [Given("a 2D array of integers {string}")]
    public void GivenAdArrayOfIntegers(string p1)
    {
        int[][] value = [[1, 2, 3], [4, 5, 6], [7, 8, 9]];
        context.Set(value, p1);
    }

    [Given("the {string} 2D array is converted into a 2D iterator")]
    public void GivenTheDArrayIsConvertedIntoAdIterator(string p0)
    {
        int[][] value = context.Get<int[][]>(p0);
        Iter<int[]> iter = value.IntoIter();
        context.Set(iter, p0);
    }


    [When("the {string} 2D iterator is flattened into {string}")]
    public void WhenTheDIteratorIsFlattened(string p0, string p1)
    {
        var iter = context.Get<Iter<int[]>>(p0);
        Iter<int> flattened = iter.Flatten<int[], int>();
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
}
