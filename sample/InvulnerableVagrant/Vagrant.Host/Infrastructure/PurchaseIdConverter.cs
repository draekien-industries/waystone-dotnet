namespace Vagrant.Host.Infrastructure;

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Vagrant.Ordering;

/// <summary>Stores a <see cref="PurchaseId" /> as the GUID it wraps.</summary>
internal sealed class PurchaseIdConverter : ValueConverter<PurchaseId, Guid>
{
    /// <summary>Creates the converter.</summary>
    public PurchaseIdConverter()
        : base(id => id.Value, value => new PurchaseId(value))
    {
    }
}
