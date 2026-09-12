namespace Waystone.Analyzers;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Waystone.Analyzers.FactoryGuards;

/// <remarks>
/// Every source here declares its own guarded type rather than borrowing
/// <c>Option</c>. The rule knows no type by name, and a test written against the
/// one type the repository happens to guard could not tell the difference.
/// </remarks>
internal static class VerifyAnalyzer
{
    private const string Preamble = """
        namespace Subject;

        using System;
        using System.Threading.Tasks;


        """;

    /// <summary>
    /// Runs the rule over <paramref name="source" /> and returns what it reported,
    /// having first failed the test if that source does not compile.
    /// </summary>
    /// <remarks>
    /// The compile check is not ceremony. A source with a typo produces no
    /// operations for the rule to walk, so it reports nothing and the test passes
    /// for the wrong reason — which is the failure mode a rule expecting silence
    /// cannot otherwise distinguish from success.
    /// </remarks>
    /// <param name="source">
    /// The source to analyse, appended to a preamble supplying the usings.
    /// </param>
    /// <returns>The reported diagnostics.</returns>
    public static async Task<ImmutableArray<Diagnostic>> Run(string source)
    {
        CSharpCompilation compilation = Compile(source);

        var errors = compilation
                    .GetDiagnostics()
                    .Where(
                         diagnostic =>
                             diagnostic.Severity == DiagnosticSeverity.Error)
                    .ToList();

        if (errors.Count > 0)
        {
            throw new InvalidOperationException(
                "The test source does not compile, so the rule had nothing to "
              + "walk: "
              + string.Join("; ", errors.Select(error => error.ToString())));
        }

        CompilationWithAnalyzers analysed = compilation.WithAnalyzers(
            ImmutableArray.Create<DiagnosticAnalyzer>(
                new UnguardedFactoryAnalyzer()));

        return await analysed.GetAnalyzerDiagnosticsAsync();
    }

    private static CSharpCompilation Compile(string source) =>
        CSharpCompilation.Create(
            "Waystone.Analyzers.Tests.Subject",
            [
                CSharpSyntaxTree.ParseText(
                    Preamble + source,
                    new CSharpParseOptions(LanguageVersion.Preview)),
            ],
            References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
               .WithNullableContextOptions(NullableContextOptions.Enable));

    private static IEnumerable<MetadataReference> References =>
        AppDomain.CurrentDomain.GetAssemblies()
                 .Where(
                      assembly => !assembly.IsDynamic
                               && assembly.Location.Length > 0)
                 .Select(
                      assembly =>
                          (MetadataReference)MetadataReference.CreateFromFile(
                              assembly.Location))
                 .Distinct();
}
