namespace Waystone.Monads.Schemas.SourceGenerators;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Waystone.Monads.Results;

internal static class Verify
{
    private const string Preamble = """
        using Waystone.Monads.Results;
        using Waystone.Monads.Schemas;

        namespace Sample
        {

        """;

    private const string Postscript = """

        }
        """;

    /// <summary>
    /// Runs the generator over <paramref name="source" /> wrapped in a namespace and
    /// the usings every subject needs, and returns what it generated along with every
    /// diagnostic the run produced.
    /// </summary>
    public static GeneratorRun Run(string source) =>
        RunRaw(Preamble + source + Postscript);

    /// <summary>
    /// Runs the generator over source given exactly as written, for the subjects that
    /// cannot sit inside the shared namespace — a schema in the global namespace, or
    /// one spread across declarations that need their own file layout.
    /// </summary>
    public static GeneratorRun RunRaw(string source) => RunRaw([source]);

    /// <summary>
    /// Runs the generator over several syntax trees, which is the only way to reach a
    /// partial class whose parts are in different files.
    /// </summary>
    public static GeneratorRun RunRaw(IReadOnlyList<string> sources)
    {
        CSharpCompilation compilation = Compile(sources);

        GeneratorDriver driver =
            CSharpGeneratorDriver.Create(new SchemaGenerator())
                                 .RunGeneratorsAndUpdateCompilation(
                                      compilation,
                                      out Compilation output,
                                      out ImmutableArray<Diagnostic>
                                          generatorDiagnostics);

        GeneratorDriverRunResult result = driver.GetRunResult();

        return new GeneratorRun(
            result.GeneratedTrees.Select(tree => tree.FilePath)
                  .ToImmutableArray(),
            result.GeneratedTrees.Select(tree => tree.ToString())
                  .ToImmutableArray(),
            generatorDiagnostics,
            output.GetDiagnostics()
                  .Where(
                       diagnostic =>
                           diagnostic.Severity >= DiagnosticSeverity.Warning)
                  .ToImmutableArray());
    }

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
        GeneratorDriver driver =
            CSharpGeneratorDriver.Create(new SchemaGenerator());

        driver = driver.RunGenerators(
            Compile([Preamble + source + Postscript]));

        string first = Emitted(driver.GetRunResult());

        driver = driver.RunGenerators(Compile([Preamble + then + Postscript]));

        return (first, Emitted(driver.GetRunResult()));
    }

    private static CSharpCompilation Compile(IReadOnlyList<string> sources) =>
        CSharpCompilation.Create(
            "Waystone.Monads.Schema.SourceGenerators.Tests.Subject",
            sources.Select(source => CSharpSyntaxTree.ParseText(source)),
            References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
               .WithNullableContextOptions(NullableContextOptions.Enable));

    private static string Emitted(GeneratorDriverRunResult result)
    {
        var diagnostics = string.Join(
            "\n",
            result.Results.SelectMany(run => run.Diagnostics)
                  .Select(diagnostic => diagnostic.ToString())
                  .OrderBy(text => text, StringComparer.Ordinal));

        string generated = string.Join(
            "\n",
            result.GeneratedTrees.Select(tree => tree.ToString()));

        return (generated + diagnostics).Replace("\r\n", "\n");
    }

    private static IEnumerable<MetadataReference> References =>
        AppDomain.CurrentDomain.GetAssemblies()
                 .Where(
                      assembly => !assembly.IsDynamic
                               && assembly.Location.Length > 0)
                 .Select(
                      assembly =>
                          (MetadataReference)MetadataReference.CreateFromFile(
                              assembly.Location))
                 .Distinct()
                 .Append(
                      MetadataReference.CreateFromFile(
                          typeof(Result<,>).Assembly.Location))
                 .Append(
                      MetadataReference.CreateFromFile(
                          typeof(SchemaConfig<,>).Assembly.Location));
}

internal sealed record GeneratorRun(
    ImmutableArray<string> HintNames,
    ImmutableArray<string> Generated,
    ImmutableArray<Diagnostic> GeneratorDiagnostics,
    ImmutableArray<Diagnostic> CompilationDiagnostics)
{
    /// <summary>
    /// The body of the single generated file with <c>\n</c> line endings, or the
    /// empty string when nothing was generated. Normalising here keeps the snapshot
    /// assertions independent of how git checked the test file out.
    /// </summary>
    public string Source =>
        (Generated.SingleOrDefault() ?? string.Empty).Replace("\r\n", "\n");

    public IEnumerable<string> DiagnosticIds =>
        GeneratorDiagnostics.Select(diagnostic => diagnostic.Id);
}
