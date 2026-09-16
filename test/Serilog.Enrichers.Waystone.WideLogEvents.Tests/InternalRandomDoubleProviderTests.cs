namespace Serilog.Enrichers.Waystone.WideLogEvents.Tests;

using Shouldly;
using Xunit;

// The range is the whole contract, and the only one worth pinning. Two draws being
// unequal is not: on net472 and net481 `new Random()` is seeded from the system
// clock, whose resolution is coarser than this loop, so consecutive draws there are
// equal and an assertion on distinctness would fail on two of the five frameworks.
public class InternalRandomDoubleProviderTests
{
    [Fact]
    public void NextDouble_StaysInsideTheIntervalTheSampleRatesAreCompared()
    {
        var provider = new InternalRandomDoubleProvider();

        for (var draw = 0; draw < 1_000; draw++)
        {
            double value = provider.NextDouble();

            value.ShouldBeInRange(0.0, 1.0);
            value.ShouldNotBe(1.0);
        }
    }
}
