namespace Waystone.Monads.SourceGenerators;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Waystone.Monads.Options;
using Waystone.Monads.SourceGenerators.ErrorCodes;

internal static class Verify
{
    private const string Preamble = """
        namespace Sample;

        using System;
        using Waystone.Monads.Results.Errors;


        """;

    /// <summary>
    /// Runs the generator over <paramref name="source" /> and returns what it
    /// generated along with every diagnostic the run produced.
    /// </summary>
    public static GeneratorRun Run(string source) =>
        Run(Preamble + source, withMonadsReference: true);

    /// <summary>
    /// Runs the generator with assembly-level attributes, which have to precede the
    /// file-scoped namespace and so cannot be appended to the preamble.
    /// </summary>
    public static GeneratorRun RunWithAssemblyAttributes(
        string attributes,
        string source) =>
        Run(
            "using Waystone.Monads.Results.Errors;"
          + Environment.NewLine
          + attributes
          + Environment.NewLine
          + Preamble
          + source,
            withMonadsReference: true);

    /// <summary>
    /// Runs the generator over a compilation that does not reference
    /// <c>Waystone.Monads</c>, so the error types cannot be resolved. The attribute
    /// is declared in the source instead, since the pipeline is keyed on it.
    /// </summary>
    public static GeneratorRun RunWithoutMonads(string source) =>
        Run(
            """
            namespace Waystone.Monads.Results.Errors
            {
                [System.AttributeUsage(System.AttributeTargets.Enum)]
                public sealed class ErrorCodeProviderAttribute : System.Attribute
                {
                }
            }

            """
          + source,
            withMonadsReference: false);

    /// <summary>
    /// Runs one driver over two identical but separately parsed compilations. The
    /// second run makes Roslyn compare the values its steps produced against the
    /// cached ones, which is the only thing that exercises the pipeline's equality.
    /// </summary>
    public static (string First, string Second) RunTwice(string source) =>
        RunTwice(source, source);

    /// <summary>
    /// Runs one driver over two different compilations, so the cached values the
    /// second run compares against do not match and the pipeline has to rebuild.
    /// </summary>
    public static (string First, string Second) RunTwice(
        string source,
        string then)
    {
        GeneratorDriver driver = Driver();

        driver = driver.RunGenerators(Compile(Preamble + source, true));

        string first = Emitted(driver.GetRunResult());

        driver = driver.RunGenerators(Compile(Preamble + then, true));

        return (first, Emitted(driver.GetRunResult()));
    }

    private static GeneratorRun Run(string source, bool withMonadsReference)
    {
        CSharpCompilation compilation = Compile(source, withMonadsReference);

        GeneratorDriver driver = Driver()
           .RunGeneratorsAndUpdateCompilation(
                compilation,
                out Compilation output,
                out ImmutableArray<Diagnostic> generatorDiagnostics);

        GeneratorDriverRunResult result = driver.GetRunResult();

        return new GeneratorRun(
            result.GeneratedTrees.Select(tree => tree.FilePath)
                  .ToImmutableArray(),
            result.GeneratedTrees.Select(tree => tree.ToString())
                  .SingleOrDefault(),
            generatorDiagnostics,
            output.GetDiagnostics()
                  .Where(
                       diagnostic =>
                           diagnostic.Severity >= DiagnosticSeverity.Warning)
                  .ToImmutableArray());
    }

    private static GeneratorDriver Driver() =>
        CSharpGeneratorDriver.Create(new ErrorCodeProviderGenerator())
                             .WithUpdatedParseOptions(ParseOptions);

    private static CSharpParseOptions ParseOptions =>
        new CSharpParseOptions(LanguageVersion.Preview);

    private static CSharpCompilation Compile(
        string source,
        bool withMonadsReference) =>
        CSharpCompilation.Create(
            "Waystone.Monads.SourceGenerators.Tests.Subject",
            [CSharpSyntaxTree.ParseText(source, ParseOptions)],
            withMonadsReference ? References : FrameworkReferences,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
               .WithNullableContextOptions(NullableContextOptions.Enable));

    private static string Emitted(GeneratorDriverRunResult result)
    {
        var diagnostics = string.Join(
            "\n",
            result.Results.SelectMany(run => run.Diagnostics)
                  .Select(diagnostic => diagnostic.ToString())
                  .OrderBy(text => text, StringComparer.Ordinal));

        string? generated = result.GeneratedTrees
                                  .Select(tree => tree.ToString())
                                  .SingleOrDefault();

        return ((generated ?? string.Empty) + diagnostics).Replace("\r\n", "\n");
    }

    private static IEnumerable<MetadataReference> FrameworkReferences =>
        AppDomain.CurrentDomain.GetAssemblies()
                 .Where(
                      assembly => !assembly.IsDynamic
                               && assembly.Location.Length > 0
                               && assembly.GetName().Name
                               != typeof(Option<>).Assembly.GetName().Name)
                 .Select(
                      assembly =>
                          (MetadataReference)MetadataReference.CreateFromFile(
                              assembly.Location))
                 .Distinct();

    private static IEnumerable<MetadataReference> References =>
        FrameworkReferences.Append(
            MetadataReference.CreateFromFile(
                typeof(Option<>).Assembly.Location));
}

internal sealed record GeneratorRun(
    ImmutableArray<string> HintNames,
    string? Generated,
    ImmutableArray<Diagnostic> GeneratorDiagnostics,
    ImmutableArray<Diagnostic> CompilationDiagnostics)
{
    /// <summary>
    /// The body of the generated file with <c>\n</c> line endings, or the empty
    /// string when nothing was generated. Normalising here keeps the snapshot
    /// assertions independent of how git checked the test file out.
    /// </summary>
    public string Source => (Generated ?? string.Empty).Replace("\r\n", "\n");

    public IEnumerable<string> DiagnosticIds =>
        GeneratorDiagnostics.Select(diagnostic => diagnostic.Id);
}
