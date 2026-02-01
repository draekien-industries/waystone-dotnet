namespace Serilog.Enrichers.Waystone.WideLogEvents.AspNetCore;

using global::Waystone.WideLogEvents;
using Microsoft.AspNetCore.Http;

internal static class WideLogEventScopeExtensions
{
    extension(WideLogEventScope scope)
    {
        public void SetHttpRequestProperties(HttpRequest request)
        {
            scope.PushProperty(
                ReservedPropertyNames.HttpRequest,
                new
                {
                    request.Method,
                    Path = request.Path.ToString(),
                    PathBase = request.PathBase.ToString(),
                    request.Scheme,
                    Host = request.Host.ToString(),
                    request.ContentType,
                    request.ContentLength,
                    request.Protocol,
                    request.Query,
                });
        }

        public void SetHttpResponseProperties(HttpResponse response)
        {
            scope.PushProperty(
                ReservedPropertyNames.HttpResponse,
                new
                {
                    response.StatusCode,
                    response.ContentType,
                    response.ContentLength,
                });
        }
    }
}
