namespace Microsoft.Extensions.Hosting;

using System;
using DependencyInjection;
using Waystone.Monads.Configs;

/// <summary>Registers Waystone.Monads on a host and hands the install to it.</summary>
public static class MonadHostApplicationBuilderExtensions
{
    /// <summary>Registers Waystone.Monads and arranges for the host to install it as it starts.</summary>
    /// <remarks>
    /// The one call a host application needs. It is
    /// <see cref="MonadServiceCollectionExtensions.AddWaystoneMonads" /> followed
    /// by <see cref="MonadServicesBuilderExtensions.EnableInstallOnStart" />, so
    /// the two-step registration those document applies here unchanged — this
    /// only spares you writing both.
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
    /// the options then in effect. Omit it to take the defaults plus whatever the
    /// container supplies.
    /// </param>
    /// <returns>
    /// The <see cref="MonadServicesBuilder" /> the registration produced, for
    /// chaining more configurations.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="builder" /> is null.</exception>
    public static MonadServicesBuilder AddWaystoneMonads(
        this IHostApplicationBuilder builder,
        Action<MonadOptionsBuilder>? configure = null)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        return builder.Services.AddWaystoneMonads(configure)
                      .EnableInstallOnStart();
    }
}
