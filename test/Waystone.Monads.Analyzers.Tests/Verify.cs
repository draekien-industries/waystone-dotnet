namespace Waystone.Monads.Analyzers;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Waystone.Monads.Options;

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
    /// Runs the analyzer over <paramref name="rawSource" /> without the usings and
    /// the <c>Subject</c> wrapper, for a rule whose subject is a whole compilation
    /// rather than a member.
    /// </summary>
    public static Task RawAnalyzerAsync<TAnalyzer>(
        string rawSource,
        params DiagnosticResult[] expected)
        where TAnalyzer : DiagnosticAnalyzer, new()
    {
        var test = new AnalyzerTest<TAnalyzer> { TestCode = rawSource };

        test.ExpectedDiagnostics.AddRange(expected);

        return test.RunAsync();
    }

    /// <summary>
    /// Runs the analyzer over <paramref name="rawSource" /> with an
    /// <c>ErrorCodes.txt</c> additional file, which is what opts a project into the
    /// registry rules.
    /// </summary>
    public static Task RegistryAnalyzerAsync<TAnalyzer>(
        string rawSource,
        string registry,
        params DiagnosticResult[] expected)
        where TAnalyzer : DiagnosticAnalyzer, new()
    {
        var test = new AnalyzerTest<TAnalyzer> { TestCode = rawSource };

        test.TestState.AdditionalFiles.Add(
            (ErrorCodeRegistry.FileName, Lf(registry)));

        test.ExpectedDiagnostics.AddRange(expected);

        return test.RunAsync();
    }

    /// <summary>
    /// Applies the code fix and asserts on the resulting <c>ErrorCodes.txt</c> rather
    /// than on the source, which the fix does not touch.
    /// </summary>
    public static Task RegistryCodeFixAsync<TAnalyzer, TCodeFix>(
        string rawSource,
        string registry,
        string fixedRegistry,
        params DiagnosticResult[] expected)
        where TAnalyzer : DiagnosticAnalyzer, new()
        where TCodeFix : CodeFixProvider, new()
    {
        var test = new CodeFixTest<TAnalyzer, TCodeFix>
        {
            TestCode = rawSource,
            FixedCode = rawSource,
        };

        test.TestState.AdditionalFiles.Add(
            (ErrorCodeRegistry.FileName, Lf(registry)));

        test.FixedState.AdditionalFiles.Add(
            (ErrorCodeRegistry.FileName, Lf(fixedRegistry)));

        ExpectNothingAfterTheFix(test);

        test.ExpectedDiagnostics.AddRange(expected);

        return test.RunAsync();
    }

    /// <summary>
    /// Registry content with LF endings, whatever the test file was checked out with.
    /// </summary>
    /// <remarks>
    /// These strings are raw string literals, so they carry the line ending of the
    /// test file itself — CRLF on a clone with <c>core.autocrlf=true</c> and LF on one
    /// without. <c>ErrorCodeRegistry.Render</c> keeps the ending of the file it
    /// rewrites and defaults to LF for a file with no ending to keep, so an expected
    /// value that varies by checkout fails on some machines and not others.
    /// </remarks>
    private static string Lf(string content) => content.Replace("\r\n", "\n");

    /// <summary>
    /// The fixed state expects exactly the diagnostics passed to it and inherits none.
    /// The default is to inherit every unfixable diagnostic from the test state, which
    /// is wrong here: WM2020 has no fix of its own and the WM2019 fix removes the entry
    /// it reports on anyway, so it does not survive into the fixed state.
    /// </summary>
    private static void ExpectNothingAfterTheFix<TAnalyzer, TCodeFix>(
        CodeFixTest<TAnalyzer, TCodeFix> test)
        where TAnalyzer : DiagnosticAnalyzer, new()
        where TCodeFix : CodeFixProvider, new() =>
        test.FixedState.InheritanceMode = StateInheritanceMode.Explicit;

    public static Task CompilerDiagnosticsAsync(
        string rawSource,
        params DiagnosticResult[] expected)
    {
        var test = new AnalyzerTest<EmptyDiagnosticAnalyzer>
        {
            TestCode = rawSource,
        };

        test.ExpectedDiagnostics.AddRange(expected);

        return test.RunAsync();
    }

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

    public static Task CodeFixAsync<TAnalyzer, TCodeFix>(
        string source,
        string fixedSource,
        DiagnosticResult[] expected,
        DiagnosticResult[] remaining)
        where TAnalyzer : DiagnosticAnalyzer, new()
        where TCodeFix : CodeFixProvider, new()
    {
        var test = new CodeFixTest<TAnalyzer, TCodeFix>
        {
            TestCode = Wrap(source),
            FixedCode = Wrap(fixedSource),
        };

        test.ExpectedDiagnostics.AddRange(expected);
        test.FixedState.ExpectedDiagnostics.AddRange(remaining);

        return test.RunAsync();
    }

    public static Task CompilerCodeFixAsync<TCodeFix>(
        string source,
        string fixedSource,
        params DiagnosticResult[] expected)
        where TCodeFix : CodeFixProvider, new()
    {
        var test = new CodeFixTest<EmptyDiagnosticAnalyzer, TCodeFix>
        {
            TestCode = Wrap(source),
            FixedCode = Wrap(fixedSource),
        };

        test.ExpectedDiagnostics.AddRange(expected);

        return test.RunAsync();
    }

    public static Task RawCodeFixAsync<TAnalyzer, TCodeFix>(
        string source,
        string fixedSource,
        params DiagnosticResult[] expected)
        where TAnalyzer : DiagnosticAnalyzer, new()
        where TCodeFix : CodeFixProvider, new()
    {
        var test = new CodeFixTest<TAnalyzer, TCodeFix>
        {
            TestCode = source,
            FixedCode = fixedSource,
        };

        test.ExpectedDiagnostics.AddRange(expected);

        return test.RunAsync();
    }

    private static string Wrap(string source) =>
        source.Contains("class ") || source.Contains("record ")
            ? Usings + source
            : Usings
            + "internal class Subject\n{\n"
            + source
            + "\n}\n";

    private const string Usings = """
                                  using System;
                                  using System.Threading.Tasks;
                                  using Waystone.Monads.Options;
                                  using Waystone.Monads.Options.Extensions;
                                  using Waystone.Monads.Results;
                                  using Waystone.Monads.Results.Errors;
                                  using Waystone.Monads.Results.Extensions;

                                  """;

    private static readonly ImmutableArray<MetadataReference> MonadReferences =
        ImmutableArray.Create<MetadataReference>(
            MetadataReference.CreateFromFile(
                typeof(Option<>).Assembly.Location));

    private sealed class AnalyzerTest<TAnalyzer>
        : CSharpAnalyzerTest<TAnalyzer, DefaultVerifier>
        where TAnalyzer : DiagnosticAnalyzer, new()
    {
        public AnalyzerTest()
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80;
            TestState.AdditionalReferences.AddRange(MonadReferences);
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
            TestState.AdditionalReferences.AddRange(MonadReferences);
            FixedState.AdditionalReferences.AddRange(MonadReferences);
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
