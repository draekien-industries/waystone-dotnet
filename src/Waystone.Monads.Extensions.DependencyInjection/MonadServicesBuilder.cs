namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// The result of registering Waystone.Monads with a service collection, and the
/// receiver companion packages extend.
/// </summary>
/// <remarks>
/// Returned by
/// <see cref="MonadServiceCollectionExtensions.AddWaystoneMonads" />. It carries
/// no state of its own — it exists so that a call which only makes sense after
/// registration can require one, rather than sitting on
/// <see cref="IServiceCollection" /> where it could be called alone.
/// <c>Waystone.Monads.Extensions.Hosting</c> uses it that way for
/// <c>EnableInstallOnStart</c>.
/// <para>
/// Reach for <see cref="Services" /> to keep registering ordinary services.
/// </para>
/// </remarks>
public sealed class MonadServicesBuilder
{
    internal MonadServicesBuilder(IServiceCollection services)
    {
        Services = services;
    }

    /// <summary>The service collection the registration was made against.</summary>
    /// <remarks>
    /// The same instance that was passed to <c>AddWaystoneMonads</c>, not a copy,
    /// so registrations made through it and through the original variable land in
    /// one place.
    /// </remarks>
    public IServiceCollection Services { get; }
}
