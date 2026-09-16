namespace Vagrant.Host.Infrastructure;

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Vagrant.Ordering;

/// <summary>Stores a <see cref="LineItemSubject" /> as the GUID it wraps.</summary>
/// <remarks>
/// The same GUID a <c>StockedItemId</c> holds, stored in a table Catalog cannot join to.
/// A purchase records which thing was sold without being able to read the shelf it came
/// from.
/// </remarks>
internal sealed class LineItemSubjectConverter : ValueConverter<LineItemSubject, Guid>
{
    /// <summary>Creates the converter.</summary>
    public LineItemSubjectConverter()
        : base(subject => subject.Value, value => new LineItemSubject(value))
    {
    }
}
