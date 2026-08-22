namespace Waystone.SourceGenerators;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Waystone.Monads.Options;
using Waystone.SourceGenerators.AwaitedReceivers;

internal static class Verify
{
    private const string Preamble = """
        namespace Waystone.Monads.Options.Extensions;

        using System;
        using System.Collections.Generic;
        using System.Threading.Tasks;
        using Waystone.Monads.Options;
        using Waystone.SourceGenerators;


        """;

    /// <summary>
    /// Runs the generator over <paramref name="source" /> and returns the one file it
    /// generated for the marked class, along with every diagnostic the run produced.
    /// </summary>
    public static GeneratorRun Run(string source)
    {
        CSharpCompilation compilation = CSharpCompilation.Create(
            "Waystone.SourceGenerators.Tests.Subject",
            [
                CSharpSyntaxTree.ParseText(
                    Preamble + source,
                    new CSharpParseOptions(LanguageVersion.Preview)),
            ],
            References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
               .WithNullableContextOptions(NullableContextOptions.Enable));

        GeneratorDriver driver = CSharpGeneratorDriver
                                .Create(new AwaitedReceiversGenerator())
                                .WithUpdatedParseOptions(
                                     new CSharpParseOptions(LanguageVersion.Preview));

        driver = driver.RunGeneratorsAndUpdateCompilation(
            compilation,
            out Compilation output,
            out ImmutableArray<Diagnostic> generatorDiagnostics);

        GeneratorDriverRunResult result = driver.GetRunResult();

        string? generated = result.GeneratedTrees
                                  .Where(
                                       tree => !tree.FilePath.EndsWith(
                                           GeneratedAttributes.HintName,
                                           StringComparison.Ordinal))
                                  .Select(tree => tree.ToString())
                                  .SingleOrDefault();

        return new GeneratorRun(
            generated,
            generatorDiagnostics,
            output.GetDiagnostics()
                  .Where(
                       diagnostic =>
                           diagnostic.Severity >= DiagnosticSeverity.Warning)
                  .ToImmutableArray());
    }

    /// <summary>
    /// Runs one driver over two separately parsed but identical compilations and
    /// returns what each run generated. The second run makes Roslyn compare the
    /// values its steps produced against the cached ones, which is the only thing
    /// that exercises the equality the pipeline records depend on.
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
        GeneratorDriver driver = CSharpGeneratorDriver
                                .Create(new AwaitedReceiversGenerator())
                                .WithUpdatedParseOptions(
                                     new CSharpParseOptions(LanguageVersion.Preview));

        driver = driver.RunGenerators(Compile(source));

        string first = Emitted(driver.GetRunResult());

        driver = driver.RunGenerators(Compile(then));

        return (first, Emitted(driver.GetRunResult()));
    }

    private static CSharpCompilation Compile(string source) =>
        CSharpCompilation.Create(
            "Waystone.SourceGenerators.Tests.Subject",
            [
                CSharpSyntaxTree.ParseText(
                    Preamble + source,
                    new CSharpParseOptions(LanguageVersion.Preview)),
            ],
            References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
               .WithNullableContextOptions(NullableContextOptions.Enable));

    private static string Emitted(GeneratorDriverRunResult result)
    {
        var emitted = string.Join(
            "\n",
            result.Results
                  .SelectMany(run => run.Diagnostics)
                  .Select(diagnostic => diagnostic.ToString())
                  .OrderBy(text => text, StringComparer.Ordinal));

        string? generated = result.GeneratedTrees
                                  .Where(
                                       tree => !tree.FilePath.EndsWith(
                                           GeneratedAttributes.HintName,
                                           StringComparison.Ordinal))
                                  .Select(tree => tree.ToString())
                                  .SingleOrDefault();

        return ((generated ?? string.Empty) + emitted).Replace(
            "\r\n",
            "\n");
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
                 .Append(
                      MetadataReference.CreateFromFile(
                          typeof(Option<>).Assembly.Location))
                 .Distinct();
}

internal sealed record GeneratorRun(
    string? Generated,
    ImmutableArray<Diagnostic> GeneratorDiagnostics,
    ImmutableArray<Diagnostic> CompilationDiagnostics)
{
    /// <summary>
    /// The body of the generated file with <c>\n</c> line endings, or the empty string
    /// when nothing was generated. Normalising here keeps the snapshot assertions
    /// independent of how git checked the test file out.
    /// </summary>
    public string Source => (Generated ?? string.Empty).Replace("\r\n", "\n");

    public IEnumerable<string> DiagnosticIds =>
        GeneratorDiagnostics.Select(diagnostic => diagnostic.Id);
}
