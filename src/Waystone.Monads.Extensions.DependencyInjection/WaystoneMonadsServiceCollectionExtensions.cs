namespace Microsoft.Extensions.DependencyInjection;

using System;
using Extensions;
using Waystone.Monads.Configs;
using Waystone.Monads.Extensions.DependencyInjection;

/// <summary>Registers the Waystone.Monads configuration a service provider will later install.</summary>
public static class WaystoneMonadsServiceCollectionExtensions
{
    /// <summary>Registers configuration for the ambient Waystone.Monads options, to be installed when the provider is ready.</summary>
    /// <remarks>
    /// This call registers only. Nothing reaches
    /// <see cref="MonadOptions.Current" /> until
    /// <see cref="WaystoneMonadsServiceProviderExtensions.UseWaystoneMonads" />
    /// runs against the built provider, because the container holds services the
    /// configuration needs and no container exists yet at registration time.
    /// Reads in between are answered from whatever options are already in effect
    /// — the library's defaults, unless
    /// <see cref="MonadOptions.Configure" /> ran first.
    /// <para>
    /// That gap is instrumented rather than enforced: a read taken after this
    /// call and before the install writes a
    /// <see cref="Waystone.Monads.Diagnostics.MonadDiagnostics.ConfigurationNotAppliedEventName" />
    /// event, so a forgotten install is visible to anything subscribed to the
    /// library's <see cref="System.Diagnostics.DiagnosticListener" />. Nothing
    /// throws, since options read early are valid options rather than a broken
    /// state.
    /// </para>
    /// <para>
    /// Registers an <see cref="ErrorCodeFactory" /> only if the collection has
    /// none, so a subclass registered beforehand survives. Call this more than
    /// once and every <paramref name="configure" /> runs at install time, in
    /// registration order, over one shared builder — later calls see and can
    /// overwrite what earlier ones set.
    /// </para>
    /// </remarks>
    /// <param name="services">The service collection to register into.</param>
    /// <param name="configure">
    /// Configures the options, run at install time against a builder seeded from
    /// the options then in effect. Omit it to take the defaults plus whatever the
    /// container supplies.
    /// </param>
    /// <returns>The service collection, for chaining more registrations.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services" /> is null.</exception>
    public static IServiceCollection AddWaystoneMonads(
        this IServiceCollection services,
        Action<MonadOptionsBuilder>? configure = null)
    {
        if (services is null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        services.TryAddSingleton<ErrorCodeFactory>();

        if (configure is not null)
        {
            services.AddSingleton(new MonadOptionsRegistration(configure));
        }

        MonadOptions.MarkConfigurationPending();

        return services;
    }
}
