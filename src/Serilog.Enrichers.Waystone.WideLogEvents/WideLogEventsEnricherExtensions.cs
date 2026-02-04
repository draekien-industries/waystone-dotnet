namespace Serilog.Enrichers.Waystone.WideLogEvents;

using Configuration;
using global::Waystone.WideLogEvents;

public static class WideLogEventsEnricherExtensions
{
    extension(LoggerEnrichmentConfiguration configuration)
    {
        /// <summary>
        /// Adds the <see cref="WideLogEventsEnricher" /> to the logger configuration,
        /// enhancing log events
        /// with scoped properties from the
        /// <see cref="WideLogEventContext" />.
        /// </summary>
        /// <returns>
        /// A <see cref="LoggerConfiguration" /> that allows additional configuration
        /// chaining.
        /// </returns>
        public LoggerConfiguration FromWideLogEventsContext() =>
            configuration.With<WideLogEventsEnricher>();
    }
}
