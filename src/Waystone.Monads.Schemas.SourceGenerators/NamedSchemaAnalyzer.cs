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

        // Every named spelling is a type in System, so anything that is not one
        // cannot match and is left before its namespace is read. A type parameter is
        // the case that matters: 'Schema.For<T>()' inside a generic method has no
        // containing namespace to render at all.
        if (method.TypeArguments[0] is not INamedTypeSymbol
            {
                ContainingNamespace: { IsGlobalNamespace: false } @namespace,
            } argument)
        {
            return;
        }

        // The key is built rather than rendered, because every display format that
        // qualifies a namespace also spells the built-in types as keywords — so
        // 'string' would never match a key, while 'System.Guid' would.
        if (!NamedSpellings.TryGetValue(
                @namespace.ToDisplayString() + "." + argument.MetadataName,
                out string named))
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
}
