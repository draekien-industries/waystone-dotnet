namespace Waystone.Monads.Schemas.SourceGenerators;

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

/// <summary>Reports a <c>Schema.Fields</c> call that could be written unqualified.</summary>
/// <remarks>
/// An analyzer rather than more of the generator, because the call it reports binds
/// to a member the generator emitted, and the generator never sees its own output.
/// The analyzer runs over the finished compilation, where it can ask what the call
/// actually reached.
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class QualifiedFieldsAnalyzer : DiagnosticAnalyzer
{
    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get;
    } = ImmutableArray.Create(Rules.PreferUnqualifiedFields);

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(
            GeneratedCodeAnalysisFlags.None);

        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(
            Report,
            SyntaxKind.InvocationExpression);
    }

    /// <summary>
    /// Matches the spelling before binding anything, so the only invocations that
    /// cost a symbol lookup are the ones already written as <c>X.Fields(...)</c>.
    /// Only the receiver is bound: whether it is the generated nested class is the
    /// whole question, and binding the call would resolve its arguments too.
    /// </summary>
    private static void Report(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;

        if (invocation.Expression is not MemberAccessExpressionSyntax access
         || access.Name.Identifier.ValueText != SchemaWriter.FieldsMember)
        {
            return;
        }

        if (context.SemanticModel
                   .GetSymbolInfo(access.Expression, context.CancellationToken)
                   .Symbol is not INamedTypeSymbol
                {
                    Name: SchemaWriter.EntryPointType,
                    ContainingType: { } schema,
                    BaseType: { Name: SchemaWriter.EntryPointType } entryPoint,
                }
         || !Symbols.IsSchemaNamespace(entryPoint.ContainingNamespace)
         || !OnlyTheGeneratedFieldsIsInScope(
                context.SemanticModel,
                access.SpanStart,
                schema))
        {
            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(
                Rules.PreferUnqualifiedFields,
                Location.Create(
                    access.SyntaxTree,
                    TextSpan.FromBounds(
                        access.Expression.SpanStart,
                        access.Name.SpanStart)),
                schema.Name,
                access.ToString()));
    }

    /// <summary>
    /// Whether an unqualified <c>Fields</c> at this position would reach the
    /// generated method rather than something of the consumer's.
    /// </summary>
    /// <remarks>
    /// Nothing in scope is the usual case, and the generator then emits the method
    /// once the call is rewritten. A <c>Fields</c> method already declared on the
    /// schema is the generated one when another call in the body is unqualified;
    /// anything else of that name would capture the rewritten call.
    /// </remarks>
    private static bool OnlyTheGeneratedFieldsIsInScope(
        SemanticModel model,
        int position,
        INamedTypeSymbol schema)
    {
        foreach (ISymbol symbol in model.LookupSymbols(
                     position,
                     name: SchemaWriter.FieldsMember))
        {
            if (symbol is not IMethodSymbol
                {
                    ReturnType: { Name: SchemaWriter.LadderType } ladder,
                }
             || !SymbolEqualityComparer.Default.Equals(
                    ladder.ContainingType,
                    schema))
            {
                return false;
            }
        }

        return true;
    }
}
