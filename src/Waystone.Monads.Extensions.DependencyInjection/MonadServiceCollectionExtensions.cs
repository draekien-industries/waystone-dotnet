namespace Microsoft.Extensions.DependencyInjection;

using System;
using Extensions;
using Waystone.Monads.Configs;

/// <summary>Registers the Waystone.Monads configuration a service provider will later install.</summary>
public static class MonadServiceCollectionExtensions
{
    /// <summary>Registers configuration for the ambient Waystone.Monads options, to be installed when the provider is ready.</summary>
    /// <remarks>
    /// This call registers only. Nothing reaches
    /// <see cref="MonadOptions.Current" /> until
    /// <see cref="MonadServiceProviderExtensions.UseWaystoneMonads" /> runs
    /// against the built provider, because the container holds services the
    /// configuration needs and no container exists yet at registration time.
    /// Reads in between are answered from whatever options are already in effect
    /// — the library's defaults, unless <see cref="MonadOptions.Configure" /> ran
    /// first.
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
    /// none, so a subclass registered beforehand survives.
    /// </para>
    /// <para>
    /// Calling this more than once accumulates rather than conflicts. Each
    /// <paramref name="configure" /> is kept, and at install time they run in
    /// registration order over one shared builder, so a later call sees and can
    /// overwrite what an earlier one set. Everything else it does is idempotent,
    /// so repeated calls register one factory and mark the configuration pending
    /// once. That makes it safe for a library to call during its own registration
    /// without knowing whether the application already has.
    /// </para>
    /// </remarks>
    /// <param name="services">The service collection to register into.</param>
    /// <param name="configure">
    /// Configures the options, run at install time against a builder seeded from
    /// the options then in effect. Omit it to take the defaults plus whatever the
    /// container supplies.
    /// </param>
    /// <returns>
    /// A <see cref="MonadServicesBuilder" /> for chaining the calls that only
    /// make sense once registration has happened, such as
    /// <c>EnableInstallOnStart</c> from
    /// <c>Waystone.Monads.Extensions.Hosting</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="services" /> is null.</exception>
    public static MonadServicesBuilder AddWaystoneMonads(
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

        return new MonadServicesBuilder(services);
    }
}
