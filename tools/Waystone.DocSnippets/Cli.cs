using Waystone.Monads.Options;
using Waystone.Monads.Results;
using Waystone.Monads.Results.Errors;

namespace Waystone.DocSnippets;

/// <summary>The command line, with everything it touches passed in.</summary>
public static class Cli
{
    /// <summary>
    /// The exit code for a documentation checkout that could not be found. It is
    /// distinct because a hook treats that as "not my problem" and carries on,
    /// where every other failure blocks the push.
    /// </summary>
    /// <remarks>
    /// <c>.githooks/pre-push</c> branches on this number as a literal, since a shell
    /// script cannot read a C# constant. Change one and change the other. The
    /// documentation repository's hook passes <c>--docs</c> outright, so it can never
    /// see this code.
    /// </remarks>
    public const int NoDocumentationRepository = 3;

    /// <summary>Runs one pass and reports it.</summary>
    /// <param name="args">
    /// <c>--check</c> to report without writing, <c>--docs &lt;path&gt;</c> to name
    /// the documentation checkout outright, and <c>--repo &lt;path&gt;</c> to name the
    /// checkout holding the samples. The documentation repository's own hook passes
    /// both, because neither is where it is running.
    /// </param>
    /// <param name="workingDirectory">
    /// Where to start looking for the repository holding the samples, used when
    /// <c>--repo</c> is absent. Any directory inside it will do.
    /// </param>
    /// <param name="gitConfig">
    /// Reads <see cref="Locator.GitConfigKey" /> given the repository root. Passed
    /// in rather than shelled out to here, so a test drives the third candidate
    /// without touching the machine's git configuration.
    /// </param>
    /// <param name="output">Where progress goes.</param>
    /// <param name="error">Where the reason for a non-zero exit goes.</param>
    /// <returns>
    /// Zero when every page matches, <see cref="NoDocumentationRepository" /> when
    /// there was nothing to check against, and one for a stale page under
    /// <c>--check</c> or any malformed input.
    /// </returns>
    public static int Run(
        string[] args,
        string workingDirectory,
        Func<string, Option<string>> gitConfig,
        TextWriter output,
        TextWriter error) =>
        Locator
           .RepositoryRoot(Argument(args, "--repo").UnwrapOr(workingDirectory))
           .AndThen(root => BuildOptions(args, root, gitConfig))
           .AndThen(options => Runner.Run(options).Map(result => (options, result)))
           .Match(pass => Report(pass.options, pass.result, output, error), failure => Fail(failure, error));

    private static Result<Options, Error> BuildOptions(
        string[] args,
        string repositoryRoot,
        Func<string, Option<string>> gitConfig) =>
        Locator
           .Resolve(
            [
                new Candidate("--docs", Argument(args, "--docs")),
                new Candidate(
                    $"${Locator.EnvironmentVariable}",
                    NotEmpty(Environment.GetEnvironmentVariable(Locator.EnvironmentVariable))),
                new Candidate($"git config {Locator.GitConfigKey}", gitConfig(repositoryRoot)),
                new Candidate(
                    "a checkout beside this one",
                    Option.Some(Path.Combine(repositoryRoot, "..", "docs"))),
                new Candidate("a sibling of this checkout", Sibling(repositoryRoot)),
            ])
           .Map(
                docsRoot => new Options(
                    repositoryRoot,
                    Path.Combine(repositoryRoot, "sample"),
                    docsRoot,
                    args.Contains("--check")));

    private static int Report(Options options, RunResult result, TextWriter output, TextWriter error)
    {
        foreach (string key in result.UnusedKeys)
        {
            output.WriteLine($"note: snippet '{key}' is defined but no page uses it.");
        }

        foreach (string page in result.StalePages)
        {
            output.WriteLine(options.Check ? $"stale: {page}" : $"updated: {page}");
        }

        if (result.StalePages.Count == 0)
        {
            output.WriteLine($"Every documentation snippet matches its source ({options.DocsRoot}).");

            return 0;
        }

        if (!options.Check)
        {
            return 0;
        }

        error.WriteLine(
            $"{result.StalePages.Count} page(s) no longer match the samples they came from.");
        error.WriteLine(
            "Run 'dotnet run --project tools/Waystone.DocSnippets' and commit the result in the "
          + "documentation repository.");

        return 1;
    }

    private static int Fail(Error failure, TextWriter error)
    {
        error.WriteLine($"{failure.Code}: {failure.Message}");

        return failure.Code == DocSnippetError.DocumentationRepositoryNotFound.ToErrorCode()
            ? NoDocumentationRepository
            : 1;
    }

    private static Option<string> Argument(string[] args, string name)
    {
        int at = Array.IndexOf(args, name);

        return at >= 0 && at + 1 < args.Length ? NotEmpty(args[at + 1]) : Option.None<string>();
    }

    private static Option<string> NotEmpty(string? value) =>
        string.IsNullOrWhiteSpace(value) ? Option.None<string>() : Option.Some(value);

    private static Option<string> Sibling(string repositoryRoot)
    {
        DirectoryInfo? parent = Directory.GetParent(repositoryRoot);

        return parent is null
            ? Option.None<string>()
            : NotEmpty(
                parent
                   .EnumerateDirectories()
                   .Select(directory => directory.FullName)
                   .Order(StringComparer.Ordinal)
                   .FirstOrDefault(Locator.IsDocumentationRepository));
    }
}
