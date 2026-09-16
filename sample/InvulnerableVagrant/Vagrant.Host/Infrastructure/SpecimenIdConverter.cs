namespace Vagrant.Host.Infrastructure;

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Vagrant.Appraisal;

/// <summary>Stores a <see cref="SpecimenId" /> as the GUID it wraps.</summary>
internal sealed class SpecimenIdConverter : ValueConverter<SpecimenId, Guid>
{
    /// <summary>Creates the converter.</summary>
    public SpecimenIdConverter()
        : base(id => id.Value, value => new SpecimenId(value))
    {
    }
}
