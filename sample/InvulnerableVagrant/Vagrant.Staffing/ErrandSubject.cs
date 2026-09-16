namespace Vagrant.Staffing;

/// <summary>What a piece of work is about.</summary>
/// <remarks>
/// Wraps a <see cref="Guid" /> this context does not interpret. Staffing cannot name a
/// <c>SpecimenId</c> without referencing Appraisal, and a bare <see cref="Guid" /> would
/// let any identifier in the shop be handed to a clerk as any other. The host
/// translates; Staffing knows only that an errand is about something.
/// </remarks>
/// <param name="Value">The underlying identifier.</param>
public readonly record struct ErrandSubject(Guid Value)
{
    /// <inheritdoc />
    public override string ToString() => Value.ToString();
}
