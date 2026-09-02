namespace Waystone.Monads.Schemas;

using System;
using System.Globalization;

/// <summary>Locates a <see cref="Violation" /> inside the value that was parsed.</summary>
/// <remarks>
/// <para>
/// Renders the way a reader would write the access by hand, so
/// <c>items[3].sku</c> is the <c>sku</c> of the fourth entry of <c>items</c>. A
/// dictionary key renders in quotation marks — <c>rates["AUD"]</c> — so a numeric
/// key stays distinguishable from a list position.
/// </para>
/// <para>
/// Compare and group by the rendered form through
/// <see cref="ViolationCollection.ByPath" /> rather than taking the path apart.
/// The segments are deliberately not exposed: the rendering is the contract, and
/// the representation behind it is free to change.
/// </para>
/// <para>
/// Immutable and safe to share. Every path is built by the parse and handed out
/// already complete.
/// </para>
/// </remarks>
public sealed class ViolationPath : IEquatable<ViolationPath>
{
    private readonly string _rendered;

    private ViolationPath(string rendered)
    {
        _rendered = rendered;
    }

    /// <summary>Gets the path of the value that was passed to the schema itself.</summary>
    /// <remarks>
    /// Renders as the empty string. A violation carrying it came from a rule about
    /// the whole subject rather than about one of its fields — a cross-field rule,
    /// or a schema applied to a bare value.
    /// </remarks>
    public static ViolationPath Root { get; } = new(string.Empty);

    /// <summary>Checks whether this path locates the parsed value itself.</summary>
    /// <remarks>
    /// True only for <see cref="Root" />. Useful for deciding whether a failure
    /// belongs against a form field or against the form.
    /// </remarks>
    public bool IsRoot => _rendered.Length == 0;

    /// <summary>Checks whether another path locates the same place.</summary>
    /// <param name="other">The path to compare against. Null is never equal.</param>
    /// <returns>True if both render identically; false otherwise.</returns>
    public bool Equals(ViolationPath? other) =>
        other is not null
     && string.Equals(_rendered, other._rendered, StringComparison.Ordinal);

    /// <summary>Returns the path as a reader would write the access.</summary>
    /// <returns>
    /// The rendered path, for example <c>items[3].sku</c>. The empty string for
    /// <see cref="Root" />.
    /// </returns>
    public override string ToString() => _rendered;

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals(obj as ViolationPath);

    /// <inheritdoc />
    public override int GetHashCode() =>
        StringComparer.Ordinal.GetHashCode(_rendered);

    internal ViolationPath Append(string property) =>
        new(IsRoot ? property : _rendered + "." + property);

    internal ViolationPath AppendIndex(int index) => new(
        _rendered
      + "["
      + index.ToString(CultureInfo.InvariantCulture)
      + "]");

    internal ViolationPath AppendKey(string key) =>
        new(_rendered + "[\"" + key + "\"]");
}
