namespace Serilog.Enrichers.Waystone.WideLogEvents.Tests;

using System.Collections.Generic;
using Core;
using Events;
using NSubstitute;
using Shouldly;
using Xunit;

public class WideLogEventsFilterExtensionsTests
{
    [Theory]
    [InlineData(0.4, 1)]
    [InlineData(0.5, 1)]
    [InlineData(0.6, 0)]
    public void WithWideLogEventsSampling_SamplesAtTheConfiguredRate(
        double randomValue,
        int emitted)
    {
        var sink = new CollectingSink();

        using Logger logger =
            new LoggerConfiguration()
               .Filter.WithWideLogEventsSampling(
                    options =>
                    {
                        options.InformationSampleRate = 0.5;
                        options.RandomDoubleProvider = Drawing(randomValue);
                    })
               .WriteTo.Sink(sink)
               .CreateLogger();

        logger.Information("a clerk took the errand");

        sink.Events.Count.ShouldBe(emitted);
    }

    // The default ErrorSampleRate is 1.0 and NextDouble draws below 1.0, so an error
    // survives whatever the default provider returns. Every other level's default
    // rate is low enough that asserting on it would be asserting on a coin toss.
    [Fact]
    public void WithWideLogEventsSampling_GivenNoConfiguration_KeepsAnError()
    {
        var sink = new CollectingSink();

        using Logger logger = new LoggerConfiguration()
                             .Filter.WithWideLogEventsSampling()
                             .WriteTo.Sink(sink)
                             .CreateLogger();

        logger.Error("every clerk is holding something");

        sink.Events.ShouldHaveSingleItem().Level.ShouldBe(LogEventLevel.Error);
    }

    private static IRandomDoubleProvider Drawing(double value)
    {
        var provider = Substitute.For<IRandomDoubleProvider>();
        provider.NextDouble().Returns(value);

        return provider;
    }

    private sealed class CollectingSink : ILogEventSink
    {
        public List<LogEvent> Events { get; } = [];

        public void Emit(LogEvent logEvent)
        {
            Events.Add(logEvent);
        }
    }
}
