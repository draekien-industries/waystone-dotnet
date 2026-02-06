namespace Serilog.Enrichers.Waystone.WideLogEvents.AspNetCore.Tests;

using System.Threading.Tasks;
using global::Waystone.WideLogEvents;
using Microsoft.AspNetCore.Http;
using Shouldly;
using Xunit;

public class WideLogEventsMiddlewareTests
{
    [Fact]
    public async Task Middleware_Creates_And_Disposes_Scope_Around_Request()
    {
        var context = new DefaultHttpContext();
        var nextExecuted = false;

        var middleware = new WideLogEventsMiddleware(Next);

        await middleware.InvokeAsync(context);

        nextExecuted.ShouldBeTrue();

        // After request completes, scope must be disposed
        WideLogEventContext.GetScopedProperties().ShouldBeEmpty();

        return;

        async Task Next(HttpContext ctx)
        {
            // Inside request pipeline, scope should be active
            WideLogEventContext.PushProperty("fromNext", 1);

            WideLogEventContext.GetScopedProperties()
               .ContainsKey("fromNext")
               .ShouldBeTrue();

            nextExecuted = true;
            await Task.CompletedTask;
        }
    }
}
