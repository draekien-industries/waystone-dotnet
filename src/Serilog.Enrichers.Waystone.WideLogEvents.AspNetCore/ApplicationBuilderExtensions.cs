namespace Serilog.Enrichers.Waystone.WideLogEvents.AspNetCore;

using Microsoft.AspNetCore.Builder;

public static class ApplicationBuilderExtensions
{
    extension(IApplicationBuilder app)
    {
        public IApplicationBuilder UseWideLogEventsContext()
        {
            app.UseMiddleware<WideLogEventsMiddleware>();

            return app;
        }
    }
}
