using System.Text.RegularExpressions;
using Waystone.Monads.Results;
using Waystone.Monads.Results.Errors;
using Waystone.Monads.Results.Extensions;

namespace Waystone.DocSnippets;

/// <summary>Lifts the named regions out of the compiled sample files.</summary>
public static partial class SnippetReader
{
    /// <summary>Reads every snippet region in one source file.</summary>
    /// <param name="text">The file's content.</param>
    /// <param name="sourcePath">
    /// The path stamped onto each snippet found. Pass it relative to the
    /// repository root — it ends up in the published markdown.
    /// </param>
    /// <returns>
    /// The snippets in the order they appear, or the first malformed region.
    /// </returns>
    public static Result<IReadOnlyList<Snippet>, Error> Read(string text, string sourcePath)
    {
        List<Snippet> snippets = [];
        List<string> body = [];
        string? key = null;

        foreach (string line in Lines.Split(text))
        {
            Match start = RegionStart().Match(line);

            if (start.Success)
            {
                if (key is not null)
                {
                    return Result.Err<IReadOnlyList<Snippet>>(
                        DocSnippetError.NestedRegion.ToError(
                            $"{sourcePath}: region '{start.Groups["name"].Value}' opens inside snippet "
                          + $"'{key}'. A snippet region cannot contain another region."));
                }

                if (SnippetKey().IsMatch(start.Groups["name"].Value))
                {
                    key = start.Groups["name"].Value;
                    body.Clear();
                }

                continue;
            }

            if (RegionEnd().IsMatch(line))
            {
                if (key is not null)
                {
                    snippets.Add(new Snippet(key, Dedent(body), sourcePath));
                    key = null;
                }

                continue;
            }

            if (key is not null)
            {
                body.Add(line);
            }
        }

        return key is null
            ? Result.Ok<IReadOnlyList<Snippet>>(snippets)
            : Result.Err<IReadOnlyList<Snippet>>(
                DocSnippetError.UnterminatedRegion.ToError(
                    $"{sourcePath}: snippet '{key}' is never closed. Add the matching #endregion."));
    }

    /// <summary>Reads every snippet under a directory tree, rejecting duplicate keys.</summary>
    /// <param name="root">The directory to walk. Build output is skipped.</param>
    /// <param name="relativeTo">The directory the stamped source paths are made relative to.</param>
    /// <returns>
    /// The snippets keyed by name, or the first file that failed to read or parse.
    /// </returns>
    public static Result<IReadOnlyDictionary<string, Snippet>, Error> ReadDirectory(
        string root,
        string relativeTo) =>
        SourceFiles(root)
           .Select(file => ReadFile(file, relativeTo))
           .Collect()
           .AndThen(files => Index(files.SelectMany(snippets => snippets)));

    private static Result<IReadOnlyList<Snippet>, Error> ReadFile(string file, string relativeTo) =>
        Result
           .Try(() => File.ReadAllText(file))
           .AndThen(text => Read(text, Paths.Forward(Path.GetRelativePath(relativeTo, file))));

    private static Result<IReadOnlyDictionary<string, Snippet>, Error> Index(
        IEnumerable<Snippet> snippets)
    {
        Dictionary<string, Snippet> found = new(StringComparer.Ordinal);

        foreach (Snippet snippet in snippets)
        {
            if (found.TryGetValue(snippet.Key, out Snippet? first))
            {
                return Result.Err<IReadOnlyDictionary<string, Snippet>>(
                    DocSnippetError.DuplicateKey.ToError(
                        $"Snippet '{snippet.Key}' is defined in both {first.SourcePath} and "
                      + $"{snippet.SourcePath}. A key names one block, so one of them has to be renamed."));
            }

            found.Add(snippet.Key, snippet);
        }

        return Result.Ok<IReadOnlyDictionary<string, Snippet>>(found);
    }

    private static IEnumerable<string> SourceFiles(string root) =>
        Directory
           .EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
           .Where(file => !BuildOutput().IsMatch(Paths.Forward(file)))
           .OrderBy(Paths.Forward, StringComparer.Ordinal);

    private static string Dedent(List<string> body)
    {
        List<string> trimmed = [..body];

        while (trimmed.Count > 0 && trimmed[0].Trim().Length == 0)
        {
            trimmed.RemoveAt(0);
        }

        while (trimmed.Count > 0 && trimmed[^1].Trim().Length == 0)
        {
            trimmed.RemoveAt(trimmed.Count - 1);
        }

        int indent = trimmed
                    .Where(line => line.Trim().Length > 0)
                    .Select(line => line.Length - line.TrimStart().Length)
                    .DefaultIfEmpty(0)
                    .Min();

        return string.Join(
            "\n",
            trimmed.Select(line => line.Length < indent ? line.TrimStart() : line[indent..]));
    }

    [GeneratedRegex(@"^\s*#region\s+(?<name>\S+)\s*$")]
    private static partial Regex RegionStart();

    [GeneratedRegex(@"^\s*#endregion\b.*$")]
    private static partial Regex RegionEnd();

    [GeneratedRegex("^[a-z0-9]+(-[a-z0-9]+)*$")]
    private static partial Regex SnippetKey();

    [GeneratedRegex("/(bin|obj)/")]
    private static partial Regex BuildOutput();
}
