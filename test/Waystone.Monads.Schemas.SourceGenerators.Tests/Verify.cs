namespace Waystone.Monads.Schemas.SourceGenerators;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
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

        GeneratorDriver driver = Drive(
            compilation,
            language,
            out Compilation output,
            out ImmutableArray<Diagnostic> generatorDiagnostics);

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
    /// Runs an analyzer over <paramref name="source" /> wrapped the same way
    /// <see cref="Run" /> wraps it, and returns every diagnostic it reported.
    /// </summary>
    /// <remarks>
    /// Separate from <see cref="Run" /> because an analyzer is not a generator: it
    /// runs over the finished compilation rather than contributing to one, so it
    /// reaches calls in places a generator never sees. The compilation references the
    /// real runtime assembly, which is what makes a rule that compares assemblies
    /// meaningful here.
    /// </remarks>
    public static ImmutableArray<Diagnostic> Analyze(
        DiagnosticAnalyzer analyzer,
        string source) =>
        Compile([Preamble + source + Postscript])
           .WithAnalyzers(ImmutableArray.Create(analyzer))
           .GetAnalyzerDiagnosticsAsync()
           .GetAwaiter()
           .GetResult();

    /// <summary>
    /// Runs an analyzer over source given exactly as written, for the subject that
    /// has to declare its own <c>Waystone.Monads.Schemas</c> namespace and so cannot
    /// sit inside the shared one.
    /// </summary>
    /// <remarks>
    /// A source declaration wins over a referenced assembly for the same
    /// fully-qualified name, so a subject spelling out <c>Schema.For</c> itself binds
    /// to its own and puts the method in the compilation's own assembly. That is the
    /// only way to reach the rule that leaves such an assembly alone.
    /// </remarks>
    public static ImmutableArray<Diagnostic> AnalyzeRaw(
        DiagnosticAnalyzer analyzer,
        string source) =>
        Compile([source])
           .WithAnalyzers(ImmutableArray.Create(analyzer))
           .GetAnalyzerDiagnosticsAsync()
           .GetAwaiter()
           .GetResult();

    /// <summary>
    /// Runs an analyzer over <paramref name="source" /> after the generator has run
    /// over it, for a rule about a call that binds only to a generated member.
    /// </summary>
    public static ImmutableArray<Diagnostic> AnalyzeGenerated(
        DiagnosticAnalyzer analyzer,
        string source) =>
        Generate(Preamble + source + Postscript)
           .WithAnalyzers(ImmutableArray.Create(analyzer))
           .GetAnalyzerDiagnosticsAsync()
           .GetAwaiter()
           .GetResult();

    /// <summary>
    /// Fixes every diagnostic the analyzer reports in <paramref name="source" />
    /// through the provider's own fix-all, and returns the subject as it reads
    /// afterwards.
    /// </summary>
    /// <remarks>
    /// Hand-built rather than <c>CSharpCodeFixTest</c>, which the other analyzer
    /// projects use: the call being fixed binds only to generated output, so the
    /// generated files are added to the workspace as ordinary documents.
    /// </remarks>
    public static async Task<string> FixAsync(
        DiagnosticAnalyzer analyzer,
        CodeFixProvider provider,
        string source)
    {
        string text = Preamble + source + Postscript;
        Compilation generated = Generate(text);

        using var workspace = new AdhocWorkspace();

        Project project = workspace
                         .AddProject("Subject", LanguageNames.CSharp)
                         .WithCompilationOptions(generated.Options)
                         .WithParseOptions(
                              generated.SyntaxTrees.First().Options)
                         .AddMetadataReferences(generated.References);

        foreach (SyntaxTree tree in generated.SyntaxTrees.Skip(1))
        {
            project = project.AddDocument(tree.FilePath, tree.GetText())
                             .Project;
        }

        Document document = project.AddDocument("Subject.cs", text);

        ImmutableArray<Diagnostic> diagnostics =
            await (await document.Project.GetCompilationAsync())!
                 .WithAnalyzers(ImmutableArray.Create(analyzer))
                 .GetAnalyzerDiagnosticsAsync();

        CodeAction fix = (await provider.GetFixAllProvider()!
                                        .GetFixAsync(
                                             new FixAllContext(
                                                 document,
                                                 provider,
                                                 FixAllScope.Document,
                                                 provider.GetType().Name,
                                                 provider.FixableDiagnosticIds,
                                                 new FixedDiagnostics(diagnostics),
                                                 CancellationToken.None)))!;

        Solution solution =
            (await fix.GetOperationsAsync(CancellationToken.None))
           .OfType<ApplyChangesOperation>()
           .Single()
           .ChangedSolution;

        string fixedText =
            (await solution.GetDocument(document.Id)!.GetTextAsync()).ToString();

        return fixedText.Substring(
            Preamble.Length,
            fixedText.Length - Preamble.Length - Postscript.Length);
    }

    private static Compilation Generate(string source)
    {
        Drive(Compile([source]), LanguageVersion.Latest, out Compilation output, out _);

        return output;
    }

    private static GeneratorDriver Drive(
        Compilation compilation,
        LanguageVersion language,
        out Compilation output,
        out ImmutableArray<Diagnostic> diagnostics) =>
        CSharpGeneratorDriver.Create(
                                  [new SchemaGenerator().AsSourceGenerator()],
                                  parseOptions: new CSharpParseOptions(language))
                             .RunGeneratorsAndUpdateCompilation(
                                  compilation,
                                  out output,
                                  out diagnostics);

    /// <summary>Hands fix-all the diagnostics already computed for the document.</summary>
    private sealed class FixedDiagnostics(ImmutableArray<Diagnostic> diagnostics)
        : FixAllContext.DiagnosticProvider
    {
        public override async Task<IEnumerable<Diagnostic>>
            GetDocumentDiagnosticsAsync(
                Document document,
                CancellationToken cancellationToken)
        {
            SyntaxTree tree = (await document.GetSyntaxTreeAsync(cancellationToken))!;

            return diagnostics.Where(found => found.Location.SourceTree == tree);
        }

        public override Task<IEnumerable<Diagnostic>> GetProjectDiagnosticsAsync(
            Project project,
            CancellationToken cancellationToken) =>
            Task.FromResult(Enumerable.Empty<Diagnostic>());

        public override Task<IEnumerable<Diagnostic>> GetAllDiagnosticsAsync(
            Project project,
            CancellationToken cancellationToken) =>
            Task.FromResult(Enumerable.Empty<Diagnostic>());
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
