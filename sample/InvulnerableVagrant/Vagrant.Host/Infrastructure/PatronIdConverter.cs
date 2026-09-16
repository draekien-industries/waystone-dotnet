namespace Vagrant.Host.Infrastructure;

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Vagrant.SharedKernel;

/// <summary>Stores a <see cref="PatronId" /> as the identifier it wraps.</summary>
internal sealed class PatronIdConverter : ValueConverter<PatronId, Guid>
{
    public PatronIdConverter()
        : base(id => id.Value, value => new PatronId(value))
    {
    }
}
