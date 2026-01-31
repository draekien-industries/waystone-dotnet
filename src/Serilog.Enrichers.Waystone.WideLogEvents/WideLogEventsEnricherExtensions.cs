namespace Serilog.Enrichers.Waystone.WideLogEvents;

using Configuration;

public static class WideLogEventsEnricherExtensions
{
    extension(LoggerEnrichmentConfiguration configuration)
    {
        public LoggerConfiguration FromWideLogEventsContext() =>
            configuration.With<WideLogEventsEnricher>();
    }
}
