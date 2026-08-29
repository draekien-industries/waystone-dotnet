namespace Microsoft.Extensions.DependencyInjection;

using System;
using Logging;
using Waystone.Monads.Configs;
using Waystone.Monads.Extensions.Logging.Configs;

/// <summary>Installs the Waystone.Monads configuration a service collection registered.</summary>
public static class MonadServiceProviderExtensions
{
    /// <summary>Installs the registered configuration as the ambient Waystone.Monads options.</summary>
    /// <remarks>
    /// Call this once, immediately after the provider is built and before any
    /// work that uses the library. Until it runs, everything
    /// <see cref="MonadServiceCollectionExtensions.AddWaystoneMonads" />
    /// registered is inert.
    /// <para>
    /// The snapshot is assembled in three steps, each overwriting the last: the
    /// options currently in effect are taken as the starting point, then the
    /// container's <see cref="ErrorCodeFactory" /> and
    /// <see cref="ILoggerFactory" /> are applied if it holds them, then each
    /// registered configuration action runs in registration order. So a delegate
    /// passed to <c>AddWaystoneMonads</c> has the last word, including over the
    /// resolved logger.
    /// </para>
    /// <para>
    /// A container holding no <see cref="ILoggerFactory" /> leaves logging
    /// unconfigured rather than failing — a worker or console application that
    /// never called <c>AddLogging</c> is a legitimate host. It resolves both
    /// services through <see cref="IServiceProvider.GetService" /> itself, so any
    /// container that can produce a provider works, including ones that are not
    /// Microsoft's.
    /// </para>
    /// <para>
    /// Publishing is atomic and affects the whole process: a concurrent reader
    /// sees either the old snapshot or the new one, never a mixture. It does not
    /// touch options set by <see cref="MonadOptions.BeginScope(Action{MonadOptionsBuilder})" />,
    /// and it reads from the process-wide options rather than from any scope open
    /// on the calling thread, so a scope active during start-up cannot leak into
    /// what every other thread will see.
    /// </para>
    /// </remarks>
    /// <param name="provider">The built service provider to resolve configuration from.</param>
    /// <returns>The service provider, for chaining more start-up calls.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="provider" /> is null.</exception>
    public static IServiceProvider UseWaystoneMonads(
        this IServiceProvider provider)
    {
        if (provider is null)
        {
            throw new ArgumentNullException(nameof(provider));
        }

        MonadOptionsBuilder builder = MonadOptions.Global.ToBuilder();

        if (provider.GetService<ErrorCodeFactory>() is { } factory)
        {
            builder.UseErrorCodeFactory(factory);
        }

        if (provider.GetService<ILoggerFactory>() is { } loggerFactory)
        {
            builder.UseLoggerFactory(loggerFactory);
        }

        foreach (MonadOptionsRegistration registration in
                 provider.GetServices<MonadOptionsRegistration>())
        {
            registration.Configure(builder);
        }

        MonadOptions.Install(builder.Build());

        return provider;
    }
}
