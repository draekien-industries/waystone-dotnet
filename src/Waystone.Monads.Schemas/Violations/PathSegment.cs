namespace Waystone.Monads.Schemas;

using System;

/// <summary>One step of a <see cref="ViolationPath" />.</summary>
/// <remarks>
/// Read these rather than taking <see cref="ViolationPath.ToString" /> apart. The
/// rendered form is built for a human, and two steps of different kinds can look
/// alike in it once a key contains punctuation.
/// </remarks>
public readonly struct PathSegment : IEquatable<PathSegment>
{
    private readonly string? _text;

    internal PathSegment(PathSegmentKind kind, string text)
    {
        Kind = kind;
        _text = text;
    }

    /// <summary>Gets what sort of step this is.</summary>
    public PathSegmentKind Kind { get; }

    /// <summary>Gets the step itself.</summary>
    /// <value>
    /// A property name, a dictionary key exactly as the key was written, or the
    /// number of a list position or a union branch. Never escaped — escaping
    /// belongs to rendering. The empty string on a default instance, which a parse
    /// never produces.
    /// </value>
    public string Text => _text ?? string.Empty;

    /// <summary>Checks whether two steps are the same step.</summary>
    /// <param name="left">The step on the left of the operator.</param>
    /// <param name="right">The step on the right of the operator.</param>
    /// <returns>True if both have the same kind and text; false otherwise.</returns>
    public static bool operator ==(PathSegment left, PathSegment right) =>
        left.Equals(right);

    /// <summary>Checks whether two steps differ.</summary>
    /// <param name="left">The step on the left of the operator.</param>
    /// <param name="right">The step on the right of the operator.</param>
    /// <returns>True if their kind or their text differs; false otherwise.</returns>
    public static bool operator !=(PathSegment left, PathSegment right) =>
        !left.Equals(right);

    /// <summary>Checks whether another step is the same step.</summary>
    /// <remarks>
    /// The text comparison is ordinal, so <c>rates["AUD"]</c> and
    /// <c>rates["aud"]</c> are different places.
    /// </remarks>
    /// <param name="other">The step to compare against.</param>
    /// <returns>True if both have the same kind and text; false otherwise.</returns>
    public bool Equals(PathSegment other) =>
        Kind == other.Kind
     && string.Equals(Text, other.Text, StringComparison.Ordinal);

    /// <inheritdoc />
    public override bool Equals(object? obj) =>
        obj is PathSegment other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        unchecked
        {
            return ((int)Kind * 397) ^ StringComparer.Ordinal.GetHashCode(Text);
        }
    }

    /// <summary>Returns the step's text, without the punctuation it renders with.</summary>
    /// <returns>
    /// <see cref="Text" />. Render a whole path through
    /// <see cref="ViolationPath.ToString" /> rather than joining these.
    /// </returns>
    public override string ToString() => Text;
}
