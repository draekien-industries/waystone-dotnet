namespace Waystone.Monads.Schemas;

using System;
using System.Globalization;
using System.Text;

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
    private static readonly Segment[] NoSegments = Array.Empty<Segment>();

    private readonly Segment[] _segments;

    private string? _rendered;

    private ViolationPath(Segment[] segments)
    {
        _segments = segments;
    }

    private enum SegmentKind
    {
        Property,
        Index,
        Key,
    }

    /// <summary>Gets the path of the value that was passed to the schema itself.</summary>
    /// <remarks>
    /// Renders as the empty string. A violation carrying it came from a rule about
    /// the whole subject rather than about one of its fields — a cross-field rule,
    /// or a schema applied to a bare value.
    /// </remarks>
    public static ViolationPath Root { get; } = new(NoSegments);

    /// <summary>Checks whether this path locates the parsed value itself.</summary>
    /// <remarks>
    /// True only for <see cref="Root" />. Useful for deciding whether a failure
    /// belongs against a form field or against the form.
    /// </remarks>
    public bool IsRoot => _segments.Length == 0;

    /// <summary>Checks whether another path locates the same place.</summary>
    /// <param name="other">The path to compare against. Null is never equal.</param>
    /// <returns>True if both render identically; false otherwise.</returns>
    public bool Equals(ViolationPath? other) =>
        other is not null
     && string.Equals(ToString(), other.ToString(), StringComparison.Ordinal);

    /// <summary>Returns the path as a reader would write the access.</summary>
    /// <returns>
    /// The rendered path, for example <c>items[3].sku</c>. The empty string for
    /// <see cref="Root" />.
    /// </returns>
    /// <remarks>
    /// Rendered once and kept, so grouping a large report by path does not re-render
    /// the same path repeatedly.
    /// </remarks>
    public override string ToString() => _rendered ??= Render();

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals(obj as ViolationPath);

    /// <inheritdoc />
    public override int GetHashCode() =>
        StringComparer.Ordinal.GetHashCode(ToString());

    internal ViolationPath Append(string property) =>
        With(new Segment(SegmentKind.Property, property));

    internal ViolationPath AppendIndex(int index) => With(
        new Segment(
            SegmentKind.Index,
            index.ToString(CultureInfo.InvariantCulture)));

    internal ViolationPath AppendKey(string key) =>
        With(new Segment(SegmentKind.Key, key));

    internal ViolationPath Nest(ViolationPath child)
    {
        if (child.IsRoot) return this;

        if (IsRoot) return child;

        var segments = new Segment[_segments.Length + child._segments.Length];

        Array.Copy(_segments, segments, _segments.Length);

        Array.Copy(
            child._segments,
            0,
            segments,
            _segments.Length,
            child._segments.Length);

        return new ViolationPath(segments);
    }

    internal ViolationPath Rename(string property)
    {
        if (_segments.Length == 0
         || _segments[_segments.Length - 1].Kind != SegmentKind.Property)
        {
            return Append(property);
        }

        var segments = new Segment[_segments.Length];

        Array.Copy(_segments, segments, _segments.Length);

        segments[_segments.Length - 1] =
            new Segment(SegmentKind.Property, property);

        return new ViolationPath(segments);
    }

    private ViolationPath With(Segment segment)
    {
        var segments = new Segment[_segments.Length + 1];

        Array.Copy(_segments, segments, _segments.Length);
        segments[_segments.Length] = segment;

        return new ViolationPath(segments);
    }

    private string Render()
    {
        if (IsRoot) return string.Empty;

        var builder = new StringBuilder();

        foreach (Segment segment in _segments)
        {
            switch (segment.Kind)
            {
                case SegmentKind.Index:
                    builder.Append('[').Append(segment.Text).Append(']');
                    break;
                case SegmentKind.Key:
                    builder.Append("[\"").Append(segment.Text).Append("\"]");
                    break;
                default:
                    if (builder.Length > 0) builder.Append('.');
                    builder.Append(segment.Text);
                    break;
            }
        }

        return builder.ToString();
    }

    private readonly struct Segment
    {
        internal Segment(SegmentKind kind, string text)
        {
            Kind = kind;
            Text = text;
        }

        internal SegmentKind Kind { get; }

        internal string Text { get; }
    }
}
