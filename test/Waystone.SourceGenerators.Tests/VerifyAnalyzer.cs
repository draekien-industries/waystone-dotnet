namespace Waystone.SourceGenerators;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Waystone.Monads.Options;
using Waystone.SourceGenerators.AsyncSurface;

internal static class VerifyAnalyzer
{
    private const string Preamble = """
        namespace Waystone.Monads.Subject;

        using System;
        using System.Threading.Tasks;
        using Waystone.Monads.Options;
        using Waystone.Monads.Results;


        """;

    /// <summary>
    /// Runs the async-surface analyzer over <paramref name="source" /> and returns
    /// every diagnostic it reported.
    /// </summary>
    /// <param name="source">The source to analyse, appended to a fixed preamble.</param>
    /// <param name="withMonads">
    /// If false, compiles without a reference to <c>Waystone.Monads</c>, which is
    /// the case the rules must stay silent for.
    /// </param>
    /// <returns>The reported diagnostics, in the order the analyzer produced them.</returns>
    public static async Task<ImmutableArray<Diagnostic>> Run(
        string source,
        bool withMonads = true)
    {
        CSharpCompilation compilation = CSharpCompilation.Create(
            "Waystone.SourceGenerators.Tests.AnalyzerSubject",
            [
                CSharpSyntaxTree.ParseText(
                    Preamble + source,
                    new CSharpParseOptions(LanguageVersion.Preview)),
            ],
            withMonads ? References : References.Where(IsNotMonads),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
               .WithNullableContextOptions(NullableContextOptions.Enable));

        CompilationWithAnalyzers analysed = compilation.WithAnalyzers(
            ImmutableArray.Create<DiagnosticAnalyzer>(new AsyncSurfaceAnalyzer()));

        return await analysed.GetAnalyzerDiagnosticsAsync();
    }

    private static bool IsNotMonads(MetadataReference reference) =>
        reference.Display?.Contains("Waystone.Monads") != true;

    private static IEnumerable<MetadataReference> References =>
        AppDomain.CurrentDomain.GetAssemblies()
                 .Where(
                      assembly => !assembly.IsDynamic
                               && assembly.Location.Length > 0)
                 .Select(
                      assembly =>
                          (MetadataReference)MetadataReference.CreateFromFile(
                              assembly.Location))
                 .Append(
                      MetadataReference.CreateFromFile(
                          typeof(Option<>).Assembly.Location))
                 .Distinct();
}
