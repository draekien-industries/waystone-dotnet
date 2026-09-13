namespace Waystone.Internal.Analyzers;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Waystone.Internal.Analyzers.AwaitedReceivers;

internal static class VerifyAwaitedReceivers
{
    /// <summary>
    /// The generator's two attributes, declared here because nothing runs the
    /// generator that normally injects them.
    /// </summary>
    private const string GeneratorAttributes = """
        namespace Waystone.Internal.SourceGenerators
        {
            [System.AttributeUsage(System.AttributeTargets.Class)]
            internal sealed class GenerateAwaitedReceiversAttribute : System.Attribute
            {
                public GenerateAwaitedReceiversAttribute(System.Type receiver)
                {
                    Receiver = receiver;
                }

                public System.Type Receiver { get; }
            }

            [System.AttributeUsage(System.AttributeTargets.Method)]
            internal sealed class ExcludeFromAwaitedReceiversAttribute : System.Attribute
            {
            }
        }

        """;

    /// <summary>
    /// A <c>CallerArgumentExpression</c> every framework in the matrix can name.
    /// </summary>
    /// <remarks>
    /// .NET Framework declares none, and a subject's references are the test host's own
    /// assemblies, so without this the net472 and net481 runs analyse a subject where
    /// the attribute failed to bind. The <c>CS0436</c> it raises on the three
    /// frameworks that do declare it never surfaces, because a run returns analyzer
    /// diagnostics only.
    /// </remarks>
    private const string CallerArgumentExpression = """
        namespace System.Runtime.CompilerServices
        {
            [System.AttributeUsage(System.AttributeTargets.Parameter)]
            internal sealed class CallerArgumentExpressionAttribute : System.Attribute
            {
                public CallerArgumentExpressionAttribute(string parameterName)
                {
                    ParameterName = parameterName;
                }

                public string ParameterName { get; }
            }
        }

        """;

    /// <summary>
    /// Runs the rule over <paramref name="source" /> in a compilation that uses the
    /// generator.
    /// </summary>
    /// <param name="source">
    /// The source to analyse, appended to a preamble that already imports the
    /// generator's namespace.
    /// </param>
    /// <returns>The reported diagnostics, in the order the analyzer produced them.</returns>
    public static Task<ImmutableArray<Diagnostic>> Run(string source) =>
        Analyse(
            source,
            GeneratorAttributes + CallerArgumentExpression,
            "using Waystone.Internal.SourceGenerators;\n");

    /// <summary>
    /// Runs the rule over <paramref name="source" /> in a compilation that does not use
    /// the generator, which is the case it must stay silent for.
    /// </summary>
    /// <remarks>
    /// The marker attribute is absent rather than merely unused, which is what a
    /// project not importing the generator's props actually looks like.
    /// </remarks>
    /// <param name="source">The source to analyse, appended to a bare preamble.</param>
    /// <returns>The reported diagnostics, in the order the analyzer produced them.</returns>
    public static Task<ImmutableArray<Diagnostic>> RunWithoutTheGenerator(string source) =>
        Analyse(source, CallerArgumentExpression, string.Empty);

    private static async Task<ImmutableArray<Diagnostic>> Analyse(
        string source,
        string support,
        string generatorUsing)
    {
        string preamble = $"""
            namespace Waystone.Monads.Subject;

            using System;
            using System.Runtime.CompilerServices;
            using System.Threading.Tasks;
            {generatorUsing}

            """;

        CSharpCompilation compilation = CSharpCompilation.Create(
            "Waystone.Internal.Analyzers.Tests.AwaitedReceiverSubject",
            [
                CSharpSyntaxTree.ParseText(support, Parse),
                CSharpSyntaxTree.ParseText(preamble + source, Parse),
            ],
            References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
               .WithNullableContextOptions(NullableContextOptions.Enable));

        CompilationWithAnalyzers analysed = compilation.WithAnalyzers(
            ImmutableArray.Create<DiagnosticAnalyzer>(
                new UncarriedParameterAttributeAnalyzer()));

        return await analysed.GetAnalyzerDiagnosticsAsync();
    }

    private static readonly CSharpParseOptions Parse =
        new(LanguageVersion.Preview);

    private static IEnumerable<MetadataReference> References =>
        AppDomain.CurrentDomain.GetAssemblies()
                 .Where(
                      assembly => !assembly.IsDynamic
                               && assembly.Location.Length > 0)
                 .Select(
                      assembly =>
                          (MetadataReference)MetadataReference.CreateFromFile(
                              assembly.Location))
                 .Distinct();
}
