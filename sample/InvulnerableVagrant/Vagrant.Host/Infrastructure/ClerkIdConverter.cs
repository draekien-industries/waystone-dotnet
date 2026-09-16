namespace Vagrant.Host.Infrastructure;

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Vagrant.Staffing;

/// <summary>Stores a <see cref="ClerkId" /> as the GUID it wraps.</summary>
internal sealed class ClerkIdConverter : ValueConverter<ClerkId, Guid>
{
    /// <summary>Creates the converter.</summary>
    public ClerkIdConverter()
        : base(id => id.Value, value => new ClerkId(value))
    {
    }
}
