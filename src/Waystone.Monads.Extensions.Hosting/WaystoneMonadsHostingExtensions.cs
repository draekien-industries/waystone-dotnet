namespace Microsoft.Extensions.DependencyInjection;

using System;
using Extensions;
using Hosting;
using Waystone.Monads.Extensions.Hosting;

/// <summary>Installs the registered Waystone.Monads configuration from the host's start-up sequence.</summary>
public static class WaystoneMonadsHostingExtensions
{
    /// <summary>Arranges for the host to install the registered configuration as it starts.</summary>
    /// <remarks>
    /// This is the whole package.
    /// <see cref="WaystoneMonadsServiceCollectionExtensions.AddWaystoneMonads" />
    /// registers configuration but cannot install it, because the container it
    /// needs does not exist yet; on a host, this removes the second call rather
    /// than trusting an application to remember
    /// <see cref="WaystoneMonadsServiceProviderExtensions.UseWaystoneMonads" />.
    /// Call it alongside <c>AddWaystoneMonads</c> — it registers the installer
    /// and nothing else, so on its own it installs the defaults.
    /// <para>
    /// The install runs in <see cref="IHostedLifecycleService.StartingAsync" />,
    /// which the host calls before <see cref="IHostedService.StartAsync" /> on
    /// every hosted service including this one. So registration order does not
    /// matter: a background service that reads
    /// <see cref="Waystone.Monads.Configs.MonadOptions" /> in its own
    /// <c>StartAsync</c> sees the installed options whether it was registered
    /// before this call or after.
    /// </para>
    /// <para>
    /// Work done before the host starts is still too early — a read taken while
    /// the service collection is being populated, or between
    /// <c>Build()</c> and <c>Run()</c>, runs ahead of any hosted service. Such a
    /// read is answered from the defaults and reported through the
    /// <see cref="Waystone.Monads.Diagnostics.MonadDiagnostics.ConfigurationNotAppliedEventName" />
    /// event, exactly as it is without this package.
    /// </para>
    /// <para>
    /// Registering the installer twice installs once: the registration is
    /// deduplicated on the implementation type.
    /// </para>
    /// </remarks>
    /// <param name="services">The service collection to register the installer into.</param>
    /// <returns>The service collection, for chaining more registrations.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services" /> is null.</exception>
    public static IServiceCollection InstallWaystoneMonadsOnStart(
        this IServiceCollection services)
    {
        if (services is null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        services.TryAddEnumerable(
            ServiceDescriptor
               .Singleton<IHostedService, WaystoneMonadsInstaller>());

        return services;
    }
}
