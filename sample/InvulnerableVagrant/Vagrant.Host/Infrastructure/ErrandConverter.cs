namespace Vagrant.Host.Infrastructure;

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Vagrant.Staffing;

/// <summary>Stores an <see cref="Errand" /> as the GUID its subject wraps.</summary>
/// <remarks>
/// <para>
/// A converter rather than a nested complex property. <see cref="Assignment" /> is a
/// positional record, and EF cannot bind a complex property to a constructor parameter —
/// leaving <c>Errand</c> as one fails at model build with "No suitable constructor was
/// found for the type 'Clerk.Holding#Assignment'". Converted to a scalar, it binds.
/// </para>
/// <para>
/// The GUID is a specimen's, and this is the only place in the shop that could tell.
/// Staffing cannot: it holds an <see cref="ErrandSubject" /> it does not interpret.
/// </para>
/// </remarks>
internal sealed class ErrandConverter : ValueConverter<Errand, Guid>
{
    /// <summary>Creates the converter.</summary>
    public ErrandConverter()
        : base(
            errand => errand.Subject.Value,
            value => new Errand(new ErrandSubject(value)))
    {
    }
}
