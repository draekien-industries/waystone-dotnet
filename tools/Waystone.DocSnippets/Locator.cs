using Waystone.Monads.Options;
using Waystone.Monads.Options.Extensions;
using Waystone.Monads.Results;
using Waystone.Monads.Results.Errors;

namespace Waystone.DocSnippets;

/// <summary>One place the documentation repository might be, and what suggested it.</summary>
/// <param name="Source">How this path was arrived at, quoted back when nothing resolves.</param>
/// <param name="Path">The path, or <c>None</c> when that source had nothing to say.</param>
public sealed record Candidate(string Source, Option<string> Path);

/// <summary>Finds the two repositories without either path being written down.</summary>
public static class Locator
{
    /// <summary>The environment variable checked second, after an explicit argument.</summary>
    public const string EnvironmentVariable = "WAYSTONE_DOCS_PATH";

    /// <summary>The git config key checked third. Set it per clone or globally.</summary>
    public const string GitConfigKey = "waystone.docs-path";

    /// <summary>Walks up from a directory to the repository that contains it.</summary>
    /// <param name="start">Any directory inside the repository.</param>
    /// <returns>The directory holding <c>.git</c>, or an error when no ancestor does.</returns>
    public static Result<string, Error> RepositoryRoot(string start)
    {
        for (DirectoryInfo? at = new(start); at is not null; at = at.Parent)
        {
            if (Directory.Exists(Path.Combine(at.FullName, ".git"))
             || File.Exists(Path.Combine(at.FullName, ".git")))
            {
                return Result.Ok(at.FullName);
            }
        }

        return Result.Err<string>(
            DocSnippetError.NotInARepository.ToError($"{start} is not inside a git repository."));
    }

    /// <summary>Picks the first candidate that is really the documentation repository.</summary>
    /// <param name="candidates">The places to try, most explicit first.</param>
    /// <returns>
    /// The full path to the documentation repository, or an error listing every
    /// candidate and how it was arrived at, so the reader can see which one to set
    /// rather than guessing.
    /// </returns>
    public static Result<string, Error> Resolve(IReadOnlyList<Candidate> candidates) =>
        candidates
           .Select(candidate => candidate.Path)
           .FirstOrNone(IsDocumentationRepository)
           .Map(Path.GetFullPath)
           .OkOr(
                DocSnippetError.DocumentationRepositoryNotFound.ToError(
                    "Could not find the documentation repository. Tried:"
                  + string.Concat(candidates.Select(Describe))
                  + $"{Environment.NewLine}Set {EnvironmentVariable}, or run "
                  + $"'git config {GitConfigKey} <path>', or clone it beside this one."));

    /// <summary>Reports whether a directory is the documentation checkout rather than some other folder.</summary>
    /// <param name="path">The directory to test.</param>
    /// <returns>True when it holds the <c>waystone.monads</c> space.</returns>
    public static bool IsDocumentationRepository(string path) =>
        path.Length > 0 && Directory.Exists(Path.Combine(path, "waystone.monads"));

    private static string Describe(Candidate candidate) =>
        candidate.Path.Match(
            path => $"{Environment.NewLine}  {candidate.Source}: {path} (no waystone.monads directory there)",
            () => $"{Environment.NewLine}  {candidate.Source}: not set");
}
