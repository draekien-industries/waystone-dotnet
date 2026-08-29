namespace Microsoft.Extensions.DependencyInjection;

using System;
using Waystone.Monads.Configs;

internal sealed class MonadOptionsRegistration
{
    internal MonadOptionsRegistration(Action<MonadOptionsBuilder> configure)
    {
        Configure = configure;
    }

    internal Action<MonadOptionsBuilder> Configure { get; }
}
