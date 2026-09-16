namespace Serilog.Enrichers.Waystone.WideLogEvents.AspNetCore.Tests;

using System.Threading.Tasks;
using global::Waystone.WideLogEvents;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

public class ApplicationBuilderExtensionsTests
{
    [Fact]
    public async Task UseWideLogEventsContext_OpensAScopeAroundTheRestOfThePipeline()
    {
        var app = new ApplicationBuilder(new ServiceCollection().BuildServiceProvider());
        var scopedInsideTheRequest = false;

        app.UseWideLogEventsContext();

        app.Run(
            _ =>
            {
                WideLogEventContext.PushProperty("clerk", "Pumat Sol");

                scopedInsideTheRequest = WideLogEventContext.GetScopedProperties()
                   .ContainsKey("clerk");

                return Task.CompletedTask;
            });

        await app.Build()(new DefaultHttpContext());

        scopedInsideTheRequest.ShouldBeTrue();
        WideLogEventContext.GetScopedProperties().ShouldBeEmpty();
    }
}
