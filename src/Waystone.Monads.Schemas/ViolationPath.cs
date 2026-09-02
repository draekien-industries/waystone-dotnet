namespace Waystone.Monads.Schemas;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

/// <summary>Locates a <see cref="Violation" /> inside the value that was parsed.</summary>
/// <remarks>
/// <para>
/// Renders the way a reader would write the access by hand, so
/// <c>items[3].sku</c> is the <c>sku</c> of the fourth entry of <c>items</c>. A
/// dictionary key renders in quotation marks — <c>rates["AUD"]</c> — so a numeric
/// key stays distinguishable from a list position, and a quotation mark or
/// backslash inside a key is escaped so that a key cannot forge a path of its
/// own. A failed <c>Schema.Any</c> branch renders in braces — <c>payment{0}</c> —
/// which is what keeps it apart from a list position.
/// </para>
/// <para>
/// <see cref="Segments" /> is the form to branch on. The rendered text is built
/// for a human, and a caller deciding what to do with a violation should ask a
/// segment its <see cref="PathSegment.Kind" /> rather than look for brackets —
/// a list position and a union branch are different things that read alike.
/// </para>
/// <para>
/// Immutable and safe to share. Every path is built by the parse and handed out
/// already complete.
/// </para>
/// </remarks>
public sealed class ViolationPath : IEquatable<ViolationPath>
{
    private static readonly PathSegment[] NoSegments =
        Array.Empty<PathSegment>();

    private readonly PathSegment[] _segments;

    private string? _rendered;

    private ViolationPath(PathSegment[] segments)
    {
        _segments = segments;
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

    /// <summary>Gets the steps of the path, outermost first.</summary>
    /// <remarks>
    /// Empty for <see cref="Root" />. This is the form to branch on: it says
    /// whether a step is a list position or a union branch, which the rendered
    /// text leaves a reader to guess at.
    /// </remarks>
    public IReadOnlyList<PathSegment> Segments => _segments;

    /// <summary>Checks whether another path locates the same place.</summary>
    /// <remarks>
    /// Compares the segments, not the rendered text. Two paths that render alike
    /// are still unequal if they got there differently — a dictionary key holding
    /// bracket punctuation against the nested lookup it imitates, say.
    /// </remarks>
    /// <param name="other">The path to compare against. Null is never equal.</param>
    /// <returns>True if both locate the same place by the same steps; false otherwise.</returns>
    public bool Equals(ViolationPath? other)
    {
        if (other is null) return false;

        if (ReferenceEquals(this, other)) return true;

        if (_segments.Length != other._segments.Length) return false;

        for (var index = 0; index < _segments.Length; index++)
        {
            if (!_segments[index].Equals(other._segments[index])) return false;
        }

        return true;
    }

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
    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 17;

            foreach (PathSegment segment in _segments)
            {
                hash = (hash * 397) ^ segment.GetHashCode();
            }

            return hash;
        }
    }

    internal ViolationPath Append(string property) =>
        With(new PathSegment(PathSegmentKind.Property, property));

    internal ViolationPath AppendIndex(int index) => With(
        new PathSegment(
            PathSegmentKind.Index,
            index.ToString(CultureInfo.InvariantCulture)));

    internal ViolationPath AppendKey(string key) =>
        With(new PathSegment(PathSegmentKind.Key, key));

    internal ViolationPath AppendBranch(int branch) => With(
        new PathSegment(
            PathSegmentKind.Branch,
            branch.ToString(CultureInfo.InvariantCulture)));

    internal ViolationPath Nest(ViolationPath child)
    {
        if (child.IsRoot) return this;

        if (IsRoot) return child;

        var segments = new PathSegment[_segments.Length + child._segments.Length];

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
         || _segments[_segments.Length - 1].Kind != PathSegmentKind.Property)
        {
            return Append(property);
        }

        var segments = new PathSegment[_segments.Length];

        Array.Copy(_segments, segments, _segments.Length);

        segments[_segments.Length - 1] =
            new PathSegment(PathSegmentKind.Property, property);

        return new ViolationPath(segments);
    }

    private ViolationPath With(PathSegment segment)
    {
        var segments = new PathSegment[_segments.Length + 1];

        Array.Copy(_segments, segments, _segments.Length);
        segments[_segments.Length] = segment;

        return new ViolationPath(segments);
    }

    private string Render()
    {
        if (IsRoot) return string.Empty;

        var builder = new StringBuilder();

        foreach (PathSegment segment in _segments)
        {
            switch (segment.Kind)
            {
                case PathSegmentKind.Index:
                    builder.Append('[').Append(segment.Text).Append(']');
                    break;
                case PathSegmentKind.Branch:
                    builder.Append('{').Append(segment.Text).Append('}');
                    break;
                case PathSegmentKind.Key:
                    builder.Append("[\"");
                    Escape(builder, segment.Text);
                    builder.Append("\"]");
                    break;
                default:
                    if (builder.Length > 0) builder.Append('.');
                    builder.Append(segment.Text);
                    break;
            }
        }

        return builder.ToString();
    }

    private static void Escape(StringBuilder builder, string key)
    {
        foreach (char character in key)
        {
            if (character is '"' or '\\') builder.Append('\\');

            builder.Append(character);
        }
    }

}
