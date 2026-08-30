namespace Waystone.DocSnippets;

using Waystone.Monads.Results.Errors;

/// <summary>
/// Every way reconciling the samples with the pages can fail. The catalog
/// generator turns each member into a constant, an <see cref="ErrorCode" /> and
/// an <see cref="Error" /> factory, so no failure here is spelled out as a
/// string at the point it is raised.
/// </summary>
[ErrorCodeCatalog]
public enum DocSnippetError
{
    /// <summary>A region opened inside a snippet region, which has no sensible reading.</summary>
    NestedRegion,

    /// <summary>A snippet region reached the end of its file without an <c>#endregion</c>.</summary>
    UnterminatedRegion,

    /// <summary>Two regions claim the same key, so a slot naming it would be ambiguous.</summary>
    DuplicateKey,

    /// <summary>A page has a slot for a key no source file defines.</summary>
    UnknownSnippet,

    /// <summary>A slot opened and the page ended, or another slot opened, before it closed.</summary>
    UnterminatedSlot,

    /// <summary>The working directory is not inside a git repository.</summary>
    NotInARepository,

    /// <summary>None of the candidate paths held the documentation repository.</summary>
    DocumentationRepositoryNotFound,
}
