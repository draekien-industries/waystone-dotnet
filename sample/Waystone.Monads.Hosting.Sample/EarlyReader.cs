namespace Waystone.Monads.Hosting.Sample;

using Microsoft.Extensions.Hosting;

internal sealed class EarlyReader : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        Report.OptionsInEffect(
            "read by a hosted service registered before the installer");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) =>
        Task.CompletedTask;
}
