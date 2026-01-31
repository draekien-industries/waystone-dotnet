namespace Serilog.Enrichers.Waystone.WideLogEvents;

using System;
using System.Collections.Generic;
using Core;
using Events;
using global::Waystone.WideLogEvents;

public class WideLogEventsEnricher : ILogEventEnricher
{
    /// <inheritdoc />
    public void Enrich(
        LogEvent logEvent,
        ILogEventPropertyFactory propertyFactory)
    {
        if (logEvent is null) throw new ArgumentNullException(nameof(logEvent));

        if (propertyFactory is null)
        {
            throw new ArgumentNullException(nameof(propertyFactory));
        }

        foreach (KeyValuePair<string, object?> property in WideLogEventContext
                    .GetProperties())
        {
            LogEventProperty logEventProperty = propertyFactory.CreateProperty(
                property.Key,
                property.Value,
                true);

            logEvent.AddPropertyIfAbsent(logEventProperty);
        }
    }
}
