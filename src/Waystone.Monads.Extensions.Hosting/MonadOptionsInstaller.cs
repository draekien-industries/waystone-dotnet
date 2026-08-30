namespace Microsoft.Extensions.Hosting;

using System;
using System.Threading;
using System.Threading.Tasks;
using DependencyInjection;

internal sealed class MonadOptionsInstaller : IHostedLifecycleService
{
    private readonly IServiceProvider _provider;

    public MonadOptionsInstaller(IServiceProvider provider)
    {
        _provider = provider;
    }

    public Task StartingAsync(CancellationToken cancellationToken)
    {
        _provider.UseWaystoneMonads();
        return Task.CompletedTask;
    }

    public Task StartAsync(CancellationToken cancellationToken) =>
        Task.CompletedTask;

    public Task StartedAsync(CancellationToken cancellationToken) =>
        Task.CompletedTask;

    public Task StoppingAsync(CancellationToken cancellationToken) =>
        Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken) =>
        Task.CompletedTask;

    public Task StoppedAsync(CancellationToken cancellationToken) =>
        Task.CompletedTask;
}
