namespace Waystone.Monads.Schemas.SourceGenerators;

using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

/// <summary>Reports a <c>Schema.For&lt;T&gt;()</c> that has a named spelling.</summary>
/// <remarks>
/// <para>
/// An analyzer rather than more of the generator, and the distinction is not
/// bookkeeping. The generator only ever sees a <c>Configure</c> body, and the
/// call this reports is as likely to sit in a shared static field — the schema
/// declared once and reused by every field that needs it, which is the shape the
/// documentation recommends.
/// </para>
/// <para>
/// It ships in the generator's assembly all the same. That assembly is packed to
/// <c>analyzers/dotnet/cs</c>, which Roslyn loads analyzers and generators from
/// alike, so this reaches a consumer with no packaging of its own.
/// </para>
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class NamedSchemaAnalyzer : DiagnosticAnalyzer
{
    private const string SchemaTypeName = "Schema";

    private const string ForMethodName = "For";

    private static readonly IReadOnlyDictionary<string, string> NamedSpellings =
        new Dictionary<string, string>
        {
            ["System.String"] = "Schema.Text",
            ["System.Boolean"] = "Schema.Bool",
            ["System.Guid"] = "Schema.Uuid",
            ["System.DateTimeOffset"] = "Schema.Timestamp",
            ["System.DateOnly"] = "Schema.Date",
            ["System.Int32"] = "Schema.Number.Int32",
            ["System.Int64"] = "Schema.Number.Int64",
            ["System.Decimal"] = "Schema.Number.Decimal",
            ["System.Double"] = "Schema.Number.Double",
        };

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get;
    } = ImmutableArray.Create(Rules.PreferANamedSchema);

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(
            GeneratedCodeAnalysisFlags.None);

        context.EnableConcurrentExecution();

        context.RegisterOperationAction(Report, OperationKind.Invocation);
    }

    private static void Report(OperationAnalysisContext context)
    {
        var invocation = (IInvocationOperation)context.Operation;

        IMethodSymbol method = invocation.TargetMethod;

        if (method.Name != ForMethodName
         || method.TypeArguments.Length != 1
         || method.ContainingType.Name != SchemaTypeName
         || !Symbols.IsSchemaNamespace(method.ContainingType.ContainingNamespace))
        {
            return;
        }

        if (SymbolEqualityComparer.Default.Equals(
                method.ContainingAssembly,
                context.Compilation.Assembly))
        {
            return;
        }

        ITypeSymbol argument = method.TypeArguments[0];

        if (!NamedSpellings.TryGetValue(MetadataNameOf(argument), out string named))
        {
            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(
                Rules.PreferANamedSchema,
                invocation.Syntax.GetLocation(),
                argument.ToDisplayString(
                    SymbolDisplayFormat.MinimallyQualifiedFormat),
                named));
    }

    /// <remarks>
    /// Built from the namespace and the metadata name rather than rendered, because
    /// every display format that qualifies a namespace also spells the built-in types
    /// as keywords — so <c>string</c> would never match a key, while
    /// <c>System.Guid</c> would.
    /// </remarks>
    private static string MetadataNameOf(ITypeSymbol type) =>
        type.ContainingNamespace is { IsGlobalNamespace: false } @namespace
            ? @namespace.ToDisplayString() + "." + type.MetadataName
            : type.MetadataName;
}
