namespace Waystone.Monads.Schemas;

using System;

/// <summary>Stands in for the value a rule that only gates would have produced.</summary>
/// <remarks>
/// <para>
/// Some rules decide whether a parse may proceed without contributing anything to
/// what it builds — a cross-field comparison, a field that must be absent, a set
/// of rules applied to the subject as a whole. There is nothing to name and
/// nothing to pass on, and a schema still has to say so in its type.
/// </para>
/// <para>
/// Carries no data and every instance is equal to every other, so the value
/// itself is never worth reading. Getting one back means the rule passed; a rule
/// that failed produces a <see cref="SchemaViolation" /> instead.
/// </para>
/// </remarks>
public readonly struct Checked : IEquatable<Checked>
{
    /// <summary>Gets the only value this type has.</summary>
    /// <remarks>
    /// Identical to <c>default</c>. Named so that returning it reads as a
    /// statement rather than as an omission.
    /// </remarks>
    public static Checked Instance => default;

    /// <summary>Checks whether another value of this type is equal to this one.</summary>
    /// <param name="other">The value to compare against.</param>
    /// <returns>Always true, since the type carries no data.</returns>
    public bool Equals(Checked other) => true;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Checked;

    /// <inheritdoc />
    public override int GetHashCode() => 0;

    /// <summary>Returns the fixed rendering of the type.</summary>
    /// <returns>Always <c>checked</c>.</returns>
    public override string ToString() => "checked";

    /// <summary>Checks whether two values of this type are equal.</summary>
    /// <param name="left">The value on the left of the operator.</param>
    /// <param name="right">The value on the right of the operator.</param>
    /// <returns>Always true, since the type carries no data.</returns>
    public static bool operator ==(Checked left, Checked right) => true;

    /// <summary>Checks whether two values of this type differ.</summary>
    /// <param name="left">The value on the left of the operator.</param>
    /// <param name="right">The value on the right of the operator.</param>
    /// <returns>Always false, since the type carries no data.</returns>
    public static bool operator !=(Checked left, Checked right) => false;
}
