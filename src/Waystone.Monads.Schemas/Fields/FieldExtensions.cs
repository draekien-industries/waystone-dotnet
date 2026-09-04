namespace Waystone.Monads.Schemas;

using System;

/// <summary>Adjustments to a field rather than to the schema it carries.</summary>
public static class FieldExtensions
{
    /// <summary>Reports this field's failures under a name you choose.</summary>
    /// <typeparam name="T">What the field yields once it passes.</typeparam>
    /// <param name="field">
    /// The field to rename, as returned by <c>Schema.Required</c> or one of its
    /// siblings.
    /// </param>
    /// <param name="name">
    /// The path segment a caller is shown, replacing the one the compiler derived
    /// from the argument text.
    /// </param>
    /// <returns>An equivalent field reporting under the new name.</returns>
    /// <remarks>
    /// <para>
    /// Reach for it when the property name is not what a caller should be shown,
    /// or when the argument was an expression rather than a member access — a
    /// literal or a method call gives a path nobody can act on.
    /// </para>
    /// <para>
    /// Prefer this over <c>Schema.Named</c> inside a field set. A schema is shared
    /// across every field of its shape, so a name baked into one silently renames
    /// all of them; a field is built per parse and cannot be reused by accident.
    /// </para>
    /// <para>
    /// On a field from <c>Schema.Extend</c> it nests rather than replaces, since
    /// that field reports at the subject's own path and so has no segment of its
    /// own to overwrite. That is how a cross-field rule gets a name.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="field" /> or <paramref name="name" /> is null.
    /// </exception>
    public static Field<T> Named<T>(this Field<T> field, string name)
        where T : notnull
    {
        if (field is null) throw new ArgumentNullException(nameof(field));
        if (name is null) throw new ArgumentNullException(nameof(name));

        return field.WithName(name);
    }

    /// <summary>Keeps this field's rules and drops the value they passed.</summary>
    /// <typeparam name="T">What the field yields once it passes, and what this discards.</typeparam>
    /// <param name="field">
    /// The field to run for its rules alone, as returned by <c>Schema.Required</c>
    /// or one of its siblings.
    /// </param>
    /// <returns>
    /// A field reporting the same violations at the same path, yielding
    /// <see cref="Checked" /> rather than <typeparamref name="T" />.
    /// </returns>
    /// <remarks>
    /// <para>
    /// For a field the caller must send correctly but that the parsed type has no
    /// place for — a wire contract another system reads, a confirmation address
    /// checked and never stored. Pass the result to <c>Refine</c>, which leaves the
    /// arity of <c>Schema.Fields</c> and the <c>Into</c> lambda for the fields that
    /// do contribute. Positionally discarding it in <c>Into</c> instead spends a
    /// slot on it and rebinds its neighbours if the field list is ever reordered.
    /// </para>
    /// <para>
    /// This is the deliberate form of what <c>WMSC0005</c> warns about, and turns
    /// that warning off here without turning it off anywhere else: the rule reads
    /// the yielded type, and this yields <see cref="Checked" />. Reach for it only
    /// where the value is genuinely unwanted — the accidental case the rule was
    /// written for looks identical afterwards.
    /// </para>
    /// <para>
    /// Prefer <c>Schema.Forbidden</c> where the field must be absent rather than
    /// merely unused, and the generated <c>Checked</c> on a field set where no field
    /// contributes a value. This one covers the mixed schema the other two do not:
    /// some fields build the result, the rest only gate it.
    /// </para>
    /// <para>
    /// Composes with <see cref="Named{T}" /> in either order, since the name belongs
    /// to the field underneath.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="field" /> is null.
    /// </exception>
    public static Field<Checked> AsChecked<T>(this Field<T> field)
        where T : notnull
    {
        if (field is null) throw new ArgumentNullException(nameof(field));

        return new CheckedField<T>(field);
    }
}
