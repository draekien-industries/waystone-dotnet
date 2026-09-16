namespace Vagrant.Appraisal;

/// <summary>The magical signature a specimen gives off under examination.</summary>
/// <param name="Obscurity">How hard the signature is to read.</param>
public readonly record struct Aura(uint Obscurity)
{
    /// <summary>Asks whether an examination read the signature.</summary>
    /// <param name="check">What the examiner rolled.</param>
    /// <returns>
    /// <c>true</c> when the check met the obscurity, <c>false</c> when it fell short.
    /// </returns>
    /// <remarks>
    /// The aura decides, not the specimen and not the caller. How hard something is to
    /// read is a property of the signature, so the comparison lives with the number it
    /// compares against.
    /// </remarks>
    public bool YieldsTo(ArcanaCheck check) =>
        check.Total >= 0 && (uint)check.Total >= Obscurity;
}
