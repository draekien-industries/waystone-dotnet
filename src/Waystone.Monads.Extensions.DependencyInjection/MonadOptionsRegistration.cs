namespace Microsoft.Extensions.DependencyInjection;

using System;
using Waystone.Monads.Configs;

internal sealed class MonadOptionsRegistration
{
    internal MonadOptionsRegistration(
        Action<IServiceProvider, MonadOptionsBuilder> configure)
    {
        Configure = configure;
    }

    internal MonadOptionsRegistration(Action<MonadOptionsBuilder> configure)
        : this((_, builder) => configure(builder))
    { }

    internal Action<IServiceProvider, MonadOptionsBuilder> Configure { get; }
}
