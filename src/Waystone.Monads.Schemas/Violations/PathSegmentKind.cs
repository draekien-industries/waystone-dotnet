namespace Waystone.Monads.Schemas;

/// <summary>What sort of step a <see cref="PathSegment" /> is.</summary>
/// <remarks>
/// The reason to read a path structurally rather than as text: a list position
/// and a union branch are different things that a reader would otherwise have to
/// tell apart by their brackets.
/// </remarks>
public enum PathSegmentKind
{
    /// <summary>A named member of the value being parsed, such as <c>sku</c>.</summary>
    Property,

    /// <summary>A position in a list, counted from zero.</summary>
    Index,

    /// <summary>A key of a dictionary, as the key itself was written.</summary>
    Key,

    /// <summary>
    /// One alternative of <c>Schema.Any</c>, counted from zero in the order the
    /// branches were given.
    /// </summary>
    /// <remarks>
    /// Present only when every branch failed. A union that matched reports nothing
    /// of its own.
    /// </remarks>
    Branch,
}
