namespace Waystone.Monads.Iterators.Steps;

using Adapters;
using Options;
using Reqnroll;
using Shouldly;

[Binding]
public sealed class FuseSteps(ScenarioContext context)
{
    [When("the number iterator is fused")]
    public void WhenTheNumberIteratorIsFused()
    {
        var numbers = context.Get<Iter<int>>("Numbers");
        Fuse<int> fused = numbers.Fuse();
        context.Set(fused, "FusedNumbers");
    }

    [When("next is invoked on number iterator {int} times")]
    public void WhenNextIsInvokedOnNumberIteratorTimes(int p0)
    {
        var fused = context.Get<Fuse<int>>("FusedNumbers");
        for (var i = 0; i < p0; i++)
        {
            Option<int> next = fused.Next();
            context.Set(next, $"FusedNumbers.Next({i})");
        }
    }

    [Then("the first {int} results should be some")]
    public void ThenTheFirstResultsShouldBeSome(int p0)
    {
        for (var i = 0; i < p0; i++)
        {
            var next = context.Get<Option<int>>($"FusedNumbers.Next({i})");
            next.IsSome.ShouldBeTrue();
        }
    }

    [Then("the next {int} results should be none")]
    public void ThenTheNextResultsShouldBeNone(int p0)
    {
        for (var i = 0; i < p0; i++)
        {
            var next = context.Get<Option<int>>($"FusedNumbers.Next({i + 2})");
            next.IsNone.ShouldBeTrue();
        }
    }
}
