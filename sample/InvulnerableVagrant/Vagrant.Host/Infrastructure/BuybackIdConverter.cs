namespace Vagrant.Host.Infrastructure;

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Vagrant.Ordering;

/// <summary>Stores a <see cref="BuybackId" /> as the GUID it wraps.</summary>
internal sealed class BuybackIdConverter : ValueConverter<BuybackId, Guid>
{
    /// <summary>Creates the converter.</summary>
    public BuybackIdConverter()
        : base(id => id.Value, value => new BuybackId(value))
    {
    }
}
