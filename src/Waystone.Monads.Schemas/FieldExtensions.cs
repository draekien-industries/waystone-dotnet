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
}
