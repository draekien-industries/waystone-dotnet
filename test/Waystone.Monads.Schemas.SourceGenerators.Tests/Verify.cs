namespace Waystone.Monads.Schemas.SourceGenerators;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
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
    public static GeneratorRun RunRaw(
        IReadOnlyList<string> sources,
        LanguageVersion language = LanguageVersion.Latest)
    {
        CSharpCompilation compilation = Compile(sources, language);

        GeneratorDriver driver =
            CSharpGeneratorDriver.Create(
                                      [
                                          new SchemaGenerator()
                                             .AsSourceGenerator(),
                                      ],
                                      parseOptions: new CSharpParseOptions(
                                          language))
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

    /// <summary>
    /// Runs the generator over a subject compiled as C# 7.3, the version a net472
    /// project still gets by default. The emitted generic constraints cannot be
    /// spelled there, so this is the only thing that proves the generator notices.
    /// </summary>
    /// <remarks>
    /// Nullable analysis is off as well, and has to be: enabling it under 7.3 is
    /// <c>CS8630</c> before the generator runs at all.
    /// </remarks>
    public static GeneratorRun RunOnCSharp73(string source) =>
        RunRaw([Preamble + source + Postscript], LanguageVersion.CSharp7_3);

    private static CSharpCompilation Compile(
        IReadOnlyList<string> sources,
        LanguageVersion language = LanguageVersion.Latest) =>
        CSharpCompilation.Create(
            "Waystone.Monads.Schemas.SourceGenerators.Tests.Subject",
            sources.Select(
                source => CSharpSyntaxTree.ParseText(
                    source,
                    new CSharpParseOptions(language))),
            References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
               .WithNullableContextOptions(
                    language == LanguageVersion.CSharp7_3
                        ? NullableContextOptions.Disable
                        : NullableContextOptions.Enable));

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

    /// <summary>Asserts the whole generated file matches a snapshot.</summary>
    /// <param name="expected">
    /// The entire expected file, as a raw string literal. Its line endings are
    /// normalised before comparing, which <see cref="Source" /> alone cannot do:
    /// the literal carries whatever endings git checked this test file out with,
    /// so asserting on it directly passes on a LF checkout and fails on a CRLF
    /// one.
    /// </param>
    public void ShouldEmit(string expected) =>
        Source.ShouldBe(expected.Replace("\r\n", "\n"));
}
