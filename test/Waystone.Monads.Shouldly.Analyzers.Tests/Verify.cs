namespace Waystone.Monads.Shouldly.Analyzers;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Waystone.Monads.Options;

/// <remarks>
/// <c>global::Shouldly</c> is spelled out because this namespace shadows it: a plain
/// <c>using Shouldly;</c> inside <c>Waystone.Monads.Shouldly.Analyzers</c> binds to the
/// enclosing <c>Waystone.Monads.Shouldly</c>, which holds no types, and every
/// assertion in this project stops compiling. It is the same resolution rule the
/// package's README describes, met from the other side.
/// </remarks>
internal static class Verify
{
    public static DiagnosticResult Diagnostic(DiagnosticDescriptor rule) =>
        new DiagnosticResult(rule);

    public static Task NoDiagnosticAsync<TAnalyzer>(string source)
        where TAnalyzer : DiagnosticAnalyzer, new() =>
        AnalyzerAsync<TAnalyzer>(source);

    public static Task AnalyzerAsync<TAnalyzer>(
        string source,
        params DiagnosticResult[] expected)
        where TAnalyzer : DiagnosticAnalyzer, new()
    {
        var test = new AnalyzerTest<TAnalyzer> { TestCode = Wrap(source) };

        test.ExpectedDiagnostics.AddRange(expected);

        return test.RunAsync();
    }

    /// <summary>
    /// Runs the analyzer over a project that has the monads and Shouldly but not the
    /// assertions package, which is every consumer of the core library.
    /// </summary>
    /// <remarks>
    /// The source still compiles there — a raw assertion is exactly the code that
    /// needs no assertions package — so this pins the gate rather than a compile
    /// error. Without it the rules would offer a consumer a fix producing source they
    /// cannot build.
    /// </remarks>
    public static Task WithoutAssertionsAsync<TAnalyzer>(string source)
        where TAnalyzer : DiagnosticAnalyzer, new() =>
        new AnalyzerTest<TAnalyzer>(withAssertions: false)
        {
            TestCode = Wrap(source),
        }.RunAsync();

    public static Task CodeFixAsync<TAnalyzer, TCodeFix>(
        string source,
        string fixedSource,
        params DiagnosticResult[] expected)
        where TAnalyzer : DiagnosticAnalyzer, new()
        where TCodeFix : CodeFixProvider, new()
    {
        var test = new CodeFixTest<TAnalyzer, TCodeFix>
        {
            TestCode = Wrap(source),
            FixedCode = Wrap(fixedSource),
        };

        test.ExpectedDiagnostics.AddRange(expected);

        return test.RunAsync();
    }

    /// <summary>
    /// Applies the fix to every diagnostic in one batch, which is how a migration
    /// across a whole suite actually runs.
    /// </summary>
    public static Task FixAllAsync<TAnalyzer, TCodeFix>(
        string source,
        string fixedSource,
        params DiagnosticResult[] expected)
        where TAnalyzer : DiagnosticAnalyzer, new()
        where TCodeFix : CodeFixProvider, new()
    {
        var test = new CodeFixTest<TAnalyzer, TCodeFix>
        {
            TestCode = Wrap(source),
            FixedCode = Wrap(fixedSource),
            CodeFixTestBehaviors = CodeFixTestBehaviors.None,
            NumberOfFixAllIterations = 1,
        };

        test.ExpectedDiagnostics.AddRange(expected);

        return test.RunAsync();
    }

    private static string Wrap(string source) =>
        source.Contains("class ")
            ? Usings + source
            : Usings
            + "internal class Subject\n{\n"
            + source
            + "\n}\n";

    private const string Usings = """
                                  using System;
                                  using System.Threading.Tasks;
                                  using Shouldly;
                                  using Waystone.Monads.Options;
                                  using Waystone.Monads.Results;

                                  """;

    private static readonly ImmutableArray<MetadataReference> WithAssertions =
        ImmutableArray.Create<MetadataReference>(
            MetadataReference.CreateFromFile(
                typeof(Option<>).Assembly.Location),
            MetadataReference.CreateFromFile(
                typeof(global::Shouldly.Should).Assembly.Location),
            MetadataReference.CreateFromFile(
                typeof(global::Shouldly.OptionAssertions).Assembly.Location));

    private static readonly ImmutableArray<MetadataReference> WithoutAssertions =
        ImmutableArray.Create<MetadataReference>(
            MetadataReference.CreateFromFile(
                typeof(Option<>).Assembly.Location),
            MetadataReference.CreateFromFile(
                typeof(global::Shouldly.Should).Assembly.Location));

    private sealed class AnalyzerTest<TAnalyzer>
        : CSharpAnalyzerTest<TAnalyzer, DefaultVerifier>
        where TAnalyzer : DiagnosticAnalyzer, new()
    {
        public AnalyzerTest(bool withAssertions = true)
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80;

            TestState.AdditionalReferences.AddRange(
                withAssertions ? WithAssertions : WithoutAssertions);

            SolutionTransforms.Add(EnableNullable);
        }

        protected override ParseOptions CreateParseOptions() =>
            ((CSharpParseOptions)base.CreateParseOptions()).WithLanguageVersion(
                LanguageVersion.Latest);
    }

    private sealed class CodeFixTest<TAnalyzer, TCodeFix>
        : CSharpCodeFixTest<TAnalyzer, TCodeFix, DefaultVerifier>
        where TAnalyzer : DiagnosticAnalyzer, new()
        where TCodeFix : CodeFixProvider, new()
    {
        public CodeFixTest()
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80;
            TestState.AdditionalReferences.AddRange(WithAssertions);
            FixedState.AdditionalReferences.AddRange(WithAssertions);
            SolutionTransforms.Add(EnableNullable);
        }

        protected override ParseOptions CreateParseOptions() =>
            ((CSharpParseOptions)base.CreateParseOptions()).WithLanguageVersion(
                LanguageVersion.Latest);
    }

    private static Solution EnableNullable(
        Solution solution,
        ProjectId projectId)
    {
        var project = solution.GetProject(projectId)!;

        var options = (CSharpCompilationOptions)project.CompilationOptions!;

        return solution.WithProjectCompilationOptions(
            projectId,
            options.WithNullableContextOptions(NullableContextOptions.Enable));
    }
}
