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
using Waystone.Monads.Schemas;

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
    /// Runs the analyzer over a source that does not compile, asserting on the rule's
    /// own diagnostics and ignoring the compiler's.
    /// </summary>
    /// <remarks>
    /// For a rule whose subject is a call that failed overload resolution. Every such
    /// source carries at least one compiler error by construction, and which one it
    /// carries is the compiler's choice rather than the rule's — listing them here
    /// would pin the test to CS0411 against CS1503 against CS1501 and assert nothing
    /// about the analyzer.
    /// </remarks>
    public static Task BrokenAnalyzerAsync<TAnalyzer>(
        string source,
        params DiagnosticResult[] expected)
        where TAnalyzer : DiagnosticAnalyzer, new()
    {
        var test = new AnalyzerTest<TAnalyzer>
        {
            TestCode = Wrap(source),
            CompilerDiagnostics = CompilerDiagnostics.None,
        };

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

    /// <summary>
    /// Compiles <paramref name="rawSource" /> against
    /// <c>Waystone.Monads.Schema</c> as well, for the closed-hierarchy probes.
    /// </summary>
    public static Task SchemaCompilerDiagnosticsAsync(
        string rawSource,
        params DiagnosticResult[] expected)
    {
        var test = new AnalyzerTest<EmptyDiagnosticAnalyzer>
        {
            TestCode = rawSource,
        };

        test.TestState.AdditionalReferences.AddRange(SchemaReferences);
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

    /// <summary>
    /// Runs a fix registered on a compiler diagnostic and states the fixed state's
    /// diagnostics outright, for a fix that resolves more than the one it is
    /// registered on.
    /// </summary>
    /// <remarks>
    /// The <c>params</c> overload lets the fixed state inherit, which drops the
    /// fixable diagnostics and keeps the rest. That is wrong for a nullability fix:
    /// CS8714 is the fixable one, and the CS8619, CS8621 and CS0029 reported beside
    /// it come from the same mismatch and go away with it, so inheriting them looks
    /// for markup the fixed source has no reason to carry. Pass the diagnostics that
    /// genuinely survive instead, which is none when the fix applies and all of them
    /// when it declines.
    /// <para>
    /// Note that <paramref name="remaining" /> means something different here than in
    /// the identically shaped <see cref="CodeFixAsync{TAnalyzer,TCodeFix}(string,
    /// string, DiagnosticResult[], DiagnosticResult[])" /> pair below, which adds to
    /// an inherited set rather than replacing it.
    /// </para>
    /// </remarks>
    public static Task CompilerCodeFixAsync<TCodeFix>(
        string source,
        string fixedSource,
        DiagnosticResult[] expected,
        DiagnosticResult[] remaining)
        where TCodeFix : CodeFixProvider, new()
    {
        var test = new CodeFixTest<EmptyDiagnosticAnalyzer, TCodeFix>
        {
            TestCode = Wrap(source),
            FixedCode = Wrap(fixedSource),
        };

        ExpectNothingAfterTheFix(test);

        test.ExpectedDiagnostics.AddRange(expected);
        test.FixedState.ExpectedDiagnostics.AddRange(remaining);

        return test.RunAsync();
    }

    /// <summary>
    /// Asserts that a fix registered on a compiler diagnostic declines, by running it
    /// over a source whose fixed state is the source itself with
    /// <paramref name="standing" /> still reported.
    /// </summary>
    public static Task DeclinedCompilerCodeFixAsync<TCodeFix>(
        string source,
        DiagnosticResult standing)
        where TCodeFix : CodeFixProvider, new() =>
        CompilerCodeFixAsync<TCodeFix>(
            source,
            source,
            new[] { standing },
            new[] { standing });

    /// <summary>
    /// Applies the code fix over a source that does not compile, ignoring the
    /// compiler's diagnostics in both states.
    /// </summary>
    /// <remarks>
    /// The counterpart of <see cref="BrokenAnalyzerAsync{TAnalyzer}" />. The fixed
    /// state is checked the same way, because the wrap this covers resolves the
    /// compiler error along with the rule's own diagnostic and asserting on that
    /// would restate the compiler's behaviour rather than the fix's.
    /// </remarks>
    public static Task BrokenCodeFixAsync<TAnalyzer, TCodeFix>(
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
            CompilerDiagnostics = CompilerDiagnostics.None,
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

    /// <summary>
    /// Gets the reference assemblies for the framework this test host is running on.
    /// </summary>
    /// <remarks>
    /// Tracks the host rather than pinning a version. Pinned, every framework in the
    /// matrix compiled the identical net8.0 source, so running this project on five
    /// of them proved one thing five times. Tracking the host is what makes the four
    /// extra runs test something the net8.0 run does not.
    /// </remarks>
    private static ReferenceAssemblies Target =>
#if NET10_0
        ReferenceAssemblies.Net.Net100;
#elif NET9_0
        ReferenceAssemblies.Net.Net90;
#elif NET8_0
        ReferenceAssemblies.Net.Net80;
#elif NET472
        WithValueTask(ReferenceAssemblies.NetFramework.Net472.Default);
#else
        WithValueTask(ReferenceAssemblies.NetFramework.Net48.Default);
#endif

#if NETFRAMEWORK
    /// <summary>
    /// Adds the package carrying <see cref="ValueTask{TResult}" />, which .NET
    /// Framework does not have.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Several rules here are tested against a <c>ValueTask</c> receiver, and without
    /// this those tests fail on <c>CS0012</c> naming an assembly their source never
    /// mentions.
    /// </para>
    /// <para>
    /// It has to be the package rather than the <c>ValueTask</c> this host has already
    /// loaded, tempting as the latter is for tracking the version by itself. Adding
    /// the runtime assembly directly gives the compilation a second definition of the
    /// type beside the platform's, and the five <c>AsTaskCodeFixTests</c> that assert
    /// on a compiler error then see it rendered against the wrong one — they expect
    /// <c>ValueTask&lt;int&gt;</c> and get the fully qualified name. Restoring the
    /// package brings its reference assembly and its transitive dependencies, which is
    /// what a consumer on this framework actually compiles against.
    /// </para>
    /// <para>
    /// The version is duplicated from <c>Directory.Packages.props</c> because a
    /// synthetic compilation takes no part in the project's package graph. Keep the
    /// two in step by hand; nothing in the build compares them.
    /// </para>
    /// </remarks>
    private static ReferenceAssemblies WithValueTask(
        ReferenceAssemblies assemblies) =>
        assemblies.AddPackages(
            ImmutableArray.Create(
                new PackageIdentity(
                    "System.Threading.Tasks.Extensions",
                    "4.6.3")));
#endif

    private static readonly ImmutableArray<MetadataReference> MonadReferences =
        ImmutableArray.Create<MetadataReference>(
            MetadataReference.CreateFromFile(
                typeof(Option<>).Assembly.Location));

    /// <summary>
    /// Added only by <see cref="SchemaCompilerDiagnosticsAsync" />, never to
    /// <see cref="MonadReferences" />.
    /// </summary>
    /// <remarks>
    /// <c>Waystone.Monads.Schema</c> declares an <c>[ErrorCodeCatalog]</c> enum, so
    /// referencing it everywhere puts <c>ViolationCode</c>'s codes into every
    /// compilation and changes what the error code registry rules see. That failed
    /// four <c>UpdateErrorCodeRegistryCodeFixTests</c> cases, which assert on the
    /// registry the fix writes.
    /// </remarks>
    private static readonly ImmutableArray<MetadataReference> SchemaReferences =
        ImmutableArray.Create<MetadataReference>(
            MetadataReference.CreateFromFile(
                typeof(Field).Assembly.Location));

    private sealed class AnalyzerTest<TAnalyzer>
        : CSharpAnalyzerTest<TAnalyzer, DefaultVerifier>
        where TAnalyzer : DiagnosticAnalyzer, new()
    {
        public AnalyzerTest()
        {
            ReferenceAssemblies = Target;
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
            ReferenceAssemblies = Target;
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
