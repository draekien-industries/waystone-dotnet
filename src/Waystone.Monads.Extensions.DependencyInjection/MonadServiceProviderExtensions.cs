namespace Microsoft.Extensions.DependencyInjection;

using System;
using Waystone.Monads.Configs;

/// <summary>Installs the Waystone.Monads configuration a service collection registered.</summary>
public static class MonadServiceProviderExtensions
{
    /// <summary>Installs the registered configuration as the ambient Waystone.Monads options.</summary>
    /// <remarks>
    /// Call this once, immediately after the provider is built and before any
    /// work that uses the library. Until it runs, everything
    /// <c>AddWaystoneMonads</c> registered is inert.
    /// <para>
    /// The snapshot is assembled in three steps, each overwriting the last: the
    /// options currently in effect are taken as the starting point, then the
    /// container's <see cref="ErrorCodeFactory" /> is applied if it holds one,
    /// then each registered configuration action runs in registration order. So a
    /// delegate passed to <c>AddWaystoneMonads</c> has the last word.
    /// </para>
    /// <para>
    /// Nothing beyond <see cref="ErrorCodeFactory" /> is resolved on your behalf.
    /// A companion package is wired by a delegate you register, which is where
    /// <paramref name="provider" /> reaches it — see
    /// <see cref="MonadServiceCollectionExtensions.AddWaystoneMonads(IServiceCollection, Action{IServiceProvider, MonadOptionsBuilder})" />.
    /// The factory is resolved through <see cref="IServiceProvider.GetService" />
    /// itself, so any container that can produce a provider works, including ones
    /// that are not Microsoft's.
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

        foreach (MonadOptionsRegistration registration in
                 provider.GetServices<MonadOptionsRegistration>())
        {
            registration.Configure(provider, builder);
        }

        MonadOptions.Install(builder.Build());

        return provider;
    }
}
