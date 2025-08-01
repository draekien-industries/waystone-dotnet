namespace Waystone.Monads.Iterators.Steps;

using System.Collections.Generic;
using Extensions;
using Reqnroll;

[Binding]
public sealed class ChainSteps(ScenarioContext context)
{
    [When("chaining {string} and {string} of integers into {string}")]
    public void WhenChainingAnd(string first, string second, string result)
    {
        Iter<int> iter0 = context.Get<IEnumerable<int>>(first).IntoIter();
        Iter<int> iter1 = context.Get<IEnumerable<int>>(second).IntoIter();
        Iter<int> chained = iter0.Chain(iter1);
        context.Set(chained, result);
    }
}
