namespace Serilog.Enrichers.Waystone.WideLogEvents.AspNetCore;

using System;
using Microsoft.AspNetCore.Builder;

public static class ApplicationBuilderExtensions
{
    extension(IApplicationBuilder app)
    {
        public IApplicationBuilder UseWideLogEvents(
            Action<WideLogEventsMiddlewareOptions>? configure = null)
        {
            var options = new WideLogEventsMiddlewareOptions();

            configure?.Invoke(options);

            app.UseMiddleware<WideLogEventsMiddleware>(options);

            return app;
        }
    }
}
