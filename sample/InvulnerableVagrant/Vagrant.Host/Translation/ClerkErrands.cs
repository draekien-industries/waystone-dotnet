namespace Vagrant.Host.Translation;

using Vagrant.Appraisal;
using Vagrant.Staffing;

/// <summary>Turns something Appraisal is holding into work a clerk can be given.</summary>
/// <remarks>
/// <para>
/// One of the two places a value crosses a context boundary, and it is here rather than
/// in either context because neither may reference the other. Staffing could not name a
/// <see cref="SpecimenId" /> without a <c>ProjectReference</c> to Appraisal, and a
/// clerk has no business reading the shelf.
/// </para>
/// <para>
/// The GUID survives the crossing and its type does not. That is the point of
/// <see cref="ErrandSubject" /> over a bare <c>Guid</c>: a clerk holds an errand about
/// something, and only this file knows the something is a specimen.
/// </para>
/// </remarks>
internal static class ClerkErrands
{
    /// <summary>Names an item on the examination shelf as an errand.</summary>
    /// <param name="specimen">Which item the work is about.</param>
    /// <returns>The errand a clerk takes on.</returns>
    public static Errand For(SpecimenId specimen) =>
        new(new ErrandSubject(specimen.Value));
}
