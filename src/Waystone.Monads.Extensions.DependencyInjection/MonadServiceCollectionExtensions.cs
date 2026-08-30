namespace Microsoft.Extensions.DependencyInjection;

using System;
using Extensions;
using Waystone.Monads.Configs;

/// <summary>Registers the Waystone.Monads configuration a service provider will later install.</summary>
public static class MonadServiceCollectionExtensions
{
    /// <summary>Asks for the ambient Waystone.Monads options as they come, with nothing configured on top.</summary>
    /// <remarks>
    /// The defaults, plus the <see cref="ErrorCodeFactory" /> the container holds.
    /// Registration and install work as the overload taking a delegate documents;
    /// this one simply registers no delegate.
    /// </remarks>
    /// <param name="services">The service collection to register into.</param>
    /// <returns>
    /// A <see cref="MonadServicesBuilder" /> for chaining the calls that only
    /// make sense once registration has happened, such as
    /// <c>EnableInstallOnStart</c> from
    /// <c>Waystone.Monads.Extensions.Hosting</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="services" /> is null.</exception>
    public static MonadServicesBuilder AddWaystoneMonads(
        this IServiceCollection services) =>
        Register(services, registration: null);

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
    /// overwrite what an earlier one set. Delegates registered through the
    /// <see cref="AddWaystoneMonads(IServiceCollection, Action{IServiceProvider, MonadOptionsBuilder})" />
    /// overload share that one order. Everything else it does is idempotent, so
    /// repeated calls register one factory and mark the configuration pending
    /// once. That makes it safe for a library to call during its own registration
    /// without knowing whether the application already has.
    /// </para>
    /// </remarks>
    /// <param name="services">The service collection to register into.</param>
    /// <param name="configure">
    /// Configures the options, run at install time against a builder seeded from
    /// the options then in effect.
    /// </param>
    /// <returns>
    /// A <see cref="MonadServicesBuilder" /> for chaining the calls that only
    /// make sense once registration has happened, such as
    /// <c>EnableInstallOnStart</c> from
    /// <c>Waystone.Monads.Extensions.Hosting</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="services" /> or <paramref name="configure" /> is null.
    /// </exception>
    public static MonadServicesBuilder AddWaystoneMonads(
        this IServiceCollection services,
        Action<MonadOptionsBuilder> configure)
    {
        if (configure is null)
        {
            throw new ArgumentNullException(nameof(configure));
        }

        return Register(services, new MonadOptionsRegistration(configure));
    }

    /// <summary>Registers configuration that needs the container itself, handing it the built provider at install time.</summary>
    /// <remarks>
    /// Reach for this overload when a setting comes from a registered service
    /// rather than from a literal — most often to point a companion package at
    /// something the container holds:
    /// <code>
    /// services.AddWaystoneMonads((provider, options) =&gt;
    ///     options.UseFallbackErrorCode("Contoso")
    ///            .UseLoggerFactoryFrom(provider));
    /// </code>
    /// Nothing is resolved on your behalf beyond
    /// <see cref="ErrorCodeFactory" />. A companion package ships its own
    /// <c>Use…</c> method and the package you installed is the one you call, so
    /// installing a package cannot change behaviour you did not ask for, and the
    /// install path never grows a branch per package.
    /// <para>
    /// Resolve singletons only. The options are one process-wide snapshot, so a
    /// scoped service captured here outlives the scope it came from — the install
    /// either trips scope validation or silently pins the root instance for the
    /// life of the process. What a missing service does is the delegate's choice:
    /// <c>GetService</c> returns null, <c>GetRequiredService</c> throws out of the
    /// install.
    /// </para>
    /// <para>
    /// Registration, accumulation and ordering are as the
    /// <see cref="AddWaystoneMonads(IServiceCollection, Action{MonadOptionsBuilder})" />
    /// overload documents, and both share one order — a delegate registered here
    /// runs after one registered by an earlier call and before a later one,
    /// whichever overload each used.
    /// </para>
    /// </remarks>
    /// <param name="services">The service collection to register into.</param>
    /// <param name="configure">
    /// Configures the options, run at install time against the built provider and
    /// a builder seeded from the options then in effect.
    /// </param>
    /// <returns>
    /// A <see cref="MonadServicesBuilder" /> for chaining the calls that only
    /// make sense once registration has happened, such as
    /// <c>EnableInstallOnStart</c> from
    /// <c>Waystone.Monads.Extensions.Hosting</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="services" /> or <paramref name="configure" /> is null.
    /// </exception>
    public static MonadServicesBuilder AddWaystoneMonads(
        this IServiceCollection services,
        Action<IServiceProvider, MonadOptionsBuilder> configure)
    {
        if (configure is null)
        {
            throw new ArgumentNullException(nameof(configure));
        }

        return Register(services, new MonadOptionsRegistration(configure));
    }

    private static MonadServicesBuilder Register(
        IServiceCollection services,
        MonadOptionsRegistration? registration)
    {
        if (services is null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        services.TryAddSingleton<ErrorCodeFactory>();

        if (registration is not null)
        {
            services.AddSingleton(registration);
        }

        MonadOptions.MarkConfigurationPending();

        return new MonadServicesBuilder(services);
    }
}
