namespace Serilog.Enrichers.Waystone.WideLogEvents;

using System;
using Configuration;

public static class WideLogEventsFilterExtensions
{
    extension(LoggerFilterConfiguration configuration)
    {
        /// <summary>
        /// Applies wide log event sampling to the logger configuration.
        /// This method allows for configuring sampling rates of log events
        /// at various levels of severity to control logging frequency
        /// and reduce log volume without losing critical information.
        /// </summary>
        /// <param name="configure">
        /// An optional action to configure the <see cref="WideLogEventsSamplingOptions" />
        /// for customizing sampling rates and behaviors. If not specified, default
        /// options are applied.
        /// </param>
        /// <returns>
        /// A <see cref="LoggerConfiguration" /> instance, enabling method chaining for
        /// additional logger configuration.
        /// </returns>
        public LoggerConfiguration WithWideLogEventsSampling(
            Action<WideLogEventsSamplingOptions>? configure = null)
        {
            var options = new WideLogEventsSamplingOptions();

            configure?.Invoke(options);

            var filter = new WideLogEventsSamplingFilter(options);

            return configuration.With(filter);
        }
    }
}
