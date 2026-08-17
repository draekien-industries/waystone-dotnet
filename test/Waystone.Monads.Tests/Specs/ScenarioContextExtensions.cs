namespace Waystone.Monads.Specs;

using System;
using System.Threading.Tasks;
using Reqnroll;

internal static class ScenarioContextExtensions
{
    internal static async Task CaptureAsync<T>(
        this ScenarioContext context,
        Func<Task<T>> operation)
    {
        try
        {
            context.Set(
                await operation().ConfigureAwait(false),
                Constants.ResultKey);
        }
        catch (Exception ex)
        {
            context.Set(ex, Constants.ExceptionKey);
        }
    }

    internal static Exception GetCapturedException(
        this ScenarioContext context) =>
        context.Get<Exception>(Constants.ExceptionKey);
}
