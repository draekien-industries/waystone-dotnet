using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Waystone.Monads.Options;

namespace Waystone.DocSnippets;

/// <summary>Reads the documentation path out of git's own configuration.</summary>
[ExcludeFromCodeCoverage(
    Justification =
        "Shells out to git. A test of it would assert against the machine's own configuration.")]
public static class GitConfig
{
    /// <summary>Reads <see cref="Locator.GitConfigKey" />, taking whatever git resolves.</summary>
    /// <param name="repositoryRoot">The directory git is run from, so a repository-local value wins over a global one.</param>
    /// <returns>The configured path, or <c>None</c> when it is unset, blank, or git is not on the path.</returns>
    public static Option<string> DocsPath(string repositoryRoot) =>
        Option
           .Try(
                () =>
                {
                    using Process? process = Process.Start(
                        new ProcessStartInfo("git", ["config", "--get", Locator.GitConfigKey])
                        {
                            WorkingDirectory = repositoryRoot,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                        });

                    string value = process?.StandardOutput.ReadToEnd().Trim() ?? string.Empty;
                    process?.WaitForExit();

                    return value;
                })
           .Filter(value => value.Length > 0);
}
