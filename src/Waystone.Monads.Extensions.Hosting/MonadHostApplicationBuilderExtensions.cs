namespace Microsoft.Extensions.Hosting;

using System;
using DependencyInjection;
using Waystone.Monads.Configs;

/// <summary>Registers Waystone.Monads on a host and hands the install to it.</summary>
public static class MonadHostApplicationBuilderExtensions
{
    /// <summary>Asks a host for the ambient Waystone.Monads options as they come, and installs them as it starts.</summary>
    /// <remarks>
    /// The defaults, plus the <see cref="ErrorCodeFactory" /> the container holds.
    /// Everything the overload taking a delegate documents applies here; this one
    /// simply registers no delegate.
    /// </remarks>
    /// <param name="builder">The host application builder to register into.</param>
    /// <returns>
    /// The <see cref="MonadServicesBuilder" /> the registration produced, for
    /// chaining more configurations.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="builder" /> is null.</exception>
    public static MonadServicesBuilder AddWaystoneMonads(
        this IHostApplicationBuilder builder)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        return builder.Services.AddWaystoneMonads().EnableInstallOnStart();
    }

    /// <summary>Registers Waystone.Monads and arranges for the host to install it as it starts.</summary>
    /// <remarks>
    /// The one call a host application needs. It is
    /// <see cref="MonadServiceCollectionExtensions.AddWaystoneMonads(IServiceCollection, Action{MonadOptionsBuilder})" />
    /// followed by
    /// <see cref="MonadServicesBuilderExtensions.EnableInstallOnStart" />, so the
    /// two-step registration those document applies here unchanged — this only
    /// spares you writing both.
    /// <para>
    /// Both <c>WebApplicationBuilder</c> and the builder from
    /// <c>Host.CreateApplicationBuilder</c> implement
    /// <see cref="IHostApplicationBuilder" />. On the older
    /// <see cref="IHostBuilder" />, which has no such interface, reach the same
    /// pair through <c>ConfigureServices</c> instead.
    /// </para>
    /// <para>
    /// Configuration is not read from <see cref="IHostApplicationBuilder.Configuration" />
    /// automatically, deliberately. Call <c>ReadFromConfiguration</c> inside
    /// <paramref name="configure" /> to opt in.
    /// </para>
    /// </remarks>
    /// <param name="builder">The host application builder to register into.</param>
    /// <param name="configure">
    /// Configures the options, run at install time against a builder seeded from
    /// the options then in effect.
    /// </param>
    /// <returns>
    /// The <see cref="MonadServicesBuilder" /> the registration produced, for
    /// chaining more configurations.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="builder" /> or <paramref name="configure" /> is null.
    /// </exception>
    public static MonadServicesBuilder AddWaystoneMonads(
        this IHostApplicationBuilder builder,
        Action<MonadOptionsBuilder> configure)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        return builder.Services.AddWaystoneMonads(configure)
                      .EnableInstallOnStart();
    }

    /// <summary>Registers configuration that needs the host's container, handing it the built provider at install time.</summary>
    /// <remarks>
    /// The shape to reach for when a setting comes from a registered service
    /// rather than from a literal, which on a host is most often the logger:
    /// <code>
    /// builder.AddWaystoneMonads((provider, options) =&gt;
    ///     options.UseFallbackErrorCode("Contoso")
    ///            .UseLoggerFactoryFrom(provider));
    /// </code>
    /// The provider is the host's own, so <c>AddLogging</c> and everything else
    /// the host registered has already run by the time
    /// <paramref name="configure" /> sees it. See
    /// <see cref="MonadServiceCollectionExtensions.AddWaystoneMonads(IServiceCollection, Action{IServiceProvider, MonadOptionsBuilder})" />
    /// for what may safely be resolved there.
    /// </remarks>
    /// <param name="builder">The host application builder to register into.</param>
    /// <param name="configure">
    /// Configures the options, run at install time against the host's provider
    /// and a builder seeded from the options then in effect.
    /// </param>
    /// <returns>
    /// The <see cref="MonadServicesBuilder" /> the registration produced, for
    /// chaining more configurations.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="builder" /> or <paramref name="configure" /> is null.
    /// </exception>
    public static MonadServicesBuilder AddWaystoneMonads(
        this IHostApplicationBuilder builder,
        Action<IServiceProvider, MonadOptionsBuilder> configure)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        return builder.Services.AddWaystoneMonads(configure)
                      .EnableInstallOnStart();
    }
}
