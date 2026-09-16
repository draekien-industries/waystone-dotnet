namespace Vagrant.Host.Infrastructure;

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Vagrant.Catalog;

/// <summary>Stores a <see cref="StockedItemId" /> as the GUID it wraps.</summary>
internal sealed class StockedItemIdConverter : ValueConverter<StockedItemId, Guid>
{
    /// <summary>Creates the converter.</summary>
    public StockedItemIdConverter()
        : base(id => id.Value, value => new StockedItemId(value))
    {
    }
}
