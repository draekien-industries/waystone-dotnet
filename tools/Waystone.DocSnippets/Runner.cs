using System.Text.RegularExpressions;
using Waystone.Monads.Results;
using Waystone.Monads.Results.Errors;
using Waystone.Monads.Results.Extensions;

namespace Waystone.DocSnippets;

/// <summary>Where the two repositories are and what to do with them.</summary>
/// <param name="RepositoryRoot">The root of this repository, which stamped source paths are relative to.</param>
/// <param name="SamplesRoot">The directory holding the compiled sample projects.</param>
/// <param name="DocsRoot">The directory holding the markdown pages, in the other repository.</param>
/// <param name="Check">
/// When true nothing is written. A page that would change is reported instead,
/// which is how the pre-push hook turns drift into a failed push.
/// </param>
public sealed record Options(
    string RepositoryRoot,
    string SamplesRoot,
    string DocsRoot,
    bool Check);

/// <summary>What one pass over the pages found.</summary>
/// <param name="StalePages">
/// Pages whose slots did not match their source, relative to the documentation
/// root. In write mode these have been rewritten; in check mode they have not.
/// </param>
/// <param name="UnusedKeys">
/// Snippet regions no page refers to. These are reported but do not fail, since
/// a region written ahead of the page that will use it is a normal half-step.
/// </param>
public sealed record RunResult(IReadOnlyList<string> StalePages, IReadOnlyList<string> UnusedKeys);

/// <summary>Runs one pass: read every region, fill every slot.</summary>
public static partial class Runner
{
    /// <summary>Reads the samples and reconciles every page against them.</summary>
    /// <param name="options">The two roots and the mode.</param>
    /// <returns>
    /// The pages that were out of date and the regions nothing used, or the first
    /// malformed source file, malformed page, or unreadable file.
    /// </returns>
    public static Result<RunResult, Error> Run(Options options) =>
        SnippetReader
           .ReadDirectory(options.SamplesRoot, options.RepositoryRoot)
           .AndThen(
                snippets => Pages(options.DocsRoot)
                           .Select(page => Reconcile(page, snippets, options))
                           .Collect()
                           .Map(pages => Summarise(pages, snippets)));

    private static Result<PageOutcome, Error> Reconcile(
        string page,
        IReadOnlyDictionary<string, Snippet> snippets,
        Options options)
    {
        string relative = Paths.Forward(Path.GetRelativePath(options.DocsRoot, page));

        return Result
              .Try(() => File.ReadAllText(page))
              .AndThen(
                   before => SnippetInjector
                            .Inject(before, snippets, relative)
                            .Map(injection => Write(page, relative, before, injection, options)));
    }

    private static PageOutcome Write(
        string page,
        string relative,
        string before,
        Injection injection,
        Options options)
    {
        bool stale = before != injection.Markdown;

        if (stale && !options.Check)
        {
            File.WriteAllText(page, injection.Markdown);
        }

        return new PageOutcome(relative, stale, injection.Keys);
    }

    private static RunResult Summarise(
        IReadOnlyList<PageOutcome> pages,
        IReadOnlyDictionary<string, Snippet> snippets)
    {
        HashSet<string> used = new(pages.SelectMany(page => page.Keys), StringComparer.Ordinal);

        return new RunResult(
            [..pages.Where(page => page.Stale).Select(page => page.Path)],
            [..snippets.Keys.Where(key => !used.Contains(key)).OrderBy(key => key, StringComparer.Ordinal)]);
    }

    private static IEnumerable<string> Pages(string root) =>
        Directory
           .EnumerateFiles(root, "*.md", SearchOption.AllDirectories)
           .Where(page => !DotDirectory().IsMatch(Paths.Forward(page)))
           .OrderBy(Paths.Forward, StringComparer.Ordinal);

    private sealed record PageOutcome(string Path, bool Stale, IReadOnlyList<string> Keys);

    [GeneratedRegex(@"/\.")]
    private static partial Regex DotDirectory();
}
