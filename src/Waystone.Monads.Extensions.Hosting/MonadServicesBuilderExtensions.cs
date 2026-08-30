namespace Microsoft.Extensions.DependencyInjection;

using System;
using Extensions;
using Hosting;

/// <summary>Hands the install of the registered Waystone.Monads configuration to the host.</summary>
public static class MonadServicesBuilderExtensions
{
    /// <summary>Arranges for the host to install the registered configuration as it starts.</summary>
    /// <remarks>
    /// This is the whole package. <c>AddWaystoneMonads</c>
    /// registers configuration but cannot install it, because the container it
    /// needs does not exist yet; on a host this removes the second call rather
    /// than trusting an application to remember
    /// <see cref="MonadServiceProviderExtensions.UseWaystoneMonads" />. It hangs
    /// off the builder <c>AddWaystoneMonads</c> returns, so asking for the
    /// install without first asking for the registration does not compile.
    /// <para>
    /// The install runs in <see cref="IHostedLifecycleService.StartingAsync" />,
    /// which the host calls on every hosted service before
    /// <see cref="IHostedService.StartAsync" /> on any of them. So registration
    /// order does not matter: a background service that reads
    /// <see cref="Waystone.Monads.Configs.MonadOptions" /> in its own
    /// <c>StartAsync</c> sees the installed options whether it was registered
    /// before this call or after.
    /// </para>
    /// <para>
    /// Work done before the host starts is still too early — a read taken while
    /// the service collection is being populated, or between <c>Build()</c> and
    /// <c>Run()</c>, runs ahead of any hosted service. Such a read is answered
    /// from the defaults and reported through the
    /// <see cref="Waystone.Monads.Diagnostics.MonadDiagnostics.ConfigurationNotAppliedEventName" />
    /// event, exactly as it is without this package.
    /// </para>
    /// <para>
    /// Calling this twice installs once: the registration is deduplicated on the
    /// implementation type.
    /// </para>
    /// </remarks>
    /// <param name="builder">
    /// The builder returned by <c>AddWaystoneMonads</c>, whose services the
    /// installer is registered into.
    /// </param>
    /// <returns>The builder, for chaining more configurations.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="builder" /> is null.</exception>
    public static MonadServicesBuilder EnableInstallOnStart(
        this MonadServicesBuilder builder)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IHostedService, MonadOptionsInstaller>());

        return builder;
    }
}
