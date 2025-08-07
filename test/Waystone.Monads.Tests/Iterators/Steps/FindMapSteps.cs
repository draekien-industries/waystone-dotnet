namespace Waystone.Monads.Iterators.Steps;

using System;
using Options;
using Reqnroll;

[Binding]
public class FindMapSteps(ScenarioContext context)
{
    [When(
        "finding the {string} element of {string} integer iterator that is greater than {int} and mapping it to its square")]
    public void
        WhenFindingTheElementOfIntegerIteratorThatIsGreaterThanAndMappingItToItsSquare(
            string first,
            string enumerable,
            int p2)
    {
        var iter = context.Get<Iter<int>>(enumerable);
        Option<int> result =
            iter.FindMap(x => x > p2
                             ? Option.Some((int)Math.Pow(x, 2))
                             : Option.None<int>());
        context.Set(result, first);
    }
}
