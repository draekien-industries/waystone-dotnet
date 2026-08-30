namespace Waystone.Monads.PreviousMajor.Sample;

using System;
using Waystone.Monads.Configs;

/// <summary>
/// Startup configuration as it is written today: one static <c>Configure</c> call
/// at the entry point, and a <c>BeginScope</c> around a block that needs different
/// settings. <c>DRA-123</c> moves configuration to dependency injection, so this
/// file is the inventory of what a consumer has to rewrite; <c>DRA-129</c> removes
/// <c>UseExceptionLogger</c>, which this file is the only caller of.
/// </summary>
internal static class Configuration
{
    internal static void AtStartup() =>
        MonadOptions.Configure(
            options => options
                      .UseFallbackErrorCode("order.unknown")
                      .UseFallbackErrorMessage("something went wrong")
                      .UseCancellationAsFailure()
                      .UseErrorCodeFactory(new ErrorCodeFactory())
                      .UseExceptionLogger(
                           (exception, caller) => Console.WriteLine(
                               $"{caller.MemberName}: {exception.Message}")));

    internal static string InsideAScope()
    {
        using MonadOptionsScope scope = MonadOptions.BeginScope(
            options => options.UseFallbackErrorCode("order.scoped"));

        return Chains.Describe(1);
    }
}
