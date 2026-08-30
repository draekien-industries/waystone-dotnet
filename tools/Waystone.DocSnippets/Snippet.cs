namespace Waystone.DocSnippets;

/// <summary>A named block of source lifted out of a compiled sample file.</summary>
/// <param name="Key">
/// The name on the <c>#region</c> that opened the block. This is what a markdown
/// slot names, so it is the join between the two repositories.
/// </param>
/// <param name="Body">The block's lines, stripped of the indentation they carried in the source file.</param>
/// <param name="SourcePath">
/// Where the block came from, relative to the repository root and using forward
/// slashes. It is stamped into the generated markdown so a reviewer reading a
/// documentation diff can find the file that produced it.
/// </param>
public sealed record Snippet(string Key, string Body, string SourcePath);
