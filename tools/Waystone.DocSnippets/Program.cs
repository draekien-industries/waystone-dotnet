using System.Diagnostics.CodeAnalysis;
using Waystone.Monads.Options;

namespace Waystone.DocSnippets;

/// <summary>
/// The entry point, and nothing else. Everything worth testing is in
/// <see cref="Cli" />, which takes its working directory, its git configuration
/// reader and both writers as arguments.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Wiring: it reads the process and hands every value to Cli.")]
public static class Program
{
    /// <summary>Reconciles the documentation pages with the samples they came from.</summary>
    /// <param name="args">The command line. See <see cref="Cli.Run" />.</param>
    /// <returns>The process exit code.</returns>
    public static int Main(string[] args) =>
        Cli.Run(
            args,
            Environment.CurrentDirectory,
            GitConfig.DocsPath,
            Console.Out,
            Console.Error);
}
