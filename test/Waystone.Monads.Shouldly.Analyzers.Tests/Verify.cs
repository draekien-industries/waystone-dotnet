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

    /// <summary>
    /// Gets the reference assemblies for the framework this test host is running on.
    /// </summary>
    /// <remarks>
    /// Tracks the host rather than pinning a version, because the references added
    /// below are the assemblies this host has already loaded. Pin net8.0 and a net9.0
    /// host hands the compilation a Shouldly built against System.Runtime 9.0.0.0,
    /// which a net8.0 compilation cannot reference: every test that compiles an
    /// assertion then fails on <c>CS1705</c> rather than on anything the analyzer did.
    /// Keeping the two in step is what lets this project run the full matrix.
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
    /// Every assertion in this package has a <c>ValueTask</c> receiver overload, so
    /// without this the fifteen tests covering them fail on <c>CS0012</c> naming an
    /// assembly their source never mentions.
    /// </para>
    /// <para>
    /// Restoring the package rather than referencing the <c>ValueTask</c> this host has
    /// already loaded, which would be the shorter way to track the version. Adding the
    /// runtime assembly directly gives the compilation a second definition of the type
    /// beside the platform's, and a test that asserts on a compiler error then sees it
    /// rendered against the wrong one. It costs nothing here today and five tests in
    /// the sibling harness, which is reason enough to keep the two the same.
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

    private static ImmutableArray<MetadataReference> References(
        bool withAssertions) =>
        withAssertions ? WithAssertions : WithoutAssertions;

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
            ReferenceAssemblies = Target;

            TestState.AdditionalReferences.AddRange(References(withAssertions));

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
            TestState.AdditionalReferences.AddRange(References(true));
            FixedState.AdditionalReferences.AddRange(References(true));
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
