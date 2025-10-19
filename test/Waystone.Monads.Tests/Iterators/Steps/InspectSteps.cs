namespace Waystone.Monads.Iterators.Steps;

using System;
using Extensions;
using NSubstitute;
using Reqnroll;

[Binding]
public sealed class InspectSteps(ScenarioContext context)
{
    private readonly Action<int> _mockLogger = Substitute.For<Action<int>>();

    [When(
        "inspecting each element of {string} iterator of integers to log {string}")]
    public void WhenInspectingEachElementOfIteratorOfIntegersToLog(
        string key,
        string logTemplate)
    {
        var enumerable = context.Get<Iter<int>>(key);
        enumerable.Inspect(item => _mockLogger(item)).Collect();
    }

    [Then("the log should contain")]
    public void ThenTheLogShouldContain(Table table)
    {
        _mockLogger.Received(table.RowCount).Invoke(Arg.Any<int>());
    }
}
