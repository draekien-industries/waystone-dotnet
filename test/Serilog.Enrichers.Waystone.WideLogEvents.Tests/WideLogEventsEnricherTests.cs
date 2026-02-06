namespace Serilog.Enrichers.Waystone.WideLogEvents.Tests;

using System.Collections.Generic;
using System.Linq;
using Core;
using Events;
using global::Waystone.WideLogEvents;
using Shouldly;
using Xunit;

public class WideLogEventsEnricherTests
{
    [Fact]
    public void Enricher_Adds_Context_Properties_To_LogEvent()
    {
        var sink = new InMemorySink();

        using Logger logger = new LoggerConfiguration()
           .Enrich.FromWideLogEventsContext()
           .WriteTo.Sink(sink)
           .CreateLogger();

        using (WideLogEventContext.BeginScope())
        {
            WideLogEventContext.PushProperty("userId", 42);
            WideLogEventContext.PushProperty("feature", "checkout");

            logger.Information("Test message");
        }

        sink.Events.Count.ShouldBe(1);
        LogEvent evt = sink.Events.Single();

        evt.Properties.ContainsKey("userId").ShouldBeTrue();

        evt.Properties["userId"]
           .ShouldBeOfType<ScalarValue>()
           .Value.ShouldBe(42);

        evt.Properties.ContainsKey("feature").ShouldBeTrue();

        evt.Properties["feature"]
           .ShouldBeOfType<ScalarValue>()
           .Value.ShouldBe("checkout");
    }

    [Fact]
    public void Enricher_Does_Not_Override_Existing_Property()
    {
        var sink = new InMemorySink();

        using Logger logger = new LoggerConfiguration()
           .Enrich.WithProperty("traceId", "existing")
           .Enrich.FromWideLogEventsContext()
           .WriteTo.Sink(sink)
           .CreateLogger();

        using (WideLogEventContext.BeginScope())
        {
            WideLogEventContext.PushProperty("traceId", "ctx");

            logger.Information("Hello");
        }

        LogEvent evt = sink.Events.Single();

        evt.Properties["traceId"]
           .ShouldBeOfType<ScalarValue>()
           .Value.ShouldBe("existing");
    }

    [Fact]
    public void Enricher_With_No_Scope_Adds_Nothing()
    {
        var sink = new InMemorySink();

        using Logger logger = new LoggerConfiguration()
           .Enrich.FromWideLogEventsContext()
           .WriteTo.Sink(sink)
           .CreateLogger();

        logger.Information("No scope");

        LogEvent evt = sink.Events.Single();
        evt.Properties.ContainsKey("anything").ShouldBeFalse();
    }

    private sealed class InMemorySink : ILogEventSink
    {
        public readonly List<LogEvent> Events = new();
        public void Emit(LogEvent logEvent) => Events.Add(logEvent);
    }
}
